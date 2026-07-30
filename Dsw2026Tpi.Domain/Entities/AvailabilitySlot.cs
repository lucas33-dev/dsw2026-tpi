using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities;

public enum SlotStatus
{
    Available,
    Booked,
    Blocked
}

public class AvailabilitySlot : EntityBase
{
    public Guid DoctorId { get; init; }
    public Doctor? Doctor { get; private set; }
    public Guid? AvailabilityRuleId { get; init; }
    public DateOnly SlotDate { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public SlotStatus Status { get; private set; }
    public bool Deleted { get; private set; }

#pragma warning disable CS8618
    private AvailabilitySlot() { }
#pragma warning restore CS8618

    public AvailabilitySlot(Guid doctorId, Guid? availabilityRuleId, DateOnly slotDate,
        TimeOnly startTime, TimeOnly endTime, Guid? id = null) : base(id)
    {
        DoctorId = doctorId;
        AvailabilityRuleId = availabilityRuleId;
        SlotDate = slotDate;
        StartTime = startTime;
        EndTime = endTime;
        Status = SlotStatus.Available;
        Deleted = false;
    }

    public void Book()
    {
        Status = SlotStatus.Booked;
    }

    public void Release()
    {
        Status = SlotStatus.Available;
    }

    public void Delete()
    {
        Deleted = true;
    }
}
