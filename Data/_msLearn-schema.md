# MS Learn Guidance Schema

Each question in an exam JSON file can include an optional `msLearn` block that maps the question to the relevant Microsoft Learn documentation.

## JSON Shape

```json
{
  "id": 1,
  "topic": "...",
  "questionText": "...",
  "msLearn": {
    "primaryTopic": "Azure Functions - Hosting Plans",
    "navigationPath": "Azure > Compute > Azure Functions > Concepts > Hosting plans",
    "searchKeywords": ["azure functions consumption plan cold start", "azure functions premium plan", "azure functions app service plan"],
    "lookFor": "Check the 'Hosting plans comparison' table; the 'Cold start' row shows which plans have warm instances.",
    "docsUrl": "https://learn.microsoft.com/azure/azure-functions/functions-scale"
  }
}
```

## Field Definitions

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `primaryTopic` | string | yes | Short label: "Service - Feature". Displayed as header. |
| `navigationPath` | string | yes | Breadcrumb path on learn.microsoft.com, using `>` separators, starting with `Azure`. |
| `searchKeywords` | string[] | yes | 3–4 keywords that typed into learn.microsoft.com/search surface the answer. Keep short and specific. |
| `lookFor` | string | yes | Which section, table heading, or UI element on the docs page contains the proof for the correct answer. |
| `docsUrl` | string | no | Direct deep link to the relevant learn.microsoft.com page. Must be a real URL — do not hallucinate. |

## Quality Rules

1. **Real URLs only** — verify `docsUrl` exists before including it. If unsure, omit it.
2. **Keywords are search-bar friendly** — 3–5 words each, not just the service name. Target what a test-taker would type under pressure.
3. **`lookFor` is actionable** — name a real section heading, table, or comparison block on the page. e.g. "Look in the 'Comparison table' under the Tier column" not "Read the docs".
4. **`navigationPath` starts with `Azure`** — follow the breadcrumb structure on learn.microsoft.com.

## Worked Example (AZ-204, Question 2)

Question: Minimize latency when processing blob uploads via Azure Functions.
Correct answer: App Service plan + Blob Storage trigger (no cold start).

```json
"msLearn": {
  "primaryTopic": "Azure Functions - Hosting Plans",
  "navigationPath": "Azure > Compute > Azure Functions > Concepts > Hosting plans",
  "searchKeywords": ["azure functions cold start consumption plan", "azure functions app service plan always on", "azure functions hosting options comparison"],
  "lookFor": "Check the 'Cold start' row in the hosting plans comparison table — Consumption shows 'Yes', App Service plan shows 'No' when Always On is enabled.",
  "docsUrl": "https://learn.microsoft.com/azure/azure-functions/functions-scale"
}
```
