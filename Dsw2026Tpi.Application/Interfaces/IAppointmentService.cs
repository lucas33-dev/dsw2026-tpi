using System;
using System.Collections.Generic;
using System.Text;

using Dsw2026Tpi.Application.Dtos;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentModel.Response> Create(AppointmentModel.Request request);
    Task<List<AppointmentModel.Response>> GetByPatientDni(long dni);
    Task Cancel(Guid id);
    Task<List<AppointmentModel.Response>> GetByDate(DateOnly? date);
    Task<List<AppointmentModel.Response>> Search(Guid? specialtyId, Guid? doctorId, long? dni, DateOnly? date);
}
