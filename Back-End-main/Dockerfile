FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Server/src/GP_Server.Api/GP_Server.Api.csproj", "Server/src/GP_Server.Api/"]
COPY ["Server/src/GP_Server.Application/GP_Server.Application.csproj", "Server/src/GP_Server.Application/"]
COPY ["Server/src/GP_Server.Domain/GP_Server.Domain.csproj", "Server/src/GP_Server.Domain/"]
COPY ["Server/src/GP_Server.Infrastructure/GP_Server.Infrastructure.csproj", "Server/src/GP_Server.Infrastructure/"]
RUN dotnet restore "Server/src/GP_Server.Api/GP_Server.Api.csproj"

COPY . .
WORKDIR "/src/Server/src/GP_Server.Api"
RUN dotnet build "GP_Server.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GP_Server.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install PostgreSQL client for health check
RUN apt-get update && apt-get install -y postgresql-client

# Copy wait-for script
COPY wait-for-db.sh /wait-for-db.sh
RUN chmod +x /wait-for-db.sh

# Create directory for storing uploaded files
RUN mkdir -p /app/wwwroot/uploads/profiles

# Use the script to wait for PostgreSQL before starting the app
CMD ["/wait-for-db.sh", "postgres", "dotnet", "GP_Server.Api.dll"]
