using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using DemandHighlight;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

static class IntegrationTest
{
    static int failed;
    static void Check(string label, bool ok)
    {
        Console.WriteLine((ok ? "ok    " : "FAIL  ") + label);
        if (!ok) failed++;
    }
    static void Step()
    {
        // Give the time-budgeted registries several frames to drain even on a slow CI host.
        for (int i = 0; i < 8; i++) { Time.unscaledTime += 0.3f; DemandMod.Tick(); }
    }
    static GameObject Child(GameItemElement item, string name)
    {
        foreach (var child in item.rectTransform.children) if (child.gameObject.name == name) return child.gameObject;
        return null;
    }
    static Image Star(GameItemElement item)
    {
        var root = Child(item, "WRT.RiskStar");
        return root != null && root.activeInHierarchy ? root.transform.GetChild(1).gameObject.GetComponent<Image>() : null;
    }
    static Image DragStar(GameItemElement item)
    {
        foreach (var child in item.fakeMiddle.rectTransform.children)
            if (child.gameObject.name == "WRT.RiskStar" && child.gameObject.activeInHierarchy)
                return child.GetChild(1).gameObject.GetComponent<Image>();
        return null;
    }
    static ConfigEntry<T> Entry<T>(string key) { return (ConfigEntry<T>)ModHelper.Config.Entries[key]; }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static WeakReference TemporaryView(out GameObject ui)
    {
        var item = new GameItemElement { Stolen = true };
        item.Validate(); Step();
        ui = Child(item, "WRT.RiskStar");
        return new WeakReference(item);
    }

    static int Main(string[] args)
    {
        var mod = new DemandMod();
        mod.Init(new ModManifest { ID = DemandMod.ModId });
        mod.OnEnable();
        Check("constructor hooks installed", HarmonyLib.Harmony.Patches.ContainsKey(typeof(GameItemElement).GetConstructor(Type.EmptyTypes)));
        Check("no startup warnings", Debug.Warnings.Count == 0);

        // None of these views is in a shop inventory. Even the shop singleton is absent.
        var locations = new[] { "warehouse", "front counter", "customer sale", "showcase", "container", "machine", "night scene", "other item UI" };
        var items = new List<GameItemElement>();
        foreach (var location in locations)
            items.Add(new GameItemElement { identifier = location, Stolen = true });
        Step();
        for (int i = 0; i < locations.Length; i++)
            Check("global star with no shop/client: " + locations[i], Star(items[i]) != null && Star(items[i]).color == MarkerSettings.StolenColor);
        var target = items[0];
        Check("badges do not intercept clicks", Star(target) != null && !Star(target).raycastTarget &&
            !Child(target, "WRT.RiskStar").transform.GetChild(0).gameObject.GetComponent<Image>().raycastTarget);
        Check("badges do not affect layout", Child(target, "WRT.RiskStar").GetComponent<LayoutElement>().ignoreLayout);

        var dragged = new GameItemElement { Stolen = true };
        Step();
        Check("idle view has one star and no drag-preview star", Star(dragged) != null && DragStar(dragged) == null);
        dragged.ToggleBackground(false, 1f, 1f);
        dragged.ToggleFakeBackground(true, new Vector4(1f,1f,1f,0.5f)); Step();
        Check("whole-stack pickup hides the star at the old inventory position", Star(dragged) == null);
        Check("drag preview retains its own top-right star", DragStar(dragged) != null);
        Check("drag-preview star follows native opacity", DragStar(dragged).color.a == 0.5f);
        Check("drag-preview star does not intercept pointer input", !DragStar(dragged).raycastTarget);
        dragged.Contraband = true; dragged.Validate(); Step();
        Check("drag preview preserves the combined red-star state", DragStar(dragged).color.r == MarkerSettings.BothColor.r && DragStar(dragged).color.g == MarkerSettings.BothColor.g);
        dragged.ToggleBackground(true,1f,1f); Step();
        Check("partial-stack drag marks both visible portions", Star(dragged) != null && DragStar(dragged) != null);
        Entry<bool>("riskStars").Set(false); Step();
        Check("star setting hides both native views", Star(dragged) == null && DragStar(dragged) == null);
        Entry<bool>("riskStars").Set(true); Step();
        Check("star setting restores both visible views", Star(dragged) != null && DragStar(dragged) != null);
        dragged.ToggleFakeBackground(false, Vector4.zero); Step();
        Check("dropping the stack removes only the drag-preview star", Star(dragged) != null && DragStar(dragged) == null);
        dragged.Destroy(); Step();
        Check("destroyed items lose all star UI immediately", Star(dragged) == null && DragStar(dragged) == null);

        target.Contraband = true; target.Validate(); Step();
        Check("both statuses use red", Star(target).color == MarkerSettings.BothColor);
        target.Stolen = false; Step();
        Check("status safety refresh works without Validate", Star(target).color == MarkerSettings.ContrabandColor);
        target.Contraband = false; Step();
        Check("ordinary items lose the star", Star(target) == null);
        target.Stolen = true; target.Validate(); Step();

        typeof(DemandMod).GetField("on", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, false);
        target.Contraband = true; target.Validate(); Step();
        Check("demand toggle off does not hide/update-block stars", Star(target) != null && Star(target).color == MarkerSettings.BothColor);
        SceneManager.Load("AfterhoursScene"); Step();
        Check("stars work across non-store scene changes", Star(target) != null);
        target.rotator = new Rotator { rectTransform = (RectTransform)new GameObject("Internal rotator").transform };
        target.RotCW(); Step();
        Check("star anchors outside the item's internal rotator", Child(target, "WRT.RiskStar").transform.parent == target.rectTransform);

        Entry<string>("bothStarColor").Set("#12345680"); Step();
        Check("live-config star colour and opacity", Star(target).color == new Color(18 / 255f, 52 / 255f, 86 / 255f, 128 / 255f));
        Entry<int>("starPixelScale").Set(3); Step();
        Check("live-config integer pixel scale", ((RectTransform)Child(target, "WRT.RiskStar").transform).sizeDelta.x == 33);
        Entry<bool>("riskStars").Set(false); Step();
        Check("dedicated stars setting hides badges", Star(target) == null);
        Entry<bool>("riskStars").Set(true); Step();
        Check("dedicated stars setting restores badges", Star(target) != null);

        Entry<string>("typeColor_FOOD").Set("#112233");
        var ordinary = new GameItemElement { itemTypes = new List<string> { "FOOD" } };
        var illegal = new GameItemElement { itemTypes = new List<string> { "FOOD", "CONTRABAND" }, Stolen = true, Contraband = true };
        Step();
        EmporiumEntry.Instance = new EmporiumEntry { invElement = new GameInventory() };
        EmporiumEntry.Instance.invElement.childItems.Add(ordinary);
        EmporiumEntry.Instance.invElement.childItems.Add(illegal);
        var buyer = new StoreClient { clientIntent = Intent.Buy };
        bool complete;
        Engine.Run(GameIntent.Read(buyer, false), GameIntent.Ask, out complete);
        while (!complete) Engine.Run(GameIntent.Read(buyer, false), GameIntent.Ask, out complete);
        Painter.Paint(Engine.Result);
        var a = Child(ordinary, "DemandHighlight").GetComponent<Image>();
        var b = Child(illegal, "DemandHighlight").GetComponent<Image>();
        Check("actual painter: same product type has same colour regardless of legal status", a.color == b.color && a.color == new Color(17 / 255f, 34 / 255f, 51 / 255f));
        Check("actual painter: no stolen stripes or contraband double-line style", a.type == Image.Type.Sliced && b.type == Image.Type.Sliced);
        Check("new demand outlines cannot cover an existing star", illegal.rectTransform.GetChild(illegal.rectTransform.childCount - 1).gameObject.name == "WRT.RiskStar");

        var box = new GameItemElement { contentWindow = new PixelWindow(), itemTypes = new List<string> { "STORAGE" } };
        var inside = new GameInventory { Parent = box };
        box.children.Add(inside); inside.children.Add(illegal); illegal.parentInventory = inside;
        EmporiumEntry.Instance.invElement.childItems.Clear();
        EmporiumEntry.Instance.invElement.childItems.Add(box);
        Engine.Release();
        Engine.Run(GameIntent.Read(buyer, false), GameIntent.Ask, out complete);
        while (!complete) Engine.Run(GameIntent.Read(buyer, false), GameIntent.Ask, out complete);
        Painter.Paint(Engine.Result); Step();
        Check("actual engine: closed container gets a contents hint", Engine.Result.Items.Count > 0 && Engine.Result.Items[0].Style.Contents);
        Check("container hints use the one configured colour", Child(box, "DemandHighlight").GetComponent<Image>().color == MarkerSettings.ContainerColor);
        Check("contents cannot turn an ordinary container into contraband", Star(box) == null);
        Entry<string>("containerOutlineColor").Set("#AABBCC");
        Painter.Paint(Engine.Result);
        Check("container colour updates without rebuilding geometry", Child(box, "DemandHighlight").GetComponent<Image>().color == new Color(170 / 255f, 187 / 255f, 204 / 255f));

        GameObject orphanUi;
        var weak = TemporaryView(out orphanUi);
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        Check("global star registry does not pin discarded item models", !weak.IsAlive);
        Step();
        Check("a discarded view cannot leave a badge on a pooled UI anchor", orphanUi == null);
        mod.OnDisable();
        Check("disable removes star UI", Star(target) == null);
        Check("disable removes constructor/event patches", HarmonyLib.Harmony.Patches.Count == 0);
        Check("no unexpected runtime warnings", Debug.Warnings.Count == 0);
        if (args.Length == 2 && args[0] == "--preview-data")
        {
            var data = new { side = Presentation.StarSide, fill = Array.ConvertAll(Presentation.StarMask(false), x => (int)x),
                edge = Array.ConvertAll(Presentation.StarMask(true), x => (int)x),
                frame = Array.ConvertAll(Outline.Alpha(Shape.Full, 100, 64, new Style(Mark.Normal, false), 2, 3, true, 0), x => (int)x),
                container = Array.ConvertAll(Outline.Alpha(Shape.Full, 100, 64, new Style(Mark.Normal, true), 2, 3, true, 0), x => (int)x) };
            System.IO.File.WriteAllText(args[1], System.Text.Json.JsonSerializer.Serialize(data));
        }
        return failed == 0 ? 0 : 1;
    }
}
