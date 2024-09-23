FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App
COPY . ./
RUN dotnet restore
RUN dotnet publish SafariDigital.Api/SafariDigital.Api.csproj -c Release -o release

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime-env
WORKDIR /App
COPY --from=build-env /App/release .
ENTRYPOINT ["dotnet", "SafariDigital.Api.dll"]