# First stage: build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj files and restore as distinct layers
COPY Parlo.Docker/Parlo.Docker.csproj ./Parlo.Docker/
COPY LoginProtocol/LoginProtocol.csproj ./LoginProtocol/
RUN dotnet restore Parlo.Docker/Parlo.Docker.csproj

# Copy everything else and build
COPY Parlo.Docker/ ./Parlo.Docker/
COPY LoginProtocol/ ./LoginProtocol/
RUN dotnet publish Parlo.Docker/Parlo.Docker.csproj -c Release -o out

# Second stage: setup the runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

# Copy the published app from the build stage
COPY --from=build-env /app/out .

# Specify the entry point of your app
ENTRYPOINT ["dotnet", "Parlo.Docker.dll"]