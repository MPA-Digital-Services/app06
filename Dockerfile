# syntax=docker/dockerfile:1

# ---------- Etapa de compilación ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia de los archivos de proyecto para aprovechar la caché de restore.
COPY ["NorthwindApi.slnx", "./"]
COPY ["src/NorthwindApi.Api/NorthwindApi.Api.csproj", "src/NorthwindApi.Api/"]
COPY ["src/NorthwindApi.Application/NorthwindApi.Application.csproj", "src/NorthwindApi.Application/"]
COPY ["src/NorthwindApi.Infrastructure/NorthwindApi.Infrastructure.csproj", "src/NorthwindApi.Infrastructure/"]

RUN dotnet restore "src/NorthwindApi.Api/NorthwindApi.Api.csproj"

# Copia del resto del código y publicación.
COPY . .
RUN dotnet publish "src/NorthwindApi.Api/NorthwindApi.Api.csproj" \
        -c Release \
        -o /app/publish \
        /p:UseAppHost=false

# ---------- Etapa de ejecución ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Fuentes TrueType para la exportación a PDF (PDFsharp 6 no incluye fuentes).
# Liberation Sans es métricamente compatible con Arial.
RUN apt-get update \
    && apt-get install -y --no-install-recommends fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_EnableDiagnostics=0

EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NorthwindApi.Api.dll"]
