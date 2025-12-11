# Backend developer notes

## Entity Framework migrations

The backend project file lives in `backend/backend.csproj`, so `dotnet ef` commands must point at that project (or be run from inside the `backend/` subfolder). Examples:

```bash
# From the repo root
cd backend

dotnet ef database update --project backend/backend.csproj --startup-project backend/backend.csproj
```

```bash
# Alternatively, from inside the project directory
cd backend/backend
dotnet ef database update
```

If the CLI reports "No project was found", ensure you are in the `backend/backend` directory or pass the `--project` (and `--startup-project` if needed) options as shown above.

### Tooling version

A local tool manifest is checked in under `.config/dotnet-tools.json` and pins `dotnet-ef` to the `8.0.8` toolchain. Before running migration commands, restore the tool to avoid runtime version mismatches such as `System.Runtime, Version=10.0.0.0`:

```bash
cd backend
dotnet tool restore
```

After restoration, use `dotnet ef` from the same directory (or pass the `--tool-path` if you restore elsewhere) so the bundled .NET 8 runtime is used.

### SQLite file location

The connection string uses `Data Source=cryptoagent.db`, and at runtime the path is resolved relative to the project content root (the `backend/` folder). This ensures the API and `dotnet ef` commands share the same SQLite file instead of creating separate copies in `bin/Debug` or other working directories.
