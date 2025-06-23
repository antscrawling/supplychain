# Use the official .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

# Copy everything
COPY . ./

# Set the entry point to run in development mode
ENTRYPOINT ["tail", "-f", "/dev/null"]
