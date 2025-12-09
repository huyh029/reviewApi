#!/bin/bash
set -e

# Apply migrations to Postgres using the configured connection string
dotnet ef database update --no-build

exec dotnet reviewApi.dll
