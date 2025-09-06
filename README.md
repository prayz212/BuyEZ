# Backend

## Identity Provider (IdP)

1. Id Token

2. Access Token

### Resource Owner Password Grant Type

### Authorization Code Grant Type

### Scope

### Policy

## gRPC communication

## Architecture (Slice architecture)

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

### Cross-Service Relationships

1. **Denormalization**

   > **Idea**: Copy relevant data (e.g., ProductId, ProductName, Price) into the entity/table at the creation time. Creation logic (e.g AddOrder) should make call(s) to referenced services to fetch the data.

2. **Event-Driven Sync**

   > **Idea**: Use an Event Bus (e.g., RabbitMQ, Kafka) to synchronize relevant data into the database.

   > **Steps**: Relevant data changed (e.g Product detail) => Publish an event to the event bus (e.g Product service) => Subscribers (e.g Order service) listen to these events and perform updates accordingly.

3. **Combine two of them with Redis cache**
   > **Idea**: Use Redis or in-memory caching for relevant data frequently used by service.

## API Gateway

### Route Mapping Rules:

1. **For shopping application:**

   > **Request to:** `http://localhost:5000/{version}/api/{service-name}/...`
   >
   > **Forward to:** `http://localhost:{service-local-port}/{version}/api/{service-local-controller}/...`

   _Example:_
   `http://localhost:5000/v1/api/catalog/query`

   => `http://localhost:5205/v1/api/products-shoppings/query`

2. **For administration application:**

   > **Request to:** `http://localhost:5000/{version}/api/admin/{service-name}/...`
   >
   > **Forward to:** `http://localhost:{service-local-port}/{version}/api/{service-local-controller}/...`

   _Example:_
   `http://localhost:5000/v1/api/admin/catalog`

   => `http://localhost:5205/v1/api/products-administrations`

# Docker

## Dockerfile

To build a Dockerfile manually, standing at folder BuyEZ/src then run the following command:

```bash
docker build -t <service-name>:<version> -f Dockerfile ../../
```

> Use environment variables in **env.txt** to configure when running docker container

## Docker Compose

To build and run docker containers automatically, standing at folder BuyEZ then run the following command:

```bash
docker-compose up -d --build
```

To stop/start/restart those running containers, run the following command:

```bash
docker-compose stop/start/restart
```

To stop and remove those running containers, run the following command:

```bash
docker-compose down
```

> Add **--rmi all** if you want to delete related images as well

# GitHub

## Commit rules

Should maintain clean, understandable and easily navigate project history by following the Git commit message pattern as table below:

| Type     | Description                                   |
| -------- | --------------------------------------------- |
| feat     | Introduce a new feature.                      |
| fix      | Bug fix or error correction.                  |
| docs     | Documentation updates only.                   |
| style    | Changes in code style (e.g., formatting).     |
| refactor | Code refactoring without behavior changes.    |
| test     | Adding or updating tests.                     |
| chore    | Maintenance tasks (e.g., dependency updates). |
| perf     | Improve performance.                          |
| ci       | Changes to CI/CD configuration.               |
| build    | Changes to build system.                      |

# How to set up Kafka infrastructure

## Step 1: Build and run Docker compose

To download and run the Kafka components, open terminal and `cd` to **${HOME}/BuyEZ/infra/kafka** then use the below command and wait until all containers are running

```bash
docker-compose up -d --build
```

## Step 2: Precreate data (1st time only)

To precreate data, you need to access the Apache Kafka UI via this address `http://localhost:8080/`, then navigate to local > Topics and add the following topics:
1. **order-created**
2. **order-packing-started**
3. **order-packed**
4. **delivery-started**
5. **delivery-succeeded**

> **Note**: Just entering `Topic Name` and set `Number of Partitions` to `1`, the remaining stay as default.

# How to run the BuyEZ application

## Step 1: Build and run Docker compose

To dockerize and run the application, open terminal and `cd` to **${HOME}/BuyEZ** then use the below command and wait until all containers are running

```bash
docker-compose up -d --build
```

## Step 2: Seed data (1st time only)

To seeding dummy data, you need to stop all services in buyez-api container group, excluding buyez-db container. Then, in `appsettings.Development.json` file, change database port at `ConnectionStrings:DefaultConnection` to buyez-db's port (which is **15432**) and manually run these 4 projects: Catalog.API, ClientManagement.API, Identity.WebApp, Identity.API.
