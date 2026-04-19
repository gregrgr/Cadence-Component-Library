FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/CadenceComponentLibraryAdmin.Domain/CadenceComponentLibraryAdmin.Domain.csproj src/CadenceComponentLibraryAdmin.Domain/
COPY src/CadenceComponentLibraryAdmin.Application/CadenceComponentLibraryAdmin.Application.csproj src/CadenceComponentLibraryAdmin.Application/
COPY src/CadenceComponentLibraryAdmin.Infrastructure/CadenceComponentLibraryAdmin.Infrastructure.csproj src/CadenceComponentLibraryAdmin.Infrastructure/
COPY src/CadenceComponentLibraryAdmin.Web/CadenceComponentLibraryAdmin.Web.csproj src/CadenceComponentLibraryAdmin.Web/

RUN dotnet restore src/CadenceComponentLibraryAdmin.Web/CadenceComponentLibraryAdmin.Web.csproj

COPY src/ src/

RUN dotnet publish src/CadenceComponentLibraryAdmin.Web/CadenceComponentLibraryAdmin.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "CadenceComponentLibraryAdmin.Web.dll"]

