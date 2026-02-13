using ChestSnap.Helpers;

namespace ChestSnap.DebugVisuals;

public static class BoundsComputer
{
    [System.Serializable]
    public class Config
    {
        public float minSize = 0.1f;
        public float mergeThreshold = 0.7f;
        public int maxBounds = 16;
        public bool removeContained = true;
        public bool mergeOverlapping = true;
        public string[] ignoreTags = [];

        public static Config Default => new Config();
    }

    public static List<WearNTear.BoundData> ComputeBounds(Transform obj, Config? config = null)
    {
        config ??= Config.Default;

        var colliders = obj.GetComponentsInChildren<Collider>(true);
        var bounds = new List<WearNTear.BoundData>();

        foreach (Collider? collider in colliders)
        {
            if(collider == null) continue;
            if (!ShouldProcess(collider, config)) continue;

            if (TryCreateBound(collider, out var bound))
            {
                if (bound.m_size.sqrMagnitude < config.minSize * config.minSize) continue;
                bounds.Add(bound);
            }
        }

        if (config.removeContained)
            RemoveContainedBounds(bounds);

        if (config.mergeOverlapping)
            MergeOverlappingBounds(bounds, config.mergeThreshold);

        ClampBoundsCount(bounds, config.maxBounds, config.mergeThreshold);

        return bounds;
    }

    private static bool ShouldProcess(Collider collider, Config config)
    {
        if (!collider.enabled || !collider.gameObject.activeInHierarchy) return false;
        if (collider.isTrigger) return false;

        foreach (string tag in config.ignoreTags)
        {
            if (!collider.CompareTag(tag)) continue;
            return false;
        }

        return true;
    }

    private static bool TryCreateBound(Collider collider, out WearNTear.BoundData bound)
    {
        bound = default;
        var t = collider.transform;
        Vector3 scale = t.lossyScale;

        switch (collider)
        {
            case BoxCollider box:
                bound = CreateBoxBound(box, t, scale);
                return true;

            case SphereCollider sphere:
                bound = CreateSphereBound(sphere, t, scale);
                return true;

            case CapsuleCollider capsule:
                bound = CreateCapsuleBound(capsule, t, scale);
                return true;

            case MeshCollider mesh when mesh.sharedMesh != null:
                bound = CreateMeshBound(mesh, t, scale);
                return true;

            default:
                return false;
        }
    }

    #region Bound Creation

    private static WearNTear.BoundData CreateBoxBound(BoxCollider box, Transform t, Vector3 scale) => new()
    {
        m_pos = t.TransformPoint(box.center),
        m_rot = t.rotation,
        m_size = Vector3.Scale(box.size, scale)
    };

    private static WearNTear.BoundData CreateSphereBound(SphereCollider sphere, Transform t, Vector3 scale)
    {
        float maxScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        float d = sphere.radius * maxScale * 2f;

        return new WearNTear.BoundData
        {
            m_pos = t.TransformPoint(sphere.center),
            m_rot = Quaternion.identity,
            m_size = new Vector3(d, d, d)
        };
    }

    private static WearNTear.BoundData CreateCapsuleBound(CapsuleCollider capsule, Transform t, Vector3 scale)
    {
        (float maxRadScale, float heightScale) = GetCapsuleScales(capsule.direction, scale);

        float r = capsule.radius * maxRadScale;
        float h = Mathf.Max(capsule.height * heightScale, r * 2f);
        float d = r * 2f;

        Vector3 size = capsule.direction switch
        {
            0 => new Vector3(h, d, d),
            2 => new Vector3(d, d, h),
            _ => new Vector3(d, h, d)
        };

        return new WearNTear.BoundData
        {
            m_pos = t.TransformPoint(capsule.center),
            m_rot = t.rotation,
            m_size = size
        };
    }

    private static (float maxRadScale, float heightScale) GetCapsuleScales(int direction, Vector3 scale) => direction switch
    {
        0 => (Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)), Mathf.Abs(scale.x)),
        2 => (Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y)), Mathf.Abs(scale.z)),
        _ => (Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z)), Mathf.Abs(scale.y))
    };

    private static WearNTear.BoundData CreateMeshBound(MeshCollider mesh, Transform t, Vector3 scale)
    {
        Bounds mb = mesh.sharedMesh.bounds;

        return new WearNTear.BoundData
        {
            m_pos = t.TransformPoint(mb.center),
            m_rot = t.rotation,
            m_size = Vector3.Scale(mb.size, scale)
        };
    }

    #endregion

    #region Filtering & Merging

    private static void RemoveContainedBounds(List<WearNTear.BoundData> bounds)
    {
        for (int i = bounds.Count - 1; i >= 0; i--)
        {
            for (int j = 0; j < bounds.Count; j++)
            {
                if (i == j) continue;
                if (IsContained(bounds[i], bounds[j]))
                {
                    bounds.RemoveAt(i);
                    break;
                }
            }
        }
    }

    private static bool IsContained(WearNTear.BoundData inner, WearNTear.BoundData outer)
    {
        float outerRadius = outer.m_size.magnitude * 0.5f;
        float innerRadius = inner.m_size.magnitude * 0.5f;
        float distance = (outer.m_pos - inner.m_pos).magnitude;

        return distance + innerRadius < outerRadius;
    }

    private static void MergeOverlappingBounds(List<WearNTear.BoundData> bounds, float threshold)
    {
        bool merged;
        do
        {
            merged = false;
            for (int i = 0; i < bounds.Count && !merged; i++)
            for (int j = i + 1; j < bounds.Count && !merged; j++)
                if (OverlapRatio(bounds[i], bounds[j]) > threshold)
                {
                    bounds[i] = MergeBounds(bounds[i], bounds[j]);
                    bounds.RemoveAt(j);
                    merged = true;
                }
        } while (merged);
    }

    private static float OverlapRatio(WearNTear.BoundData a, WearNTear.BoundData b)
    {
        Vector3 minA = a.m_pos - a.m_size * 0.5f;
        Vector3 maxA = a.m_pos + a.m_size * 0.5f;
        Vector3 minB = b.m_pos - b.m_size * 0.5f;
        Vector3 maxB = b.m_pos + b.m_size * 0.5f;

        Vector3 overlap = Vector3.Max(
            Vector3.zero,
            Vector3.Min(maxA, maxB) - Vector3.Max(minA, minB));

        float overlapVolume = overlap.x * overlap.y * overlap.z;
        float smallerVolume = Mathf.Min(
            a.m_size.x * a.m_size.y * a.m_size.z,
            b.m_size.x * b.m_size.y * b.m_size.z);

        return smallerVolume > 0 ? overlapVolume / smallerVolume : 0f;
    }

    private static WearNTear.BoundData MergeBounds(WearNTear.BoundData a, WearNTear.BoundData b)
    {
        Vector3 min = Vector3.Min(a.m_pos - a.m_size * 0.5f, b.m_pos - b.m_size * 0.5f);
        Vector3 max = Vector3.Max(a.m_pos + a.m_size * 0.5f, b.m_pos + b.m_size * 0.5f);

        return new WearNTear.BoundData
        {
            m_pos = (min + max) * 0.5f,
            m_rot = Quaternion.Slerp(a.m_rot, b.m_rot, 0.5f),
            m_size = max - min
        };
    }

    private static void ClampBoundsCount(List<WearNTear.BoundData> bounds, int maxCount, float mergeThreshold)
    {
        float adjustedThreshold = mergeThreshold;

        while (bounds.Count > maxCount)
        {
            adjustedThreshold = Mathf.Max(0f, adjustedThreshold - 0.1f);

            MergeOverlappingBounds(bounds, adjustedThreshold);

            if (adjustedThreshold <= 0f)
            {
                RemoveSmallestBounds(bounds, bounds.Count - maxCount);
                break;
            }
        }
    }

    private static void RemoveSmallestBounds(List<WearNTear.BoundData> bounds, int count)
    {
        bounds.Sort((a, b) => a.m_size.sqrMagnitude.CompareTo(b.m_size.sqrMagnitude));
        bounds.RemoveRange(0, count);
    }

    #endregion

    #region Finding corners

    public static List<Vector3> FindOuterCorners(List<WearNTear.BoundData>? bounds, bool onlyBottom = false)
    {
        var result = new List<Vector3>();

        if (bounds == null || bounds.Count == 0) return result;

        // Один bound — просто его углы
        if (bounds.Count == 1) return GetBoundCorners(bounds[0], onlyBottom);

        // Собираем все углы всех bounds
        var allCorners = new List<Vector3>();
        foreach (var bound in bounds)
            allCorners.AddRange(GetBoundCorners(bound, onlyBottom: false));

        // Фильтруем: убираем точки внутри других bounds
        var outerCorners = new List<Vector3>();
        foreach (var corner in allCorners)
        {
            if (IsInsideAnyBound(corner, bounds)) continue;
            outerCorners.Add(corner);
        }

        if (onlyBottom) outerCorners = FilterBottomCorners(outerCorners);

        // Convex hull для получения внешнего контура
        return ComputeConvexHull(outerCorners, onlyBottom);
    }

    private static List<Vector3> GetBoundCorners(WearNTear.BoundData bound, bool onlyBottom)
    {
        var corners = new List<Vector3>(8);

        Vector3 halfSize = bound.m_size * 0.5f;
        Vector3 center = bound.m_pos;

        // Локальные смещения углов
        Vector3[] offsets =
        [
            new(-1, -1, -1),
            new(-1, -1, 1),
            new(-1, 1, -1),
            new(-1, 1, 1),
            new(1, -1, -1),
            new(1, -1, 1),
            new(1, 1, -1),
            new(1, 1, 1)
        ];

        foreach (var offset in offsets)
        {
            if (onlyBottom && offset.y > 0) continue;

            var localCorner = Vector3.Scale(halfSize, offset);
            var worldCorner = center + bound.m_rot * localCorner;
            corners.Add(worldCorner);
        }

        return corners;
    }

    private static bool IsInsideAnyBound(Vector3 point, List<WearNTear.BoundData> bounds)
    {
        const float EPSILON = 0.001f;

        foreach (var bound in bounds)
        {
            // Трансформируем точку в локальное пространство bound
            Vector3 localPoint = Quaternion.Inverse(bound.m_rot) * (point - bound.m_pos);
            Vector3 halfSize = bound.m_size * 0.5f;

            if (Mathf.Abs(localPoint.x) < halfSize.x - EPSILON &&
                Mathf.Abs(localPoint.y) < halfSize.y - EPSILON &&
                Mathf.Abs(localPoint.z) < halfSize.z - EPSILON)
                return true;
        }

        return false;
    }

    private static List<Vector3> FilterBottomCorners(List<Vector3> corners)
    {
        if (corners.Count == 0) return corners;

        float minY = corners.Min(c => c.y);
        const float TOLERANCE = 0.05f;

        return corners.Where(c => c.y <= minY + TOLERANCE).ToList();
    }

    private static List<Vector3> ComputeConvexHull(List<Vector3> points, bool is2D)
    {
        if (points.Count <= 3) return points;
        return is2D ? ConvexHull2D(points) : ConvexHull3D(points);
    }

    private static List<Vector3> ConvexHull2D(List<Vector3> points)
    {
        if (points.Count <= 3) return new List<Vector3>(points);

        // Алгоритм Грэхэма для XZ плоскости
        var sorted = points.OrderBy(p => p.x).ThenBy(p => p.z).ToList();

        var lower = new List<Vector3>();
        foreach (var p in sorted)
        {
            while (lower.Count >= 2 && Cross2D(lower[lower.Count - 2], lower[lower.Count - 1], p) <= 0)
                lower.RemoveAt(lower.Count - 1);
            lower.Add(p);
        }

        var upper = new List<Vector3>();
        for (int i = sorted.Count - 1; i >= 0; i--)
        {
            var p = sorted[i];
            while (upper.Count >= 2 && Cross2D(upper[upper.Count - 2], upper[upper.Count - 1], p) <= 0)
                upper.RemoveAt(upper.Count - 1);
            upper.Add(p);
        }

        // Объединяем, убирая дубликаты
        lower.RemoveAt(lower.Count - 1);
        upper.RemoveAt(upper.Count - 1);

        lower.AddRange(upper);
        return lower;
    }

    private static float Cross2D(Vector3 o, Vector3 a, Vector3 b)
    {
        // Векторное произведение в плоскости XZ
        return (a.x - o.x) * (b.z - o.z) - (a.z - o.z) * (b.x - o.x);
    }

    private static List<Vector3> ConvexHull3D(List<Vector3> points)
    {
        // Упрощённый подход: разбиваем по Y-уровням и делаем 2D hull для каждого
        const float yTolerance = 0.1f;

        var result = new List<Vector3>();

        // Группируем по высоте
        var yLevels = points
            .Select(p => Mathf.Round(p.y / yTolerance) * yTolerance)
            .Distinct()
            .OrderBy(y => y)
            .ToList();

        foreach (var y in yLevels)
        {
            var levelPoints = points
                .Where(p => Mathf.Abs(p.y - y) < yTolerance)
                .ToList();

            result.AddRange(levelPoints.Count > 2 ? ConvexHull2D(levelPoints) : levelPoints);
        }

        // Убираем дубликаты
        return result.Distinct(new Vector3EqualityComparer(0.001f)).ToList();
    }

    private class Vector3EqualityComparer : IEqualityComparer<Vector3>
    {
        private readonly float _epsilon;

        public Vector3EqualityComparer(float epsilon) => _epsilon = epsilon;

        public bool Equals(Vector3 a, Vector3 b) => Vector3.Distance(a, b) < _epsilon;

        public int GetHashCode(Vector3 obj) => obj.GetHashCode();
    }

    #endregion
}