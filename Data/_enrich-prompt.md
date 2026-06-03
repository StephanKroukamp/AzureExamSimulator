# AZ-204 MS Learn Enrichment Prompt

Use this prompt in a Claude Code session to populate `msLearn` fields in an exam JSON file.

## Instructions

Open a conversation with Claude Code, attach or paste the target JSON file (e.g. `AZ-204.json`), then send the prompt below.

---

## Prompt

```
You are an expert Azure Solutions Architect and Microsoft Certified Trainer.

I have attached an exam JSON file. Your task is to enrich each question with an `msLearn` block that maps it to the exact Microsoft Learn documentation a test-taker would need during an open-book exam.

**Schema** (from `Data/_msLearn-schema.md`):
```json
{
  "msLearn": {
    "primaryTopic": "Service - Feature",
    "navigationPath": "Azure > Category > Service > Concept",
    "searchKeywords": ["keyword1 phrase", "keyword2 phrase", "keyword3 phrase"],
    "lookFor": "Specific section, table, or heading on the docs page that proves the correct answer.",
    "docsUrl": "https://learn.microsoft.com/azure/..."
  }
}
```

**Output format**: Return a JSON array of patches — one object per question:
```json
[
  { "id": 1, "msLearn": { ... } },
  { "id": 2, "msLearn": { ... } }
]
```

**Quality constraints**:
1. `docsUrl` must be a real learn.microsoft.com URL — omit if not certain.
2. `searchKeywords` must be 3–4 phrases that surface the answer when typed into learn.microsoft.com/search.
3. `lookFor` must name a real section heading, table, or comparison block on the linked page.
4. `navigationPath` uses `>` separators and starts with `Azure`.
5. Focus entirely on WHERE to find the proof, not on explaining the answer.

Process all questions in the file.
```

---

## Merging the patches

After Claude returns the JSON array, ask it to merge the patches into the original file:

```
Now merge those msLearn patches into the original AZ-204.json — add the `msLearn` field to each question object and write the updated file.
```

Claude Code can edit the JSON file directly. Diff-review before committing.

## Validation

Run the app and open AZ-204 in Study Mode to verify:
- MS Learn navigation block appears under each question
- Search keyword chips link to learn.microsoft.com/search
- "Open docs" button (if docsUrl provided) opens the correct page
