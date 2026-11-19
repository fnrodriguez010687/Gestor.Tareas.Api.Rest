# API REST para Gestión de Tareas

API desarrollada con ASP.NET Core 9 (C#) y PostgreSQL para crear, consultar, actualizar, eliminar y filtrar tareas.

## Requisitos

- .NET SDK 9
- PostgreSQL 15 o compatible
- Una conexión local o remota a PostgreSQL
- EF Core CLI: `dotnet tool install --global dotnet-ef` (si aún no está instalado)

## Configuración de PostgreSQL

No se debe guardar la contraseña en `appsettings.json` ni subirla al repositorio.

Para desarrollo local, configure la cadena de conexión mediante una variable de entorno:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=gestor_tareas;Username=postgres;Password=TU_PASSWORD"
```

También puede usar User Secrets:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=gestor_tareas;Username=postgres;Password=TU_PASSWORD"
```

La base `gestor_tareas` debe existir o debe tener permisos para que PostgreSQL la cree previamente. Ejemplo:

```powershell
& "C:\Program Files\PostgreSQL\15\bin\psql.exe" -h localhost -p 5432 -U postgres -d postgres
CREATE DATABASE gestor_tareas;
```

## Ejecutar

```powershell
dotnet restore
dotnet build
dotnet run
```

La aplicación aplica automáticamente las migraciones pendientes al iniciar.

Por defecto, el perfil HTTP usa `http://localhost:5123`. La URL HTTPS configurada puede variar según `Properties/launchSettings.json`.

## Migraciones

Crear una migración:

```powershell
dotnet ef migrations add NombreDeLaMigracion
```

Aplicar migraciones:

```powershell
dotnet ef database update
```

El proyecto incluye una migración inicial y una migración que transforma el estado almacenado como texto a un valor entero.

## Modelo de tarea

| Campo | Tipo | Reglas |
|---|---|---|
| `id` | entero | Generado por PostgreSQL |
| `title` | texto | Obligatorio, máximo 200 caracteres |
| `description` | texto | Opcional, máximo 2000 caracteres |
| `status` | entero | Obligatorio: `1`, `2` o `3` |
| `assignee` | texto | Obligatorio, máximo 120 caracteres |
| `dueDate` | fecha | Obligatorio, formato `AAAA-MM-DD` |

Estados:

| Valor | Nombre |
|---:|---|
| `1` | Pendiente |
| `2` | EnCurso |
| `3` | Hecha |

Las respuestas incluyen `status` con el número y `statusName` con el nombre legible.

## Endpoints

| Método | Ruta | Resultado |
|---|---|---|
| `GET` | `/api/tasks` | Lista paginada |
| `GET` | `/api/tasks/{id}` | Tarea por identificador |
| `POST` | `/api/tasks` | Crea una tarea |
| `PUT` | `/api/tasks/{id}` | Actualiza una tarea |
| `DELETE` | `/api/tasks/{id}` | Elimina una tarea |

Ejemplo de listado:

```text
GET /api/tasks?page=1&pageSize=20
GET /api/tasks?status=2&assignee=Ana&page=1&pageSize=20
```

Combinación de filtros:

- `status`: `1`, `2` o `3`.
- `assignee`: coincidencia exacta después de eliminar espacios externos.
- `page`: comienza en `1`.
- `pageSize`: entre `1` y `100`.

Ejemplo de creación:

```json
{
  "title": "Preparar entrega",
  "description": "Completar la API de tareas",
  "status": 1,
  "assignee": "Ana",
  "dueDate": "2026-12-01"
}
```

Códigos HTTP principales:

- `200 OK`: consulta realizada.
- `201 Created`: tarea creada; incluye la cabecera `Location`.
- `204 No Content`: tarea actualizada o eliminada.
- `400 Bad Request`: paginación, estado o datos inválidos.
- `404 Not Found`: el identificador no existe.

## Arquitectura y patrones

El proyecto usa una separación por carpetas dentro de un único proyecto, manteniendo una estructura simple:

- `Domain`: entidad `TaskItem` y enum `TaskState`.
- `Application`: interfaces, servicio de aplicación y contrato de paginación.
- `Infrastructure`: `AppDbContext`, repositorio EF Core, Unit of Work y migraciones.
- `API`: endpoints Minimal API, DTOs y traducción de errores HTTP.

### Repository y Unit of Work

`ITaskRepository` abstrae el acceso a datos y `IUnitOfWork.SaveChangesAsync()` delega a `AppDbContext.SaveChangesAsync()`. Los endpoints no acceden directamente al `DbContext`.

### SOLID

- Responsabilidad única: endpoints HTTP, servicio de aplicación y repositorio tienen responsabilidades separadas.
- Abierto/cerrado: nuevas reglas o filtros pueden incorporarse sin alterar las entidades HTTP básicas.
- Sustitución de Liskov e inversión de dependencias: Application depende de interfaces implementadas por Infrastructure.
- Segregación de interfaces: el repositorio y el servicio exponen operaciones relacionadas, no interfaces generales innecesarias.

EF Core ya administra transacciones y `SaveChanges()`, por lo que Unit of Work se implementa como una abstracción de persistencia mantenible. Para este alcance no era imprescindible, pero demuestra la separación de Application y Infrastructure.

## Validación y errores

La API valida formato y límites mediante DTOs. El servicio de aplicación valida título, descripción, estado, responsable y fecha límite antes de persistir.

Los estados se representan como números explícitos para que integraciones externas no dependan de nombres localizados:

- No se reutilizan valores existentes.
- Se rechazan números no definidos.
- `statusName` proporciona el nombre legible como información adicional.

La migración `ConvertTaskStatusToInt` convierte los textos conocidos `Pendiente`, `EnCurso` y `Hecha` a `1`, `2` y `3`. Si encuentra un estado desconocido, lanza una excepción y aborta la migración. El rollback realiza la conversión inversa.

## Pruebas

El proyecto incluye el proyecto `tests/Gestor.Tareas.Api.Rest.Tests`.

Ejecutar todas las pruebas:

```powershell
dotnet test
```

Pruebas unitarias:

- Cálculo de páginas con un repositorio simulando 5 elementos y tamaño 2.
- Rechazo de título vacío.
- Rechazo de estado inválido.

Pruebas de integración:

- `POST /api/tasks` devuelve `201`.
- Estado inválido devuelve `400`.
- Identificador inexistente devuelve `404`.

Las integraciones usan `WebApplicationFactory` y PostgreSQL local con una base separada `gestor_tareas_tests`. El fixture aplica migraciones y limpia `Tasks` antes de cada caso.

Ejecutadas en el entorno local:

- 6 pruebas totales.
- 3 pruebas unitarias correctas.
- 3 pruebas de integración correctas.

## Decisiones técnicas

- Minimal API por el tamaño del proyecto y su bajo nivel de repetición.
- Un único proyecto con capas lógicas, evitando la complejidad inicial de cuatro proyectos ensamblados.
- PostgreSQL mediante Npgsql y EF Core.
- Migraciones automáticas al iniciar la aplicación.
- Estados persistidos como enteros para estabilidad de integraciones.
- DTOs separados de entidades de dominio para no exponer `TaskItem` directamente.
- Fechas límite con `DateOnly`, sin hora ni zona horaria.

## Mejoras con más tiempo

- Añadir autenticación y autorización por roles.
- Incorporar paginación basada en cursor para conjuntos muy grandes.
- Crear filtros por rango de fecha límite.
- Añadir búsqueda parcial y normalizada por responsable.
- Definir un mecanismo consistente de códigos de error y un formato RFC 7807 para `ProblemDetails`.
- Incorporar logging estructurado y trazas OpenTelemetry.
- Añadir Docker Compose para API y PostgreSQL.
- Elevar la separación a proyectos distintos si el dominio creciera.
- Medir cobertura y automatizar pruebas en CI/CD.

## Uso de inteligencia artificial

La IA se utilizó como asistente de implementación y revisión técnica, no como reemplazo de validación manual.

### Solicitudes realizadas

- Diseñar la API y descomponer el trabajo en commits pequeños.
- Crear entidad, DbContext, migraciones y CRUD con ASP.NET Core.
- Adoptar PostgreSQL en lugar de SQLite.
- Aplicar por capas Repository, Unit of Work y principios SOLID sin crear proyectos excesivos.
- Cambiar los estados a valores numéricos y crear una migración segura.
- Crear pruebas unitarias e integración.
- Documentar decisiones, ejecución y errores conocidos.

### Cambios aceptados

- EF Core/Npgsql 9 para el proyecto .NET 9.
- Minimal API en vez de cambiar a una arquitectura completa basada en controladores.
- Migraciones automáticas al arrancar.
- Estados numéricos explícitos `1`, `2` y `3`, con nombre legible adicional en las respuestas.
- PostgreSQL local como base de pruebas y desarrollo.

### Correcciones realizadas

- Se reemplazó el endpoint inicial de clima por `/api/tasks`.
- Se añadió `TaskState` con valores explícitos para evitar ambigüedad con tipos del framework.
- Se corrigió una migración automática que solo alteraba tipos y podía perder datos; se sustituyó por CASE SQL para texto→entero y rollback inverso.
- Se sustituyó el acceso directo a `DbContext` en el endpoint por `ITaskService`, Repository y Unit of Work.
- Se corrigió el fixture de integración para inyectar la cadena antes de construir el host.
- Se ajustaron las pruebas asíncronas para esperar correctamente las excepciones.

No se aceptaron respuestas de IA sin compilación, ejecución o revisión del diff. PostgreSQL, migraciones y endpoints principales se validaron localmente antes de documentarlos.