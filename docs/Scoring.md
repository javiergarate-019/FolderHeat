# FolderHeat Scoring

FolderHeat ranks folders by a dynamic heat score. The score is meant to answer:

> Which folder is the user most likely to want right now?

The current v0.2 formula is deliberately small:

```text
heat = pinnedBoost + frequencyWeight + recencyWeight + contextBoost
```

## Inputs

### Pinning

Pinned folders receive a large boost so manual user intent wins over ordinary recency.

### Frequency

Frequency uses logarithmic growth:

```text
log2(accessCount + 1) * 10
```

This rewards repeated use without letting old high-use folders dominate forever.

### Recency

Recency is bucketed:

- less than 5 minutes: strong boost
- less than 1 hour: high boost
- less than 1 day: medium boost
- less than 7 days: low boost
- older: minimal boost

### Ignored Folders

Ignored folders are not rankable and receive negative infinity as their score.

## v0.3 Direction

Context signals add separate boosts instead of pretending every detected folder was manually opened from FolderHeat.

Current context boosts:

```text
active folder: +250
likely next folder: +150
related folder: +75
```

Context signals should be bounded and explainable. The app should avoid noisy sources that inflate heat without a clear user action.

The popup exposes the main rank reason as:

- Explorer
- Next
- Related
- Pinned
- Recent
- Frequent
- Tracked
