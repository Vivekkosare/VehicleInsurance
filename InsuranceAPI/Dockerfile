FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["InsuranceAPI.csproj", "."]
RUN dotnet restore "InsuranceAPI.csproj"
COPY . .
RUN dotnet build "InsuranceAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InsuranceAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "InsuranceAPI.dll"]
