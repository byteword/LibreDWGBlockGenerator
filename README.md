# LibreDWG Block Generator

LibreDWG Block Generator is an independent command-line application for generating
dynamic block DWG files from implementation-neutral JSON specifications.

The public interface is deliberately small:

```text
JSON specification -> command-line generator -> DWG file
```

The specification describes CAD concepts such as geometry, parameters, grips, and
actions. It does not expose LibreDWG structs, handles, or memory layouts, so another
DWG backend can implement the same contract.

## Status

This repository currently provides the versioned specification, JSON Schema, CLI
contract, input validation, compatibility fixtures, and the first native backend
probe. The probe writes and re-reads one static R2000 polyline block. Dynamic R2004
generation remains unavailable and fails explicitly with exit code `3`.

LibreDWG support for writing post-R2000 DWG versions and several dynamic-block
objects is experimental. Generated files must therefore pass both structural
round-trip checks and validation in a compatible CAD application before release.

## Commands

```powershell
dotnet run --project src/LibreDWG.BlockGenerator.Cli -- validate `
  --input examples/rectangle-linear-stretch.json

dotnet run --project src/LibreDWG.BlockGenerator.Cli -- generate `
  --input examples/rectangle-linear-stretch.json `
  --output artifacts/rectangle.dwg
```

See [the input specification](docs/input-spec-v1.md),
[CLI contract](docs/command-line.md), and
[JSON Schema](schema/dynamic-block-spec-v1.schema.json). Native build instructions
are in [docs/native-build.md](docs/native-build.md).

## Build

```powershell
dotnet build LibreDWGBlockGenerator.slnx
dotnet run --project tests/LibreDWG.BlockGenerator.SmokeTests
```

## License

LibreDWG Block Generator is licensed under GPL-3.0-or-later. See [LICENSE](LICENSE).
