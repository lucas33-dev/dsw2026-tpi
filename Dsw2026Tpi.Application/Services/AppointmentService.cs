using System;
using System.Collections.Generic;
using System.Text;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dsw2026Tpi.Application.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IPersistence _persistence;

    public AppointmentService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<AppointmentModel.Response> Create(AppointmentModel.Request request)
    {
        ValidateRequest(request);

        var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
        if (doctor is null || !doctor.IsActive)
            throw new EntityNotFoundException(nameof(Doctor));

        var slot = await _persistence.GetById<AvailabilitySlot>(request.AvailabilityId);
        if (slot is null || slot.DoctorId != request.DoctorId || slot.Deleted)
            throw new EntityNotFoundException(nameof(AvailabilitySlot));

        if (slot.Status != SlotStatus.Available)
            throw new BusinessRuleException("El turno ya no esta disponible", "APPOINTMENT_CONFLICT");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (slot.SlotDate < today)
            throw new BusinessRuleException("No se pueden reservar turnos en el pasado", "APPOINTMENT_PAST");

        var patient = await _persistence.First<Patient>(p => p.Dni == request.Patient.Dni && !p.Deleted);
        if (patient is null)
            throw new EntityNotFoundException(nameof(Patient));

        try
        {
            slot.Book();
            await _persistence.Update(slot);

            var appointment = new Appointment(slot.Id, patient.Id, request.Reason);
            await _persistence.Add(appointment);

            return await BuildResponse(appointment, slot, doctor, patient);
        }
        catch (DbUpdateException)
        {
            slot.Release();
            await _persistence.Update(slot);
            throw new BusinessRuleException("El turno ya fue reservado por otro paciente", "APPOINTMENT_CONFLICT");
        }
    }

    public async Task<List<AppointmentModel.Response>> GetByPatientDni(long dni)
    {
        var patient = await _persistence.First<Patient>(p => p.Dni == dni && !p.Deleted);
        if (patient is null) return [];

        var appointments = await _persistence.GetFiltered<Appointment>(a =>
            a.PatientId == patient.Id && a.Status == AppointmentStatus.Booked,
            nameof(Appointment.AvailabilitySlot));

        if (appointments is null) return [];

        var result = new List<AppointmentModel.Response>();
        foreach (var appointment in appointments)
        {
            var slot = appointment.AvailabilitySlot!;
            var doctor = await _persistence.GetById<Doctor>(slot.DoctorId);
            result.Add(await BuildResponse(appointment, slot, doctor!, patient));
        }
        return result;
    }

    public async Task Cancel(Guid id)
    {
        var appointment = await _persistence.GetById<Appointment>(id, nameof(Appointment.AvailabilitySlot));
        if (appointment is null)
            throw new EntityNotFoundException(nameof(Appointment));

        if (appointment.Status != AppointmentStatus.Booked)
            throw new BusinessRuleException("Solo se pueden cancelar turnos reservados", "APPOINTMENT_NOT_CANCELLABLE");

        appointment.Cancel();
        await _persistence.Update(appointment);

        var slot = appointment.AvailabilitySlot;
        if (slot is not null)
        {
            slot.Release();
            await _persistence.Update(slot);
        }
    }

    public async Task<List<AppointmentModel.Response>> GetByDate(DateOnly? date)
    {
        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var appointments = await _persistence.GetFiltered<Appointment>(a =>
            a.AvailabilitySlot!.SlotDate == targetDate,
            nameof(Appointment.AvailabilitySlot), nameof(Appointment.Patient));

        return await BuildResponseList(appointments);
    }

    public async Task<List<AppointmentModel.Response>> Search(Guid? specialtyId, Guid? doctorId, long? dni, DateOnly? date)
    {
        var appointments = await _persistence.GetFiltered<Appointment>(a =>
            (doctorId == null || a.AvailabilitySlot!.DoctorId == doctorId) &&
            (date == null || a.AvailabilitySlot!.SlotDate == date) &&
            (dni == null || a.Patient!.Dni == dni),
            nameof(Appointment.AvailabilitySlot), nameof(Appointment.Patient));

        if (appointments is null) return [];

        if (specialtyId is not null)
        {
            var filtered = new List<Appointment>();
            foreach (var a in appointments)
            {
                var doctor = await _persistence.GetById<Doctor>(a.AvailabilitySlot!.DoctorId);
                if (doctor?.SpecialityId == specialtyId) filtered.Add(a);
            }
            appointments = filtered;
        }

        return await BuildResponseList(appointments);
    }

    private async Task<List<AppointmentModel.Response>> BuildResponseList(IEnumerable<Appointment>? appointments)
    {
        var result = new List<AppointmentModel.Response>();
        if (appointments is null) return result;

        foreach (var appointment in appointments)
        {
            var slot = appointment.AvailabilitySlot!;
            var patient = appointment.Patient!;
            var doctor = await _persistence.GetById<Doctor>(slot.DoctorId);
            result.Add(await BuildResponse(appointment, slot, doctor!, patient));
        }
        return result;
    }

    private static Task<AppointmentModel.Response> BuildResponse(Appointment appointment, AvailabilitySlot slot, Doctor doctor, Patient patient)
    {
        return Task.FromResult(new AppointmentModel.Response(
            appointment.Id,
            doctor.Id,
            doctor.Name,
            slot.SlotDate,
            slot.StartTime.ToString("HH:mm"),
            slot.EndTime.ToString("HH:mm"),
            patient.Dni,
            appointment.Reason,
            appointment.Status.ToString()
        ));
    }

    private static void ValidateRequest(AppointmentModel.Request request)
    {
        var exception = new ValidationException();

        if (request.Patient.Dni.ToString().Length is < 7 or > 10)
            exception.WithDetail(nameof(request.Patient.Dni), "El dni debe tener entre 7 y 10 digitos");

        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length < 5)
            exception.WithDetail(nameof(request.Reason), "El motivo debe tener al menos 5 caracteres");

        if (exception.Error.Details.Count > 0) throw exception;
    }
}
