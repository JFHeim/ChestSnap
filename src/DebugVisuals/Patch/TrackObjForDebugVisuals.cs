using ChestSnap.Config;
using ChestSnap.Helpers;

namespace ChestSnap.DebugVisuals.Patch;

[HarmonyPatch, HarmonyWrapSafe]
internal static class TrackObjForDebugVisualsPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.Start))]
    private static void Track(WearNTear __instance)
    {
        if (ConfigsContainer.ShowDebugVisuals is DebugVisualsDisplayMode.Hidden) return;
        if (Helper.IsMainScene() == false) return;
        if (Helper.IsDedicatedServer()) return;
        if(DebugOverlayManager.Instance == null) return;

        if (Piece.s_ghostLayer.Equals(__instance.gameObject.layer)) return;
        if(ConfigsContainer.SnappointLookup.ContainsKey(Utils.GetPrefabName(__instance.name)) == false) return;

        DebugOverlayManager.Instance.RegisterObject(__instance.transform);
        DebugOverlayManager.Instance.RegisterSnapPoints(__instance.transform);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.OnDestroy))]
    private static void Untrack(WearNTear __instance)
    {
        if (ConfigsContainer.ShowDebugVisuals is DebugVisualsDisplayMode.Hidden) return;
        if (Helper.IsMainScene() == false) return;
        if (Helper.IsDedicatedServer()) return;
        if(DebugOverlayManager.Instance == null) return;

        if (Piece.s_ghostLayer.Equals(__instance.gameObject.layer)) return;
        if(ConfigsContainer.SnappointLookup.ContainsKey(Utils.GetPrefabName(__instance.name)) == false) return;

        DebugOverlayManager.Instance.UnregisterObject(__instance.transform);
        DebugOverlayManager.Instance.UnregisterPoints(__instance.transform);
    }
}