// A stand-in UI/game API, ONLY for tests. Never compiled or shipped in the mod DLL.
// These tests exercise production code, but cannot establish compatibility with real game assemblies.
using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine
{
    public class Object
    {
        internal bool destroyed;
        public HideFlags hideFlags;
        public static void Destroy(Object o)
        {
            if (ReferenceEquals(o, null)) return;
            o.destroyed = true;
            var go = o as GameObject;
            if (!ReferenceEquals(go, null)) { go.activeSelf = false; go.transform.SetParent(null, false); go.transform.destroyed = true; }
        }
        public static void DontDestroyOnLoad(Object o) { }
        public static bool operator ==(Object a, Object b)
        {
            bool an = ReferenceEquals(a, null) || a.destroyed, bn = ReferenceEquals(b, null) || b.destroyed;
            return an || bn ? an == bn : ReferenceEquals(a, b);
        }
        public static bool operator !=(Object a, Object b) { return !(a == b); }
        public override bool Equals(object o) { return ReferenceEquals(this, o); }
        public override int GetHashCode() { return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this); }
    }
    public class Component : Object
    {
        public GameObject gameObject;
        public Transform transform { get { return gameObject.transform; } }
    }
    public class MonoBehaviour : Component { }
    public class GameObject : Object
    {
        public string name;
        public Transform transform;
        public bool activeSelf = true;
        public bool activeInHierarchy { get { return activeSelf && (transform.parent == null || transform.parent.gameObject.activeInHierarchy); } }
        readonly Dictionary<Type, Component> components = new Dictionary<Type, Component>();
        public GameObject(string name, params Type[] types)
        {
            this.name = name;
            transform = new RectTransform { gameObject = this };
            components[typeof(RectTransform)] = transform;
            foreach (var t in types) if (t != typeof(RectTransform)) AddComponent(t);
        }
        Component AddComponent(Type type) { var c = (Component)Activator.CreateInstance(type); c.gameObject = this; components[type] = c; return c; }
        public T AddComponent<T>() where T : Component, new() { return (T)AddComponent(typeof(T)); }
        public T GetComponent<T>() where T : Component { Component c; return components.TryGetValue(typeof(T), out c) ? c as T : null; }
        public void SetActive(bool active) { activeSelf = active; }
    }
    public class Transform : Component
    {
        public Transform parent;
        public readonly List<Transform> children = new List<Transform>();
        public int childCount { get { return children.Count; } }
        public Vector3 localScale = Vector3.one;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Transform GetChild(int i) { return children[i]; }
        public void SetParent(Transform p, bool worldPositionStays)
        {
            if (parent != null) parent.children.Remove(this);
            parent = p; if (p != null) p.children.Add(this);
        }
        public void SetAsLastSibling() { if (parent != null) { parent.children.Remove(this); parent.children.Add(this); } }
        public Vector3 TransformPoint(Vector3 p)
        {
            double angle=localRotation.Z*Math.PI/180.0, c=Math.Cos(angle),s=Math.Sin(angle);
            float x=p.x*localScale.x,y=p.y*localScale.y;
            var v = new Vector3((float)(x*c-y*s)+localPosition.x, (float)(x*s+y*c)+localPosition.y, p.z);
            return parent == null ? v : parent.TransformPoint(v);
        }
        public Vector3 InverseTransformPoint(Vector3 p)
        {
            if (parent != null) p = parent.InverseTransformPoint(p);
            double angle=-localRotation.Z*Math.PI/180.0,c=Math.Cos(angle),s=Math.Sin(angle);
            float x=p.x-localPosition.x,y=p.y-localPosition.y;
            return new Vector3((float)(x*c-y*s)/localScale.x, (float)(x*s+y*c)/localScale.y, p.z);
        }
    }
    public class RectTransform : Transform
    {
        public Rect rect = new Rect(0, 0, 64, 64);
        public Vector2 anchorMin, anchorMax, pivot, offsetMin, offsetMax, sizeDelta, anchoredPosition;
        public void GetWorldCorners(Vector3[] corners)
        {
            corners[0] = TransformPoint(new Vector3(rect.xMin, rect.yMin, 0));
            corners[1] = TransformPoint(new Vector3(rect.xMin, rect.yMin + rect.height, 0));
            corners[2] = TransformPoint(new Vector3(rect.xMin + rect.width, rect.yMin + rect.height, 0));
            corners[3] = TransformPoint(new Vector3(rect.xMin + rect.width, rect.yMin, 0));
        }
    }
    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero { get { return new Vector2(); } }
        public static Vector2 one { get { return new Vector2(1, 1); } }
    }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 one { get { return new Vector3(1, 1, 1); } }
    }
    public struct Vector4 { public float x,y,z,w; public Vector4(float x, float y, float z, float w) { this.x=x; this.y=y; this.z=z; this.w=w; } public static Vector4 zero { get { return new Vector4(); } } }
    public struct Quaternion
    {
        public float Z;
        public static Quaternion identity { get { return new Quaternion(); } }
        public static Quaternion Euler(float x,float y,float z) { return new Quaternion {Z=z}; }
    }
    public struct Rect
    {
        public float xMin, yMin, width, height;
        public float xMax { get { return xMin+width; } }
        public float yMax { get { return yMin+height; } }
        public Rect(float x, float y, float w, float h) { xMin=x; yMin=y; width=w; height=h; }
    }
    public class RectOffset { public RectOffset(int l,int r,int t,int b) { } }
    public enum TextAnchor { UpperLeft, MiddleLeft }
    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b, float a = 1f) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static bool operator ==(Color a, Color b) { return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a; }
        public static bool operator !=(Color a, Color b) { return !(a == b); }
        public override bool Equals(object o) { return o is Color && this == (Color)o; }
        public override int GetHashCode() { return r.GetHashCode(); }
    }
    public struct Color32 { public byte r, g, b, a; public Color32(byte r, byte g, byte b, byte a) { this.r = r; this.g = g; this.b = b; this.a = a; } }
    public class Texture2D : Object
    {
        public FilterMode filterMode; public TextureWrapMode wrapMode;
        public Color32[] Pixels;
        public Texture2D(int w, int h, TextureFormat format, bool mipmap) { }
        public void SetPixels32(Color32[] pixels) { Pixels = pixels; }
        public void Apply(bool update, bool unreadable) { }
    }
    public class Sprite : Object
    {
        public Texture2D texture;
        public static Sprite Create(Texture2D texture, Rect rect, Vector2 pivot, float ppu, uint extrude = 0,
            SpriteMeshType type = SpriteMeshType.FullRect, Vector4 border = default(Vector4)) { return new Sprite { texture = texture }; }
    }
    public class Canvas : Component { public float referencePixelsPerUnit = 100; }
    public enum HideFlags { HideAndDontSave }
    public enum TextureFormat { RGBA32 }
    public enum FilterMode { Point }
    public enum TextureWrapMode { Clamp }
    public enum SpriteMeshType { FullRect }
    public enum KeyCode { None, F5, F6 }
    public static class Input { public static bool GetKeyDown(KeyCode key) { return false; } }
    public static class PlayerPrefs
    {
        public static int GetInt(string key, int fallback) { return fallback; }
        public static void SetInt(string key, int value) { } public static void Save() { }
    }
    public static class Time { public static float unscaledTime; }
    public static class Debug
    {
        public static readonly List<string> Warnings = new List<string>();
        public static void Log(object x) { }
        public static void LogWarning(object x) { Warnings.Add(x.ToString()); Console.WriteLine("WARN " + x); }
    }
    public static class Mathf
    {
        public static int RoundToInt(float v) { return (int)Math.Round(v); }
        public static int Clamp(int v, int min, int max) { return Math.Min(max, Math.Max(min, v)); }
        public static float Clamp(float v, float min, float max) { return Math.Min(max, Math.Max(min, v)); }
        public static int Min(int a, int b) { return Math.Min(a, b); }
    }
}
namespace UnityEngine.UI
{
    public class Image : UnityEngine.Component
    {
        public enum Type { Simple, Sliced, Tiled }
        public bool raycastTarget = true, fillCenter;
        public bool enabled = true;
        public UnityEngine.Sprite sprite;
        public Type type;
        public UnityEngine.Color color = new UnityEngine.Color(1,1,1,1);
        public UnityEngine.Canvas canvas;
        public float pixelsPerUnitMultiplier;
    }
    public class LayoutElement : UnityEngine.Component
    {
        public bool ignoreLayout;
        public int layoutPriority = 1;
        public float minWidth, preferredWidth, flexibleWidth, minHeight, preferredHeight, flexibleHeight;
    }
    public class RectMask2D : UnityEngine.Component { }
    public class VerticalLayoutGroup : UnityEngine.Component
    {
        public UnityEngine.RectOffset padding;
        public float spacing;
        public bool childControlWidth, childControlHeight, childForceExpandWidth, childForceExpandHeight;
        public UnityEngine.TextAnchor childAlignment;
    }
}
namespace TMPro
{
    public enum TextOverflowModes { Ellipsis }
    public class TextMeshProUGUI
    {
        public bool enableAutoSizing, enableWordWrapping;
        public float fontSize;
        public TextOverflowModes overflowMode;
    }
}
namespace UnityEngine.SceneManagement
{
    public struct Scene { public string name; }
    public enum LoadSceneMode { Single, Additive }
    public static class SceneManager
    {
        public static event Action<Scene, LoadSceneMode> sceneLoaded;
        public static void Load(string name) { if (sceneLoaded != null) sceneLoaded(new Scene { name = name }, LoadSceneMode.Single); }
    }
}
namespace HarmonyLib
{
    public class HarmonyMethod { public MethodInfo Method; public HarmonyMethod(MethodInfo method) { Method = method; } }
    public class Harmony
    {
        public static readonly Dictionary<MethodBase, MethodInfo> Patches = new Dictionary<MethodBase, MethodInfo>();
        public Harmony(string id) { }
        public void Patch(MethodBase target, HarmonyMethod prefix = null, HarmonyMethod postfix = null) { Patches[target] = postfix.Method; }
        public void UnpatchAll(string id) { Patches.Clear(); }
        public static void Notify(MethodBase target, object instance, params object[] originalArgs)
        {
            MethodInfo m; if (!Patches.TryGetValue(target, out m)) return;
            var args = new object[m.GetParameters().Length];
            for (int i = 0; i < args.Length; i++)
            {
                string name = m.GetParameters()[i].Name;
                int index;
                if (name == "__instance") args[i] = instance;
                else if (name.StartsWith("__") && int.TryParse(name.Substring(2), out index)) args[i] = originalArgs[index];
            }
            m.Invoke(null, args);
        }
    }
}

public class ConfigEntry { public string Key; }
public class ConfigEntry<T> : ConfigEntry
{
    public T Value;
    public event Action<T> OnChanged;
    public void Set(T value) { Value = value; if (OnChanged != null) OnChanged(value); }
}
public class ModConfig
{
    public string ModId = DemandHighlight.DemandMod.ModId;
    public readonly Dictionary<string, object> Entries = new Dictionary<string, object>();
    public ConfigEntry<T> Define<T>(string key, T initial, string section, Func<string> label, Func<string> tooltip = null, bool restart = false)
    {
        var entry = new ConfigEntry<T> { Key = key, Value = initial }; Entries[key] = entry; return entry;
    }
    public ConfigEntry<int> Define(string key, int initial, string section, Func<string> label, int min, int max,
        Func<string> tooltip = null, bool restart = false) { return Define<int>(key, initial, section, label, tooltip, restart); }
}
public class CustomUIElement
{
    public UnityEngine.GameObject root;
    public TMPro.TextMeshProUGUI text = new TMPro.TextMeshProUGUI();
    public string id, Label;
    public bool Checked;
    public Action<bool> Toggle;
    public Action Click;
    public CustomUIElement SetFixedWidth(float width)
    {
        var layout = root.GetComponent<UnityEngine.UI.LayoutElement>() ?? root.AddComponent<UnityEngine.UI.LayoutElement>();
        layout.minWidth = layout.preferredWidth = width; layout.flexibleWidth = 0;
        return this;
    }
    public CustomUIElement SetPreferredSize(float w,float h) { return this; }
    public CustomUIElement SetTooltip(string s) { return this; }
    public CustomUIElement SetText(string s) { Label=s; return this; }
    public CustomUIElement SetBool(bool on,bool notify) { Checked=on; if(notify && Toggle!=null) Toggle(on); return this; }
}
public class CustomUIBuilder
{
    public CustomUIElement Last;
    public UnityEngine.RectTransform Host;
    public static List<CustomUIElement> Built = new List<CustomUIElement>();
    public static void ClearContainer(UnityEngine.RectTransform host) { Built.Clear(); }
    public static void RebuildLayout(UnityEngine.RectTransform host) { }
    public CustomUIBuilder BeginRow(float gap,UnityEngine.TextAnchor a) { return this; }
    public CustomUIBuilder BeginScroll(float height) { return this; }
    public CustomUIBuilder BeginGrid(int columns,float w,float h,float gap) { return this; }
    public CustomUIBuilder End() { return this; }
    CustomUIElement Add(string s,string id)
    {
        Last=new CustomUIElement {root=new UnityEngine.GameObject("Widget"),id=id,Label=s};
        if(Host!=null) Last.root.transform.SetParent(Host,false);
        Built.Add(Last); return Last;
    }
    public CustomUIBuilder AddLabel(string s,string id) { Add(s,id); return this; }
    public CustomUIBuilder AddButton(string s,Action click,string id) { Add(s,id).Click=click; return this; }
    public CustomUIBuilder AddToggle(string s,bool on,Action<bool> edit,string id) { var e=Add(s,id);e.Checked=on;e.Toggle=edit;return this; }
}
public class CustomUIManager : UnityEngine.MonoBehaviour
{
    public static CustomUIManager Instance;
    public CustomUIBuilder BuildInto(UnityEngine.RectTransform host,bool meta,string id) { return new CustomUIBuilder {Host=host}; }
}
public static class ModConfigWindow
{
    // Mirrors the native BuildEntry's fixed width and the priority-1 conflict in the supplied game/TMP IL.
    static void BuildEntry(CustomUIBuilder builder, ModConfig config, ConfigEntry entry)
    {
        builder.Last = new CustomUIElement { root = new UnityEngine.GameObject("input_" + entry.Key) };
        builder.Last.SetFixedWidth(245);
        HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(), null, builder, config, entry);
    }
    public static CustomUIElement BuildTestEntry(ModConfig config, ConfigEntry entry)
    {
        var builder = new CustomUIBuilder(); BuildEntry(builder, config, entry); return builder.Last;
    }
}
public class UISettings : UnityEngine.MonoBehaviour
{
    public static UISettings current = new UISettings();
    public int gridSize = 16;
}
public static class ModHelper
{
    public static readonly ModConfig Config = new ModConfig();
    public static ModConfig GetConfig(string id) { return Config; }
    public static string GetLocalized(string id, string key) { return key; }
    public static void AddGameOptionKeyBind(string mod, string section, string key, Func<string> label,
        Func<UnityEngine.KeyCode> get, Action<UnityEngine.KeyCode> set, Func<string> tip) { }
}
public interface IMod { void Init(ModManifest manifest); void OnEnable(); void OnDisable(); }
public class ModManifest { public string ID; }
public static class KeyBindHelper { public static bool SuppressHotkeys; }
public static class TypeHelper { public static readonly List<string> ALL_TYPES_LIST = new List<string> { "FOOD", "TOOL", "CONTRABAND" }; }
public class GraphNodeStorage { public List<GraphNodeStorage> children = new List<GraphNodeStorage>(); }
public class GameInventory : GraphNodeStorage
{
    public List<GameItem> childItems = new List<GameItem>(); public GameItem Parent;
    public GameItem GetParentItem() { return Parent; }
}
public class TagState { public bool Enabled; public bool GetBool() { return Enabled; } }
public class ModifiedState { public Dictionary<string, TagState> dict = new Dictionary<string, TagState>(); }
public class ItemCondition { public string identifier; }
public class ItemFeature { public bool useCondition; public ItemCondition GetCurrentItemCondition() { return null; } }
public class GameItem : GraphNodeStorage
{
    public Func<GameItem, GameInventory, bool> mayInventoryRemoveItemFunc;
    public string identifier = "test";
    public bool Stolen, Contraband, Owned = true, Immovable;
    public GameInventory parentInventory;
    public List<string> itemTypes = new List<string>();
    public List<ItemFeature> itemFeatures = new List<ItemFeature>();
    public ModifiedState modifiedState = new ModifiedState();
    public PixelWindow contentWindow;
    public GridShape shape, modifiedShape;
    public long GetCurrentValue(bool a,bool b,bool c,bool d,long e) { return 1; }
    public int GetItemSize() { return shape==null?0:shape.width*shape.height; }
    public bool IsTag(string tag) { TagState state; return modifiedState.dict.TryGetValue(tag, out state) && state.Enabled; }
    public bool IsGameItemType(string type) { return itemTypes.Contains(type); }
    public bool IsMovable() { return !Immovable; }
    public void EnableTag(string key, bool validate) { }
    public void DisableTag(string key, bool validate) { }
    public void SetEnableDisableTag(string key, bool validate) { }
    public void ModifyTag(string key, Action<TagState> change, bool validate) { }
    public void SetGameItemType(string type) { }
    public void AddParent(GraphNodeStorage node) { }
    public void RemoveParent(GraphNodeStorage node) { }
}
public class GameItemElement : GameItem
{
    public UnityEngine.RectTransform rectTransform;
    public TreeNodeRender rotator, fakeRotator, background, fakeBackground, fakeMiddle;
    bool destroyed;
    public GameItemElement()
    {
        rectTransform = (UnityEngine.RectTransform)new UnityEngine.GameObject("Item").transform;
        rotator=TreeNodeRender.Create("Rotator");rotator.rectTransform.SetParent(rectTransform,false);
        background = TreeNodeRender.Create("Background");
        background.rectTransform.SetParent(rotator.rectTransform,false);
        fakeMiddle = TreeNodeRender.Create("Drag view");
        fakeRotator=TreeNodeRender.Create("Drag rotator");fakeRotator.rectTransform.SetParent(fakeMiddle.rectTransform,false);
        fakeBackground = TreeNodeRender.Create("Drag background");
        fakeBackground.rectTransform.SetParent(fakeRotator.rectTransform,false);
        fakeBackground.image.enabled = false;
        HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(), this);
    }
    public void Validate() { HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(), this); }
    void Rotate(float amount)
    {
        rotator.rectTransform.localRotation=UnityEngine.Quaternion.Euler(0,0,rotator.rectTransform.localRotation.Z+amount);
        fakeRotator.rectTransform.localRotation=rotator.rectTransform.localRotation;
        UpdateRectTransform();
    }
    void Mirror()
    {
        var s=rotator.rectTransform.localScale;
        rotator.rectTransform.localScale=new UnityEngine.Vector3(-s.x,s.y,s.z);
        fakeRotator.rectTransform.localScale=rotator.rectTransform.localScale;
        UpdateRectTransform();
    }
    public void RotCW() { GridRotCW();HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(), this); }
    public void RotCCW() { GridRotCCW();HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(), this); }
    public void Rot180() { GridRot180();HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(), this); }
    public void FlipX() { GridFlipX();HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(), this); }
    public void FlipY() { GridFlipY();HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(), this); }
    public void GridRotCW() { Rotate(-90);HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(), this); }
    public void GridRotCCW() { Rotate(90);HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(), this); }
    public void GridRot180() { Rotate(180);HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(), this); }
    public void GridFlipX() { Mirror();HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(), this); }
    public void GridFlipY() { Mirror();Rotate(180);HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(), this); }
    public void UpdateRectTransform() { HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(), this); }
    public bool IsDestroyed() { return destroyed; }
    public void Destroy() { destroyed = true; HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(), this); }
    public void ToggleBackground(bool visible, float opacity, float brightness)
    {
        background.image.enabled = visible;
        background.image.color = new UnityEngine.Color(brightness,brightness,brightness,opacity);
        HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(), this);
    }
    public void ToggleFakeBackground(bool visible, UnityEngine.Vector4 color)
    {
        fakeBackground.image.enabled = visible;
        fakeBackground.image.color = new UnityEngine.Color(color.x,color.y,color.z,color.w);
        HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(), this);
    }
}
public class TreeNodeRender : UnityEngine.Component
{
    public UnityEngine.RectTransform rectTransform { get { return (UnityEngine.RectTransform)transform; } }
    public UnityEngine.UI.Image image { get { return gameObject.GetComponent<UnityEngine.UI.Image>(); } }
    public static TreeNodeRender Create(string name)
    {
        var go = new UnityEngine.GameObject(name);
        go.AddComponent<UnityEngine.UI.Image>();
        return go.AddComponent<TreeNodeRender>();
    }
}
public class GridShape { public int width = 1, height = 1, orientation; public bool flipped; public int GetLocal(int x, int y) { return 1; } }
public class PixelWindow
{
    public bool isLocked, Visible;
    public object childElement;
    public List<GameItem> parentItems = new List<GameItem>();
    public bool IsVisible() { return Visible; }
    public UnityEngine.RectTransform rectTransform;
    public void Show() { Visible = true; HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(),this); }
    public void Hide() { Visible = false; HarmonyLib.Harmony.Notify(MethodBase.GetCurrentMethod(),this); }
}
public class MachineryTabs
{
    public int count;
    public UnityEngine.RectTransform rectTransform;
    public object GetPage(int i) { return null; }
}
public class MachineryIntake { public object content; }
public class MachineryPanel { public object content; }
public class MachineryPanelRow
{
    public List<MachineryPanel> Panels = new List<MachineryPanel>();
    public MachineryPanel GetPanel(string role) { return null; }
}
public static class ContrabandHelper
{
    public static bool IsContraband(GameItem item) { return item.Contraband; }
    public static bool IsBlockSearchContainer(GameItem item) { return false; }
}
public static class StolenHelper { public static bool IsStolenItem(GameItem item) { return item.Stolen; } }
public static class GeneralHelper { public static bool IsItemOwned(GameItem item) { return item.Owned; } }
public static class LockHelper { public static bool IsLockedContainer(GameItem item) { return item.IsTag("LOCKED_CONTAINER_TAG"); } }
public class EmporiumEntry
{
    public static EmporiumEntry Instance;
    public GameInventory invElement, frontInvinvElement, backInvinvElement, backInvinvElementCounter,
        showcaseElement, bazarLeftinvElement, hiddenElement, soldElement, trashInvElement;
    public PixelWindow invWindow, trashInvWindow;
}
public class StoreClient
{
    public string identifier;
    public int clientIntent;
    public List<string> clientBuyingTagList = new List<string>(), clientBuyingIdList = new List<string>(), varietyTrackingList = new List<string>();
    public bool isRequireVariety;
    public BarterOffer barterOffer;
    public bool IsClientBuyingThisItem(GameItem item, bool ignoreContents) { return item.IsGameItemType("FOOD"); }
}
public class BarterOffer { public List<BarterCondition> conditions; }
public class BarterCondition { public string requiredItemId, requiredItemTag; public List<string> blacklistedConditionId; }
public class ClientInstance { public StoreClient Blueprint; public StoreClient GetClientBlueprint() { return Blueprint; } }
public class PlayerStore { public static PlayerStore Instance; public ClientInstance currentClientInstance; }
namespace DemandHighlight
{
    // Unmodified peripheral UI/API is replaced in this integration fixture.
    public static class Panel
    {
        public static Func<string, string> L; public static Action Changed; public static Func<bool> Numbers;
        public static bool Typing;
        public static int Toggles;
        public static IEnumerable<string> KnownTypes() { return TypeHelper.ALL_TYPES_LIST; }
        public static string TypeName(string type) { return type; }
        public static void SetNumbers(bool value) { } public static void Toggle() { Toggles++; }
        public static void Refresh() { } public static void Destroy() { }
    }
    public static class Marks { public static void Offer() { } public static void Withdraw() { } public static void Release() { } }
    public static class QueryHost { public static void Publish() { } public static void Withdraw() { } }
}
