FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/QQuote.Insurance.Domain/QQuote.Insurance.Domain.csproj src/QQuote.Insurance.Domain/
COPY src/QQuote.Insurance.Application/QQuote.Insurance.Application.csproj src/QQuote.Insurance.Application/
COPY src/QQuote.Insurance.Infrastructure/QQuote.Insurance.Infrastructure.csproj src/QQuote.Insurance.Infrastructure/
COPY src/QQuote.Insurance.API/QQuote.Insurance.API.csproj src/QQuote.Insurance.API/
RUN dotnet restore src/QQuote.Insurance.API/QQuote.Insurance.API.csproj

COPY . .
RUN dotnet publish src/QQuote.Insurance.API/QQuote.Insurance.API.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "QQuote.Insurance.API.dll"]
