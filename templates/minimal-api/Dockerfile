FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, so this layer is cached until the .csproj changes
COPY ["src/MyCompany.MinimalApi/MyCompany.MinimalApi.csproj", "src/MyCompany.MinimalApi/"]
RUN dotnet restore "src/MyCompany.MinimalApi/MyCompany.MinimalApi.csproj"

COPY . .
RUN dotnet publish "src/MyCompany.MinimalApi/MyCompany.MinimalApi.csproj" \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Do not run as root (the user "app" ships with the aspnet image)
USER $APP_UID

ENTRYPOINT ["dotnet", "MyCompany.MinimalApi.dll"]