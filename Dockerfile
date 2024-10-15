# Use the official .NET SDK image as a build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory in the container
WORKDIR /app

# Copy the .csproj and .sln files to the container
COPY *.sln .
COPY Lyvads.API.Presentation/Lyvads.API.Presentation*.csproj ./Lyvads.API.Presentation/
COPY Lyvads.Application/*.csproj ./Lyvads.Application/
COPY Lyvads.Infrastructure/*.csproj ./Lyvads.Infrastructure/
COPY Lyvads.Domain/*.csproj ./Lyvads.Domain/
COPY Lyvads.Shared/*.csproj ./Lyvads.Shared/

# Restore NuGet packages for all projects
RUN dotnet restore

# Copy the rest of the source code to the container
COPY . .

# Build the application
RUN dotnet build -c Release
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
RUN pwsh -Command "Install-Module -Name Az -AllowClobber -Force"

# Publish the application
RUN dotnet publish -c Release -o /app/publish --no-restore

# Use the official ASP.NET Core runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory in the container
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Expose port 80 for the API
EXPOSE 80

# Define environment variables if needed
# ENV ASPNETCORE_ENVIRONMENT=Production

# Start the API when the container starts
ENTRYPOINT ["dotnet", "Lyvads.API.Presentation.dll"]