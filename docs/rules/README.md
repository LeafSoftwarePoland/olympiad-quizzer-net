# Competition Rules

One file per competition / olympiad. Each file contains:
1. A plain-text explanation of the competition structure and rules.
2. A machine-readable JSON block (`## Machine-readable mode definition`) that the app reads at runtime to configure quiz sessions.

## JSON schema per file

```json
{
  "olympiad_id": "string — e.g. OIJ",
  "name": "string — full Polish name",
  "governing_body": "string — e.g. MEN",
  "source_url": "string",
  "seasons_available": ["int"],
  "stages": [
    {
      "stage_id": "string — e.g. E1",
      "name": "string",
      "question_count": "int | null",
      "time_limit_minutes": "int | null",
      "allowed_question_types": ["string"],
      "answer_strict_casing": "bool",
      "strict_casing": "bool",
      "partial_points": "bool",
      "passing_threshold": "int | null",
      "points_per_question": "int | null"
    }
  ]
}
```

`null` means not confirmed / not applicable. Do not guess — leave null.

## Files

| File | Competition |
|---|---|
| [oij.md](oij.md) | Olimpiada Informatyczna Juniorów (OIJ) |
