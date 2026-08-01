using System;
using System.Collections.Generic;
using System.Text;
namespace Dsw2026Tpi.CrossCutting.Helpers;

public static class AvailabilityCalculator
{
    public static List<DateOnly> GetDatesForDayOfWeek(int year, int month, DayOfWeek dayOfWeek)
    {
        var dates = new List<DateOnly>();
        var current = new DateOnly(year, month, 1);

        while (current.DayOfWeek != dayOfWeek)
            current = current.AddDays(1);

        while (current.Month == month)
        {
            dates.Add(current);
            current = current.AddDays(7);
        }

        return dates;
    }

    public static List<(TimeOnly Start, TimeOnly End)> GetTimeSlots(TimeOnly startTime, TimeOnly endTime)
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
}
