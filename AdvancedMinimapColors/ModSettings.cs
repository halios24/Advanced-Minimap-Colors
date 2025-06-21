using System;
using System.IO;
using System.Text.Json;

namespace AdvancedMinimapColors;

public class ModSettings
{

    // New colors for player body icons
    public float localPlayerRedTeamRedValue { get; set; } = 1f;
    public float localPlayerRedTeamGreenValue { get; set; } = 0f;
    public float localPlayerRedTeamBlueValue { get; set; } = 0f; // Default: Red

    public float localPlayerBlueTeamRedValue { get; set; } = 0f;
    public float localPlayerBlueTeamGreenValue { get; set; } = 0f;
    public float localPlayerBlueTeamBlueValue { get; set; } = 1f; // Default: Blue

    public float nonLocalRedTeamRedValue { get; set; } = 1f;
    public float nonLocalRedTeamGreenValue { get; set; } = 0f;
    public float nonLocalRedTeamBlueValue { get; set; } = 0.7f; // Default: Pink

    public float nonLocalBlueTeamRedValue { get; set; } = 0f;
    public float nonLocalBlueTeamGreenValue { get; set; } = 1f;
    public float nonLocalBlueTeamBlueValue { get; set; } = 1f; // Default: Cyan

    // New colors for labels
    public float RedTeamTextRedValue { get; set; } = 0f;
    public float RedTeamTextGreenValue { get; set; } = 0f;
    public float RedTeamTextBlueValue { get; set; } = 0f; // Default: Black

    public float BlueTeamTextRedValue { get; set; } =0.5f;
    public float BlueTeamTextGreenValue { get; set; } = 0.5f;
    public float BlueTeamTextBlueValue { get; set; } = 0.5f; // Default: Gray
        
    public float puckIconRedValue { get; set; } = 1f;
    public float puckIconGreenValue { get; set; } = 1f;
    public float puckIconBlueValue { get; set; } = 1f; // Default: White
    
    static string ConfigurationFileName = $"{Plugin.MOD_NAME}.json";

    public static ModSettings Load()
    {
        var path = GetConfigPath();
        var dir = Path.GetDirectoryName(path);

        // 1) make sure "config/" exists
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
            Plugin.Log($"Created missing /config directory");
        }

        if (File.Exists(path))
        {
            try
            {
                var json = File.ReadAllText(path);
                var settings = JsonSerializer.Deserialize<ModSettings>(json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                return settings ?? new ModSettings();
            }
            catch (JsonException je)
            {
                Plugin.Log($"Corrupt config JSON, using defaults: {je.Message}");
                return new ModSettings();
            }
        }

        var defaults = new ModSettings();
        File.WriteAllText(path,
            JsonSerializer.Serialize(defaults, new JsonSerializerOptions
            {
                WriteIndented = true
            }));

        Plugin.Log($"Config file `{path}` did not exist, created with defaults.");
        return defaults;
    }

    public void Save()
    {
        var path = GetConfigPath();
        var dir = Path.GetDirectoryName(path);

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllText(path,
            JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            }));
    }

    public static string GetConfigPath()
    {
        string rootPath = Path.GetFullPath(".");
        string configPath = Path.Combine(rootPath, "config", ConfigurationFileName);
        return configPath;
    }
}
