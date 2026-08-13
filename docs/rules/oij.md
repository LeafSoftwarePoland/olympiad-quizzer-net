# OIJ — Olimpiada Informatyczna Juniorów

Polish national junior informatics olympiad organized under the Ministry of Education (MEN). Open to students up to year 8 of primary school (szkoła podstawowa).

**Source:** https://oij.edu.pl

## Competition structure

Three stages:

- **Etap Szkolny (E1)** — school stage. Conducted in each school separately. Typical format: 30 multiple-choice and short-answer questions, 90 minutes. Single correct answer per question. No partial credit. Top scorers advance to E2.
- **Etap Okręgowy (E2)** — district/regional stage. Conducted at designated exam centers. Format varies by year — details not yet confirmed for all seasons.
- **Etap Ogólnopolski (E3)** — national final. Conducted centrally. Format varies — details not yet confirmed.

Answers are letters (A/B/C/D) or short text values. Case sensitivity: not enforced — `a` and `A` are treated as equivalent.

Seasons in the question bank: 2019, 2020, 2021, 2023, 2024, 2025 (2022 data not available).

## Machine-readable mode definition

```json
{
  "olympiad_id": "OIJ",
  "name": "Olimpiada Informatyczna Juniorów",
  "governing_body": "MEN",
  "source_url": "https://oij.edu.pl",
  "seasons_available": [2019, 2020, 2021, 2023, 2024, 2025],
  "stages": [
    {
      "stage_id": "E1",
      "name": "Etap Szkolny",
      "question_count": 30,
      "time_limit_minutes": 90,
      "allowed_question_types": ["single", "multi", "shortAnswer"],
      "answer_strict_casing": false,
      "strict_casing": false,
      "partial_points": false,
      "passing_threshold": null,
      "points_per_question": 1
    },
    {
      "stage_id": "E2",
      "name": "Etap Okręgowy",
      "question_count": null,
      "time_limit_minutes": null,
      "allowed_question_types": ["single", "multi", "shortAnswer"],
      "answer_strict_casing": false,
      "strict_casing": false,
      "partial_points": false,
      "passing_threshold": null,
      "points_per_question": null
    },
    {
      "stage_id": "E3",
      "name": "Etap Ogólnopolski",
      "question_count": null,
      "time_limit_minutes": null,
      "allowed_question_types": ["single", "multi", "shortAnswer"],
      "answer_strict_casing": false,
      "strict_casing": false,
      "partial_points": false,
      "passing_threshold": null,
      "points_per_question": null
    }
  ]
}
```

`null` = not confirmed. Do not substitute guesses — leave null until verified against official OIJ materials.
