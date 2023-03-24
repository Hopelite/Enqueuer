FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /Enqueuer
EXPOSE 8443

ENV ASPNETCORE_URLS=http://+:8443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /Enqueuer

# Copy the source code
COPY ["src", "./src/"]

# Build and publish the project
WORKDIR /Enqueuer/src/Enqueuer.Telegram.API
# Restore project dependencies
RUN dotnet restore
# Build the project
RUN dotnet publish -c Release -o /Enqueuer/publish

# Launch the app
FROM base AS final
WORKDIR /Enqueuer
COPY --from=build /Enqueuer/publish ./
COPY --from=build /Enqueuer/src/Enqueuer.Telegram.API/enqueuer.db ./
ENTRYPOINT [ "dotnet", "Enqueuer.Telegram.API.dll" ]