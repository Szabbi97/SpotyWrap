FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy csproj and restore dependencies
COPY SpotyWrap/*.csproj ./SpotyWrap/
RUN dotnet restore ./SpotyWrap/SpotyWrap.csproj

# Copy everything else and build
COPY SpotyWrap/. ./SpotyWrap/
WORKDIR /source/SpotyWrap
RUN dotnet publish -c Release -o /app

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# Create a non-root user
RUN useradd -m myappuser
USER myappuser

EXPOSE 8080
ENTRYPOINT ["dotnet", "SpotyWrap.dll"]
