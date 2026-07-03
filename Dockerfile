# build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore NextAtlet.Api/NextAtlet.Api.csproj
RUN dotnet publish NextAtlet.Api/NextAtlet.Api.csproj -c Release -o /app

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app .

ENTRYPOINT ["dotnet", "NextAtlet.Api.dll"]
