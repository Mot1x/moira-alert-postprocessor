# Интеграция с Moira - Краткая инструкция

## Что было сделано

1. ✅ Созданы модели данных (DTOs) для работы с Moira webhook
2. ✅ Реализован сервис обработки алертов `AlertProccessingService`
3. ✅ Создан контроллер `MoiraAlertRecieveController` с endpoints:
   - `POST /api/MoiraAlertRecieve` - получение алертов
   - `GET /api/MoiraAlertRecieve/health` - health check
   - `GET /api/MoiraAlertRecieve/recent` - последние алерты

## Как настроить интеграцию с Moira

### 1. Создать Contact в Moira

```bash
# Через API Moira
curl -X POST http://localhost:8091/api/contact \
  -H "Content-Type: application/json" \
  -d '{
    "type": "webhook",
    "value": "http://your-server:5000/api/MoiraAlertRecieve",
    "user": "YOUR_USER"
  }'
```

### 2. Создать Subscription

```bash
curl -X POST http://localhost:8091/api/subscription \
  -H "Content-Type: application/json" \
  -d '{
    "contacts": ["CONTACT_ID"],
    "tags": ["test", "production"],
    "user": "YOUR_USER"
  }'
```

### 3. Создать Trigger с тегами

Важно: триггер должен иметь теги, указанные в Subscription!

## Формат данных Moira

Когда Moira отправляет алерт, она отправляет JSON:

```json
{
  "trigger": {
    "id": "trigger-id",
    "name": "Trigger Name",
    "description": "Description",
    "tags": ["test"]
  },
  "events": [{
    "metric": "metric.name",
    "values": {"target1": 100.0},
    "timestamp": 1640995200,
    "trigger_event": true,
    "state": "ERROR",
    "old_state": "OK"
  }],
  "contact": { ... },
  "throttled": false
}
```

## Структура проекта

```
Api/
├── Controllers/
│   └── MoiraAlertRecieve/
│       ├── Dtos/                        # Модели данных
│       │   ├── MoiraWebHookPayload.cs
│       │   ├── MoiraTriggerData.cs
│       │   ├── MoiraEventData.cs
│       │   ├── MoiraContactData.cs
│       │   └── MoiraAlertSummary.cs
│       └── MoiraAlertRecieveController.cs  # Контроллер
└── UseCases/
    └── MoiraAlertReceive/
        ├── Interfaces/
        │   └── IAlertProcessingService.cs  # Интерфейс сервиса
        └── AlertProccessingService.cs      # Реализация сервиса
```

## Дополнительная информация

Подробная инструкция по интеграции с Moira находится в файле `MOIRA_SUBSCRIPTION_GUIDE.md` в корне репозитория Moira.

## Тестирование

```bash
# Health check
curl http://localhost:5000/api/MoiraAlertRecieve/health

# Получить последние алерты
curl http://localhost:5000/api/MoiraAlertRecieve/recent?count=5
```
