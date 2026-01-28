# ================================
# Stage 1: Build
# ================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto para restaurar dependencias (layer caching)
COPY ["src/OrderIntegration.Api/OrderIntegration.Api.csproj", "src/OrderIntegration.Api/"]
RUN dotnet restore "src/OrderIntegration.Api/OrderIntegration.Api.csproj"

# Copiar el resto del código fuente
COPY . .

# Compilar y publicar
WORKDIR "/src/src/OrderIntegration.Api"
RUN dotnet publish "OrderIntegration.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ================================
# Stage 2: Runtime
# ================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Instalar wget para healthcheck y crear usuario no-root
RUN apt-get update && apt-get install -y --no-install-recommends wget \
    && rm -rf /var/lib/apt/lists/* \
    && adduser --disabled-password --gecos '' appuser

# Copiar artifacts de build
COPY --from=build /app/publish .

# Configurar variables de entorno por defecto
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Exponer puerto
EXPOSE 8080

# Cambiar a usuario no-root
USER appuser

# Punto de entrada
# Nota: Health check se configura en docker-compose.yml
ENTRYPOINT ["dotnet", "OrderIntegration.Api.dll"]
