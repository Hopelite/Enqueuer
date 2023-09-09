FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
EXPOSE 8443

ENV ASPNETCORE_URLS=http://+:8443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Copy the source code
COPY ["src", "src/"]

# Build and publish the project
WORKDIR /src/Enqueuer.Telegram.API

# Restore project dependencies
RUN dotnet restore

# Build the project to validate whether it has compile errors
RUN dotnet build "Enqueuer.Telegram.API.csproj" -c Release -o /app/build

# Build the final app
FROM build AS publish
RUN dotnet publish "Enqueuer.Telegram.API.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Launch the app
FROM base AS final
WORKDIR /app/publish
COPY --from=publish /app/publish .
ENTRYPOINT [ "dotnet", "Enqueuer.Telegram.API.dll" ]