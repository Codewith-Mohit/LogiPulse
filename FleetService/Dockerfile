FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SharedContracts/SharedContracts.csproj", "SharedContracts/"]
COPY ["FleetService/FleetService.csproj", "FleetService/"]

RUN dotnet restore "FleetService/FleetService.csproj"
COPY . .
WORKDIR "/src/FleetService"
RUN dotnet build "FleetService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FleetService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FleetService.dll"]