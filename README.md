# Trabajo Práctico Integrador
## Desarrollo de Software 2026

Acceso al [documento](https://frtutneduar-my.sharepoint.com/:b:/g/personal/franciscovicente_doc_frt_utn_edu_ar/IQD-5kaAARqnT5eL7EnPMCPgAX2LFXXX6e3p-u1C43z5rsQ?e=lbbpnz)

### Integrantes
- Ignacio Matías Ferreyra (58432)
- Lucas Tomás Ferreyra (58294)

### Cómo ejecutar

Clonar y restaurar paquetes:

    git clone https://github.com/lucas33-dev/dsw2026-tpi.git
    cd dsw2026-tpi
    dotnet restore

Crear la base de datos. Hay dos contextos (Identity y el de negocio), hay que correr los dos:

    dotnet ef database update --project Dsw2026Tpi.Data --startup-project Dsw2026Tpi.Api --context Dsw2026TpiDbContext
    dotnet ef database update --project Dsw2026Tpi.Data --startup-project Dsw2026Tpi.Api --context AuthenticationDbContext

Correr desde Visual Studio poniendo Dsw2026Tpi.Api como proyecto de inicio, o por consola:

    dotnet run --project Dsw2026Tpi.Api

Se abre Swagger para probar los endpoints.

Usamos SQL Server LocalDB, la cadena de conexión está en appsettings.Development.json.

### Autenticación

El admin entra con email y password. El paciente entra con email y dni, y si no existe se registra solo en el primer login. Los dos devuelven un token JWT que hay que mandar en el header Authorization: Bearer {token} (en Swagger, con el botón Authorize).

### Endpoints

Auth:
- POST /api/auth/admin/register
- POST /api/auth/admin/login
- POST /api/auth/patient/login

Especialidades (admin):
- GET /api/specialties
- POST /api/specialties
- PUT /api/specialties/{id}
- DELETE /api/specialties/{id}

Médicos (admin):
- GET /api/doctors
- POST /api/doctors
- PUT /api/doctors/{id}
- DELETE /api/doctors/{id}
- GET /api/doctors/{id}/availabilities

Disponibilidades (admin):
- POST /api/availabilities
- PUT /api/availabilities

Turnos:
- POST /api/appointments (paciente)
- GET /api/appointments/patient?dni={dni} (paciente)
- DELETE /api/appointments/{id} (paciente)
- GET /api/appointments?date={fecha} (admin)
- GET /api/appointments/search (admin)

---

Instrucciones de la cátedra:
* Realizar una bifurcación por grupo
* Crear una rama de larga duración `development`
* Completar `README` con los integrantes en cada bifurcación
* Todos los integrantes deben participar con confirmaciones en el repositorio bifurcado
* Organizar el trabajo en equipo y crear ramas temporales
* Actualizar la rama de larga duración mediante **pull-requests**
* No eliminar las ramas temporales
