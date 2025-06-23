# Use the official .NET SDK image as a build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file and project files
COPY SupplyChainFinance.sln .
COPY Core/ ./Core/
COPY BankPortal/ ./BankPortal/
COPY ClientPortal/ ./ClientPortal/
COPY *.csproj ./

# Restore dependencies
RUN dotnet restore SupplyChainFinance.sln

# Copy the rest of the application and build it
COPY . ./
# Build ClientPortal
RUN dotnet publish -c Release -o out/clientportal ClientPortal/ClientPortal.csproj
# Build BankPortal
RUN dotnet publish -c Release -o out/bankportal BankPortal/BankPortal.csproj
# Build ManageFacilities
RUN dotnet publish -c Release -o out/managefacilities ManageFacilities.csproj

# Create runtime images
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS clientportal
WORKDIR /app
COPY --from=build /app/out/clientportal .
EXPOSE 8502
ENTRYPOINT ["dotnet", "ClientPortal.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS bankportal
WORKDIR /app
COPY --from=build /app/out/bankportal .
EXPOSE 8503
ENTRYPOINT ["dotnet", "BankPortal.dll"]





