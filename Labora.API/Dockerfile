 
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Labora.API/Labora.API.csproj", "Labora.API/"]
COPY ["Labora.Application/Labora.Application.csproj", "Labora.Application/"]
COPY ["Labora.Domain/Labora.Domain.csproj", "Labora.Domain/"]
COPY ["Labora.Infrastructure/Labora.Infrastructure.csproj", "Labora.Infrastructure/"]
RUN dotnet restore "Labora.API/Labora.API.csproj"
COPY . .
WORKDIR "/src/Labora.API"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Labora.API.dll"]