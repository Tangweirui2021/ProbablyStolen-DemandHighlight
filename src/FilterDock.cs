using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DemandHighlight
{
    // An embedded native UI, not a window: lives below the trashcan, left of the main inventory.
    // The advanced window and this dock edit the same Filter expression, so only one query ever reaches Engine.
    public static class FilterDock
    {
        const float Scale = 1f / 3f, RowHeight = 30f;
        static GameObject root;
        static RectTransform rect;
        static EmporiumEntry owner;
        static CustomUIElement mode;
        static readonly Dictionary<string, CustomUIElement> ticks = new Dictionary<string, CustomUIElement>();
        static readonly Vector3[] corners = new Vector3[4];
        static int shownVersion = -1;
        static float width, height, nextTypes, retry;
        static string signature;
        static bool warned;

        static string L(string key) { return Panel.L(key); }

        public static void Tick()
        {
            if (Time.unscaledTime < retry) return;
            try { Refresh(); }
            catch (Exception e)
            {
                Destroy(); retry = Time.unscaledTime + 2f;
                if (!warned) { warned = true; Debug.LogWarning(DemandMod.Tag + "filter dock: " + e.Message); }
            }
        }

        static void Refresh()
        {
            var shop = EmporiumEntry.Instance;
            if (shop != owner) Destroy();
            var inventory = shop != null ? shop.invWindow : null;
            var anchor = inventory != null ? inventory.rectTransform : null;
            var parent = anchor != null ? anchor.parent as RectTransform : null;
            var ui = CustomUIManager.Instance;
            if (parent == null || ui == null || !anchor.gameObject.activeInHierarchy)
            { if (root != null) root.SetActive(false); return; }

            anchor.GetWorldCorners(corners);
            var bottomLeft = parent.InverseTransformPoint(corners[0]);
            var trash = shop.trashInvWindow;
            var trashRect = trash != null ? trash.rectTransform : null;
            bool hasTrash = trashRect != null && trashRect.gameObject.activeInHierarchy;
            Vector3 trashBottom = default(Vector3);
            if (hasTrash) { trashRect.GetWorldCorners(corners); trashBottom = parent.InverseTransformPoint(corners[0]); }
            var bounds = DockLayout.Fit(bottomLeft.x, bottomLeft.y, parent.rect.xMin, parent.rect.yMin,
                hasTrash, trashBottom.x, trashBottom.y);
            if (!bounds.Visible) { if (root != null) root.SetActive(false); return; }

            if (root == null)
            {
                owner = shop;
                root = new GameObject("WRT.FilterDock", typeof(RectTransform));
                rect = (RectTransform)root.transform;
                rect.SetParent(parent, false);
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(1f, 0f);
                rect.localScale = new Vector3(Scale, Scale, 1f);
                var image = root.AddComponent<Image>();
                image.color = new Color(0.13f, 0.12f, 0.18f, 0.92f);
                root.AddComponent<RectMask2D>();
                var layout = root.AddComponent<VerticalLayoutGroup>();
                layout.padding = new RectOffset(4, 4, 4, 4); layout.spacing = 2f;
                layout.childControlWidth = layout.childControlHeight = true;
                layout.childForceExpandWidth = layout.childForceExpandHeight = false;
                layout.childAlignment = TextAnchor.UpperLeft;
                width = height = -1f;
            }
            root.SetActive(true);
            if (rect.parent != parent) rect.SetParent(parent, false);
            rect.localPosition = new Vector3(bounds.Right, bounds.Bottom, 0f);
            rect.sizeDelta = new Vector2(bounds.Width / Scale, bounds.Height / Scale);

            bool resized = Math.Abs(width - bounds.Width) > 0.25f || Math.Abs(height - bounds.Height) > 0.25f;
            if (resized || Time.unscaledTime >= nextTypes)
            {
                nextTypes = Time.unscaledTime + 0.5f;
                var types = Types();
                var current = L("dock_customer") + "|" + Panel.TypeName("FOOD") + "|" + string.Join(",", types);
                if (resized || signature != current)
                {
                    width = bounds.Width; height = bounds.Height; signature = current;
                    Build(ui, types, bounds.Columns);
                }
            }
            if (shownVersion != Filter.Version) Sync();
        }

        static List<string> Types()
        {
            var types = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "STOLEN", "CONTRABAND" };
            foreach (var type in Panel.KnownTypes()) if (!string.IsNullOrEmpty(type) && seen.Add(type)) types.Add(type);
            var all = TypeHelper.ALL_TYPES_LIST;
            if (all != null) foreach (var type in all) if (!string.IsNullOrEmpty(type) && seen.Add(type)) types.Add(type);
            return types;
        }

        static void Build(CustomUIManager ui, List<string> types, int columns)
        {
            ticks.Clear(); shownVersion = -1;
            CustomUIBuilder.ClearContainer(rect);
            var b = ui.BuildInto(rect, false, "wrt_type_demand_stars_dock");
            float inside = rect.sizeDelta.x - 8f;
            b.BeginRow(4f, TextAnchor.MiddleLeft);
            b.AddLabel(" ", "mode"); mode = b.Last;
            Fit(mode, inside - 128f);
            mode.SetTooltip(L("dock_tip"));
            b.AddButton(L("dock_clear"), Clear, "clear"); Fit(b.Last, 56f);
            b.AddButton(L("dock_more"), Panel.Toggle, "more"); Fit(b.Last, 64f);
            b.End();
            b.BeginScroll(Math.Max(30f, rect.sizeDelta.y - RowHeight - 10f));
            float cell = (inside - 16f - (columns - 1) * 2f) / columns;
            b.BeginGrid(columns, cell, RowHeight, 2f);
            Add(b, "$stolen", L("filter_stolen"), on => Filter.SetStolen(on ? Facets.State.In : Facets.State.Off), cell);
            Add(b, "$contraband", L("filter_contraband"), on => Filter.SetContraband(on ? Facets.State.In : Facets.State.Off), cell);
            foreach (var type in types)
            {
                string id = type;
                Add(b, id, Panel.TypeName(id), on => Filter.SetType(id, on ? Facets.State.In : Facets.State.Off), cell);
            }
            b.End(); b.End();
            CustomUIBuilder.RebuildLayout(rect);
            Sync();
        }

        static void Add(CustomUIBuilder b, string key, string label, Action<bool> edit, float cell)
        {
            b.AddToggle(label, false, on => { edit(on); Panel.Changed(); Sync(); }, key);
            var widget = b.Last;
            Fit(widget, cell);
            widget.SetTooltip(label + "\n" + L("dock_tip"));
            ticks[key] = widget;
        }

        static void Fit(CustomUIElement widget, float size)
        {
            if (widget == null || widget.root == null) return;
            widget.SetFixedWidth(size).SetPreferredSize(size, RowHeight);
            var layout = widget.root.GetComponent<LayoutElement>();
            layout.layoutPriority = 2;
            layout.minHeight = layout.preferredHeight = RowHeight; layout.flexibleHeight = 0f;
            if (widget.text != null)
            {
                widget.text.enableAutoSizing = false; widget.text.fontSize = 20f;
                widget.text.enableWordWrapping = false; widget.text.overflowMode = TextOverflowModes.Ellipsis;
            }
        }

        static void Sync()
        {
            shownVersion = Filter.Version;
            foreach (var pair in ticks)
            {
                var state = pair.Key == "$stolen" ? Filter.Stolen : pair.Key == "$contraband" ? Filter.Contraband : Filter.OfType(pair.Key);
                pair.Value.SetBool(state != Facets.State.Off, false);
                string label = pair.Key == "$stolen" ? L("filter_stolen") : pair.Key == "$contraband" ? L("filter_contraband") : Panel.TypeName(pair.Key);
                pair.Value.SetText(state == Facets.State.Out ? "<s>" + label + "</s>" : label);
            }
            if (mode != null) mode.SetText(L(Filter.Active ? "dock_manual" : "dock_customer"));
        }

        static void Clear() { Filter.Clear(); Panel.Changed(); Sync(); }

        public static void Destroy()
        {
            if (root != null) UnityEngine.Object.Destroy(root);
            root = null; rect = null; owner = null; mode = null;
            ticks.Clear(); signature = null; shownVersion = -1; nextTypes = 0f;
        }
    }
}
