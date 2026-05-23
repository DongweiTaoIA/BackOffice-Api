# Upgrade Options — BackOffice

Assessment: 6 projects on net9.0, upgrading to net10.0. Simple TFM bump with minor package updates and a few API incompatibilities in the Api project.

## Strategy

### Upgrade Strategy
All projects are modern .NET 9 with a shallow dependency graph — atomic upgrade is the fastest approach.

| Value | Description |
|-------|-------------|
| **All-at-Once** (selected) | Upgrade all projects simultaneously in a single atomic pass |
| Top-Down | Upgrade entry-point apps first, multi-target shared libraries temporarily |
