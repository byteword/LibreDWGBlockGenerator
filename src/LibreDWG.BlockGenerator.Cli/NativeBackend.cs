using System.Runtime.InteropServices;

internal static class NativeBackend
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int GenerateDelegate(
        nint outputPath,
        nint blockName,
        [In] double[] origin,
        [In] double[] coordinates,
        nuint pointCount,
        int closed);

    public static int GenerateR2000PolylineBlock(
        string outputPath,
        string blockName,
        double[] origin,
        double[] coordinates,
        int pointCount,
        bool closed)
    {
        var configured = Environment.GetEnvironmentVariable("LIBREDWG_BLOCK_GENERATOR_NATIVE");
        var libraryPath = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(AppContext.BaseDirectory, "lbg_native.dll")
            : Path.GetFullPath(configured);
        if (!File.Exists(libraryPath))
            throw new DllNotFoundException(
                $"Native backend not found. Set LIBREDWG_BLOCK_GENERATOR_NATIVE or copy lbg_native.dll beside the CLI. Expected: {libraryPath}");

        var library = NativeLibrary.Load(libraryPath);
        var outputPointer = Marshal.StringToCoTaskMemUTF8(outputPath);
        var namePointer = Marshal.StringToCoTaskMemUTF8(blockName);
        try
        {
            var export = NativeLibrary.GetExport(library, "lbg_generate_r2000_polyline_block");
            var generate = Marshal.GetDelegateForFunctionPointer<GenerateDelegate>(export);
            return generate(outputPointer, namePointer, origin, coordinates, (nuint)pointCount, closed ? 1 : 0);
        }
        finally
        {
            Marshal.FreeCoTaskMem(outputPointer);
            Marshal.FreeCoTaskMem(namePointer);
            NativeLibrary.Free(library);
        }
    }
}
