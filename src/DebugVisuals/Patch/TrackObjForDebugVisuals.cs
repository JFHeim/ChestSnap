using ChestSnap.Config;
using ChestSnap.Helpers;

namespace ChestSnap.DebugVisuals.Patch;

[HarmonyPatch, HarmonyWrapSafe]
file static class TrackObjForDebugVisualsPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Start))]
    private static void Track(WearNTear __instance)
    {
        if (Helper.IsMainScene() == false) return;
        if (Helper.IsDedicatedServer()) return;

        var layer = __instance.gameObject.layer;
        if (layer == Piece.s_ghostLayer) return;
        if(ConfigsContainer.SnappointLookup.ContainsKey(Utils.GetPrefabName(__instance.name)) == false) return;

        DebugOverlayManager.Instance?.RegisterObject(__instance.transform);
        DebugOverlayManager.Instance?.RegisterSnapPoints(__instance.transform);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.OnDestroy))]
    private static void Untrack(WearNTear __instance)
    {
        if (Helper.IsMainScene() == false) return;
        if (Helper.IsDedicatedServer()) return;

        var layer = __instance.gameObject.layer;
        if (layer == Piece.s_ghostLayer) return;
        if(ConfigsContainer.SnappointLookup.ContainsKey(Utils.GetPrefabName(__instance.name)) == false) return;

        DebugOverlayManager.Instance?.UnregisterObject(__instance.transform);
        DebugOverlayManager.Instance?.UnregisterPoints(__instance.transform);
    }
}