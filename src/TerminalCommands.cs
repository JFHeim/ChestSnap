using ChestSnap.Config;
using ChestSnap.DebugVisuals;
using ChestSnap.Helpers;

namespace ChestSnap;

[HarmonyPatch(typeof(Terminal)), HarmonyWrapSafe]
file static class TerminalCommands
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Terminal.InitTerminal))]
    private static void Postfix()
    {
        _ = new Terminal.ConsoleCommand("generate_snap_points",
            "[onlyBottom:bool](optional) - Generate snap points for a building piece you are currently hovering with hammer. This points will be added to the config",
            args =>
            {
                if (DebugOverlayManager.Instance == null)
                {
                    Console.instance.AddString("DebugOverlayManager is not initialized yet, this should never happen");
                    Log.Error("[generate_snap_points] DebugOverlayManager is not initialized yet, this should never happen");
                    return false;
                }

                bool onlyBottom = false;
                if (args.Length >= 2)
                {
                    var boolStr = args[1];
                    if (string.IsNullOrEmpty(boolStr)) onlyBottom = true;
                    else
                    {
                        if (!bool.TryParse(boolStr, out var onlyBottom_parse))
                        {
                            Console.instance.AddString("Invalid argument, onlyBottom excepts only 'true' and false'");
                            return false;
                        }

                        onlyBottom = onlyBottom_parse;
                    }
                }

                var piece = Player.m_localPlayer?.m_hoveringPiece?.gameObject;
                if (piece == null)
                {
                    Console.instance.AddString("You are not hovering with hammer any piece right now");
                    return false;
                }

                var pieceTransform = piece.transform;
                var bounds = BoundsComputer.ComputeBounds(pieceTransform);
                var snapPointsCoords = BoundsComputer.FindOuterCorners(bounds, onlyBottom);
                snapPointsCoords = snapPointsCoords
                    .Select(x => pieceTransform.InverseTransformPoint(x))
                    .Select(x => x.Round(1))
                    .ToList();

                Dictionary<string, Vector3[]> newSnappointLookup = new (ConfigsContainer.SnappointLookup);
                newSnappointLookup.Remove(Utils.GetPrefabName(piece));
                newSnappointLookup.Add(Utils.GetPrefabName(piece), snapPointsCoords.ToArray());
                ConfigsContainer.UpdateSnappointLookup(newSnappointLookup);

                return true;
            }, true);
    }
}