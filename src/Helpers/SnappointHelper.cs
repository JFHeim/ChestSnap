using ChestSnap.Config;
using ChestSnap.DebugVisuals;

namespace ChestSnap.Helpers;

public static class SnappointHelper
{
    // TODO: do not block the thread, make the snappoints update process gradual and cancelable wia enumerator
    public static void RecreateSnappoints()
    {
        if(Helper.IsMainScene() == false) return;
        if(Helper.IsDedicatedServer()) return;
        if(!ZNetScene.instance) return;

        var snappointLookup = ConfigsContainer.SnappointLookup;
        foreach (var pair in snappointLookup)
        {
            var prefabName = pair.Key;
            var points = pair.Value;

            var prefab = ZNetScene.instance.GetPrefab(prefabName);
            if (!prefab)
            {
                // Log.Warning($"Prefab with a name '{prefabName}' not found");
                continue;
            }

            RecreateSnappointsInObject(prefab, points);
        }

        var snappointLookupByHash = snappointLookup
            .ToDictionary(
                x => x.Key.GetStableHashCode(),
                x => x.Value);

        var objectsOnScene = ZNetScene.instance.m_instances
            // .Where(x => snappointLookupByHash.ContainsKey(x.Key.m_prefab))
            .GroupBy(x=> x.Key.m_prefab)
            .ToDictionary(
                x => x.Key,
                x=> x.Select(g=> g.Value.gameObject).ToArray());

        foreach (var pair in objectsOnScene)
        {
            var prefabHash = pair.Key;
            var gameObjects = pair.Value;

            foreach (var instance in gameObjects)
            {
                if(!instance) continue;

                var transform = instance.transform;
                if (DebugOverlayManager.Instance)
                {
                    DebugOverlayManager.Instance.UnregisterPoints(transform);
                    DebugOverlayManager.Instance.UnregisterObject(transform);
                }

                if (!snappointLookupByHash.TryGetValue(prefabHash, out var points)) continue;

                RecreateSnappointsInObject(instance, points);
                if (DebugOverlayManager.Instance)
                {
                    DebugOverlayManager.Instance.RegisterObject(transform);
                    DebugOverlayManager.Instance.RegisterSnapPoints(transform);
                }
            }
        }

        foreach (var pair in snappointLookupByHash)
        {
            var prefabHash = pair.Key;
            var points = pair.Value;

            if (!objectsOnScene.TryGetValue(prefabHash, out var gameObjects)) continue;

            foreach (var instance in gameObjects)
            {
                if(!instance) continue;
                RecreateSnappointsInObject(instance, points);
                if (DebugOverlayManager.Instance)
                {
                    var transform = instance.transform;
                    DebugOverlayManager.Instance.UnregisterPoints(transform);
                    DebugOverlayManager.Instance.UnregisterObject(transform);
                    DebugOverlayManager.Instance.RegisterObject(transform);
                    DebugOverlayManager.Instance.RegisterSnapPoints(transform);
                }
            }
        }
    }

    public static void RecreateSnappointsInObject(GameObject obj, Vector3[] points)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            var child = obj.transform.GetChild(i);
            if (child.CompareTag("snappoint") || child.name == "_snappoint")
            {
                child.name += " [Destroyed]";
                Destroy(child.gameObject);
            }
        }

        foreach (var pos in points)
            new GameObject("_snappoint")
            {
                transform =
                {
                    parent = obj.transform,
                    localPosition = pos
                },
                tag = "snappoint"
            }.SetActive(false);

        // Log.Info($"{points.Length} snap points added to " + obj);
    }
}