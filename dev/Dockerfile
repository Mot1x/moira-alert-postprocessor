# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем .csproj
COPY MoiraAlertPostprocessor/MoiraAlertPostprocessor.csproj ./MoiraAlertPostprocessor/

# Восстанавливаем зависимости
WORKDIR /src/MoiraAlertPostprocessor
RUN dotnet restore

# Копируем остальной исходный код проекта
COPY MoiraAlertPostprocessor/ ./

# Публикуем в /app/publish
RUN dotnet publish -c Release -o /app/publish --no-restore


# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Копируем опубликованные файлы
COPY --from=build /app/publish .

# Настройка порта
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Запуск
ENTRYPOINT ["dotnet", "MoiraAlertPostprocessor.dll"]