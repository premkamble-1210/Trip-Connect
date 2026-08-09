FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY API_LAYER/API_LAYER.csproj API_LAYER/
COPY APPLICATION_LAYER/APPLICATION_LAYER.csproj APPLICATION_LAYER/
COPY DOMAIN_LAYER/DOMAIN_LAYER.csproj DOMAIN_LAYER/
COPY INFRASTRUCTURE_LAYER/INFRASTRUCTURE_LAYER.csproj INFRASTRUCTURE_LAYER/

RUN dotnet restore API_LAYER/API_LAYER.csproj

COPY . .
RUN dotnet publish API_LAYER/API_LAYER.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "API_LAYER.dll"]