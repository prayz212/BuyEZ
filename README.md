# Backend

## Database

To run PostgreSQL database in Docker:

```bash
docker run --name postgres_study -p 5432:5432 -e POSTGRES_USER=your-pg-user -e POSTGRES_PASSWORD=your-secure-pw -d postgres
```

To add a new migration from the root folder:

```bash
dotnet ef migrations add MigrationName --project .\Application --startup-project .\Apis --output-dir Infrastructure\Migrations
```

To remove a migration from the root folder:

```bash
dotnet ef migrations remove --project .\Application --startup-project .\Apis
```

To update database from the root folder:

```bash
dotnet ef database update MigrationName --project .\Application --startup-project .\Apis
```


## API Gateway 

Route mapping rules: 
http://localhost:5000/{version}/api/{service-name}/... => http://localhost:{service-local-port}/{version}/api/{service-local-name}/...

Example 1: http://localhost:5000/v1/api/catalog/query => http://localhost:5205/v1/api/products-shoppings/query
Example 2: http://localhost:5000/v1/api/admin/catalog => http://localhost:5205/v1/api/products-administrations