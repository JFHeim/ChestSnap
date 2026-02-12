namespace ChestSnap.Patch;

[HarmonyPatch]
file static class AddSnapPointsPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static void StartTimer()
    {
        if (Helper.IsMainScene() == false) return;
        if (Helper.IsServer(true) == false) return;

        // TODO: Move snap points to be determined by config

        SnappointHelper.AddSnappoints("piece_chest_wood", [
            new Vector3(0.8f, 0f, 0.37f),
            new Vector3(0.8f, 0f, -0.37f),
            new Vector3(-0.8f, 0f, 0.37f),
            new Vector3(-0.8f, 0f, -0.37f),
            new Vector3(0.65f, 0.8f, 0.35f),
            new Vector3(0.65f, 0.8f, -0.35f),
            new Vector3(-0.65f, 0.8f, 0.35f),
            new Vector3(-0.65f, 0.8f, -0.35f)
        ]);
        SnappointHelper.AddSnappoints("piece_chest", [
            new Vector3(0.9f, 0f, 0.47f),
            new Vector3(0.9f, 0f, -0.47f),
            new Vector3(-0.9f, 0f, 0.47f),
            new Vector3(-0.9f, 0f, -0.47f),
            new Vector3(0.7f, 1.1f, 0.47f),
            new Vector3(0.7f, 1.1f, -0.47f),
            new Vector3(-0.7f, 1.1f, 0.47f),
            new Vector3(-0.7f, 1.1f, -0.47f)
        ]);
        SnappointHelper.AddSnappoints("piece_chest_private", [
            new Vector3(0.45f, 0f, 0.25f),
            new Vector3(0.45f, 0f, -0.25f),
            new Vector3(-0.45f, 0f, 0.25f),
            new Vector3(-0.45f, 0f, -0.25f),
            new Vector3(0.36f, 0.55f, 0.23f),
            new Vector3(0.36f, 0.55f, -0.23f),
            new Vector3(-0.36f, 0.55f, 0.23f),
            new Vector3(-0.36f, 0.55f, -0.23f)
        ]);
        SnappointHelper.AddSnappoints("piece_chest_blackmetal", [
            new Vector3(1.1f, 0f, 0.7f),
            new Vector3(1.1f, 0f, -0.7f),
            new Vector3(-1.1f, 0f, 0.7f),
            new Vector3(-1.1f, 0f, -0.7f),
            new Vector3(0.85f, 1.07f, 0.6f),
            new Vector3(0.85f, 1.07f, -0.6f),
            new Vector3(-0.85f, 1.07f, 0.6f),
            new Vector3(-0.85f, 1.07f, -0.6f)
        ]);
        SnappointHelper.AddSnappoints("rk_crate", [
            new Vector3(0.3f, 0f, 0.3f),
            new Vector3(0.3f, 0f, -0.3f),
            new Vector3(-0.3f, 0f, 0.3f),
            new Vector3(-0.3f, 0f, -0.3f),
            new Vector3(0.3f, 0.62f, 0.3f),
            new Vector3(0.3f, 0.62f, -0.3f),
            new Vector3(-0.3f, 0.62f, 0.3f),
            new Vector3(-0.3f, 0.62f, -0.3f)
        ]);
        SnappointHelper.AddSnappoints("rk_crate2", [
            new Vector3(0.5f, -0.018f, 0.5f),
            new Vector3(0.5f, -0.018f, -0.5f),
            new Vector3(-0.5f, -0.018f, 0.5f),
            new Vector3(-0.5f, -0.018f, -0.5f),
            new Vector3(0.5f, 1.042f, 0.5f),
            new Vector3(0.5f, 1.042f, -0.5f),
            new Vector3(-0.5f, 1.042f, 0.5f),
            new Vector3(-0.5f, 1.042f, -0.5f)
        ]);
        SnappointHelper.AddSnappoints("piece_chest_treasure", [
            new Vector3(0.55f, -0.018f, 0.36f),
            new Vector3(0.55f, -0.018f, -0.36f),
            new Vector3(-0.55f, -0.018f, 0.36f),
            new Vector3(-0.55f, -0.018f, -0.36f),
            new Vector3(0.4f, 0.55f, 0.3f),
            new Vector3(0.4f, 0.55f, -0.3f),
            new Vector3(-0.4f, 0.55f, 0.3f),
            new Vector3(-0.4f, 0.55f, -0.3f)
        ]);
        SnappointHelper.AddSnappoints("MS_piece_chest_branch", [
            new Vector3(-0.55f, 0.05f, 0.74f),
            new Vector3(-0.55f, 0.05f, -0.28f),
            new Vector3(-0.55f, 0.8f, 0.74f),
            new Vector3(-0.55f, 0.8f, -0.28f),
            new Vector3(1f, 0.05f, 0.74f),
            new Vector3(1f, 0.05f, -0.28f),
            new Vector3(1f, 0.8f, 0.74f),
            new Vector3(1f, 0.8f, -0.28f)
        ]);
        SnappointHelper.AddSnappoints("MS_piece_chest_redstone", [
            new Vector3(1f, 0f, 0.4f),
            new Vector3(1f, 0f, -0.4f),
            new Vector3(-1f, 0f, 0.4f),
            new Vector3(-1f, 0f, -0.4f),
            new Vector3(0.85f, 0.65f, 0.4f),
            new Vector3(0.85f, 0.65f, -0.4f),
            new Vector3(-0.85f, 0.65f, 0.4f),
            new Vector3(-0.85f, 0.65f, -0.4f)
        ]);
        SnappointHelper.AddSnappoints("piece_appleBox", [
            new Vector3(-0.34f, -0.87f, 0.68f),
            new Vector3(0.45f, -0.87f, 0.68f),
            new Vector3(-0.34f, -0.87f, -0.53f),
            new Vector3(0.45f, -0.87f, -0.53f),
            new Vector3(-0.34f, -0.3f, 0.68f),
            new Vector3(0.45f, -0.3f, 0.68f),
            new Vector3(-0.34f, -0.3f, -0.53f),
            new Vector3(0.45f, -0.3f, -0.53f)
        ]);
        SnappointHelper.AddSnappoints("piece_garlicBox", [
            new Vector3(-0.34f, -0.87f, 0.68f),
            new Vector3(0.45f, -0.87f, 0.68f),
            new Vector3(-0.34f, -0.87f, -0.53f),
            new Vector3(0.45f, -0.87f, -0.53f),
            new Vector3(-0.34f, -0.3f, 0.68f),
            new Vector3(0.45f, -0.3f, 0.68f),
            new Vector3(-0.34f, -0.3f, -0.53f),
            new Vector3(0.45f, -0.3f, -0.53f)
        ]);
        SnappointHelper.AddSnappoints("piece_pepperBox", [
            new Vector3(-0.34f, -0.87f, 0.68f),
            new Vector3(0.45f, -0.87f, 0.68f),
            new Vector3(-0.34f, -0.87f, -0.53f),
            new Vector3(0.45f, -0.87f, -0.53f),
            new Vector3(-0.34f, -0.3f, 0.68f),
            new Vector3(0.45f, -0.3f, 0.68f),
            new Vector3(-0.34f, -0.3f, -0.53f),
            new Vector3(0.45f, -0.3f, -0.53f)
        ]);
        SnappointHelper.AddSnappoints("piece_potatoBox", [
            new Vector3(-0.34f, -0.87f, 0.68f),
            new Vector3(0.45f, -0.87f, 0.68f),
            new Vector3(-0.34f, -0.87f, -0.53f),
            new Vector3(0.45f, -0.87f, -0.53f),
            new Vector3(-0.34f, -0.3f, 0.68f),
            new Vector3(0.45f, -0.3f, 0.68f),
            new Vector3(-0.34f, -0.3f, -0.53f),
            new Vector3(0.45f, -0.3f, -0.53f)
        ]);
        SnappointHelper.AddSnappoints("piece_saltBox", [
            new Vector3(-0.34f, -0.87f, 0.68f),
            new Vector3(0.45f, -0.87f, 0.68f),
            new Vector3(-0.34f, -0.87f, -0.53f),
            new Vector3(0.45f, -0.87f, -0.53f),
            new Vector3(-0.34f, -0.3f, 0.68f),
            new Vector3(0.45f, -0.3f, 0.68f),
            new Vector3(-0.34f, -0.3f, -0.53f),
            new Vector3(0.45f, -0.3f, -0.53f)
        ]);
        SnappointHelper.AddSnappoints("piece_tomatoBox", [
            new Vector3(-0.34f, -0.87f, 0.68f),
            new Vector3(0.45f, -0.87f, 0.68f),
            new Vector3(-0.34f, -0.87f, -0.53f),
            new Vector3(0.45f, -0.87f, -0.53f),
            new Vector3(-0.34f, -0.3f, 0.68f),
            new Vector3(0.45f, -0.3f, 0.68f),
            new Vector3(-0.34f, -0.3f, -0.53f),
            new Vector3(0.45f, -0.3f, -0.53f)
        ]);
        SnappointHelper.AddSnappoints("piece_cultivatedGround", [
            new Vector3(-1.5f, 0f, 2.3f),
            new Vector3(1.5f, 0f, 2.3f),
            new Vector3(-1.5f, 0f, -2.26f),
            new Vector3(1.5f, 0f, -2.26f),
            new Vector3(-0.34f, 1f, 2.3f),
            new Vector3(0.45f, 1f, 2.3f),
            new Vector3(-0.34f, 1f, -2.26f),
            new Vector3(0.45f, 1f, -2.26f)
        ]);
        SnappointHelper.AddSnappoints("piece_cultivatedGround_big", [
            new Vector3(-2.9f, 0f, 2.3f),
            new Vector3(1.5f, 0f, 2.3f),
            new Vector3(-2.9f, 0f, -2.26f),
            new Vector3(1.5f, 0f, -2.26f),
            new Vector3(-2.9f, 1f, 2.3f),
            new Vector3(0.45f, 1f, 2.3f),
            new Vector3(-2.9f, 1f, -2.26f),
            new Vector3(0.45f, 1f, -2.26f)
        ]);
        SnappointHelper.AddSnappoints("piece_cultivatedGround_small", [
            new Vector3(-0.15f, 0f, 2.3f),
            new Vector3(1.5f, 0f, 2.3f),
            new Vector3(-0.15f, 0f, -2.26f),
            new Vector3(1.5f, 0f, -2.26f),
            new Vector3(-0.15f, 1f, 2.3f),
            new Vector3(0.45f, 1f, 2.3f),
            new Vector3(-0.15f, 1f, -2.26f),
            new Vector3(0.45f, 1f, -2.26f)
        ]);
        SnappointHelper.AddSnappoints("piece_cultivatedGround_small_small", [
            new Vector3(-0.15f, 0f, 0.175f),
            new Vector3(1.5f, 0f, 2.3f),
            new Vector3(-0.15f, 0f, 0.175f),
            new Vector3(1.5f, 0f, -2.26f),
            new Vector3(-0.15f, 1f, 0.175f),
            new Vector3(1.5f, 1f, 0.175f),
            new Vector3(-0.15f, 1f, -2.26f),
            new Vector3(1.5f, 1f, -2.26f)
        ]);
    }
}