FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SharedContracts/SharedContracts.csproj", "SharedContracts/"]
COPY ["NotifierService/NotifierService.csproj", "NotifierService/"]

RUN dotnet restore "NotifierService/NotifierService.csproj"
COPY . .
WORKDIR "/src/NotifierService"
RUN dotnet build "NotifierService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotifierService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotifierService.dll"]