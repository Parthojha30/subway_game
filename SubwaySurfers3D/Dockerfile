# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY SubwaySurfers3D.csproj ./
RUN dotnet restore

# Copy all code and publish
COPY . ./
RUN dotnet publish SubwaySurfers3D.csproj -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

# Expose port 80 & 5000
EXPOSE 80
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "SubwaySurfers3D.dll"]
