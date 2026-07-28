# Dynamic Block Specification v1

The specification describes one dynamic block using implementation-neutral CAD
concepts. A conforming generator accepts UTF-8 JSON and produces one DWG file.
It does not expose any library-specific object model.

## Initial subset

- `schemaVersion` is `1.0`.
- The initial target is `r2004` and `millimeters`.
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

