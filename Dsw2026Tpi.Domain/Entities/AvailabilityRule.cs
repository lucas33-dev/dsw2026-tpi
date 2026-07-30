using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities;

public class AvailabilityRule : EntityBase
{
    public Guid DoctorId { get; init; }
    public Doctor? Doctor { get; private set; }
    public int Month { get; init; }
    public int Year { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public bool Deleted { get; private set; }

#pragma warning disable CS8618
    private AvailabilityRule() { }
#pragma warning restore CS8618

    public AvailabilityRule(Guid doctorId, int month, int year, DayOfWeek dayOfWeek,
        TimeOnly startTime, TimeOnly endTime, Guid? id = null) : base(id)
    {
        DoctorId = doctorId;
        Month = month;
        Year = year;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        Deleted = false;
    }

    public void Delete()
    {
        Deleted = true;
    }
}
