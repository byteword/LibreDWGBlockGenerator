# Versioning

LDBGen uses four numeric components:

```text
MAJOR.MINOR.PATCH.BUILD
```

The initial baseline is `0.1.0.0`.

## Components

- **MAJOR** changes only for an explicitly designated large-scale or incompatible
  redesign.
- **MINOR** increases when functionality is added in a backward-compatible release.
- **PATCH** increases for backward-compatible bug fixes.
- **BUILD** increases for the next changed source build when no major, minor, or
  patch increment applies.

When MAJOR, MINOR, or PATCH increases, all components to its right reset to zero.
Repeated builds of the exact same commit retain the same version so that artifacts
remain reproducible. Any subsequent source modification intended for a new build
must update the version before it is committed.

Examples:

| Change | Previous | Next |
|---|---:|---:|
| Build-only source revision | `0.1.0.0` | `0.1.0.1` |
| Backward-compatible bug fix | `0.1.0.7` | `0.1.1.0` |
| New feature | `0.1.4.3` | `0.2.0.0` |
| Explicit major redesign | `0.8.2.5` | `1.0.0.0` |
