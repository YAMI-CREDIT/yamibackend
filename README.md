# yamibackend
Repository to host yami backend.


## Development setup

### The EF Core CLI Tool
`dotnet-ef` is a command line tool that runs on your machine to generate migration files. Install it using the command below.

```bash
dotnet tool install --global dotnet-ef
```

### Clean and Restore
The commands below help you start your build on a clean slate.

```bash
dotnet clean
dotnet nuget locals all --clear
dotnet restore
```

### Setup Object Relational Mapping (ORM)

`dotnet ef migrations add` reads your DbContext/model classes and generates a C# file describing "here's the SQL needed to create this schema." This has to happen every time you change your models (e.g., add a new property later).

`dotnet ef database update` runs the generated migration against your DB, physically creating the Users table.

> [!NOTE]
> Before you use `dotnet ef` to set up your database, make sure it is running by following the instructions in [Run a local PostgreSQL DB](#run-a-local-postgresql-db)

```bash
dotnet ef migrations add InitialCreate \
  --project src/Yami.Features/Users/Users.csproj \
  --startup-project src/Yami.Api/Yami.Api.csproj \
  --context UsersDbContext \
  --output-dir EFInfrastructure/Data/Migrations

export ConnectionStrings__DefaultConnection='Host=localhost;Port=5432;Database=yami;Username=yami;Password=yami'
dotnet ef database update \
  --project src/Yami.Features/Users/Users.csproj \
  --startup-project src/Yami.Api/Yami.Api.csproj \
  --context UsersDbContext
```

If You've created the database before and you need to start on a clean DB slate, run:

```bash
dotnet ef database drop \
  --project src/Yami.Features/Users/Users.csproj \
  --startup-project src/Yami.Api/Yami.Api.csproj \
  --context UsersDbContext \
  --force
  ```

Delete the src/Yami.Features/Users/EFInfrastructure folder and then run the `dotnet ef migrations` and `dotnet ef database update` above again:

### Run the app 
```bash
export ConnectionStrings__DefaultConnection='Host=localhost;Port=5432;Database=yami;Username=yami;Password=yami'
dotnet run \
  --project src/Yami.Api/Yami.Api.csproj \
  --environment Development
```

## Test
For example to test the users endpoint I created in src/Yami.Features/Users/Users.Api/Controllers/RegisterUserController.cs
```bash
curl -v -X POST http://localhost:5000/api/v1/users \
  -H "Content-Type: application/json" \
  -d '{"phone": "+2348012345678", "name": "Akin Benson", "userType": "wholesaler", "dateOfBirth": "13-03-1974"}'
```


## Run a local PostgreSQL DB
``` bash
docker run --name yami-postgres \
  -e POSTGRES_USER=yami \
  -e POSTGRES_PASSWORD=yami \
  -e POSTGRES_DB=yami \
  -p 5432:5432 \
  -v yami-postgres-data:/var/lib/postgresql/data \
  -d postgres:16
```

start, stop or remove the DB container
```bash
docker stop yami-postgres
docker start yami-postgres
docker rm -f yami-postgres
```

To view data via a GUI; 
- [Download and install pgadmin](https://www.pgadmin.org/download/)
- Open pgadmin click on `Add New Server` (picture reference below)
- Connect with:

```bash
Name: Yami
Host: localhost
Port: 5432
Database: yami
Username: yami
Password: yami
```
<br>
<br>

![pgadmin home page](./assets/pgadmin_view.png)


> [!NOTE]
> Please ignore the ./docker/Dokerfile. It's just there as a placeholder for now