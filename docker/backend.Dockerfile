FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# 1. Copiar la solución (si usas el nuevo formato .slnx o .sln tradicional) desde la raíz
# Nota: Ajusta la ruta si tu .slnx está dentro de una carpeta /backend en la raíz.
COPY *.slnx ./ 

# 2. Copiar los archivos de proyecto manteniendo la estructura de carpetas exacta
COPY backend/src/QQuote.Insurance.Domain/QQuote.Insurance.Domain.csproj ./backend/src/QQuote.Insurance.Domain/
COPY backend/src/QQuote.Insurance.Application/QQuote.Insurance.Application.csproj ./backend/src/QQuote.Insurance.Application/
COPY backend/src/QQuote.Insurance.Infrastructure/QQuote.Insurance.Infrastructure.csproj ./backend/src/QQuote.Insurance.Infrastructure/
COPY backend/src/QQuote.Insurance.API/QQuote.Insurance.API.csproj ./backend/src/QQuote.Insurance.API/

# 3. Restaurar apuntando al proyecto API (esto restaurará automáticamente Domain, Application e Infrastructure)
RUN dotnet restore backend/src/QQuote.Insurance.API/QQuote.Insurance.API.csproj

# 4. Copiar TODO el código fuente ahora que el restore terminó
COPY . .

# 5. Publicar con el flag --no-restore apuntando a la ruta correcta
RUN dotnet publish backend/src/QQuote.Insurance.API/QQuote.Insurance.API.csproj -c Release -o /app/publish --no-restore

# Etapa de Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "QQuote.Insurance.API.dll"]