using ChestSnap.Helpers;

namespace ChestSnap.DebugVisuals.Patch;

[HarmonyPatch, HarmonyWrapSafe]
file static class FindGoodFontPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ConnectPanel), nameof(ConnectPanel.Start))]
    private static void GetFont(ConnectPanel __instance)
    {
        if (Helper.IsMainScene() == false) return;
        if (Helper.IsDedicatedServer()) return;

        DebugOverlayManager.Instance?.labelFont = __instance.m_worldField.font;
    }
}