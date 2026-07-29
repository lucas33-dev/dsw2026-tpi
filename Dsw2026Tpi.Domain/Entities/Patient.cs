namespace Dsw2026Tpi.Domain.Entities;

public class Patient : EntityBase
{
    public string UserId { get; init; }
    public long Dni { get; init; }
    public string FullName { get; init; }
    public bool Deleted { get; private set; }

#pragma warning disable CS8618
    private Patient()
    {
    }
#pragma warning restore CS8618

    public Patient(string userId, long dni, string fullName, Guid? id = null) : base(id)
    {
        UserId = userId;
        Dni = dni;
        FullName = fullName;
        Deleted = false;
    }

    public void Delete()
    {
        Deleted = true;
    }
}