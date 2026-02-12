namespace ChestSnap;

public static class SnappointHelper
{
    public static void AddSnappoints(string name, Vector3[] points)
    {
        var prefab = ZNetScene.instance.GetPrefab(name);
        if (!prefab) return;

        foreach (var pos in points)
            new GameObject("_snappoint")
            {
                transform =
                {
                    parent = prefab.transform,
                    localPosition = pos
                },
                tag = "snappoint"
            }.SetActive(false);

        Log.Info("Snappoints added to " + name);
    }
}