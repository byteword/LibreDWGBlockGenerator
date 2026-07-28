# Dynamic Block Specification v1

The specification describes one dynamic block using implementation-neutral CAD
concepts. A conforming generator accepts UTF-8 JSON and produces one DWG file.
It does not expose any library-specific object model.

## Initial subset

- `schemaVersion` is `1.0`.
- Schema v1 recognizes `r2000` and `r2004`; units are `millimeters`.
- The stable LibreDWG 0.13.4 probe only writes static `r2000` blocks.
- Coordinates use block-local coordinates.
- Every geometry, parameter, and action ID is non-empty and globally unique.
- Geometry: open or closed `polyline2d`.
- Parameters: `linear`.
- Actions: `stretch`, selecting geometry by ID and using two XY frame corners.

Later revisions may add visibility, flip, rotation, lookup, move, scale,
attributes, and more geometry without exposing a DWG library's internals.

## Conformance

A generator must reject unknown fields and references, duplicate IDs, invalid
point dimensions, unsupported versions, and unsupported features. It must not
silently emit static geometry when requested dynamic behavior cannot be encoded.
