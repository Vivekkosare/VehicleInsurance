FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
# Install dotnet-ef tool globally for migrations
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
# Copy the solution file and all project folders (preserving structure)
COPY VehicleInsurance.sln ./
COPY InsuranceAPI/ ./InsuranceAPI/
COPY VehicleInsurance.Shared/ ./VehicleInsurance.Shared/
# Restore from the API project folder
WORKDIR /src/InsuranceAPI
RUN dotnet restore "InsuranceAPI.csproj"
RUN dotnet build "InsuranceAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "/src/InsuranceAPI/InsuranceAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use SDK image as final image so dotnet-ef is available at runtime
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN dotnet tool install --global dotnet-ef
ENV ASPNETCORE_URLS=http://+:80
ENV PATH="$PATH:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "InsuranceAPI.dll"]
