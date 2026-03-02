FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Restore dependencies first (layer caching)
COPY *.csproj ./
RUN dotnet restore

# Build and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5014
EXPOSE 5014

ENTRYPOINT ["dotnet", "ElevatorSimulator.Api.dll"]
