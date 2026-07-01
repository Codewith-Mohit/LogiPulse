FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SharedContracts/SharedContracts.csproj", "SharedContracts/"]
COPY ["OrderAPIs/OrderAPIs.csproj", "OrderAPIs/"]

RUN dotnet restore "OrderAPIs/OrderAPIs.csproj"
COPY . .
WORKDIR "/src/OrderAPIs"
RUN dotnet build "OrderAPIs.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderAPIs.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderAPIs.dll"]