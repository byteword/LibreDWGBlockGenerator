# Native backend build

The native backend links to LibreDWG and is therefore part of this GPL-3.0-or-later
program. No LibreDWG binary is committed to this repository.

## Pinned development baseline

- LibreDWG release: `0.13.4`
- Tag object: `278a079918ef9ab0a90d7588d3a4d459816bb0c9`
- Checked-out commit: `e3774bd4020fcfebb68150361db74b8b34d170fe`
- Required submodule: `jsmn` at `85695f3d5903b1cd5b4030efe50db3b4f5f3c928`

Clone the official source recursively, configure it with CMake, and build the
`libredwg` target. Then configure `native/CMakeLists.txt` with:

```text
-DLIBREDWG_SOURCE_DIR=<checkout>
-DLIBREDWG_BINARY_DIR=<checkout>/build
```

Build `lbg_native`, then place `lbg_native.dll` and `libredwg.dll` beside the CLI.
Alternatively set `LIBREDWG_BLOCK_GENERATOR_NATIVE` to the absolute path of
`lbg_native.dll`; its dependent `libredwg.dll` must remain discoverable by the OS.

After building the managed solution, run the end-to-end probe with:

```powershell
./scripts/verify-native.ps1 -NativeLibrary ./native/build/Release/lbg_native.dll
```

The 0.13.4 add API documents R2000 as the supported DWG writer target. R2004 and
dynamic-block object writing are deliberately excluded from the stable backend.
