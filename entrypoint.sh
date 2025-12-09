#!/bin/bash
set -e

# Apply migrations to PostgreSQL using source project (available at /app)
PROJECT_PATH="/app/reviewApi.csproj"
cd /app
dotnet ef database update --project "$PROJECT_PATH" --startup-project "$PROJECT_PATH" --no-build

# Run published app
cd /out
exec dotnet reviewApi.dll
