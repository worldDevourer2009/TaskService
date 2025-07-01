FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TaskHandler.Api/TaskHandler.Api.csproj", "TaskHandler.Api/"]
COPY ["TaskHandler.Application/TaskHandler.Application.csproj", "TaskHandler.Application/"]
COPY ["TaskHandler.Domain/TaskHandler.Domain.csproj", "TaskHandler.Domain/"]
COPY ["TaskHandler.Infrastructure/TaskHandler.Infrastructure.csproj", "TaskHandler.Infrastructure/"]
ARG NUGET_TOKEN
RUN dotnet nuget add source "https://nuget.pkg.github.com/worldDevourer2009/index.json" \
  --name github \
  --username worldDevourer2009 \
  --password $NUGET_TOKEN \
  --store-password-in-clear-text
RUN dotnet restore "TaskHandler.Api/TaskHandler.Api.csproj"
COPY . .
WORKDIR "/src/TaskHandler.Api"
RUN dotnet build "./TaskHandler.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TaskHandler.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskHandler.Api.dll"]
