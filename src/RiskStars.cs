using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace DemandHighlight
{
    // ALL GameItemElement views, not the shop's inventories. Constructors/Validate register any item view:
    // customer goods, counters, showcases, machines, open containers and other scenes use the same path.
    // Weak registrations cannot keep a sold item, a closed window or an old scene alive.
    public static class RiskStars
    {
        sealed class Registration
        {
            public WeakReference Item, Root = new WeakReference(null);
        }
        sealed class Badge
        {
            public Registration Registration;
            public GameObject Root;
            public RectTransform Rect, Anchor;
            public Image Fill, Edge;
            public float Next;
            public bool Queued;
        }

        static ConditionalWeakTable<GameItemElement, Badge> badges = new ConditionalWeakTable<GameItemElement, Badge>();
        static readonly List<Registration> views = new List<Registration>();
        static readonly Queue<WeakReference> urgent = new Queue<WeakReference>();
        static readonly System.Diagnostics.Stopwatch clock = new System.Diagnostics.Stopwatch();
        static int cursor;
        static Sprite star, edge;
        static bool warned;

        public static void Track(GameItemElement item)
        {
            if (item == null) return;
            Badge badge;
            if (!badges.TryGetValue(item, out badge))
            {
                badge = new Badge { Registration = new Registration { Item = new WeakReference(item) } };
                badges.Add(item, badge);
                views.Add(badge.Registration);
            }
            badge.Next = 0f;
            if (!badge.Queued) { urgent.Enqueue(badge.Registration.Item); badge.Queued = true; }
        }

        public static void Changed(GameItem item) { Track(item as GameItemElement); }

        // Runs BEFORE the demand-toggle/no-customer early returns. It never asks Engine, Shop or GameIntent.
        public static void Tick()
        {
            float now = Time.unscaledTime;
            clock.Restart();
            int count = 0;
            // Newly created/validated/moved items take priority over the periodic safety refresh.
            while (urgent.Count > 0 && count++ < 128)
            {
                var item = urgent.Dequeue().Target as GameItemElement;
                Badge badge;
                if (item == null || !badges.TryGetValue(item, out badge)) continue;
                badge.Queued = false;
                RefreshSafe(item, badge, now);
                if (clock.Elapsed.TotalMilliseconds >= 1.5) break;
            }
            int remaining = Math.Min(256, views.Count);
            while (remaining-- > 0 && clock.Elapsed.TotalMilliseconds < 1.5)
            {
                if (cursor >= views.Count) cursor = 0;
                var registration = views[cursor];
                var item = registration.Item.Target as GameItemElement;
                Badge badge;
                if (item == null || !badges.TryGetValue(item, out badge))
                {
                    // A pooled UI anchor can outlive its model. Remove an orphaned badge without retaining that UI.
                    var root = registration.Root.Target as GameObject;
                    if (root != null) UnityEngine.Object.Destroy(root);
                    views.RemoveAt(cursor); continue;
                }
                cursor++;
                if (now >= badge.Next) RefreshSafe(item, badge, now);
            }
            clock.Stop();
        }

        static void RefreshSafe(GameItemElement item, Badge badge, float now)
        {
            badge.Next = now + 0.25f;
            try { Refresh(item, badge); }
            catch (Exception e)
            {
                Hide(badge);
                if (!warned) { warned = true; Debug.LogWarning(DemandMod.Tag + "star refresh failed: " + e.Message); }
            }
        }

        static void Refresh(GameItemElement item, Badge badge)
        {
            var anchor = item.rectTransform;
            if (!MarkerSettings.StarsEnabled || anchor == null || !anchor.gameObject.activeInHierarchy)
            { Hide(badge); return; }
            // The two helpers are deliberately read independently. A box is not marked for its contents' risks.
            var risk = Presentation.RiskOf(StolenHelper.IsStolenItem(item), ContrabandHelper.IsContraband(item));
            if (risk == Risk.None) { Hide(badge); return; }
            var rect = anchor.rect;
            if (rect.width < 1f || rect.height < 1f) { Hide(badge); return; }
            Ensure(badge, anchor);
            badge.Root.SetActive(true);
            badge.Rect.SetAsLastSibling();
            // Anchor to the outer item view, not its internal rotator: the star stays upright at the UI's top right.
            badge.Rect.localRotation = Quaternion.identity;
            badge.Rect.localScale = Vector3.one;
            int pixelScale = Mathf.Clamp(MarkerSettings.StarScale, 1, 4);
            float side = Presentation.StarSide * pixelScale;
            badge.Rect.sizeDelta = new Vector2(side, side);
            badge.Rect.anchoredPosition = new Vector2(-MarkerSettings.StarInset, -MarkerSettings.StarInset);
            badge.Fill.color = MarkerSettings.Star(risk);
            badge.Edge.color = MarkerSettings.StarOutlineColor;
        }

        static void Ensure(Badge badge, RectTransform anchor)
        {
            if (badge.Root == null)
            {
                var go = new GameObject("WRT.RiskStar", typeof(RectTransform));
                badge.Root = go;
                badge.Registration.Root.Target = go;
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
            // Pure mask uses top-down rows; Unity textures use bottom-up rows.
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

        public static void BringToFront(GameItemElement item)
        {
            Badge badge;
            if (item != null && badges.TryGetValue(item, out badge) && badge.Root != null) badge.Rect.SetAsLastSibling();
        }

        // Scene transitions may already have constructed the new scene's views. Keep their weak registrations.
        public static void ReleaseViews()
        {
            foreach (var registration in views)
            {
                var root = registration.Root.Target as GameObject;
                if (root != null) UnityEngine.Object.Destroy(root);
                registration.Root.Target = null;
                var item = registration.Item.Target as GameItemElement;
                Badge badge;
                if (item == null || !badges.TryGetValue(item, out badge)) continue;
                badge.Root = null; badge.Anchor = badge.Rect = null;
                badge.Fill = badge.Edge = null; badge.Next = 0f;
            }
        }

        public static void DestroyAll()
        {
            ReleaseViews();
            badges = new ConditionalWeakTable<GameItemElement, Badge>();
            views.Clear(); urgent.Clear(); cursor = 0;
            foreach (var s in new[] { star, edge })
                if (s != null) { UnityEngine.Object.Destroy(s.texture); UnityEngine.Object.Destroy(s); }
            star = edge = null;
        }
    }
}
