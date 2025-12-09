#!/bin/bash
set -e

# Apply migrations to PostgreSQL using source project (available at /app)
cd /app
dotnet ef database update --project reviewApi.csproj --startup-project reviewApi.csproj --no-build

# Run published app
cd /out
exec dotnet reviewApi.dll
