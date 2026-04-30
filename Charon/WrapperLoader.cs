using System.Runtime.InteropServices;
using static CharonAnchor.WrapperInterop;

namespace CharonAnchor;

/// <summary>
/// Handles loading wrapper.node and bypassing anti-reverse engineering checks
/// </summary>
public class WrapperLoader : IDisposable
{
    private IntPtr _wrapperHandle = IntPtr.Zero;
    private nuint _baseAddress = 0;
    private bool _disposed = false;

    // Version offsets (function address relative to base)
    public static readonly Dictionary<string, nuint> VersionOffsets = new()
    {
        ["3.2.19"] = 0x5ADE220,
        ["3.2.28"] = 0x57E1131,
    };

    private readonly string _workingDirectory;
    private readonly string _version;

    public WrapperLoader(string? workingDirectory = null, string version = "3.2.28")
    {
        _workingDirectory = workingDirectory ?? Directory.GetCurrentDirectory();
        _version = version;
    }

    /// <summary>
    /// Initialize: preload dependencies, create bypass symlink, load wrapper.node
    /// </summary>
    public bool Initialize()
    {
        // 1. Preload dependencies
        PreloadDependencies();

        // 2. Create DFLJ bypass symlink (anti-reverse check bypass)
        var bypassPath = CreateBypassSymlink();
        if (bypassPath == null)
        {
            Console.Error.WriteLine("Failed to create bypass symlink");
            return false;
        }

        // 3. Load wrapper.node from bypass path
        _wrapperHandle = dlopen(bypassPath, RTLD_LAZY);
        if (_wrapperHandle == IntPtr.Zero)
        {
            Console.Error.WriteLine($"dlopen failed: {dlerror()}");
            return false;
        }

        // 4. Find base address by parsing /proc/self/maps
        _baseAddress = FindBaseAddress(bypassPath);
        if (_baseAddress == 0)
        {
            Console.Error.WriteLine("Failed to find wrapper.node base address");
            dlclose(_wrapperHandle);
            _wrapperHandle = IntPtr.Zero;
            return false;
        }

        Console.WriteLine($"Loaded wrapper.node at base: 0x{_baseAddress:X}");
        return true;
    }

    private void PreloadDependencies()
    {
        var libs = new[]
        {
            "libgnutls.so.30",
            Path.Combine(_workingDirectory, "libsymbols.so"),
            Path.Combine(_workingDirectory, "libbugly.so"),
            Path.Combine(_workingDirectory, "libcrbase.so"),
        };

        foreach (var lib in libs)
        {
            var handle = dlopen(lib, RTLD_LAZY | RTLD_GLOBAL);
            if (handle != IntPtr.Zero)
            {
                Console.WriteLine($"Preloaded: {lib}");
            }
            else
            {
                Console.WriteLine($"Failed to preload {lib}: {dlerror()}");
            }
        }
    }

    private string? CreateBypassSymlink()
    {
        // Create /tmp/DFLJ directory
        const string tmpDir = "/tmp/DFLJ";
        mkdir(tmpDir, 0755);

        // Get absolute path to wrapper.node
        var wrapperPath = Path.Combine(_workingDirectory, "wrapper.node");
        if (!Path.IsPathRooted(wrapperPath))
        {
            wrapperPath = Path.GetFullPath(wrapperPath);
        }

        // Create symlink
        var symlinkPath = Path.Combine(tmpDir, "wrapper.node");
        unlink(symlinkPath); // Remove existing

        if (symlink(wrapperPath, symlinkPath) != 0)
        {
            Console.Error.WriteLine($"Failed to create symlink: {dlerror()}");
            return null;
        }

        Console.WriteLine($"Created bypass symlink: {symlinkPath} -> {wrapperPath}");
        return symlinkPath;
    }

    private nuint FindBaseAddress(string path)
    {
        // Parse /proc/self/maps to find wrapper.node base address
        var mapsFile = "/proc/self/maps";
        if (!File.Exists(mapsFile))
        {
            Console.Error.WriteLine("/proc/self/maps not found");
            return 0;
        }

        var lines = File.ReadAllLines(mapsFile);
        foreach (var line in lines)
        {
            if (line.Contains(path) || line.Contains("wrapper.node"))
            {
                // Format: "7f1234000000-7f1234500000 r-xp ... /path/to/wrapper.node"
                var parts = line.Split(' ');
                if (parts.Length > 0)
                {
                    var addrRange = parts[0].Split('-');
                    if (addrRange.Length > 0)
                    {
                        if (nuint.TryParse(addrRange[0], System.Globalization.NumberStyles.HexNumber, null, out nuint baseAddr))
                        {
                            return baseAddr;
                        }
                    }
                }
            }
        }

        return 0;
    }

    /// <summary>
    /// Get sign function delegate for the specified version
    /// </summary>
    public T GetSignFunc<T>() where T : Delegate
    {
        if (_baseAddress == 0)
        {
            throw new InvalidOperationException("Wrapper not loaded");
        }

        var offset = VersionOffsets.TryGetValue(_version, out var o) ? o : VersionOffsets["3.2.28"];
        var funcAddr = _baseAddress + offset;

        Console.WriteLine($"Sign function at: 0x{funcAddr:X} (base=0x{_baseAddress:X} + offset=0x{offset:X})");
        return GetDelegateForFunctionPointer<T>(funcAddr);
    }

    /// <summary>
    /// Get the current version's offset
    /// </summary>
    public nuint GetOffset() => VersionOffsets.TryGetValue(_version, out var o) ? o : VersionOffsets["3.2.28"];

    public nuint BaseAddress => _baseAddress;

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_wrapperHandle != IntPtr.Zero)
            {
                dlclose(_wrapperHandle);
                _wrapperHandle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }
}