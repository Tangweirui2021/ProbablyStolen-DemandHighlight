using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace DemandHighlight
{
    // Global native item views, including both the inventory view and the game's separate drag/stack preview.
    // Registration does not depend on shop roots or the current customer. All registry roots are weak.
    public static class RiskStars
    {
        sealed class Registration
        {
            public WeakReference Item, Root = new WeakReference(null), DragRoot = new WeakReference(null);
        }
        sealed class Badge
        {
            public GameObject Root;
            public RectTransform Rect, Anchor;
            public Image Fill, Edge;
        }
        sealed class Entry
        {
            public Registration Registration;
            public readonly Badge Normal = new Badge(), Drag = new Badge();
            public float Next;
            public bool Queued;
        }

        static ConditionalWeakTable<GameItemElement, Entry> entries = new ConditionalWeakTable<GameItemElement, Entry>();
        static readonly List<Registration> views = new List<Registration>();
        static readonly Queue<WeakReference> urgent = new Queue<WeakReference>();
        static readonly System.Diagnostics.Stopwatch clock = new System.Diagnostics.Stopwatch();
        static int cursor;
        static Sprite star, edge;
        static bool warned;
        static readonly Vector3[] corners = new Vector3[4];

        public static void Track(GameItemElement item)
        {
            if (item == null || item.IsDestroyed()) return;
            Entry entry;
            if (!entries.TryGetValue(item, out entry))
            {
                entry = new Entry { Registration = new Registration { Item = new WeakReference(item) } };
                entries.Add(item, entry);
                views.Add(entry.Registration);
            }
            entry.Next = 0f;
            if (!entry.Queued) { urgent.Enqueue(entry.Registration.Item); entry.Queued = true; }
        }

        public static void Changed(GameItem item) { Track(item as GameItemElement); }

        public static void Remove(GameItemElement item)
        {
            Entry entry;
            if (item == null || !entries.TryGetValue(item, out entry)) return;
            Release(entry.Registration);
            entries.Remove(item);
        }

        // Always runs before the demand toggle, no-customer and no-shop early returns.
        public static void Tick()
        {
            float now = Time.unscaledTime;
            clock.Restart();
            int count = 0;
            while (urgent.Count > 0 && count++ < 128)
            {
                var item = urgent.Dequeue().Target as GameItemElement;
                Entry entry;
                if (item == null || !entries.TryGetValue(item, out entry)) continue;
                entry.Queued = false;
                RefreshSafe(item, entry, now);
                if (clock.Elapsed.TotalMilliseconds >= 1.5) break;
            }
            int remaining = Math.Min(256, views.Count);
            while (remaining-- > 0 && clock.Elapsed.TotalMilliseconds < 1.5)
            {
                if (cursor >= views.Count) cursor = 0;
                var registration = views[cursor];
                var item = registration.Item.Target as GameItemElement;
                Entry entry;
                if (item == null || !entries.TryGetValue(item, out entry) || item.IsDestroyed())
                {
                    Release(registration);
                    if (item != null) entries.Remove(item);
                    views.RemoveAt(cursor); continue;
                }
                cursor++;
                if (now >= entry.Next) RefreshSafe(item, entry, now);
            }
            clock.Stop();
        }

        static void RefreshSafe(GameItemElement item, Entry entry, float now)
        {
            entry.Next = now + 0.25f;
            try { Refresh(item, entry); }
            catch (Exception e)
            {
                Hide(entry.Normal); Hide(entry.Drag);
                if (!warned) { warned = true; Debug.LogWarning(DemandMod.Tag + "star refresh failed: " + e.Message); }
            }
        }

        static void Refresh(GameItemElement item, Entry entry)
        {
            if (!MarkerSettings.StarsEnabled || item.IsDestroyed())
            { Hide(entry.Normal); Hide(entry.Drag); return; }
            var risk = Presentation.RiskOf(StolenHelper.IsStolenItem(item), ContrabandHelper.IsContraband(item));
            if (risk == Risk.None) { Hide(entry.Normal); Hide(entry.Drag); return; }

            // The game hides an exhausted original stack by disabling its image, while keeping its object active.
            // A partial-stack drag can legitimately have both views visible. Follow each native view separately.
            var visual = item.rotator;
            Render(entry.Normal, entry.Registration.Root, item.rectTransform,
                visual != null ? visual.rectTransform : item.rectTransform, VisibleOpacity(item.background), risk);
            var fake = item.fakeMiddle;
            var fakeVisual = item.fakeRotator;
            Render(entry.Drag, entry.Registration.DragRoot, fake != null ? fake.rectTransform : null,
                fakeVisual != null ? fakeVisual.rectTransform : (fake != null ? fake.rectTransform : null),
                VisibleOpacity(item.fakeBackground), risk);
        }

        static float VisibleOpacity(TreeNodeRender node)
        {
            if (node == null || node.image == null || !node.image.enabled) return 0f;
            return Mathf.Clamp(node.image.color.a, 0f, 1f);
        }

        static void Render(Badge badge, WeakReference root, RectTransform anchor, RectTransform visual, float opacity, Risk risk)
        {
            if (anchor == null || visual == null || !anchor.gameObject.activeInHierarchy || opacity <= 0f)
            { Hide(badge); return; }
            // Middle is upright, but its rectangle is not the rotated footprint. Measure the actual rotator
            // in that upright coordinate system; pivots, mirroring and native node offsets then come along.
            visual.GetWorldCorners(corners);
            var first = anchor.InverseTransformPoint(corners[0]);
            float minX = first.x, maxX = first.x, minY = first.y, maxY = first.y;
            for (int i = 1; i < corners.Length; i++)
            {
                var p = anchor.InverseTransformPoint(corners[i]);
                minX = Math.Min(minX, p.x); maxX = Math.Max(maxX, p.x);
                minY = Math.Min(minY, p.y); maxY = Math.Max(maxY, p.y);
            }
            if (maxX - minX < 1f || maxY - minY < 1f) { Hide(badge); return; }
            Ensure(badge, root, anchor);
            badge.Root.SetActive(true);
            badge.Rect.SetAsLastSibling();
            badge.Rect.localRotation = Quaternion.identity;
            badge.Rect.localScale = Vector3.one;
            var ui = UISettings.current;
            float side = Presentation.StarSize(ui != null ? ui.gridSize : 16, MarkerSettings.StarScale);
            side = Math.Min(side, Math.Min(maxX - minX, maxY - minY));
            badge.Rect.sizeDelta = new Vector2(side, side);
            var bounds = anchor.rect;
            badge.Rect.anchoredPosition = new Vector2(maxX - bounds.xMax - MarkerSettings.StarInset,
                maxY - bounds.yMax - MarkerSettings.StarInset);
            var fill = MarkerSettings.Star(risk); fill.a *= opacity;
            var outline = MarkerSettings.StarOutlineColor; outline.a *= opacity;
            badge.Fill.color = fill;
            badge.Edge.color = outline;
        }

        static void Ensure(Badge badge, WeakReference root, RectTransform anchor)
        {
            if (badge.Root == null)
            {
                var go = new GameObject("WRT.RiskStar", typeof(RectTransform));
                badge.Root = go; root.Target = go;
                badge.Rect = (RectTransform)go.transform;
                go.AddComponent<LayoutElement>().ignoreLayout = true;
                badge.Edge = Image(go.transform, "Outline", Sprite(true));
                badge.Fill = Image(go.transform, "Star", Sprite(false));
            }
            if (badge.Anchor != anchor || badge.Rect.parent != anchor)
            {
                badge.Rect.SetParent(anchor, false);
                badge.Anchor = anchor;
            }
            badge.Rect.anchorMin = badge.Rect.anchorMax = new Vector2(1f, 1f);
            badge.Rect.pivot = new Vector2(1f, 1f);
        }

        static Image Image(Transform parent, string name, Sprite sprite)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.AddComponent<LayoutElement>().ignoreLayout = true;
            var image = go.AddComponent<Image>();
            image.raycastTarget = false;
            image.sprite = sprite;
            image.type = UnityEngine.UI.Image.Type.Simple;
            return image;
        }

        static Sprite Sprite(bool outline)
        {
            var existing = outline ? edge : star;
            if (existing != null) return existing;
            int side = Presentation.StarSide;
            var mask = Presentation.StarMask(outline);
            var pixels = new Color32[mask.Length];
            for (int y = 0; y < side; y++) for (int x = 0; x < side; x++)
                pixels[(side - 1 - y) * side + x] = new Color32(255, 255, 255, mask[y * side + x]);
            var texture = new Texture2D(side, side, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.SetPixels32(pixels); texture.Apply(false, true);
            var sprite = UnityEngine.Sprite.Create(texture, new Rect(0, 0, side, side), new Vector2(0.5f, 0.5f), 100f);
            sprite.hideFlags = HideFlags.HideAndDontSave;
            if (outline) edge = sprite; else star = sprite;
            return sprite;
        }

        static void Hide(Badge badge) { if (badge.Root != null) badge.Root.SetActive(false); }
        static void Release(Registration registration)
        {
            var root = registration.Root.Target as GameObject;
            if (root != null) UnityEngine.Object.Destroy(root);
            root = registration.DragRoot.Target as GameObject;
            if (root != null) UnityEngine.Object.Destroy(root);
            registration.Root.Target = registration.DragRoot.Target = null;
        }

        public static void BringToFront(GameItemElement item)
        {
            Entry entry;
            if (item == null || !entries.TryGetValue(item, out entry)) return;
            if (entry.Normal.Root != null) entry.Normal.Rect.SetAsLastSibling();
            if (entry.Drag.Root != null) entry.Drag.Rect.SetAsLastSibling();
        }

        // New scene views may already exist when sceneLoaded fires. Keep their weak registrations.
        public static void ReleaseViews()
        {
            foreach (var registration in views)
            {
                Release(registration);
                var item = registration.Item.Target as GameItemElement;
                Entry entry;
                if (item == null || !entries.TryGetValue(item, out entry)) continue;
                entry.Normal.Root = entry.Drag.Root = null;
                entry.Normal.Anchor = entry.Normal.Rect = entry.Drag.Anchor = entry.Drag.Rect = null;
                entry.Normal.Fill = entry.Normal.Edge = entry.Drag.Fill = entry.Drag.Edge = null;
                entry.Next = 0f;
            }
        }

        public static void DestroyAll()
        {
            ReleaseViews();
            entries = new ConditionalWeakTable<GameItemElement, Entry>();
            views.Clear(); urgent.Clear(); cursor = 0;
            foreach (var s in new[] { star, edge })
                if (s != null) { UnityEngine.Object.Destroy(s.texture); UnityEngine.Object.Destroy(s); }
            star = edge = null;
        }
    }
}
