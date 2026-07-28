# Command-line contract

## Validate

```text
libredwg-block-generator validate --input <spec.json>
```

## Generate

```text
libredwg-block-generator generate --input <spec.json> --output <file.dwg>
```

The output is written only after the complete in-memory document has been encoded
and checked. Failure must not leave a file at the requested output path.

The current native milestone generates one static `polyline2d` block in R2000 and
then reopens the DWG to verify the requested block table record. R2004 and dynamic
actions return code `3` until the experimental writer is separately proven.

| Code | Meaning |
|---:|---|
| `0` | Success |
| `2` | CLI or specification error |
| `3` | Feature or generation backend unavailable |
| `4` | Dynamic block object graph construction failed |
| `5` | DWG encoding failed |
| `6` | Round-trip verification failed |
| `7` | Output path or file operation failed |
| `10` | Unexpected internal failure |

Diagnostics go to standard error. Standard output is reserved for stable
automation output and a future optional machine-readable report.
