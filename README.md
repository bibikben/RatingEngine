# BTSS.Rating (.NET 10)

This solution is a starter scaffold under the master namespace **BTSS.Rating**:
- **BTSS.Rating.Api** - Rating API with Swagger
- **BTSS.Rating.Admin** - Radzen Blazor Server Admin portal (starter pages)
- **BTSS.Rating.Application/Domain/Infrastructure** - Clean Architecture layers
- **BTSS.Rating.Shared** - Shared DTOs/enums

## Database scripts
The project assumes you have scripts like:
- RatingDbCreate.sql
- DefaultValueInsert.sql

Provision **RatingDb** and update connection strings in:
- `BTSS.Rating.Api/appsettings.json`
- `BTSS.Rating.Admin/appsettings.json`

## Build/run
From the repo root:

```bash
dotnet restore
dotnet build

# Run API
dotnet run --project BTSS.Rating.Api

# Run Admin
dotnet run --project BTSS.Rating.Admin
```

## Next steps
1. Add EF Core mappings for your RatingDb schema
2. Implement contract lookup + lane matching in `RatingService`
3. Build Radzen CRUD pages for contracts and rate tables
4. Add reporting queries (application query services + UI)
