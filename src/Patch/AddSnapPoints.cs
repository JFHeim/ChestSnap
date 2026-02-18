using ChestSnap.Helpers;

namespace ChestSnap.Patch;

[HarmonyPatch]
file static class AddSnapPointsPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static void AddSnappointsAtStart()
    {
        if (Helper.IsMainScene() == false) return;
        if (Helper.IsDedicatedServer()) return;

        SnappointHelper.RecreateSnappoints();
    }
}