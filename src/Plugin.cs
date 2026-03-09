using ChestSnap.Config;
using ChestSnap.DebugVisuals;
using ChestSnap.Helpers;

namespace ChestSnap;

[BepInEx.BepInPlugin(Consts.ModGuid, Consts.ModName, Consts.ModVersion)]
public class Plugin : BepInEx.BaseUnityPlugin
{
    public static Plugin? Instance { get; private set; }
    private Harmony _harmony = null!;

    private void Awake()
    {
        Instance = this;
        Log.Initialize(this);

        if (Helper.IsDedicatedServer())
        {
            Log.Error($"{Consts.ModGuid} is intended to be ran only on clients, you as a server admin shall not install it an a dedicated server");
            return;
        }

        _harmony = new Harmony(Consts.ModGuid);
        _harmony.PatchAll();
        ConfigsContainer.InitializeConfiguration(this);

        var managerGo = new GameObject("DebugOverlay");
        managerGo.AddComponent<DebugOverlayManager>();
    }
}
