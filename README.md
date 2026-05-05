# SGIP FinTech API

Backend del Sistema de Gestion de Inversiones y Prestamos. Expone una API REST para simular prestamos, crear solicitudes, aprobar o rechazar prestamos, consultar cronogramas de pago, procesar cuotas y registrar transacciones idempotentes.

## Links de despliegue

- Frontend URL: https://fintech-web-two.vercel.app
- Backend URL (Swagger): https://fintech-api-production-7a89.up.railway.app/swagger/index.html
- Credenciales de prueba: no aplica. El backend no implementa autenticacion; se usa `userId` directamente en las solicitudes.

## Tecnologias utilizadas

### Stack del proyecto

- ASP.NET Core Web API
- .NET 10
- Entity Framework Core
- PostgreSQL
- Swagger / OpenAPI
- xUnit

### Librerias principales

- `Microsoft.AspNetCore.OpenApi`: soporte OpenAPI en ASP.NET Core.
- `Microsoft.EntityFrameworkCore.Design`: tooling de migraciones y scaffolding.
- `Npgsql.EntityFrameworkCore.PostgreSQL`: proveedor PostgreSQL para Entity Framework Core.
- `Swashbuckle.AspNetCore`: generacion y visualizacion de Swagger UI.
- `xUnit`: pruebas unitarias del dominio y servicios.

### Decisiones tecnicas importantes

- Se eligio ASP.NET Core por su tipado fuerte, estructura clara para APIs y buena integracion con Entity Framework Core.
- Se uso PostgreSQL como base relacional por su robustez y compatibilidad con Railway.
- Las migraciones se aplican al iniciar la aplicacion para simplificar el despliegue.
- La tasa efectiva anual se recibe como porcentaje entero: `24` representa 24%, no `0.24`.
- Las reglas de negocio viven en servicios para evitar controladores con demasiada logica.

## Instalacion local

### Prerrequisitos

- .NET 10 SDK
- PostgreSQL 14+
- EF Core CLI

```bash
dotnet tool install --global dotnet-ef
```

> Nota: aunque el formato de referencia menciona .NET 8, este proyecto esta configurado con `net10.0` en `fintech-api.csproj`.

### Backend

```bash
cd fintech-api
dotnet restore
dotnet ef database update
dotnet run
```

Swagger local:

```text
http://localhost:5249/swagger
```

## Variables de entorno

### Backend (.env o appsettings.json)

La API requiere una conexion PostgreSQL mediante `DATABASE_URL`.

```env
DATABASE_URL=postgresql://user:password@localhost:5432/fintech
```

Ejemplo local:

```bash
cd fintech-api
export DATABASE_URL="postgresql://postgres:postgres@localhost:5432/fintech"
dotnet run
```

## Testing

```bash
cd fintech-api
dotnet test FinTech.Tests/FinTech.Tests.csproj
```

No se implementaron integration tests ni unit test en el backend por falta de tiempo.

## Arquitectura

Estructura principal:

```text
fintech-api/
├── Controllers/       # Endpoints REST
├── DTOs/              # Contratos de entrada y salida
├── Models/            # Entidades, enums y estados
├── Services/          # Reglas de negocio y casos de uso
├── Repositories/      # Abstraccion de persistencia
├── Data/              # DbContext, migraciones y configuracion
├── Utils/             # Calculadora financiera
└── FinTech.Tests/     # Pruebas unitarias
```

Patrones implementados:

- Repository Pattern: separa acceso a datos de reglas de negocio. Implementado en `Repositories/Interfaces/ILoanRepository.cs`, `Repositories/Interfaces/ITransactionRepository.cs`, `Repositories/Implementations/LoanRepository.cs` y `Repositories/Implementations/TransactionRepository.cs`.
- Service Layer: concentra validaciones y flujos de aplicacion. Implementado en `Services/LoanService.cs`, `Services/TransactionService.cs` y `Services/PaymentService.cs`, con contratos en `Services/Interfaces/`.
- Strategy simplificado: `FinancialCalculator` soporta cuota fija y cuota decreciente seleccionando el calculo segun `LoanType`. Implementado en `Utils/FinancialCalculator.cs`.
- DTO Pattern: evita exponer directamente entidades internas en la API. Implementado en `DTOs/LoanDtos.cs` y `DTOs/TransactionDtos.cs`, y usado desde `Controllers/LoansController.cs` y `Controllers/TransactionsController.cs`.

## Decisiones de diseno

- Se uso Entity Framework Core para reducir codigo repetitivo de persistencia y manejar migraciones.
- Se mantuvieron controladores delgados para que la logica sea mas facil de probar.
- Se dejo la aprobacion automatica como regla de dominio dentro de `LoanService`.
- Se simplifico la autenticacion porque el foco de la prueba era el flujo financiero y no gestion de usuarios.

Trade-offs realizados:

- No se implemento autenticacion ni autorizacion.
- No se implementaron pruebas de integracion ni unit test por falta de tiempo.
- La configuracion financiera esta en codigo y no en tablas parametrizables.

## Supuestos y limitaciones

- El cliente se identifica por `userId`.
- La TEA permitida esta entre 18% y 35%.
- El monto permitido esta entre 500 y 50000.
- El plazo permitido esta entre 6 y 60 meses.
- Un cliente no puede superar 3 prestamos activos.
- La suma de cuotas activas no puede superar el 40% del ingreso mensual.
- Las transacciones usan `idempotencyKey` para evitar duplicados.
- No hay integration tests en backend por falta de tiempo.

Mejoras futuras:

- Agregar autenticacion con roles.
- Implementar integration tests contra PostgreSQL.
- Crear un `PaymentRepository` o `LoanPaymentRepository` para separar la persistencia de pagos del `PaymentService` y mantener consistencia con los otros servicios.
- Agregar pruebas E2E del flujo completo.
- Parametrizar reglas financieras desde base de datos.
- Agregar observabilidad con logs estructurados y metricas.
- Soportar pagos parciales, mora, seguros y comisiones.
