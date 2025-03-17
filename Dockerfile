FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /App
COPY . ./
RUN dotnet restore
RUN dotnet publish Digital.Core.Api/Digital.Core.Api.csproj -c Release -o release

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime-env
WORKDIR /App
COPY --from=build-env /App/release .

ARG GIT_VERSION
ARG GIT_COMMIT_SHA
ARG GIT_RELEASE

ENV git_version=${GIT_VERSION}
ENV git_commit_sha=${GIT_COMMIT_SHA}
ENV git_release=${GIT_RELEASE}

EXPOSE 8080
EXPOSE 80

ENTRYPOINT ["dotnet", "Digital.Core.Api.dll"]