# Moira Alert Postprocessor (Ollama-only)

Сервис принимает вебхуки Moira и генерирует рекомендации с помощью локальной LLM через Ollama. Ответ модели логируется в консоль и возвращается в HTTP-ответе.

Возможности
- Эндпойнт: POST /moira/alert
- Парсинг входного JSON (структура Moira)
- Интеграция с Ollama (HTTP /api/generate)
- Вывод ответа модели в консоль

Требования
- .NET 8 SDK
- Запущенная Ollama и загруженная модель (пример: gpt-oss:20b-cloud)

Подготовка Ollama (один раз)
- Убедитесь, что Ollama работает на http://localhost:11434
- Загрузите модель (пример):
```powershell
ollama pull gpt-oss:20b-cloud
```

Быстрый старт (локально)
1) Перейдите в папку проекта
2) Сборка
```powershell
dotnet build
```
3) Запуск приложения на http://localhost:5079
- PowerShell:
```powershell
$env:ASPNETCORE_URLS = 'http://localhost:5079'
$env:Ollama__Endpoint = 'http://localhost:11434/api/generate'
$env:Ollama__Model = 'gpt-oss:20b-cloud'
$env:Ollama__TimeoutSeconds = '30'

dotnet run --project .\MoiraAlertPostprocessor.csproj
```
- cmd.exe (альтернатива):
```bat
set ASPNETCORE_URLS=http://localhost:5079
set Ollama__Endpoint=http://localhost:11434/api/generate
set Ollama__Model=gpt-oss:20b-cloud
set Ollama__TimeoutSeconds=30

dotnet run --project MoiraAlertPostprocessor.csproj
```
4) Health-check (проверка что сервис слушает порт)
- PowerShell:
```powershell
Invoke-RestMethod -Uri http://localhost:5079/health -Method Get | ConvertTo-Json -Depth 6
```
- cmd.exe:
```bat
curl http://localhost:5079/health
```
Ожидается ответ вида: {"status":"ok","urls":"http://localhost:5079"}

5) Отправьте пример алерта в приложение
- PowerShell (рекомендуется):
```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:5079/moira/alert -ContentType 'application/json' -InFile '.\sample_payload.json' | ConvertTo-Json -Depth 6
```
- cmd.exe:
```bat
curl -X POST http://localhost:5079/moira/alert -H "Content-Type: application/json" -d @sample_payload.json
```
Ожидаемо в консоли dotnet run появится блок "--- NLP Suggestion ---" с summary/details/actions.

Проверка Ollama напрямую (опционально)
- PowerShell:
```powershell
$body = @{ model='gpt-oss:20b-cloud'; prompt='Скажи "pong"'; stream=$false } | ConvertTo-Json
Invoke-RestMethod -Uri 'http://localhost:11434/api/generate' -Method Post -ContentType 'application/json' -Body $body | ConvertTo-Json -Depth 6
```
- cmd.exe:
```bat
curl -X POST http://localhost:11434/api/generate -H "Content-Type: application/json" -d "{\"model\": \"gpt-oss:20b-cloud\", \"prompt\": \"Скажи 'pong'\", \"stream\": false}"
```

Troubleshooting
- "Невозможно соединиться с удаленным сервером":
  1) Проверьте, что сервис запущен и health возвращает ok (см. шаг 4)
  2) Порт 5079 может быть занят; попробуйте другой порт:
     - PowerShell:
       ```powershell
       $env:ASPNETCORE_URLS = 'http://localhost:5080'
       dotnet run --project .\MoiraAlertPostprocessor.csproj
       ```
     - cmd.exe:
       ```bat
       set ASPNETCORE_URLS=http://localhost:5080
       dotnet run --project MoiraAlertPostprocessor.csproj
       ```
  3) Остановите зависший экземпляр перед пересборкой/перезапуском:
     - PowerShell:
       ```powershell
       Get-Process -Name MoiraAlertPostprocessor -ErrorAction SilentlyContinue | Stop-Process -Force
       ```
     - cmd.exe:
       ```bat
       taskkill /IM MoiraAlertPostprocessor.exe /F
       ```
  4) Проверьте доступность Ollama:
     - PowerShell:
       ```powershell
       Invoke-RestMethod -Uri http://localhost:11434/api/generate -Method Post -ContentType 'application/json' -Body (@{ model='gpt-oss:20b-cloud'; prompt='ping'; stream=$false } | ConvertTo-Json)
       ```

Docker
1) Сборка и запуск:
```powershell
cd "C:\Users\honor\OneDrive\Рабочий стол\Проект Moira\project\src\MoiraAlertPostprocessor"
docker compose up --build
```
2) Запрос к контейнеру:
```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:8080/moira/alert -ContentType 'application/json' -InFile '.\sample_payload.json' | ConvertTo-Json -Depth 6
```
Примечания
- В Docker compose сервис публикуется на порт 8080 (порт маппится как 8080:8080)
- Для локального запуска по умолчанию используется порт 5079 (см. launchSettings.json или переменную ASPNETCORE_URLS)
- docker-compose настроен на доступ к Ollama на хосте через host.docker.internal:11434
- Переменные: OLLAMA_ENDPOINT, OLLAMA_MODEL, OLLAMA_TIMEOUT_SECONDS

Пример входного JSON (из Moira)
```
{ "trigger": { "id": "04908d37-1fe3-4710-94fb-a2e10c2258ed", "name": "trigger52", "description": "fxvvfdgbv", "tags": ["test"] }, "events": [ { "metric": "user_requests_total;endpoint=/;instance=metric-spawner:8000;job=metric_spawner", "values": { "t1": 15 }, "timestamp": 1761576520, "trigger_event": false, "state": "ERROR", "old_state": "NODATA" }, { "metric": "trigger52", "values": null, "timestamp": 1761576527, "trigger_event": true, "state": "OK", "old_state": "NODATA" } ], "contact": { "type": "webhook", "value": "http://alert-receiver:8080/moira/alert", "id": "e6abe5c5-d9cb-4770-adf7-66caaca9d11f", "user": "anonymous", "team": "" }, "plot": "много всего", "plots": [], "throttled": false }
```

Лицензия
- Проект распространяется под лицензией MIT (см. LICENSE).
