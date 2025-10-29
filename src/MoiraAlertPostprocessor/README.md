```markdown
# Moira Alert Postprocessor

Run locally:
1. dotnet build
2. cd src/MoiraPostprocessor.Api
3. dotnet run

Or with Docker:
1. docker build -t moira-postprocessor .
2. docker run -p 8080:8080 -e GptOss__Endpoint="http://..." moira-postprocessor

Webhook endpoint: POST http://<host>:8080/moira/alert

Example payload file: sample_payload.json

Configure env:
- GptOss__Endpoint
- GptOss__Model
```