using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos;

public record AppointmentModel
{
    public record PatientDto(long Dni);
    public record Request(Guid DoctorId, Guid AvailabilityId, PatientDto Patient, string Reason);

    public record Response(
        Guid Id,
        Guid DoctorId,
        string DoctorName,
        DateOnly Date,
        string StartTime,
        string EndTime,
        long PatientDni,
        string Reason,
        string Status
    );
}
