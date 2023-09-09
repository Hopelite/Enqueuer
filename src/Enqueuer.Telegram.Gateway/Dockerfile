FROM mcr.microsoft.com/azure-functions/dotnet:4 AS base
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy the source code
COPY ["src/Enqueuer.Telegram.Gateway/Enqueuer.Telegram.Gateway.csproj", "Enqueuer.Telegram.Gateway/" ]

# Restore project dependencies
RUN dotnet restore "Enqueuer.Telegram.Gateway/Enqueuer.Telegram.Gateway.csproj"

# Copy only the source code of a project
COPY ["src/Enqueuer.Telegram.Gateway/", "."]

# Build the project to validate whether it has compile errors
WORKDIR "/src/Enqueuer.Telegram.Gateway"
RUN dotnet build "Enqueuer.Telegram.Gateway.csproj" -c Release -o /app/build

# Build the final Azure Functions app
FROM build AS publish
RUN dotnet publish "Enqueuer.Telegram.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Save only the created app
FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true