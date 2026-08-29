FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /source

COPY . .

RUN dotnet restore "src/Hosts/YaeaY.Account.EventProcessing.Worker/YaeaY.Account.EventProcessing.Worker.csproj" \
    && dotnet publish "src/Hosts/YaeaY.Account.EventProcessing.Worker/YaeaY.Account.EventProcessing.Worker.csproj" \
       --configuration Release \
       --output /app/publish \
       -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "YaeaY.Account.EventProcessing.Worker.dll"]

