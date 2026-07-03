FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy everything in /server so relative ProjectReferences resolve
COPY . .

# Restore + publish the SPECIFIC entry project, not the whole solution
RUN dotnet restore ./NextAtlet.Api/NextAtlet.Api.csproj
RUN dotnet publish ./NextAtlet.Api/NextAtlet.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Railway injects $PORT — bind Kestrel to it
ENV ASPNETCORE_URLS=http://+:${PORT}
ENTRYPOINT ["dotnet", "NextAtlet.Api.dll"]
