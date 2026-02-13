namespace ChestSnap.Helpers;

public static class SnappointHelper
{
    public static void RecreateSnappointsInObject(GameObject obj, Vector3[] points)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            var child = obj.transform.GetChild(i);
            if(child.tag == "snappoint" || child.name == "_snappoint")
                Destroy(child.gameObject);
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

        Log.Info($"{points.Length} snap points added to " + obj);
    }

    public static void RecreateSnappointsInPrefab(string name, Vector3[] points)
    {
        var prefab = ZNetScene.instance.GetPrefab(name);
        if (!prefab) return;

        for (int i = 0; i < prefab.transform.childCount; i++)
        {
            var child = prefab.transform.GetChild(i);
            if(child.tag == "snappoint" || child.name == "_snappoint")
                Destroy(child.gameObject);
        }

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

        Log.Info($"{points.Length} snap points added to " + prefab);
    }

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