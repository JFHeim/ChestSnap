using System.Globalization;
using ChestSnap.Config;
using ChestSnap.Helpers;
using TMPro;
using UnityEngine.UI;

namespace ChestSnap.DebugVisuals;

public class DebugOverlayManager : MonoBehaviour
{
    public static DebugOverlayManager? Instance { get; private set; }

    [Header("Rendering")]
    public float lineWidth = 1.5f;
    public float pointRadius = 5f;
    public float sizeModifier = 0.5f;
    public int pointSegments = 4;

    [Header("Labels")]
    public int labelFontSize = 10;
    public Color labelTextColor = Color.white;
    public Color labelBackgroundColor = new Color(0f, 0f, 0f, 0.7f);
    public Vector2 labelPadding = new Vector2(4f, 2f);
    public Vector2 labelOffset = new Vector2(6f, 6f);
    public string labelFormat = "0.##";
    public TMP_FontAsset? labelFont;

    private Canvas _canvas;
    private OverlayGraphic _graphic;
    private Camera? _cam;

    private readonly List<TrackedObject> _objects = [];
    private readonly List<TrackedPoint> _points = [];

    private readonly List<LabelWidget> _labelPool = [];
    private int _labelsUsed;

    private struct LabelWidget
    {
        public GameObject root;
        public TextMeshProUGUI text;
        public Image background;
        public RectTransform rt;
    }

    private struct TrackedObject
    {
        public Transform transform;
        public List<WearNTear.BoundData>? bounds;
    }

    private struct TrackedPoint
    {
        public Transform transform;
        public Vector3 localPos;
        public bool forceShow;
    }

    private static readonly int[] _edgeA = [0, 1, 2, 3, 4, 5, 6, 7, 0, 1, 2, 3];

    private static readonly int[] _edgeB = [1, 2, 3, 0, 5, 6, 7, 4, 4, 5, 6, 7];

    private static readonly Vector3[] _cornerSigns =
    [
        new(-1, -1, -1),
        new(+1, -1, -1),
        new(+1, +1, -1),
        new(-1, +1, -1),
        new(-1, -1, +1),
        new(+1, -1, +1),
        new(+1, +1, +1),
        new(-1, +1, +1),
    ];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildCanvas();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void LateUpdate()
    {
        if (!Helper.IsMainScene()) return;

        if (_cam == null || !_cam.isActiveAndEnabled)
            _cam = Camera.main;

        _graphic.Clear();
        BeginLabels();

        if (ConfigsContainer.ShowDebugVisuals == DebugVisualsDisplayMode.Hidden  || _cam == null)
        {
            EndLabels();
            _graphic.SetVerticesDirty();
            return;
        }

        CleanupNulls();
        DrawTrackedBoxes();
        DrawSnapPoints();

        EndLabels();
        _graphic.SetVerticesDirty();
    }

    // ── Public API ────────────────────────────────────────────

    public void RegisterObject(Transform target)
    {
        if (target == null) return;
        _objects.Add(new TrackedObject
        {
            transform = target,
            bounds = BoundsComputer.ComputeBounds(target)
        });
    }

    public void UnregisterObject(Transform target)
    {
        if (target == null) return;
        _objects.RemoveAll(o => o.transform == target);
    }

    public void RegisterPoint(Transform parent, Vector3 localPos, bool forceShow = false)
    {
        if (parent == null) return;
        _points.Add(new TrackedPoint
        {
            transform = parent,
            localPos = localPos,
            forceShow = forceShow
        });
    }

    public void RegisterSnapPoints(Transform parent)
    {
        if (parent == null) return;

        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if(child.tag == "snappoint" || child.name == "_snappoint")
            {
                Log.Info($"RegisterSnapPoints - on '{parent.name}' at {child.localPosition}");
                _points.Add(new TrackedPoint
                {
                    transform = parent,
                    localPos = child.localPosition
                });}
        }
    }

    public void UnregisterPoints(Transform parent)
    {
        _points.RemoveAll(p => p.transform == parent);
    }

    public void ClearAll()
    {
        _objects.Clear();
        _points.Clear();
    }

    private void BuildCanvas()
    {
        var canvasGo = new GameObject("DebugOverlay_Canvas");
        canvasGo.transform.SetParent(transform, false);

        _canvas = canvasGo.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 32767;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var rendererGo = new GameObject("Graphic");
        rendererGo.transform.SetParent(canvasGo.transform, false);

        var rt = rendererGo.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        _graphic = rendererGo.AddComponent<OverlayGraphic>();
        _graphic.raycastTarget = false;
    }

    private LabelWidget CreateLabel()
    {
        var root = new GameObject("DebugLabel");
        root.transform.SetParent(_canvas.transform, false);

        var rt = root.AddComponent<RectTransform>();
        rt.pivot = new Vector2(0f, 0f);

        var bg = root.AddComponent<Image>();
        bg.color = labelBackgroundColor;
        bg.raycastTarget = false;

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(root.transform, false);

        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = labelPadding;
        textRt.offsetMax = -labelPadding;

        textGo.SetActive(false);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = labelFontSize;
        tmp.color = labelTextColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        tmp.richText = false;
        if (labelFont != null) tmp.font = labelFont;
        textGo.SetActive(true);

        var fitter = root.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var layout = textGo.AddComponent<LayoutElement>();
        layout.ignoreLayout = false;

        root.SetActive(false);

        return new LabelWidget
        {
            root = root,
            text = tmp,
            background = bg,
            rt = rt
        };
    }

    private LabelWidget AcquireLabel()
    {
        if (_labelsUsed >= _labelPool.Count) _labelPool.Add(CreateLabel());

        var lw = _labelPool[_labelsUsed];
        _labelsUsed++;
        return lw;
    }

    private void BeginLabels() => _labelsUsed = 0;

    private void EndLabels()
    {
        for (int i = 0; i < _labelPool.Count; i++)
        {
            bool shouldBeActive = i < _labelsUsed;
            if (_labelPool[i].root.activeSelf != shouldBeActive)
                _labelPool[i].root.SetActive(shouldBeActive);
        }
    }

    private void PlaceLabel(Vector2 screenPos, string content)
    {
        var lw = AcquireLabel();

        lw.text.text = content;
        lw.text.fontSize = labelFontSize;
        lw.text.color = labelTextColor;
        lw.background.color = labelBackgroundColor;

        var textRt = lw.text.rectTransform;
        textRt.offsetMin = labelPadding;
        textRt.offsetMax = -labelPadding;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.GetComponent<RectTransform>(), screenPos + labelOffset, null, out Vector2 local);

        lw.rt.anchoredPosition = local;
        lw.root.SetActive(true);

        LayoutRebuilder.ForceRebuildLayoutImmediate(lw.rt);
    }

    private readonly Vector3[] _corners = new Vector3[8];
    private readonly Vector2[] _screen = new Vector2[8];

    private void DrawTrackedBoxes()
    {
        var displayMode = ConfigsContainer.ShowDebugVisuals;
        if (displayMode == DebugVisualsDisplayMode.Hidden) return;
        if (ConfigsContainer.DrawBounds == false) return;

        var hoveringTransform = Player.m_localPlayer?.m_hoveringPiece?.transform;

        foreach (var obj in _objects)
        {
            var bounds = obj.bounds;
            if (bounds == null) continue;

            if(displayMode == DebugVisualsDisplayMode.OnHover && obj.transform != hoveringTransform) continue;

            foreach (var bd in bounds) DrawBoxWorld(bd.m_pos, bd.m_rot, bd.m_size, ConfigsContainer.BoundsColor);

            if(displayMode == DebugVisualsDisplayMode.OnHover) break;
        }
    }

    private void DrawBox(Transform parent, WearNTear.BoundData bd)
    {
        if (!_cam) return;

        Vector3 h = bd.m_size * 0.5f;

        _corners[0] = new Vector3(-h.x, -h.y, -h.z);
        _corners[1] = new Vector3(+h.x, -h.y, -h.z);
        _corners[2] = new Vector3(+h.x, +h.y, -h.z);
        _corners[3] = new Vector3(-h.x, +h.y, -h.z);
        _corners[4] = new Vector3(-h.x, -h.y, +h.z);
        _corners[5] = new Vector3(+h.x, -h.y, +h.z);
        _corners[6] = new Vector3(+h.x, +h.y, +h.z);
        _corners[7] = new Vector3(-h.x, +h.y, +h.z);

        for (int i = 0; i < 8; i++)
        {
            Vector3 world = parent.TransformPoint(bd.m_pos + bd.m_rot * _corners[i]);
            Vector3 vp = _cam.WorldToViewportPoint(world);
            if (vp.z <= 0f) return;
            _screen[i] = _cam.WorldToScreenPoint(world);
        }

        for (int e = 0; e < 12; e++)
            _graphic.AddLine(
                _screen[_edgeA[e]],
                _screen[_edgeB[e]],
                lineWidth,
                ConfigsContainer.BoundsColor
            );
    }

    private void DrawBoxWorld(Vector3 worldCenter, Quaternion worldRot, Vector3 size, Color color)
    {
        if (!_cam) return;
        Vector3 h = size * sizeModifier;

        _corners[0] = new Vector3(-h.x, -h.y, -h.z);
        _corners[1] = new Vector3(+h.x, -h.y, -h.z);
        _corners[2] = new Vector3(+h.x, +h.y, -h.z);
        _corners[3] = new Vector3(-h.x, +h.y, -h.z);
        _corners[4] = new Vector3(-h.x, -h.y, +h.z);
        _corners[5] = new Vector3(+h.x, -h.y, +h.z);
        _corners[6] = new Vector3(+h.x, +h.y, +h.z);
        _corners[7] = new Vector3(-h.x, +h.y, +h.z);

        var depths = new float[8];
        var worldPts = new Vector3[8];

        for (int i = 0; i < 8; i++)
        {
            worldPts[i] = worldCenter + worldRot * _corners[i];
            depths[i] = _cam.WorldToViewportPoint(worldPts[i]).z;
        }

        for (int e = 0; e < 12; e++)
        {
            int ia = _edgeA[e];
            int ib = _edgeB[e];

            if (depths[ia] <= 0f || depths[ib] <= 0f) continue;

            Vector2 sa = _cam.WorldToScreenPoint(worldPts[ia]);
            Vector2 sb = _cam.WorldToScreenPoint(worldPts[ib]);

            _graphic.AddLine(sa, sb, lineWidth, color);
        }

        if (ConfigsContainer.DrawBoundsCornersLocalPosition == false) return;

        string fmt = labelFormat;
        for (int i = 0; i < 8; i++)
        {
            if (depths[i] <= 0f) continue;

            Vector3 local = (worldCenter - worldPts[i]).Round(1);

            string label = $"({local.x.ToString(fmt, CultureInfo.InvariantCulture)}, "
                         + $"{local.y.ToString(fmt, CultureInfo.InvariantCulture)}, "
                         + $"{local.z.ToString(fmt, CultureInfo.InvariantCulture)})";

            Vector2 sp = _cam.WorldToScreenPoint(worldPts[i]);
            PlaceLabel(sp, label);
        }
    }

    private void DrawSnapPoints()
    {
        var displayMode = ConfigsContainer.ShowDebugVisuals;
        if (displayMode == DebugVisualsDisplayMode.Hidden) return;
        if (ConfigsContainer.DrawSnappoints == false) return;

        if (!_cam) return;

        var hoveringTransform = Player.m_localPlayer?.m_hoveringPiece?.transform;

        foreach (var pt in _points)
        {
            if(pt.forceShow == false
               && displayMode == DebugVisualsDisplayMode.OnHover
               && pt.transform != hoveringTransform) continue;

            var ptLocalPos = pt.localPos;
            Vector3 world = pt.transform.TransformPoint(ptLocalPos);
            Vector3 vp = _cam.WorldToViewportPoint(world);
            if (vp.z <= 0f) continue;

            Vector2 sp = _cam.WorldToScreenPoint(world);
            var color = ConfigsContainer.SnapPointOverlayColor;
            _graphic.AddCircle(sp, pointRadius, color, pointSegments);

            if(ConfigsContainer.DrawSnappointsLocalPosition == false) continue;
            string fmt = labelFormat;
            string label = $"({ptLocalPos.x.ToString(fmt, CultureInfo.InvariantCulture)}, "
                           + $"{ptLocalPos.y.ToString(fmt, CultureInfo.InvariantCulture)}, "
                           + $"{ptLocalPos.z.ToString(fmt, CultureInfo.InvariantCulture)})";

            PlaceLabel(sp, label);
        }

        if (ConfigsContainer.DrawBoundsCornersLocalPosition == false) return;
    }

    public void DrawTestScreenBox()
    {
        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;
        float s = 100f;

        Vector2 a = new(cx - s, cy - s);
        Vector2 b = new(cx + s, cy - s);
        Vector2 c = new(cx + s, cy + s);
        Vector2 d = new(cx - s, cy + s);

        _graphic.AddLine(a, b, 4f, Color.magenta);
        _graphic.AddLine(b, c, 4f, Color.magenta);
        _graphic.AddLine(c, d, 4f, Color.magenta);
        _graphic.AddLine(d, a, 4f, Color.magenta);
    }

    private void CleanupNulls()
    {
        _objects.RemoveAll(o => o.transform == null || o.bounds == null);
        _points.RemoveAll(p => p.transform == null);
    }

    private class OverlayGraphic : Graphic
    {
        private struct LineDef
        {
            public Vector2 a, b;
            public float width;
            public Color32 color;
        }

        private struct CircleDef
        {
            public Vector2 center;
            public float radius;
            public Color32 color;
            public int segments;
        }

        private readonly List<LineDef> _lines = new(256);
        private readonly List<CircleDef> _circles = new(64);

        protected override void UpdateMaterial()
        {
            base.UpdateMaterial();
            canvasRenderer.materialCount = 1;
            canvasRenderer.SetMaterial(defaultGraphicMaterial, Texture2D.whiteTexture);
        }

        public void Clear()
        {
            _lines.Clear();
            _circles.Clear();
        }

        public void AddLine(Vector2 a, Vector2 b, float width, Color color)
        {
            _lines.Add(new LineDef
            {
                a = a,
                b = b,
                width = width,
                color = color
            });
        }

        public void AddCircle(Vector2 center, float radius, Color color, int segments)
        {
            _circles.Add(new CircleDef
            {
                center = center,
                radius = radius,
                color = color,
                segments = Mathf.Max(segments, 4)
            });
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);
            vh.Clear();

            foreach (var l in _lines)
                PopulateLine(vh, l);

            foreach (var c in _circles)
                PopulateCircle(vh, c);
        }

        private void PopulateLine(VertexHelper vh, LineDef l)
        {
            Vector2 a = ScreenToLocal(l.a);
            Vector2 b = ScreenToLocal(l.b);

            Vector2 dir = b - a;
            if (dir.sqrMagnitude < 0.001f) return;

            Vector2 perp = new Vector2(-dir.normalized.y, dir.normalized.x) * (l.width * 0.5f);

            int idx = vh.currentVertCount;

            vh.AddVert(a + perp, l.color, Vector2.zero);
            vh.AddVert(a - perp, l.color, Vector2.zero);
            vh.AddVert(b - perp, l.color, Vector2.zero);
            vh.AddVert(b + perp, l.color, Vector2.zero);

            vh.AddTriangle(idx, idx + 1, idx + 2);
            vh.AddTriangle(idx, idx + 2, idx + 3);
        }

        private void PopulateCircle(VertexHelper vh, CircleDef c)
        {
            Vector2 center = ScreenToLocal(c.center);
            int seg = c.segments;
            int centerIdx = vh.currentVertCount;

            vh.AddVert(center, c.color, Vector2.zero);

            float step = Mathf.PI * 2f / seg;
            for (int i = 0; i <= seg; i++)
            {
                float angle = i * step;
                Vector2 p = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * c.radius;

                vh.AddVert(p, c.color, Vector2.zero);

                if (i > 0)
                    vh.AddTriangle(
                        centerIdx,
                        centerIdx + i,
                        centerIdx + i + 1
                    );
            }
        }

        private Vector2 ScreenToLocal(Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPos, null, out Vector2 local);
            return local;
        }
    }
}