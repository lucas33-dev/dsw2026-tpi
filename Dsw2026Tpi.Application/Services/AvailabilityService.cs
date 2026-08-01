using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Helpers;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly IPersistence _persistence;
    private readonly ILogger<AvailabilityService> _logger;

    public AvailabilityService(IPersistence persistence, ILogger<AvailabilityService> logger)
    {
        _persistence = persistence;
        _logger = logger;
    }
    public async Task<List<AvailabilityModel.DayResponse>> GetByDoctor(Guid doctorId)
    {
        var doctor = await _persistence.GetById<Doctor>(doctorId);
        if (doctor is null) throw new EntityNotFoundException(nameof(Doctor));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var rules = await _persistence.GetFiltered<AvailabilityRule>(r =>
            r.DoctorId == doctorId && !r.Deleted && r.Year == today.Year && r.Month == today.Month);

        if (rules is null) return [];

        return rules.Select(r => new AvailabilityModel.DayResponse(
            r.DayOfWeek.ToSpanish(),
            r.StartTime.ToString("HH:mm"),
            r.EndTime.ToString("HH:mm")
        )).ToList();
    }

    public async Task SetAvailability(AvailabilityModel.Request request)
    {
        var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
        if (doctor is null) throw new EntityNotFoundException(nameof(Doctor));

        ValidateRequest(request);
        _logger.LogInformation("Iniciando generacion de disponibilidad para el medico {DoctorId} con {DaysCount} dias",
        request.DoctorId, request.Days.Count);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var year = today.Year;
        var month = today.Month;

        var existingRules = await _persistence.GetFiltered<AvailabilityRule>(r =>
            r.DoctorId == request.DoctorId && !r.Deleted && r.Year == year && r.Month == month);

        if (existingRules is not null)
        {
            foreach (var oldRule in existingRules)
            {
                oldRule.Delete();
                await _persistence.Update(oldRule);
            }
        }

        var existingSlots = await _persistence.GetFiltered<AvailabilitySlot>(s =>
            s.DoctorId == request.DoctorId && !s.Deleted &&
            s.SlotDate >= today && s.SlotDate.Year == year && s.SlotDate.Month == month &&
            s.Status == SlotStatus.Available);

        if (existingSlots is not null)
        {
            foreach (var oldSlot in existingSlots)
            {
                oldSlot.Delete();
                await _persistence.Update(oldSlot);
            }
        }

        foreach (var day in request.Days)
        {
            var dayOfWeek = day.Day.ToDayOfWeek();
            var startTime = TimeOnly.Parse(day.StartTime);
            var endTime = TimeOnly.Parse(day.EndTime);

            var existingRule = await _persistence.First<AvailabilityRule>(r =>
     r.DoctorId == request.DoctorId && r.Year == year && r.Month == month &&
     r.DayOfWeek == dayOfWeek && r.StartTime == startTime && r.EndTime == endTime);

            AvailabilityRule rule;

            if (existingRule is not null)
            {
                existingRule.Reactivate();
                await _persistence.Update(existingRule);
                rule = existingRule;
            }
            else
            {
                rule = new AvailabilityRule(request.DoctorId, month, year, dayOfWeek, startTime, endTime);
                await _persistence.Add(rule);
            }

            var dates = AvailabilityCalculator.GetDatesForDayOfWeek(year, month, dayOfWeek)
    .Where(d => d >= today)
    .ToList();

            var timeSlots = AvailabilityCalculator.GetTimeSlots(startTime, endTime);

            foreach (var date in dates)
            {
                foreach (var (slotStart, slotEnd) in timeSlots)
                {
                    var existingSlot = await _persistence.First<AvailabilitySlot>(s =>
                        s.DoctorId == request.DoctorId && s.SlotDate == date && s.StartTime == slotStart);

                    if (existingSlot is not null && existingSlot.Deleted)
                    {
                        existingSlot.GetType().GetProperty(nameof(AvailabilitySlot.Deleted))!.SetValue(existingSlot, false);//usé reflexión (GetProperty().SetValue()) para reactivar el Deleted de un slot, porque a diferencia de AvailabilityRule, no le pusimos un método Reactivate() a AvailabilitySlot. Es exactamente el mismo parche que ya identificamos como pendiente de mejora en Especialidades — lo dejamos anotado, pero para no frenar el avance ahora, funciona igual.
                        await _persistence.Update(existingSlot);
                    }
                    else if (existingSlot is null)
                    {
                        var slot = new AvailabilitySlot(request.DoctorId, rule.Id, date, slotStart, slotEnd);
                        await _persistence.Add(slot);
                    }
                }
            }
        }
        _logger.LogInformation("Disponibilidad generada correctamente para el medico {DoctorId}", request.DoctorId);
    }

    private static List<DateOnly> GetDatesForDayOfWeek(int year, int month, DayOfWeek dayOfWeek)
    {
        var dates = new List<DateOnly>();
        var current = new DateOnly(year, month, 1);

        while (current.DayOfWeek != dayOfWeek)
        {
            current = current.AddDays(1);
        }

        while (current.Month == month)
        {
            dates.Add(current);
            current = current.AddDays(7);
        }

        return dates;
    }

    private static List<(TimeOnly Start, TimeOnly End)> GetTimeSlots(TimeOnly startTime, TimeOnly endTime)
    {
        var slots = new List<(TimeOnly, TimeOnly)>();
        var current = startTime;

        while (current.AddMinutes(30) <= endTime)
        {
            var slotEnd = current.AddMinutes(30);
            slots.Add((current, slotEnd));
            current = slotEnd;
        }

        return slots;
    }

    private static void ValidateRequest(AvailabilityModel.Request request)
    {
        var exception = new ValidationException();

        if (request.Days is null || request.Days.Count == 0)
            exception.WithDetail(nameof(request.Days), "Debe indicar al menos un dia");

        foreach (var day in request.Days ?? [])
        {
            if (!TimeOnly.TryParse(day.StartTime, out var start) || !TimeOnly.TryParse(day.EndTime, out var end))
            {
                exception.WithDetail(day.Day, "Formato de hora invalido");
                continue;
            }

            if (start >= end)
                exception.WithDetail(day.Day, "La hora de inicio debe ser menor a la de fin");
        }

        if (exception.Error.Details.Count > 0) throw exception;
    }
}
