# Use the official .NET SDK image as a build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project file and restore dependencies
COPY BankPortal.csproj ./
COPY ../Core/ /Core/
RUN dotnet restore

# Copy the rest of the application and build it
COPY . ./
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image for the runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose the port and run the application
EXPOSE 8503
ENTRYPOINT ["dotnet", "BankPortal.dll"]
