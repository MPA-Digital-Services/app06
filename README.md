# NorthwindApi

[![Pipeline](https://gitlab.com/NAMESPACE/PROJECT/badges/main/pipeline.svg)](https://gitlab.com/NAMESPACE/PROJECT/-/pipelines)
[![Cobertura](https://gitlab.com/NAMESPACE/PROJECT/badges/main/coverage.svg)](https://gitlab.com/NAMESPACE/PROJECT/-/pipelines)

> Sustituya `NAMESPACE/PROJECT` por la ruta real del proyecto en GitLab para que los
> badges de pipeline y cobertura apunten al repositorio correcto.

API REST sobre la base de datos **Northwind** (SQL Server), construida con **ASP.NET Core (.NET 10)**
y **Entity Framework Core (database-first)**. Expone:

- **CRUD** de las entidades de Northwind (Categories, Customers, Employees, Products, Orders, …).
- **Solo lectura** de las **16 vistas** de la base de datos.
- Invocación **explícita y parametrizada** de los **7 procedimientos almacenados**.
- **Dashboard** con métricas de ventas (JSON) y una **página web** de panel.
- **Exportación de reportes a PDF y XLSX**.
- **Swagger UI**, comprobación de **salud** y manejo de errores con **ProblemDetails**.

---

## Estructura de la solución

```
NorthwindApi.slnx
instnwnd.sql                              Script de creación/datos de Northwind
Dockerfile                                Imagen de la API (multi-stage, Kestrel)
docker-compose.yml                        SQL Server 2022 + init de BD + API (desarrollo)
.gitlab-ci.yml                            Pipeline CI/CD (build, test, package, deploy)
.dockerignore / .env.example              Contexto de build y plantilla de variables
deploy/
  sqlserver/                              Imagen de SQL Server (volumen persistente + init Northwind)
  docker-compose.prod.yml                 Producción: consume las imágenes del registro de GitLab
  deploy.sh                               Despliegue por SSH sobre Docker Compose
scripts/init-northwind.sh                 Carga de la base Northwind (CI y local)
docs/database-analysis.md                 Análisis del esquema y los datos
src/
  NorthwindApi.Api/                       Capa web (controllers, middleware, Swagger, dashboard)
  NorthwindApi.Application/               DTOs, modelos, abstracciones, utilidades comunes
  NorthwindApi.Infrastructure/            EF Core (scaffolded), servicios de datos, reportes
tests/
  NorthwindApi.Tests/                     Pruebas unitarias (InMemory) e integración (WebApplicationFactory)
```

Dependencias: `Api → Application + Infrastructure`, `Infrastructure → Application`, `Tests → Api + Infrastructure + Application`.

---

## Requisitos

- **.NET SDK 10** (`net10.0`).
- **SQL Server 2022** (objetivo de producción) o **SQL Server LocalDB** (desarrollo).
- (Opcional, para re-scaffolding) `dotnet tool install --global dotnet-ef --version 10.*`.

---

## Desarrollo local paso a paso

De cero a API corriendo en desarrollo (Windows + LocalDB):

1. **Crear la base de datos** (ver [Puesta en marcha de la base de datos](#puesta-en-marcha-de-la-base-de-datos)):
   `CREATE DATABASE Northwind` y carga de `instnwnd.sql` con `sqlcmd`.
2. **Configurar la cadena de conexión** con User Secrets (ver
   [Configuración de la cadena de conexión](#configuración-de-la-cadena-de-conexión)).
3. **Ejecutar la API**: `dotnet run --project src/NorthwindApi.Api` y abrir
   `http://localhost:5297/swagger` y `http://localhost:5297/health`.
4. **Verificar con pruebas**: `dotnet test` (unitarias en memoria + integración
   contra la base real; ver [Pruebas](#pruebas)).
5. **Extender**: para un nuevo recurso CRUD siga
   [Cómo agregar un nuevo recurso CRUD](#cómo-agregar-un-nuevo-recurso-crud); si cambió
   el esquema, regenere el contexto según
   [EF Core database-first (re-scaffolding)](#ef-core-database-first-re-scaffolding).

---

## Puesta en marcha de la base de datos

El script de origen es `instnwnd.sql` (raíz del repositorio). Con **sqlcmd**:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "IF DB_ID('Northwind') IS NULL CREATE DATABASE Northwind;"
sqlcmd -S "(localdb)\MSSQLLocalDB" -d Northwind -i instnwnd.sql -f 65001
```

Verificación rápida:

```powershell
sqlcmd -S "(localdb)\MSSQLLocalDB" -d Northwind -Q "SELECT (SELECT COUNT(*) FROM sys.tables) AS tablas, (SELECT COUNT(*) FROM sys.views) AS vistas, (SELECT COUNT(*) FROM sys.procedures) AS proc;"
```

Resultado esperado: **13 tablas, 16 vistas, 7 procedimientos**.

---

## Configuración de la cadena de conexión

La cadena se lee de `ConnectionStrings:Northwind`. **Nunca** se codifican credenciales en el repositorio.

**Desarrollo (User Secrets):**

```powershell
dotnet user-secrets set "ConnectionStrings:Northwind" `
  "Server=(localdb)\MSSQLLocalDB;Database=Northwind;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True" `
  --project src/NorthwindApi.Api
```

**Producción (variable de entorno):**

```
ConnectionStrings__Northwind = "Server=servidor;Database=Northwind;User Id=app;Password=***;TrustServerCertificate=True"
```

> `appsettings.json` mantiene la clave `ConnectionStrings:Northwind` vacía como plantilla.

---

## Ejecución

```powershell
dotnet run --project src/NorthwindApi.Api
```

| Recurso      | URL                              |
|--------------|----------------------------------|
| Dashboard    | `http://localhost:5297/`         |
| Swagger UI   | `http://localhost:5297/swagger`  |
| Salud        | `http://localhost:5297/health`   |

### Comandos de ejecución

| Comando                                            | Descripción                                          |
| -------------------------------------------------- | ---------------------------------------------------- |
| `dotnet restore`                                   | Restaura los paquetes NuGet de la solución           |
| `dotnet build NorthwindApi.slnx`                   | Compila toda la solución (0 errores / 0 advertencias)|
| `dotnet build -c Release NorthwindApi.slnx`        | Compila en configuración Release                     |
| `dotnet run --project src/NorthwindApi.Api`        | Arranca la API en desarrollo (`:5297`)               |
| `dotnet watch --project src/NorthwindApi.Api run`  | Recompila y reinicia al detectar cambios             |
| `dotnet test`                                      | Ejecuta la suite de pruebas (unitarias + integración)|
| `dotnet clean`                                     | Limpia los artefactos de compilación                 |
| `dotnet publish src/NorthwindApi.Api -c Release`   | Genera artefactos listos para despliegue             |

---

## Desarrollo del backend

### Arquitectura por capas

La solución se divide en tres proyectos con dependencias unidireccionales
(`Api → Application + Infrastructure`, `Infrastructure → Application`):

```
┌──────────────────────────────────────────────────────────────────┐
│ NorthwindApi.Api            (presentación)                        │
│   Controllers · ExceptionHandlingMiddleware · Program.cs          │
│   Swagger · CORS · archivos estáticos (panel) · /health           │
└───────────────┬──────────────────────────────────────────────────┘
                │ depende de
    ┌───────────┴───────────┐
    ▼                       ▼
┌──────────────────────┐  ┌────────────────────────────────────────┐
│ Application          │  │ Infrastructure                          │
│  · DTOs / Modelos    │◄─┤  · EF Core (Data/Generated, scaffold)   │
│  · Abstracciones     │  │  · CrudServiceBase / EntityServices     │
│  · PageQuery, orden  │  │  · ViewService · StoredProcedureService │
│  · Excepciones       │  │  · DashboardService · ReportService     │
└──────────────────────┘  │  · Exportadores PDF / XLSX              │
                          └────────────────────────────────────────┘
```

**Flujo de una petición:**

```
HTTP → Controller → Servicio (abstracción en Application)
     → EF Core / SqlQuery (Infrastructure) → SQL Server
     ↑ ExceptionHandlingMiddleware traduce excepciones a ProblemDetails (400/404/409/500)
```

### Responsabilidades por proyecto

- **`Application`** — independiente de EF y ASP.NET. Define los **DTOs** (records con
  `DataAnnotations` en los *parámetros de constructor*), los **modelos de resultado** (vistas,
  procedimientos, dashboard, reportes), las **abstracciones** (`ICrudService<…>`, `IQueryServices`)
  y utilidades transversales (`PageQuery`, `PagedResult`, `QueryableExtensions`, `Exceptions`).
- **`Infrastructure`** — implementa las abstracciones con EF Core: el `NorthwindContext` y las
  entidades *scaffolded*, los servicios de datos y los exportadores de reportes. Se registra todo
  con `AddInfrastructure(connectionString)`.
- **`Api`** — capa web: controllers delgados que delegan en los servicios, middleware de errores,
  configuración de Swagger/CORS/estáticos y el endpoint de salud.

### EF Core database-first (re-scaffolding)

El contexto y las entidades viven en `src/NorthwindApi.Infrastructure/Data/Generated`. Son
**generados** y **regenerables**: no deben editarse a mano. La fábrica de tiempo de diseño
`NorthwindContextFactory` resuelve la cadena desde `ConnectionStrings__Northwind`,
`NORTHWIND_CONNECTION` o LocalDB por defecto.

Para regenerarlos (por ejemplo tras cambiar la base):

```powershell
$env:ConnectionStrings__Northwind = "Server=(localdb)\MSSQLLocalDB;Database=Northwind;Trusted_Connection=True;TrustServerCertificate=True"
dotnet ef dbcontext scaffold `
  "Microsoft.EntityFrameworkCore.SqlServer" `
  --project src/NorthwindApi.Infrastructure `
  --context NorthwindContext `
  --context-dir Data/Generated `
  --output-dir Data/Generated `
  --use-database-names `
  --no-onconfiguring `
  --force
```

Ajustes aplicados **después** del scaffolding (documentados para repetirlos al regenerar):

- Las **vistas** se mapean como `HasNoKey().ToView(...)`.
- `CustomerCustomerDemo` y `EmployeeTerritories` se modelan como relaciones
  **muchos-a-muchos** (navegaciones de omisión), sin entidad intermedia.
- Tipos nativos preservados: `float`/`real` para `Discount`, `short`/`smallint` para cantidades.

### Patrón CRUD

Cada entidad concreta fija su clave, sus DTOs y su filtro fuertemente tipado extendiendo
`ICrudService<TKey, TDto, TCreate, TUpdate, TFilter>`. La implementación reutiliza
`CrudServiceBase<TEntity, …>`, que aporta el flujo genérico y delega en métodos abstractos del
servicio concreto:

| Método abstracto            | Responsabilidad                                     |
|-----------------------------|-----------------------------------------------------|
| `BuildQuery(filter)`        | consulta de lectura **sin seguimiento**, ya filtrada |
| `ApplySort(query, paging)`  | orden por **lista blanca** de expresiones            |
| `FindByIdAsync(id)`         | búsqueda por clave (conjunto **rastreado**)          |
| `Map(entity)`               | entidad → DTO                                        |
| `ToEntity(dto)` / `Apply(…)`| DTO → entidad (create/update)                        |
| `ValidateReferencesAsync(…)`| validación de FK antes de guardar                    |

Los servicios concretos se declaran en `Infrastructure/Services/EntityServices.cs` y se registran
en `DependencyInjection.RegisterCrudServices`.

### Vistas, procedimientos, dashboard y reportes

- **Vistas** (`ViewService`, 16 vistas): proyecciones de solo lectura con paginación y filtros
  propios. `GET /api/views` devuelve el catálogo.
- **Procedimientos** (`StoredProcedureService`, 7 procedimientos): invocados con
  `Database.SqlQuery<T>` y **FormattableString** (parámetros reales, nunca concatenación de SQL).
- **Dashboard** (`DashboardService`): agregados de ventas (`summary`, `sales-by-month`, `top-*`, …).
- **Reportes** (`ReportService` + exportadores): `PdfReportExporter` (PDFsharp/MigraDoc) y
  `XlsxReportExporter` (ClosedXML), unificados por el compuesto `IReportExporter` (`ReportExporter`).

### Manejo de errores y validación

- La validación por **DataAnnotations** se integra en `InvalidModelStateResponseFactory`
  (`Program.cs`) y devuelve `ValidationProblemDetails` (HTTP 400) de forma consistente.
- `ExceptionHandlingMiddleware` traduce excepciones de dominio y **errores de SQL Server** a
  códigos HTTP: 404 (`NotFoundException`), 409 (`ConflictException`, FK `547`, unicidad `2601/2627`)
  y 500 para el resto, siempre como `ProblemDetails`.

### Cómo agregar un nuevo recurso CRUD

1. **DTOs** en `Application/Dtos`: `XxxDto`, `XxxCreateDto`, `XxxUpdateDto`, `XxxFilter` (records con
   `DataAnnotations` en los parámetros de constructor).
2. **Interfaz** en `Application/Abstractions/ICrudService.cs`: `IXxxService : ICrudService<…>`.
3. **Servicio** en `Infrastructure/Services/EntityServices.cs`, derivando de `CrudServiceBase<…>`.
4. **Registro** en `DependencyInjection.RegisterCrudServices`.
5. **Controller** en `Api/Controllers`, derivando de `CrudControllerBase<…>`.

### Compilación y verificación

```powershell
dotnet build NorthwindApi.slnx     # 0 advertencias / 0 errores
dotnet test                        # suite unitaria + integración
```

---

## API

### Entidades (CRUD)

| Recurso            | Ruta base                         | Clave          |
|--------------------|-----------------------------------|----------------|
| Categorías         | `/api/categories`                 | `int`          |
| Transportistas     | `/api/shippers`                   | `int`          |
| Proveedores        | `/api/suppliers`                  | `int`          |
| Clientes           | `/api/customers`                  | `string(5)`    |
| Empleados          | `/api/employees`                  | `int`          |
| Productos          | `/api/products`                   | `int`          |
| Pedidos            | `/api/orders`                     | `int`          |
| Líneas de pedido   | `/api/orders/{orderId}/details`   | `(int,int)`    |
| Regiones           | `/api/regions`                    | `int`          |
| Territorios        | `/api/territories`                | `string`       |
| Demográficos       | `/api/customer-demographics`      | `string`       |

Colección con `?page=&pageSize=&sort=&order=` (paginación acotada a 100 por página) y filtros propios de cada recurso.

### Vistas (solo lectura) — `/api/views`

`GET /api/views` lista el catálogo. Cada vista tiene su ruta, p. ej.:

```
GET /api/views/invoices?page=1&pageSize=20&customerId=ALFKI
GET /api/views/sales-by-category
GET /api/views/order-details-extended?orderId=10248
```

### Procedimientos almacenados — `/api/stored-procedures`

```
GET /api/stored-procedures/ten-most-expensive-products
GET /api/stored-procedures/employee-sales-by-country?beginningDate=1997-01-01&endingDate=1997-12-31
GET /api/stored-procedures/sales-by-year?beginningDate=1997-01-01&endingDate=1997-12-31
GET /api/stored-procedures/customer-order-detail/10248
GET /api/stored-procedures/customer-orders/ALFKI
GET /api/stored-procedures/customer-order-history/ALFKI
GET /api/stored-procedures/sales-by-category?categoryName=Beverages&ordYear=1997
```

### Dashboard — `/api/dashboard`

```
summary, sales-by-month, sales-by-quarter, sales-by-year,
top-products, top-categories, top-customers, recent-orders, order-status
```

Filtros opcionales `from`/`to` (por `OrderDate`) y `top` donde aplique.

### Reportes (PDF / XLSX) — `/api/reports`

```
GET /api/reports/sales-by-category.pdf
GET /api/reports/sales-by-year.xlsx
GET /api/reports/top-products.pdf?top=15
GET /api/reports/top-customers.xlsx
GET /api/reports/invoices.pdf?customerId=ALFKI
```

---

## Pruebas

```powershell
dotnet test
```

- **Unitarias**: `PageQuery`, ordenamiento por lista blanca, servicios CRUD con EF Core **InMemory**.
- **Integración**: `WebApplicationFactory` contra la base de datos Northwind real (endpoints de lectura,
  procedimientos, exportaciones y mapeo de errores 400/404/409).

Las pruebas de integración usan la cadena de conexión `NORTHWIND_TEST_CONNECTION` si está definida;
si no, LocalDB `(localdb)\MSSQLLocalDB` con la base `Northwind`.

En GitLab CI, el job `test` levanta **SQL Server 2022** como *service* y define
`NORTHWIND_TEST_CONNECTION` apuntando a ese contenedor; no se usa LocalDB.

---

## Pipeline CI/CD (GitLab)

El pipeline [`.gitlab-ci.yml`](.gitlab-ci.yml) cubre la **integración continua** en cada
`push` a la rama principal, cada *merge request*, cada tag `v*` y las ejecuciones
manuales o programadas. El **despliegue** es un job **manual** que se lanza desde la
vista del pipeline (**Run** ▶ en el job `deploy`).

```
push / merge request / tag v*
        │
        ├──► build ──► test ──► package:api ───────┐
        │                                          ├──► deploy (manual, SSH)
        └──────────────► package:sqlserver ────────┘
```

| # | Stage | Job | Descripción |
|---|-------|-----|-------------|
| 1 | `build` | **build** | Restaura paquetes, compila la solución en `Release` y publica la API en `artifacts/publish`. |
| 2 | `test` | **test** | Levanta **SQL Server 2022** como *service* del runner, instala `sqlcmd`, carga Northwind con `scripts/init-northwind.sh` y ejecuta `dotnet test` con cobertura. |
| 3 | `package` | **package:api** | Construye la imagen de la API y la publica en el **GitLab Container Registry** (`$CI_REGISTRY_IMAGE`) con las etiquetas `sha-<8>`, `latest` y el tag `v*`. |
| 4 | `package` | **package:sqlserver** | Construye la imagen de SQL Server 2022 con `instnwnd.sql` y la publica en el mismo repositorio con los tags `<tag>-sqlserver` y `sqlserver-latest`. |
| 5 | `deploy` | **deploy** | Job **manual**: copia por SSH el compose, el script y `instnwnd.sql` al servidor, autentica en el registro, descarga las imágenes, inicializa Northwind y verifica `/health`. |

En los *merge requests* las imágenes **no se publican** y el despliegue no se ejecuta.
Los `push` a la rama principal y los tags publican imágenes, pero **no despliegan**:
el despliegue siempre requiere pulsar **Run** en el job `deploy`.

### Requisitos del runner

El pipeline asume un **GitLab Runner con executor `docker`** y **modo privilegiado**
(necesario para Docker-in-Docker en los jobs `package:*`). Registro de ejemplo:

```bash
gitlab-runner register \
  --url https://gitlab.com/ \
  --token <runner-token> \
  --executor docker \
  --docker-image docker:27-cli \
  --docker-privileged
```

Si el runner no puede usar Docker-in-Docker, sustituya los jobs `package:*` por
[Kaniko](https://docs.gitlab.com/ee/ci/docker/using_kaniko.html) o ejecútelos en un
runner con acceso al daemon de Docker.

El registro de contenedores del proyecto debe estar **habilitado**
(*Settings → General → Visibility, project features, permissions → Container Registry*).

### Variables de CI/CD

Configurar en **Settings → CI/CD → Variables**:

| Nombre | Tipo | Descripción |
|--------|------|-------------|
| `MSSQL_SA_PASSWORD` | Variable (masked) | Contraseña de `sa`; mínimo 12 caracteres, sin espacios, `;`, `$`, comillas ni barras. Se usa en el servicio de pruebas y en el despliegue. |
| `DEPLOY_HOST` | Variable | Host o IP del servidor de despliegue. |
| `DEPLOY_USER` | Variable | Usuario SSH con acceso a Docker. |
| `DEPLOY_SSH_PRIVATE_KEY` | Variable (masked) | Clave privada SSH sin *passphrase*. |
| `DEPLOY_SSH_KNOWN_HOSTS` | Variable (opcional) | Salida de `ssh-keyscan -H <host>`. Si falta, el job confía en la clave del host en el primer uso (TOFU). |
| `DEPLOY_PATH` | Variable (opcional) | Ruta de despliegue. Por defecto `/opt/northwind-api`. |
| `API_PORT` / `SQL_PORT` | Variable (opcional) | Puertos publicados. Por defecto `8080` / `1433`. |
| `DEPLOY_URL` | Variable (opcional) | URL pública de la API; se muestra como enlace del environment `production`. |
| `DEPLOY_REGISTRY_USER` | Variable (opcional) | Usuario alternativo del registro. Por defecto `$CI_REGISTRY_USER`. |
| `DEPLOY_REGISTRY_TOKEN` | Variable (opcional, masked) | Token alternativo del registro. Por defecto `$CI_REGISTRY_PASSWORD`. |

Las credenciales del registro (`$CI_REGISTRY`, `$CI_REGISTRY_IMAGE`, `$CI_REGISTRY_USER`,
`$CI_REGISTRY_PASSWORD`) las proporciona GitLab automáticamente; no hay que crearlas.
Para despliegues fuera del pipeline (por ejemplo un *deploy token* de larga duración)
use `DEPLOY_REGISTRY_USER` y `DEPLOY_REGISTRY_TOKEN`.

### Despliegue

El job `deploy` usa [`deploy/deploy.sh`](deploy/deploy.sh) y
[`deploy/docker-compose.prod.yml`](deploy/docker-compose.prod.yml):

1. Autentica en `$CI_REGISTRY` y descarga `$CI_REGISTRY_IMAGE:<tag>` y
   `$CI_REGISTRY_IMAGE:<tag>-sqlserver`.
2. Levanta SQL Server y espera a que su *healthcheck* esté en `healthy`.
3. Inicializa Northwind con `instnwnd.sql` sólo la primera vez (si `dbo.Customers`
   aún no existe).
4. Despliega la API, espera a `/health` y, si falla, **revierte** a la imagen anterior.

El servidor debe tener **Docker Engine + Docker Compose v2**, los puertos `API_PORT` y
`SQL_PORT` libres y acceso de lectura al registro del proyecto. La primera ejecución
descarga la imagen de SQL Server (~1,5 GB) y puede tardar varios minutos.

Para desplegar una versión concreta, ejecute el pipeline del commit o tag deseado:
la imagen usa la etiqueta inmutable `sha-<8>` (o el tag `v*`), lo que permite volver
atrás desplegando un pipeline anterior.

### Puesta en marcha desde cero

1. Suba el repositorio a GitLab y compruebe que existe un runner disponible
   (*Settings → CI/CD → Runners*).
2. Active el **Container Registry** del proyecto.
3. Cree las variables `MSSQL_SA_PASSWORD`, `DEPLOY_HOST`, `DEPLOY_USER` y
   `DEPLOY_SSH_PRIVATE_KEY` en *Settings → CI/CD → Variables*.
4. Haga `push` a la rama principal: se ejecutarán `build`, `test`, `package:api` y
   `package:sqlserver`, y se publicarán las imágenes.
5. Vaya a **Build → Pipelines**, abra el pipeline y pulse **Run** (▶) en el job `deploy`.
6. Verifique la API:

   ```bash
   curl -fsS http://<host>:<API_PORT>/health
   # {"status":"Healthy","database":"Northwind"}
   ```

### Actualizaciones y mantenimiento

- **API**: cada `push` a la rama principal publica `latest` y `sha-<8>`; el
  despliegue se lanza manualmente y descarga la etiqueta del commit.
- **SQL Server**: la imagen se reconstruye en cada pipeline y se publica como
  `<tag>-sqlserver`; el volumen persistente conserva los datos entre despliegues.
- **Contraseña de `sa`**: no cambiar `MSSQL_SA_PASSWORD` después del primer arranque
  sin cambiarla también dentro de SQL Server (`ALTER LOGIN sa WITH PASSWORD = ...`).
- **Datos**: el volumen `mssql-data` persiste entre despliegues. `docker compose down -v`
  o eliminar el volumen borra los datos; la siguiente ejecución vuelve a cargar
  `instnwnd.sql`.
- **Volver a una imagen anterior**: ejecute el pipeline de un commit/tag anterior y
  lance `deploy`; o ejecute `deploy.sh` a mano en el servidor con un archivo `.env`
  cuyo `API_IMAGE` apunte a la imagen anterior.

> El despliegue por SSH se usa porque GitLab CI no impone una plataforma de ejecución.
> Si prefiere Kubernetes, sustituya el job `deploy` por `kubectl`/Helm.

## Manejo de errores

Respuestas **ProblemDetails** (RFC 7807):

| Situación                                   | HTTP |
|---------------------------------------------|------|
| Validación (DataAnnotations / dominio)      | 400  |
| Recurso no encontrado                        | 404  |
| Clave foránea / unicidad / concurrencia     | 409  |
| Error no controlado                          | 500  |

---

## Despliegue con Docker

El despliegue contempla **dos contenedores Linux** orquestados con Docker Compose:

| Servicio   | Imagen                                          | Rol                                             |
|------------|-------------------------------------------------|-------------------------------------------------|
| `sqlserver`| `mcr.microsoft.com/mssql/server:2022-latest`     | SQL Server 2022 (edición Developer)              |
| `db-init`  | `mcr.microsoft.com/mssql/server:2022-latest`     | Crea `Northwind` y carga `instnwnd.sql` (una vez)|
| `api`      | build local (`Dockerfile`, Kestrel)              | API ASP.NET Core 10 en `:8080`                   |

> **Nota sobre IIS:** IIS requiere un contenedor **Windows** (`windowsservercore`), y una imagen
> oficial de SQL Server en contenedor es **Linux**. Un mismo host Docker no ejecuta contenedores
> Linux y Windows a la vez, por lo que este despliegue usa **Kestrel sobre Linux**. Para desplegar
> sobre IIS, publique la API en un host Windows con el *ASP.NET Core Hosting Bundle* y apunte la
> cadena de conexión a un SQL Server accesible por TCP.

### Requisitos

- Docker Engine / Docker Desktop con contenedores **Linux**.
- Puertos libres: **8080** (API) y **1433** (SQL Server).

### Puesta en marcha

```powershell
# (Opcional) fijar una contraseña de "sa" distinta a la de ejemplo:
Copy-Item .env.example .env      # y edite MSSQL_SA_PASSWORD

docker compose up --build -d
```

El arranque respeta el orden: `sqlserver` (healthcheck) → `db-init` (carga de datos) → `api`.

| Recurso     | URL                                     |
|-------------|-----------------------------------------|
| Dashboard   | `http://localhost:8080/`                |
| Swagger UI  | `http://localhost:8080/swagger`         |
| Salud       | `http://localhost:8080/health`          |

```
docker compose logs -f api        # seguir la API
docker compose down               # detener (conserva el volumen de datos)
docker compose down -v            # detener y borrar los datos
```

### Detalles de la configuración

- **`Dockerfile`** — build multi-stage (`sdk:10.0` → `aspnet:10.0`). En la imagen final se instalan
  las fuentes **Liberation** para la exportación a PDF (PDFsharp 6 no incluye fuentes): en Linux el
  `FileSystemFontResolver` localiza una fuente TrueType del sistema y la usa como sustituto
  métricamente compatible de Arial.
- **Cadena de conexión** — se inyecta por variable de entorno
  `ConnectionStrings__Northwind = Server=sqlserver,1433;Database=Northwind;User Id=sa;…`. El nombre
  `sqlserver` es el nombre del servicio en la red de Compose.
- **Datos** — se inicializan una sola vez con `instnwnd.sql` (montado como volumen en `db-init`) y
  persisten en el volumen `mssql-data`.
- **Entorno** — `docker-compose.yml` arranca la API en `ASPNETCORE_ENVIRONMENT=Development` para
  exponer Swagger UI y el panel. Cámbielo a `Production` para ocultarlos.
- **Reinicio de la carga de datos** — para volver a ejecutar `db-init`:
  `docker compose run --rm db-init` (tras `docker compose down`).

> Este `docker-compose.yml` es el de **desarrollo**: construye la imagen localmente
> (`--build`). El despliegue real lo hace el pipeline de GitLab, usando las imágenes
> publicadas en el **GitLab Container Registry**. `deploy/docker-compose.prod.yml` y
> `deploy/deploy.sh` son los archivos que usa el job `deploy`. Véase
> [Pipeline CI/CD (GitLab)](#pipeline-cicd-gitlab).

---

## Decisiones de diseño

- **Database-first**: el contexto y las entidades generadas viven en
  `src/NorthwindApi.Infrastructure/Data/Generated` (scaffolding con `--use-database-names`).
  Son regenerables y **no** deben editarse a mano.
- **Sin pagos en el esquema**: no existe ninguna tabla de pagos/cobros. Por eso **“ventas”** se define
  como la suma de importes de línea de `Order Details` (`UnitPrice * Quantity * (1 - Discount)`).
- **Estado del pedido derivado**: no hay columna de estado; se infiere de `ShippedDate`/`RequiredDate`
  (`Enviado` / `Vencido` / `Pendiente`).
- **Tablas puente colapsadas**: `CustomerCustomerDemo` y `EmployeeTerritories` se modelan como
  relaciones *muchos-a-muchos* (navegaciones de omisión), sin entidades propias.
- **Vistas de solo lectura**: mapeadas como `HasNoKey().ToView(...)`.
- **Tipos**: `Discount` es `float`/`real`; `Quantity`, `UnitsInStock`, etc. son `smallint` (`short`);
  `money` → `decimal`. Los DTO de resultados respetan `short` para `smallint`.
- **Validación**: con `DataAnnotations` en **parámetros de constructor de los records** (no en propiedades),
  lo que ASP.NET Core integra en ProblemDetails automáticamente.
- **Reportes**: **PDFsharp/MigraDoc** (MIT) para PDF y **ClosedXML** (MIT) para XLSX. Se descartó
  QuestPDF por su licencia comercial. Para PDFsharp 6, en Windows se habilita
  `GlobalFontSettings.UseWindowsFontsUnderWindows` y en Linux/contenedores se registra
  `FileSystemFontResolver` (lee una fuente TrueType del sistema; `fonts-liberation` en la imagen).
- **Consultas parametrizadas**: los procedimientos se invocan con `Database.SqlQuery<T>` y
  *FormattableString* (nunca concatenación de SQL).
- **Orden seguro**: el parámetro `sort` se resuelve contra una lista blanca de expresiones; nunca se
  construye SQL a partir de texto libre.

---

## Limitaciones conocidas

- Los reportes de “Facturas” limitan a 5000 filas por exportación.
- Las vistas keyless no admiten `ORDER BY` en el servidor sin una clave; se ordenan en memoria/por columna
  representativa.
- `Ten Most Expensive Products` usa `SET ROWCOUNT` (obsoleto) dentro del procedimiento original; se expone
  tal cual viene en `instnwnd.sql`.
