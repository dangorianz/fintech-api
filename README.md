# SGIP FinTech API

Backend para el Sistema de Gestion de Inversiones y Prestamos. Implementa simulacion de prestamos, solicitud/aprobacion, cronogramas de pago y transacciones idempotentes.

## Stack

- .NET 10 Minimal API
- Entity Framework Core 10
- PostgreSQL con Npgsql
- Swagger UI con Swashbuckle
- xUnit para tests

## Arquitectura

La solucion esta separada en capas simples:

- `Models`: entidades, enums y estados del dominio.
- `DTOs`: contratos de entrada/salida de la API.
- `Utils`: calculadora financiera para TEA, TEM, sistema frances y aleman.
- `Services`: reglas de negocio y orquestacion.
- `Repositories`: interfaces e implementaciones EF Core e in-memory.
- `Data`: `ApplicationDbContext`, seed data, migracion inicial y helper de `DATABASE_URL`.

Patrones usados:

- Repository Pattern para abstraer persistencia.
- Strategy simplificado en `FinancialCalculator` para soportar cuota fija y decreciente.

## Reglas Cubiertas

- Monto entre 500 y 50000.
- Plazo entre 6 y 60 meses.
- TEA entre 18% y 35%, con 24% por defecto.
- TEM calculada como `(1 + TEA)^(1/12) - 1`.
- Cronograma mensual con ajuste de dia 31 a ultimo dia del mes.
- Maximo 3 prestamos abiertos por cliente.
- Suma de cuotas abiertas no mayor al 40% de ingreso mensual.
- Aprobacion automatica para montos menores a 10000 y menos de 2 prestamos abiertos.
- Idempotencia por `idempotencyKey` en transacciones.

## Endpoints

- `POST /api/loans/simulate`
- `POST /api/loans`
- `GET /api/loans?userId=user-123`
- `GET /api/loans/{id}`
- `GET /api/loans/{id}/schedule`
- `PATCH /api/loans/{id}/approve`
- `PATCH /api/loans/{id}/reject`
- `POST /api/transactions`
- `GET /api/transactions?type=Payment&status=Completed`
- `GET /api/transactions/{id}`

## Ejecucion Local

Sin `DATABASE_URL`, la API usa repositorios en memoria con seed data para probar rapido.

```bash
dotnet restore
dotnet run
```

Swagger queda disponible en:

```text
http://localhost:5249/swagger
```

Usuarios seed:

- `user-123`, ingreso mensual `5000`
- `user-456`, ingreso mensual `3200`

## PostgreSQL

Para usar base real:

```bash
export DATABASE_URL="postgresql://user:password@host:5432/database"
dotnet run
```

Al iniciar, la API aplica migraciones automaticamente y carga seed data si la base esta vacia.

## Tests

```bash
dotnet test FinTech.Tests/FinTech.Tests.csproj
```

Incluye pruebas de calculo frances, cronograma, validaciones de monto/plazo, ratio de deuda e idempotencia.

## Supuestos

- No hay autenticacion; se usa `userId` directo como permitia el documento.
- La aprobacion automatica deja el prestamo en `Approved` y genera desembolso `Completed`.
- El repositorio in-memory es solo para desarrollo rapido; produccion debe usar PostgreSQL.
