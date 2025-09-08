# 1. Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copiamos el archivo de solución y los proyectos (para cachear dependencias)
COPY ApiFinanzas.sln ./
COPY src/ ./src/

# Restauramos dependencias y compilamos en Release
RUN dotnet restore
RUN dotnet publish src/ApiFinanzas.WebApi/ApiFinanzas.WebApi.csproj -c Release -o deploy

# 2. Etapa runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/deploy ./

# Render expone el puerto por default en $PORT
ENV ASPNETCORE_URLS=http://+:$PORT
ENTRYPOINT ["dotnet", "ApiFinanzas.WebApi.dll"]
