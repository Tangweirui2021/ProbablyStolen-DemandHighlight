// Runs only metadata checks and the game's own ModScanner, not the game or its Unity runtime.
// Scanner harness adapted from KCN-judu/ProbablyStolen-Mods tools/modding/ScanTest.cs (Apache-2.0).
using System;
using System.IO;
using System.Linq;
using System.Reflection;

static class Program
{
    static string managed;
    static int Main(string[] args)
    {
        if (args.Length != 2) { Console.Error.WriteLine("GameCheck <Managed folder> <built mod folder>"); return 2; }
        managed = Path.GetFullPath(args[0]);
        AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => {
            string path = Path.Combine(managed, new AssemblyName(e.Name).Name + ".dll");
            return File.Exists(path) ? Assembly.LoadFrom(path) : null;
        };
        return Run(Path.GetFullPath(args[1]));
    }

    static int Run(string modFolder)
    {
        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        int targets = 0;
        Action<Type, string, Type[]> method = (type, name, parameters) => {
            var found = type.GetMethod(name, flags, null, parameters, null);
            if (found == null) throw new MissingMethodException(type.Name, name);
            targets++; Console.WriteLine("TARGET " + type.Name + "." + found);
        };
        var constructors = typeof(GameItemElement).GetConstructors(flags);
        if (constructors.Length == 0) throw new MissingMethodException("GameItemElement constructors");
        targets += constructors.Length;
        foreach (var c in constructors) Console.WriteLine("TARGET GameItemElement." + c);
        foreach (var name in new[] { "Validate", "Destroy", "RotCW", "RotCCW", "IsDestroyed" })
            method(typeof(GameItemElement), name, Type.EmptyTypes);
        foreach (var name in new[] { "ToggleBackground", "ToggleFakeBackground" })
        {
            var methods = typeof(GameItemElement).GetMethods(flags).Where(m => m.Name == name).ToArray();
            if (methods.Length == 0) throw new MissingMethodException("GameItemElement", name);
            targets += methods.Length;
            foreach (var m in methods) Console.WriteLine("TARGET GameItemElement." + m);
        }
        foreach (var name in new[] { "EnableTag", "DisableTag", "SetEnableDisableTag" })
            method(typeof(GameItem), name, new[] { typeof(string), typeof(bool) });
        method(typeof(GameItem), "ModifyTag", new[] { typeof(string), typeof(Action<TagState>), typeof(bool) });
        method(typeof(GameItem), "SetGameItemType", new[] { typeof(string) });
        foreach (var name in new[] { "AddParent", "RemoveParent" })
            method(typeof(GameItem), name, new[] { typeof(GraphNodeStorage) });
        foreach (var name in new[] { "Show", "Hide" })
        {
            var methods = typeof(PixelWindow).GetMethods(flags).Where(m => m.Name == name).ToArray();
            if (methods.Length == 0) throw new MissingMethodException("PixelWindow", name);
            targets += methods.Length;
        }
        Console.WriteLine("API targets verified: " + targets);
        var dlls = System.IO.Directory.GetFiles(modFolder, "*.dll");
        if (dlls.Length != 1 || Path.GetFileName(dlls[0]) != "TypeDemandStars.dll")
            throw new InvalidOperationException("Expected only the mod DLL in its output folder");
        var allowed = System.IO.Directory.GetFiles(managed, "*.dll").Select(Path.GetFileNameWithoutExtension).ToList();
        var result = ModScanner.Scan(dlls, allowed, modFolder, true);
        Console.WriteLine("Methods scanned: " + result.MethodsScanned + "; refused: " + result.Refused + "; error: " + (result.Error ?? "none"));
        Console.WriteLine("Capabilities: " + (result.Capabilities.Count == 0 ? "none" : string.Join(", ", result.Capabilities.ToArray())));
        foreach (var finding in result.HardFlags) Console.WriteLine("HARD " + Describe(finding));
        foreach (var finding in result.Findings) Console.WriteLine("FIND " + Describe(finding));
        return !result.Refused && result.Error == null && result.Capabilities.Count == 0 && result.HardFlags.Count == 0 ? 0 : 1;
    }

    static string Describe(object finding)
    {
        return string.Join(" | ", finding.GetType().GetFields().Select(f => f.Name + "=" + f.GetValue(finding)));
    }
}
