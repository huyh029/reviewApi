# Single-stage image (SDK) so we can run EF migrations at startup
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

# Install dotnet-ef tool for migrations (match SDK/runtime 9.x)
RUN dotnet tool install --global dotnet-ef --version 9.0.8
ENV PATH="$PATH:/root/.dotnet/tools"

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /out

WORKDIR /out

# Default connection string (override with env on deployment, e.g. DATABASE_URL)
ENV ConnectionStrings__DefaultConnection="Host=postgres;Port=5432;Database=reviewdb;Username=postgres;Password=postgres"
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY entrypoint.sh /out/entrypoint.sh
RUN chmod +x /out/entrypoint.sh

ENTRYPOINT ["/out/entrypoint.sh"]
