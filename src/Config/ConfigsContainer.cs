using System.Globalization;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using ChestSnap.DebugVisuals;
using ChestSnap.Helpers;
using ChestSnap.Helpers.Yaml;
using YamlDotNet;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ChestSnap.Config;

public partial class ConfigsContainer
{
    public static Dictionary<string, Vector3[]> SnappointLookup => Instance._customSnappointLookup;
    public static DebugVisualsDisplayMode ShowDebugVisuals => Instance._showDebugVisuals.Value;
    public static bool DrawBounds => Instance._drawBounds.Value;
    public static bool DrawSnappoints => Instance._drawSnappoints.Value;
    public static bool DrawSnappointsLocalPosition => Instance._drawSnappointsLocalPosition.Value;
    public static bool DrawBoundsCornersLocalPosition => Instance._drawBoundsCornersLocalPosition.Value;
    public static Color SnapPointOverlayColor => Instance._snapPointOverlayColor.Value;
    public static Color BoundsColor => Instance._boundsColor.Value;

    private readonly ConfigEntry<DebugVisualsDisplayMode> _showDebugVisuals;
    private readonly ConfigEntry<bool> _drawBounds;
    private readonly ConfigEntry<bool> _drawSnappoints;
    private readonly ConfigEntry<bool> _drawSnappointsLocalPosition;
    private readonly ConfigEntry<bool> _drawBoundsCornersLocalPosition;
    private readonly ConfigEntry<Color> _snapPointOverlayColor;
    private readonly ConfigEntry<Color> _boundsColor;

    private Dictionary<string, Vector3[]> _customSnappointLookup = [];
    private readonly ISerializer _yamlSerializer;
    private readonly IDeserializer _yamlDeserializer;
    private readonly string _snappointsDataYamlPath;

    private ConfigsContainer()
    {
        var snappointsDataYamlFilename = $"{Plugin.Info.Metadata.GUID}__snappoints_data.yaml";
        _snappointsDataYamlPath = Path.Combine(Paths.ConfigPath, snappointsDataYamlFilename);
        ConfigFilesToWatch.Add(snappointsDataYamlFilename);

        _showDebugVisuals = config("Debug Overlay", "Show debug visuals", DebugVisualsDisplayMode.Hidden, "");
        _drawBounds = config("Debug Overlay", "Draw object bounds", false, "");
        _drawSnappoints = config("Debug Overlay", "Draw object snap points", true, "");
        _drawSnappointsLocalPosition = config("Debug Overlay", "Draw snap points position", true, "");
        _drawBoundsCornersLocalPosition = config("Debug Overlay", "Draw bounds corners position", false, "");
        _snapPointOverlayColor = config("Debug Overlay", "Snap points color", Color.cyan, "");
        _boundsColor = config("Debug Overlay", "Object bounds color", Color.green, "");

        _yamlSerializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .WithTypeConverter(new Vector3YamlConverter())
            .Build();
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .WithTypeConverter(new Vector3YamlConverter())
            .Build();

        CreateDefaultSnappointsYamlFile();
    }

    private void CreateDefaultSnappointsYamlFile()
    {
        if (File.Exists(_snappointsDataYamlPath)) return;

        string? defaultSnappointsYaml = EmbeddedFileLoader.GetText("default_snappoints.yaml");
        if (defaultSnappointsYaml is null)
        {
            Log.Error("Failed to load default snappoints, this should not happen. Report it to the mod developer");
            return;
        }

        File.WriteAllText(_snappointsDataYamlPath, defaultSnappointsYaml);
    }

    private void ApplyConfiguration()
    {
        CreateDefaultSnappointsYamlFile();

        string yamlFromFile;
        try
        {
            yamlFromFile = File.ReadAllText(_snappointsDataYamlPath);
        }
        catch (Exception e)
        {
            Log.Error(e, $"Failed to open snappoints file at '{_snappointsDataYamlPath}'");
            return;
        }

        try
        {
            var dictionary = _yamlDeserializer.Deserialize<Dictionary<string, Vector3[]>>(yamlFromFile);
            _customSnappointLookup.Clear();
            _customSnappointLookup = dictionary;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to parse snappoints file, check it for syntax errors");
            return;
        }

        SnappointHelper.RecreateSnappoints();
    }
}