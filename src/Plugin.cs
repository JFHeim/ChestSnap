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
            Log.Error($"{Consts.ModGuid} is intended to be run only on clients; as a server admin, you should not install it on a dedicated server");
            return;
        }

        _harmony = new Harmony(Consts.ModGuid);
        _harmony.PatchAll();
        ConfigsContainer.InitializeConfiguration(this);

        var debugOverlayGo = new GameObject("DebugOverlay");
        debugOverlayGo.AddComponent<DebugOverlayManager>();
        DontDestroyOnLoad(debugOverlayGo);
    }
}
