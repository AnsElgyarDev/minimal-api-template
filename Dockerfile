FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/MyCompany.MinimalApi/MyCompany.MinimalApi.csproj", "src/MyCompany.MinimalApi/"]
RUN dotnet restore "src/MyCompany.MinimalApi/MyCompany.MinimalApi.csproj"

COPY . .
WORKDIR "/src/src/MyCompany.MinimalApi"
RUN dotnet build "MyCompany.MinimalApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyCompany.MinimalApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyCompany.MinimalApi.dll"]