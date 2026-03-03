using ChestSnap.Config;
using ChestSnap.DebugVisuals;

namespace ChestSnap.Helpers;

public static class SnappointHelper
{
    private static Coroutine? _activeCoroutine;

    public static void RecreateSnappoints(string? onlyForAPrefab = null)
    {
        if (!Plugin.Instance || !Helper.IsMainScene() || Helper.IsDedicatedServer() || !ZNetScene.instance) return;

        if (_activeCoroutine != null) Plugin.Instance.StopCoroutine(_activeCoroutine);
        _activeCoroutine = Plugin.Instance.StartCoroutine(RecreateSnappointsRoutine(onlyForAPrefab));
    }

    private static IEnumerator RecreateSnappointsRoutine(string? onlyForAPrefab = null)
    {
        var snappointLookup = ConfigsContainer.SnappointLookup;
        if (!string.IsNullOrEmpty(onlyForAPrefab))
            snappointLookup = snappointLookup
                .Where(x => x.Key == onlyForAPrefab)
                .ToDictionary(x => x.Key, x => x.Value);

        foreach (var pair in snappointLookup)
        {
            if (!ZNetScene.instance) yield break;

            var prefab = ZNetScene.instance.GetPrefab(pair.Key);
            if (prefab) RecreateSnappointsInObject(prefab, pair.Value);

            yield return null;
        }

        if (!ZNetScene.instance) yield break;

        var snappointLookupByHash = snappointLookup.ToDictionary(
            x => x.Key.GetStableHashCode(),
            x => x.Value);

        var onlyForAPrefabHash = onlyForAPrefab?.GetStableHashCode();
        var objectsOnScene = ZNetScene.instance.m_instances
            .Where(x => string.IsNullOrEmpty(onlyForAPrefab) || x.Key.m_prefab == onlyForAPrefabHash)
            .GroupBy(x => x.Key.m_prefab)
            .ToDictionary(
                x => x.Key,
                x => x.Select(g => g.Value.gameObject).ToArray()
            );

        foreach (var pair in objectsOnScene)
        {
            if (!ZNetScene.instance) yield break;

            var prefabHash = pair.Key;
            if (!snappointLookupByHash.TryGetValue(prefabHash, out var points))
            {
                if (DebugOverlayManager.Instance)
                    foreach (var instance in pair.Value)
                    {
                        var transform = instance.transform;
                        DebugOverlayManager.Instance.UnregisterPoints(transform);
                        DebugOverlayManager.Instance.UnregisterObject(transform);
                    }
                continue;
            }

            foreach (var instance in pair.Value)
            {
                if (!instance) continue;

                var transform = instance.transform;
                if (DebugOverlayManager.Instance)
                {
                    DebugOverlayManager.Instance.UnregisterPoints(transform);
                    DebugOverlayManager.Instance.UnregisterObject(transform);
                }

                RecreateSnappointsInObject(instance, points);

                if (DebugOverlayManager.Instance)
                {
                    DebugOverlayManager.Instance.RegisterObject(transform);
                    DebugOverlayManager.Instance.RegisterSnapPoints(transform);
                }

                yield return null;
            }
        }

        _activeCoroutine = null;
    }

    public static void RecreateSnappointsInObject(GameObject obj, Vector3[] points)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            var child = obj.transform.GetChild(i);
            if (child.CompareTag("snappoint"))
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
    }
}