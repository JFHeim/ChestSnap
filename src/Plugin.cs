using ChestSnap.Config;
using ChestSnap.DebugVisuals;

namespace ChestSnap;

[BepInEx.BepInPlugin(Consts.ModGuid, Consts.ModName, Consts.ModVersion)]
public class Plugin : BepInEx.BaseUnityPlugin
{
    public static Plugin? Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        Log.InitializeConfiguration(this);
        new Harmony(Consts.ModGuid).PatchAll();
        ConfigsContainer.InitializeConfiguration(this);

        var managerGo = new GameObject("DebugOverlay");
        managerGo.AddComponent<DebugOverlayManager>();
    }
}
