# Backend image for Railway. Build context = repo root (so /database is available):
#   docker build -f backend/Dockerfile -t smartmaint-api .
# Railway: Settings > Build > Dockerfile path = backend/Dockerfile, Root directory = /

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY backend/global.json backend/Directory.Build.props backend/Directory.Packages.props backend/SmartMaint.slnx backend/
COPY backend/src/SmartMaint.Api/SmartMaint.Api.csproj backend/src/SmartMaint.Api/
RUN dotnet restore backend/src/SmartMaint.Api/SmartMaint.Api.csproj
COPY backend/src backend/src
COPY database database
RUN dotnet publish backend/src/SmartMaint.Api/SmartMaint.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "SmartMaint.Api.dll"]
