# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем .csproj: solution/project/project.csproj
COPY MoiraAlertPostprocessor/MoiraAlertPostprocessor/MoiraAlertPostprocessor.csproj ./MoiraAlertPostprocessor/

WORKDIR /src/MoiraAlertPostprocessor
RUN dotnet restore

# Копируем весь код проекта
COPY MoiraAlertPostprocessor/MoiraAlertPostprocessor/ ./

RUN dotnet publish -c Release -o /app/publish --no-restore


# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MoiraAlertPostprocessor.dll"]