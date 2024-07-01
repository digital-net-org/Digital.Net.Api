FROM mcr.microsoft.com/dotnet/sdk:8.0 AS safaridigital-api-build
WORKDIR /App
COPY . ./

# Build solution
RUN dotnet restore
RUN dotnet publish SafariDigital.Api/SafariDigital.Api.csproj -c Release -o release

# Run solution
FROM mcr.microsoft.com/dotnet/aspnet:8.0 as safaridigital-api-runtime
WORKDIR /App
COPY --from=build-env /App/release .
ENTRYPOINT ["dotnet", "SafariDigital.Api.dll"]