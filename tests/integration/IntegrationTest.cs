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
    static bool Outlined(GameItemElement item) { var go=Child(item,"DemandHighlight"); return go!=null && go.activeInHierarchy; }
    static CustomUIElement Widget(string id) { return CustomUIBuilder.Built.Find(e=>e.id==id); }
    static bool Near(float a,float b) { return Math.Abs(a-b)<0.002f; }
    static void FlushGeometry() { for(int i=0;i<8;i++) DemandMod.Tick(); }

    static void RotationStars()
    {
        int oldScale=Entry<int>("starPixelScale").Value;
        Entry<int>("starPixelScale").Set(2);
        var item=new GameItemElement {Stolen=true,Contraband=true};
        item.rectTransform.rect=new Rect(0,0,32,16);
        item.fakeMiddle.rectTransform.rect=new Rect(0,0,32,16);
        var visual=item.rotator.rectTransform;var fake=item.fakeRotator.rectTransform;
        visual.rect=fake.rect=new Rect(-8,-12,32,16);
        visual.localPosition=new Vector3(7,-3,0);fake.localPosition=new Vector3(-5,6,0);
        var canvas=(RectTransform)new GameObject("Scaled inventory canvas").transform;
        canvas.localScale=new Vector3(2,3,1);canvas.localPosition=new Vector3(100,200,0);
        item.rectTransform.SetParent(canvas,false);item.fakeMiddle.rectTransform.SetParent(canvas,false);
        item.rectTransform.localPosition=new Vector3(13,-21,0);
        item.fakeMiddle.rectTransform.localPosition=new Vector3(150,-80,0);
        item.ToggleFakeBackground(true,new Vector4(1,1,1,0.5f));Step();
        // Analytic corner expectations for a non-square rectangle with an asymmetric pivot and nonzero origin.
        var expected=new[] {new Vector2(-3,-17),new Vector2(-15,3),new Vector2(-19,-9),new Vector2(-23,-13),
            new Vector2(-19,-17),new Vector2(-15,-13),new Vector2(-3,-9),new Vector2(-23,3)};
        for(int mirrored=0;mirrored<2;mirrored++)for(int turn=0;turn<4;turn++)
        {
            visual.localRotation=fake.localRotation=Quaternion.Euler(0,0,turn*90);
            visual.localScale=fake.localScale=new Vector3(mirrored==0?1:-1,1,1);
            item.UpdateRectTransform();FlushGeometry();
            var normal=(RectTransform)Child(item,"WRT.RiskStar").transform;
            var drag=(RectTransform)DragStar(item).transform.parent;
            var p=expected[mirrored*4+turn];
            Check("rotated top-right corner: turn "+turn+" mirrored "+mirrored, Near(normal.anchoredPosition.x,p.x) && Near(normal.anchoredPosition.y,p.y));
            Check("drag corner is computed independently: turn "+turn+" mirrored "+mirrored, Near(drag.anchoredPosition.x,p.x-12) && Near(drag.anchoredPosition.y,p.y+9));
            Check("rotation preserves upright compact risk stars: turn "+turn+" mirrored "+mirrored,
                normal.localRotation.Z==0 && drag.localRotation.Z==0 && normal.localScale.x==1 && drag.localScale.x==1 && normal.sizeDelta.x==5 && drag.sizeDelta.x==5 && Star(item).color==MarkerSettings.BothColor && Near(DragStar(item).color.a,MarkerSettings.BothColor.a*0.5f));
        }
        visual.localRotation=Quaternion.Euler(0,0,90);fake.localRotation=Quaternion.Euler(0,0,270);
        visual.localScale=fake.localScale=Vector3.one;item.UpdateRectTransform();FlushGeometry();
        Check("split-stack views can have different current rotations", Near(((RectTransform)Child(item,"WRT.RiskStar").transform).anchoredPosition.y,3) && Near(((RectTransform)DragStar(item).transform.parent).anchoredPosition.y,-4));
        visual.rect=new Rect(-8,-8,16,16);visual.localPosition=new Vector3(8,-8,0);
        item.rectTransform.rect=new Rect(0,-16,16,16);
        for(int turn=0;turn<4;turn++)
        {
            visual.localRotation=Quaternion.Euler(0,0,turn*90);item.UpdateRectTransform();FlushGeometry();
            var r=(RectTransform)Child(item,"WRT.RiskStar").transform;
            Check("one-cell square retains its top-right corner at turn "+turn, Near(r.anchoredPosition.x,-2) && Near(r.anchoredPosition.y,-2));
        }
        foreach(var name in new[] {"RotCW","RotCCW","Rot180","FlipX","FlipY","GridRotCW","GridRotCCW","GridRot180","GridFlipX","GridFlipY","UpdateRectTransform"})
            Check("native transform callback installed: "+name,HarmonyLib.Harmony.Patches.ContainsKey(typeof(GameItemElement).GetMethod(name)));
        item.Destroy();Step();Entry<int>("starPixelScale").Set(oldScale);
    }

    static void TrashAndDock()
    {
        Filter.Clear(); Engine.Release();
        typeof(DemandMod).GetField("on", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, true);
        var shop = new EmporiumEntry { invElement=new GameInventory(), trashInvElement=new GameInventory() };
        EmporiumEntry.Instance=shop;
        var buyer=new StoreClient {clientIntent=Intent.Buy};
        PlayerStore.Instance=new PlayerStore {currentClientInstance=new ClientInstance {Blueprint=buyer}};
        var food=new GameItemElement {Stolen=true,itemTypes=new List<string>{"FOOD"}};
        var tool=new GameItemElement {Contraband=true,itemTypes=new List<string>{"TOOL"}};
        var normalFood=new GameItemElement {itemTypes=new List<string>{"FOOD"}};
        var bin=new GameItemElement {contentWindow=new PixelWindow(),itemTypes=new List<string>{"STORAGE"},mayInventoryRemoveItemFunc=(i,p)=>false};
        bin.contentWindow.parentItems.Add(bin);
        var inside=new GameInventory {Parent=bin};bin.children.Add(inside);
        inside.childItems.Add(food);inside.childItems.Add(tool);inside.children.Add(food);inside.children.Add(tool);
        food.parentInventory=tool.parentInventory=inside;
        shop.trashInvElement.childItems.Add(bin);shop.trashInvElement.children.Add(bin);
        shop.invElement.childItems.Add(normalFood);shop.invElement.children.Add(normalFood);
        Step();
        Check("closed fixed trashcan has a customer contents hint", Engine.Result.Items.Exists(h=>h.Element==bin && h.Style.Contents) && Outlined(bin));
        Check("closed trashcan contents are not painted through its window", !Outlined(food));
        bin.contentWindow.Show();Step();
        Check("opening trashcan marks its wanted contents and the bin", Outlined(food) && Outlined(bin) && !Outlined(tool));
        var all=new List<GameItem>();Shop.Each(all.Add);
        Check("trash stock is included exactly once in counts and query traversal", all.Count==4 && all.FindAll(i=>i==food).Count==1);
        inside.childItems.Remove(food);inside.children.Remove(food);bin.Validate();Step();
        Check("removing the final wanted trash item removes the bin hint", !Outlined(bin));
        inside.childItems.Add(food);inside.children.Add(food);bin.Validate();Step();

        var canvas=(RectTransform)new GameObject("Pixel canvas").transform;canvas.rect=new Rect(0,0,640,360);
        var inventory=(RectTransform)new GameObject("Inventory window").transform;inventory.SetParent(canvas,false);
        inventory.rect=new Rect(0,0,300,150);inventory.localPosition=new Vector3(320,20,0);
        var trash=(RectTransform)new GameObject("Trash window").transform;trash.SetParent(canvas,false);
        trash.rect=new Rect(0,0,60,112);trash.localPosition=new Vector3(230,68,0);
        shop.invWindow=new PixelWindow {rectTransform=inventory};shop.trashInvWindow=new PixelWindow {rectTransform=trash};
        CustomUIManager.Instance=new CustomUIManager();Step();
        var dock=Widget("mode").root.transform.parent as RectTransform;
        Check("checkbox dock is embedded beside the inventory", dock.parent==canvas && dock.localPosition.x==316 && dock.localPosition.y==20);
        Check("dock provides native category and risk checkboxes", Widget("FOOD")!=null && Widget("TOOL")!=null && Widget("$stolen")!=null && Widget("$contraband")!=null && Widget("CONTRABAND")==null);
        Widget("TOOL").SetBool(true,true);Step();
        Check("manual selection completely replaces customer demand", Engine.Current.Kind==Query.Op.Filter && Outlined(tool) && !Outlined(food) && !Outlined(normalFood));
        Check("manual selection marks the fixed trashcan too", Outlined(bin) && Engine.Result.Items.Exists(h=>h.Element==bin && h.Style.Contents));
        Widget("FOOD").SetBool(true,true);Step();
        Check("multiple category checkboxes match either type", Outlined(tool) && Outlined(food) && Outlined(normalFood));
        Widget("$stolen").SetBool(true,true);Step();
        Check("type and risk groups intersect", Outlined(food) && !Outlined(tool) && !Outlined(normalFood));
        Widget("$contraband").SetBool(true,true);Step();
        Check("risk checkboxes match stolen or contraband", Outlined(food) && Outlined(tool) && !Outlined(normalFood));
        Check("risk stars remain independent of manual demand outlines", Star(food)!=null && Star(tool)!=null);
        Widget("clear").Click();Step();
        Check("clear restores the current customer's demand", !Filter.Active && Engine.Current.Kind!=Query.Op.Filter && Outlined(food) && Outlined(normalFood) && !Outlined(tool));
        Filter.SetContraband(Facets.State.In);Panel.Changed();Step();
        Check("dock reflects edits from the shared advanced filter", Widget("$contraband").Checked && Outlined(tool) && !Outlined(food));
        Widget("$contraband").SetBool(false,true);Step();
        Check("unchecking the final selection restores customer demand", !Filter.Active && Outlined(food) && !Outlined(tool));
        Filter.SetType("FOOD",Facets.State.Out);Panel.Changed();Step();
        Check("advanced exclusions are visible and removable in the dock", Widget("FOOD").Checked && Widget("FOOD").Label.StartsWith("<s>"));
        Widget("FOOD").SetBool(false,true);Step();
        int toggles=Panel.Toggles;Widget("more").Click();
        Check("more opens the existing advanced filter", Panel.Toggles==toggles+1);
        inventory.gameObject.SetActive(false);Step();
        Check("hidden inventory also hides its dock", !dock.gameObject.activeInHierarchy);
        inventory.gameObject.SetActive(true);Step();
        Check("returning to the inventory restores the dock", dock.gameObject.activeInHierarchy);
        Widget("TOOL").SetBool(true,true);Step();
        PlayerStore.Instance.currentClientInstance=null;Step();
        Check("manual checkboxes work with no customer present", Engine.Current.Kind==Query.Op.Filter && Outlined(tool) && !Outlined(food));
        SceneManager.Load("MainMenu");EmporiumEntry.Instance=null;Step();
        Check("leaving the store destroys dock UI and keeps only filter data", dock.gameObject==null && Filter.OfType("TOOL")==Facets.State.In);
        Filter.Clear();PlayerStore.Instance=null;CustomUIManager.Instance=null;
    }

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
        var priorityEntry = Entry<string>("typePriority");
        var priorityField = ModConfigWindow.BuildTestEntry(ModHelper.Config, priorityEntry).root.GetComponent<LayoutElement>();
        // TMP's content preference is thousands of pixels long. It must lose to the fixed-width entry.
        float tmpPreference = MarkerSettings.DefaultPriority.Length * 8f;
        float effectiveWidth = priorityField.layoutPriority > 1 ? priorityField.preferredWidth : Math.Max(tmpPreference, priorityField.preferredWidth);
        Check("long priority input cannot squeeze its label", effectiveWidth == 245 && priorityField.minWidth == 245 && priorityField.flexibleWidth == 0);
        Check("opening priority settings preserves the complete value", priorityEntry.Value == MarkerSettings.DefaultPriority);
        Check("other settings keep native layout", ModConfigWindow.BuildTestEntry(ModHelper.Config, Entry<string>("stolenStarColor")).root.GetComponent<LayoutElement>().layoutPriority == 1);
        Check("other mods keep native layout even with the same key", ModConfigWindow.BuildTestEntry(new ModConfig { ModId = "other_mod" }, priorityEntry).root.GetComponent<LayoutElement>().layoutPriority == 1);

        // None of these views is in a shop inventory. Even the shop singleton is absent.
        var locations = new[] { "warehouse", "front counter", "customer sale", "showcase", "container", "machine", "night scene", "other item UI" };
        var items = new List<GameItemElement>();
        foreach (var location in locations)
            items.Add(new GameItemElement { identifier = location, Stolen = true });
        Step();
        for (int i = 0; i < locations.Length; i++)
            Check("global star with no shop/client: " + locations[i], Star(items[i]) != null && Star(items[i]).color == MarkerSettings.StolenColor);
        var target = items[0];
        var starRect = (RectTransform)Child(target, "WRT.RiskStar").transform;
        Check("existing default size level now fits one third of a 16px cell", starRect.sizeDelta.x == 5 && starRect.sizeDelta.y == 5);
        Check("compact star keeps the top-right corner and configured inset", starRect.anchorMin.x == 1 && starRect.anchorMin.y == 1 && starRect.pivot.x == 1 && starRect.pivot.y == 1 && starRect.anchoredPosition.x == -2 && starRect.anchoredPosition.y == -2);
        target.rectTransform.rect = new Rect(0,0,16,16); Step();
        Check("smallest one-cell item keeps the compact star", starRect.sizeDelta.x == 5);
        target.rectTransform.rect = new Rect(0,0,160,96); Step();
        Check("large container does not enlarge the star", starRect.sizeDelta.x == 5);
        UISettings.current.gridSize = 24; Step();
        Check("star follows the game's grid size instead of item dimensions", starRect.sizeDelta.x == 8);
        UISettings.current.gridSize = 16; Step();
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
        Check("drag preview uses the same compact dimensions", ((RectTransform)DragStar(dragged).transform.parent).sizeDelta.x == 5);
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
        target.RotCW(); Step();
        Check("star anchors outside the item's internal rotator", Child(target, "WRT.RiskStar").transform.parent == target.rectTransform);

        Entry<string>("bothStarColor").Set("#12345680"); Step();
        Check("live-config star colour and opacity", Star(target).color == new Color(18 / 255f, 52 / 255f, 86 / 255f, 128 / 255f));
        Entry<int>("starPixelScale").Set(3); Step();
        Check("live-config size level is relative to one cell", ((RectTransform)Child(target, "WRT.RiskStar").transform).sizeDelta.x == 8);
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

        RotationStars();
        TrashAndDock();
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
            var data = new { side = Presentation.StarSide, grid = 16, starSize = Presentation.StarSize(16, 2), fill = Array.ConvertAll(Presentation.StarMask(false), x => (int)x),
                edge = Array.ConvertAll(Presentation.StarMask(true), x => (int)x),
                frame = Array.ConvertAll(Outline.Alpha(Shape.Full, 100, 64, new Style(Mark.Normal, false), 2, 3, true, 0), x => (int)x),
                container = Array.ConvertAll(Outline.Alpha(Shape.Full, 100, 64, new Style(Mark.Normal, true), 2, 3, true, 0), x => (int)x) };
            System.IO.File.WriteAllText(args[1], System.Text.Json.JsonSerializer.Serialize(data));
        }
        return failed == 0 ? 0 : 1;
    }
}
