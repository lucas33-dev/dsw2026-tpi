using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IPersistence _persistence;

    public DoctorService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<DoctorModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
        var doctors = await _persistence.Paginate<Doctor, string>(pageSize, pageIndex, d => d.IsActive &&
                                                   (string.IsNullOrWhiteSpace(name) || d.Name.Contains(name)), x => x.Name, nameof(Doctor.Speciality));

        return doctors.Map(d => new DoctorModel.Response(d.Id, d.Name, d.LicenseNumber,
            new DoctorModel.SpecialityDto(d.Speciality?.Id, d.Speciality?.Name)));
    }

    public async Task<DoctorModel.Response> Create(DoctorModel.Request request)
    {
        ValidateRequest(request);

        var speciality = await _persistence.GetById<Speciality>(request.SpecialityId);
        if (speciality is null)
            throw new EntityNotFoundException(nameof(Speciality));

        var doctor = new Doctor(request.Name, request.LicenseNumber, speciality);
        await _persistence.Add(doctor);

        return new DoctorModel.Response(doctor.Id, doctor.Name, doctor.LicenseNumber,
            new DoctorModel.SpecialityDto(speciality.Id, speciality.Name));
    }

    public async Task<DoctorModel.Response> Update(Guid id, DoctorModel.Request request)
    {
        ValidateRequest(request);

        var doctor = await _persistence.GetById<Doctor>(id, nameof(Doctor.Speciality));
        if (doctor is null || !doctor.IsActive)
            throw new EntityNotFoundException(nameof(Doctor));

        var speciality = await _persistence.GetById<Speciality>(request.SpecialityId);
        if (speciality is null)
            throw new EntityNotFoundException(nameof(Speciality));

        doctor.GetType().GetProperty(nameof(Doctor.Name))!.SetValue(doctor, request.Name);
        doctor.GetType().GetProperty(nameof(Doctor.LicenseNumber))!.SetValue(doctor, request.LicenseNumber);
        doctor.GetType().GetProperty(nameof(Doctor.SpecialityId))!.SetValue(doctor, speciality.Id);

        await _persistence.Update(doctor);

        return new DoctorModel.Response(doctor.Id, doctor.Name, doctor.LicenseNumber,
            new DoctorModel.SpecialityDto(speciality.Id, speciality.Name));
    }

    public async Task Delete(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(id);
        if (doctor is null || !doctor.IsActive)
            throw new EntityNotFoundException(nameof(Doctor));

        doctor.Deactivate();
        await _persistence.Update(doctor);
    }

    private static void ValidateRequest(DoctorModel.Request request)
    {
        var exception = new ValidationException();

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 3 || request.Name.Length > 100)
            exception.WithDetail(nameof(request.Name), "El nombre debe tener entre 3 y 100 caracteres");

        if (string.IsNullOrWhiteSpace(request.LicenseNumber))
            exception.WithDetail(nameof(request.LicenseNumber), "La matricula es obligatoria");

        if (exception.Error.Details.Count > 0) throw exception;
    }
}