FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /source

COPY . .

RUN dotnet restore "src/YaeaY.Account.Presentation/YaeaY.Account.Presentation.Server/YaeaY.Account.Presentation.Server.csproj" \
    && dotnet publish "src/YaeaY.Account.Presentation/YaeaY.Account.Presentation.Server/YaeaY.Account.Presentation.Server.csproj" \
       --configuration Release \
       --output /app/publish \
       -p:UseAppHost=false \
       -p:SkipClientProjectReference=true

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

COPY --from=build /app/publish .

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=3 \
  CMD curl --fail --silent http://127.0.0.1:8080/health || exit 1

ENTRYPOINT ["dotnet", "YaeaY.Account.Presentation.Server.dll"]

