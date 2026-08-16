using System.Reflection;
using System.Runtime.Loader;
using Godot;

namespace RelicTracker;

// Loads RelicTracker.BaseLib.dll only when BaseLib is already present (optional Better Mod Menu config).
internal static class BaseLibConfigLoader
{
    private const int MaxAttempts = 60;
    private const string BridgeAssemblyName = "RelicTracker.BaseLib";
    private const string RegistrationTypeName = "RelicTracker.BaseLibConfigRegistration";

    private static int _attempts;
    private static bool _done;
    private static bool _resolvingHooked;

    public static void Schedule() => Callable.From(TryRegister).CallDeferred();

    private static void TryRegister()
    {
        if (_done)
        {
            return;
        }

        if (GetAssembly("BaseLib") is null || !IsBaseLibReady())
        {
            Retry();
            return;
        }

        try
        {
            Assembly? bridge = GetAssembly(BridgeAssemblyName) ?? LoadBridge();
            if (bridge is null)
            {
                ModLog.Info("BaseLib is present but RelicTracker.BaseLib.dll was not found; Config page unavailable.");
                _done = true;
                return;
            }

            MethodInfo? register = bridge
                .GetType(RegistrationTypeName)
                ?.GetMethod("Register", BindingFlags.Public | BindingFlags.Static);

            if (register is null)
            {
                ModLog.Warning("RelicTracker.BaseLib.dll is missing BaseLibConfigRegistration.Register.");
                _done = true;
                return;
            }

            register.Invoke(null, null);
            _done = true;
            ModLog.Info("Registered RelicTracker config with BaseLib.");
        }
        catch (Exception ex)
        {
            Exception root = ex is TargetInvocationException { InnerException: { } inner } ? inner : ex;
            ModLog.Warning($"BaseLib config registration failed: {root}");
            _done = true;
        }
    }

    private static void Retry()
    {
        if (++_attempts >= MaxAttempts)
        {
            _done = true;
            if (GetAssembly("BaseLib") is not null)
            {
                ModLog.Warning("BaseLib config not registered after waiting. RelicTracker still runs without it.");
            }

            return;
        }

        Schedule();
    }

    private static bool IsBaseLibReady()
    {
        try
        {
            Type? registry = GetAssembly("BaseLib")?.GetType("BaseLib.Config.ModConfigRegistry");
            MethodInfo? get = registry?.GetMethod(
                "Get",
                BindingFlags.Public | BindingFlags.Static,
                binder: null,
                types: [typeof(string)],
                modifiers: null);

            return get?.Invoke(null, ["BaseLib"]) is not null;
        }
        catch
        {
            return false;
        }
    }

    private static Assembly? GetAssembly(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == name);

    private static Assembly? LoadBridge()
    {
        string? directory = Path.GetDirectoryName(typeof(MainFile).Assembly.Location);
        if (string.IsNullOrEmpty(directory))
        {
            string? exeDirectory = Path.GetDirectoryName(OS.GetExecutablePath());
            directory = string.IsNullOrEmpty(exeDirectory)
                ? null
                : Path.Combine(exeDirectory, "mods", MainFile.ModId);
        }

        if (directory is null)
        {
            return null;
        }

        string path = Path.Combine(directory, BridgeAssemblyName + ".dll");
        if (!File.Exists(path))
        {
            return null;
        }

        AssemblyLoadContext alc =
            AssemblyLoadContext.GetLoadContext(typeof(MainFile).Assembly) ?? AssemblyLoadContext.Default;

        if (!_resolvingHooked)
        {
            _resolvingHooked = true;
            alc.Resolving += (_, name) => name.Name == "BaseLib" ? GetAssembly("BaseLib") : null;
        }

        return alc.LoadFromAssemblyPath(path);
    }
}
