using ChestSnap.Helpers;

namespace ChestSnap.DebugVisuals.Patch;

[HarmonyPatch, HarmonyWrapSafe]
file static class TrackObjForDebugVisualsPatch
{
    // [HarmonyPostfix]
    // [HarmonyPatch(typeof(GameCamera), nameof(GameCamera.Awake))]
    // private static void CreateManger(GameCamera __instance)
    // {
    //     if (Helper.IsMainScene() == false) return;
    //     if (Helper.IsServer(true) == false) return;
    //
    //     if(!__instance.gameObject.GetComponent<DebugOverlayManager>())
    //         __instance.gameObject.AddComponent<DebugOverlayManager>();
    // }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Piece), nameof(Piece.Awake))]
    private static void Track(Piece __instance)
    {
        if (Helper.IsMainScene() == false) return;
        if (Helper.IsServer(true) == false) return;

        if (__instance.gameObject.layer == Piece.s_ghostLayer)
        {
            Log.Warning($"Chest '{Utils.GetPrefabName(__instance.name)}' is a ghost !!!");
            return;
        }
        // if((__instance.m_piece?.m_name?.Contains("chest") ?? false) == false) return;

        DebugOverlayManager.Instance?.RegisterObject(__instance.transform);
        DebugOverlayManager.Instance?.RegisterSnapPoints(__instance.transform);

        Log.Info($"Registered overlay for {__instance.name}");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Piece), nameof(Piece.OnDestroy))]
    private static void Untrack(Piece __instance)
    {
        if (Helper.IsMainScene() == false) return;
        if (Helper.IsServer(true) == false) return;

        if(__instance.gameObject.layer == Piece.s_ghostLayer) return;
        // if((__instance.m_name?.Contains("chest") ?? false) == false) return;

        DebugOverlayManager.Instance?.UnregisterObject(__instance.transform);
        DebugOverlayManager.Instance?.UnregisterPoints(__instance.transform);
    }

    public static void DiagnoseBounds(WearNTear wnt)
    {
        if (wnt == null)
        {
            Debug.Log("[Diag] WearNTear is null");
            return;
        }

        if (wnt.m_bounds == null)
        {
            Debug.Log($"[Diag] {wnt.name}: m_bounds is null");
            return;
        }

        Debug.Log($"[Diag] {wnt.name}: m_bounds.Count = {wnt.m_bounds.Count}");

        for (int i = 0; i < wnt.m_bounds.Count; i++)
        {
            var b = wnt.m_bounds[i];
            Debug.Log(
                $"  [{i}] pos={b.m_pos} rot={b.m_rot.eulerAngles} "
                + $"size={b.m_size} sizeMag={b.m_size.magnitude}"
            );
        }
    }
}