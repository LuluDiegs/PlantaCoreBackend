FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["PlantaCoreAPI.API/PlantaCoreAPI.API.csproj", "PlantaCoreAPI.API/"]
COPY ["PlantaCoreAPI.Application/PlantaCoreAPI.Application.csproj", "PlantaCoreAPI.Application/"]
COPY ["PlantaCoreAPI.Domain/PlantaCoreAPI.Domain.csproj", "PlantaCoreAPI.Domain/"]
COPY ["PlantaCoreAPI.Infrastructure/PlantaCoreAPI.Infrastructure.csproj", "PlantaCoreAPI.Infrastructure/"]
RUN dotnet restore "PlantaCoreAPI.API/PlantaCoreAPI.API.csproj"

COPY . .
RUN dotnet publish "PlantaCoreAPI.API/PlantaCoreAPI.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "PlantaCoreAPI.API.dll"]
