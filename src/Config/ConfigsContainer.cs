using System.Globalization;
using BepInEx.Configuration;
using ChestSnap.DebugVisuals;

namespace ChestSnap.Config;

public partial class ConfigsContainer
{
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

    private ConfigsContainer()
    {
        _showDebugVisuals               = config("Debug Overlay", "Show debug visuals", DebugVisualsDisplayMode.Hidden, "");
        _drawBounds                     = config("Debug Overlay", "Draw object bounds", false, "");
        _drawSnappoints                 = config("Debug Overlay", "Draw object snap points", true, "");
        _drawSnappointsLocalPosition    = config("Debug Overlay", "Draw snap points position", true, "");
        _drawBoundsCornersLocalPosition = config("Debug Overlay", "Draw bounds corners position", false, "");
        _snapPointOverlayColor          = config("Debug Overlay", "Snap points color", Color.cyan, "");
        _boundsColor                    = config("Debug Overlay", "Object bounds color", Color.green, "");
    }

    private void ApplyConfiguration()
    {
    }
}