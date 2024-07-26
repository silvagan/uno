using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application;

internal class Options
{
    public string name { get; set; } = "";

    public static string? GetOptionsFilePath()
    {
        var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (appdata == null) return null;

        var projectFolder = Path.Combine(appdata, "Uno");

        return Path.Combine(projectFolder, "options.json");
    }

    public bool Save()
    {
        var optionsPath = GetOptionsFilePath();
        if (optionsPath == null) return false;

        var optionsDirectory = Path.GetDirectoryName(optionsPath);
        Debug.Assert(optionsDirectory != null);
        Directory.CreateDirectory(optionsDirectory);

        var optionsJson = JsonSerializer.Serialize(this);

        try
        {
            File.WriteAllText(optionsPath, optionsJson);
        }
        catch
        {
            return false;
        }

        return true;
    }

    public bool Load()
    {
        var optionsPath = GetOptionsFilePath();
        if (optionsPath == null) return false;

        string optionsJson;
        try
        {
            optionsJson = File.ReadAllText(optionsPath);
        }
        catch
        {
            return false;
        }

        var loadedProfile = JsonSerializer.Deserialize<Options>(optionsJson);
        if (loadedProfile == null) return false;

        name = loadedProfile.name;

        return true;
    }
}
