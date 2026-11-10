using System;
using System.Collections.Generic;
using UnityEngine;

namespace DemandHighlight
{
    // The game supplies a generated settings panel and persists the entries to ModConfigs/<id>.json.
    // No custom file access, YAML dependency, restart or external loader is needed.
    public static class MarkerSettings
    {
        public const string DefaultPriority = "FRESH_FOOD,PROCESSED_FOOD,TREAT,WATER_RATION,WATER,ALCOHOL,BREVAGE,FOOD," +
            "MEDICAL_TOOL,MEDICAL,ORGAN,MELEE_WEAPON,FIREARM,AMMUNITION,MUNITION,GUN_MOD,WEAPON,ARMOR," +
            "WATER_PURIFICATION_SUPPLY,CHEMICAL_SUPPLIES,MEDICAL_SUPPLIES,FARMING_SUPPLIES,OXYGEN_SUPPLY," +
            "ENERGY_CELL,FILTER,MODIFIED_SEED,SEED,REFINED_MATERIAL,BASIC_MATERIAL,MATERIAL," +
            "HYDROPONIC_MODULE,ALARM_MODULE,PRINTER_MODULE,EVAPORATOR_MODULE,MODULE,NODE,MACHINE," +
            "TOOL,DOCUMENT,ACCESS_CARD,LOTTERY,ART,SUBSTANCE,POISON,NARCOTIC,HOUSEHOLD_GOOD,JUNK," +
            "SUPPLY_CRATE,STORAGE,NOT_FOR_RESALE,MODDABLE,LUXURY_ITEM,RARE,HIGHEND,MIDLINE,LOWTECH,MAKESHIFT";

        static ModConfig config;
        static Func<string, string> localize;
        static Action changed;
        static readonly Dictionary<string, Color> typeColors = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);
        static string[] priority = DefaultPriority.Split(',');
        public static int Generation { get; private set; }
        public static Color DefaultColor = Hex("#5CEB6B"), ContainerColor = Hex("#80D8FF");
        public static Color StolenColor = Hex("#FFD84A"), ContrabandColor = Hex("#66ED80"), BothColor = Hex("#FF5656");
        public static Color StarOutlineColor = Hex("#211C30");
        public static int StarScale = 2, StarInset = 2;
        public static bool StarsEnabled = true;

        public static void Init(string modId, Func<string, string> l, Action onChanged)
        {
            if (config != null) return;
            localize = l; changed = onChanged;
            config = ModHelper.GetConfig(modId);
            ColorOption("defaultOutlineColor", "#5CEB6B", "Outline colors / 框线颜色", () => L("opt_default_color"), c => DefaultColor = c);
            ColorOption("containerOutlineColor", "#80D8FF", "Outline colors / 框线颜色", () => L("opt_container_color"), c => ContainerColor = c);
            ColorOption("stolenStarColor", "#FFD84A", "Risk stars / 属性星标", () => L("opt_stolen_color"), c => StolenColor = c);
            ColorOption("contrabandStarColor", "#66ED80", "Risk stars / 属性星标", () => L("opt_contraband_color"), c => ContrabandColor = c);
            ColorOption("bothStarColor", "#FF5656", "Risk stars / 属性星标", () => L("opt_both_color"), c => BothColor = c);
            ColorOption("starOutlineColor", "#211C30", "Risk stars / 属性星标", () => L("opt_star_edge"), c => StarOutlineColor = c);
            var enabled = config.Define("riskStars", true, "Risk stars / 属性星标", () => L("opt_stars"));
            StarsEnabled = enabled.Value;
            enabled.OnChanged += v => { StarsEnabled = v; Touch(); };
            var scale = config.Define("starPixelScale", 2, "Risk stars / 属性星标", () => L("opt_star_scale"), 1, 4);
            StarScale = scale.Value;
            scale.OnChanged += v => { StarScale = v; Touch(); };
            var inset = config.Define("starInset", 2, "Risk stars / 属性星标", () => L("opt_star_inset"), 0, 16);
            StarInset = inset.Value;
            inset.OnChanged += v => { StarInset = v; Touch(); };
            var order = config.Define("typePriority", DefaultPriority, "Outline colors / 框线颜色", () => L("opt_type_priority"));
            ReadPriority(order.Value);
            order.OnChanged += v => { ReadPriority(v); Touch(); };
            foreach (var type in Panel.KnownTypes()) EnsureType(type);
            var all = TypeHelper.ALL_TYPES_LIST;
            if (all != null) foreach (var type in all) EnsureType(type);
        }

        static string L(string key) { return localize != null ? localize(key) : key; }
        static Color Hex(string hex)
        {
            MarkerColor c;
            MarkerColor.TryParse(hex, out c);
            return new Color(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }
        static void Touch() { Generation++; if (changed != null) changed(); }
        static void ReadPriority(string text)
        {
            var list = new List<string>();
            foreach (var piece in (text ?? "").Split(','))
                if (!string.IsNullOrEmpty(piece.Trim())) list.Add(piece.Trim());
            priority = list.ToArray();
        }

        static void ColorOption(string key, string initial, string section, Func<string> label, Action<Color> apply)
        {
            var entry = config.Define(key, initial, section, label, tooltip: () => L("opt_hex_tip"));
            string last = initial;
            Action<string> read = value => {
                MarkerColor parsed;
                if (MarkerColor.TryParse(value, out parsed)) { last = parsed.Hex; apply(Hex(last)); }
                else { Debug.LogWarning(DemandMod.Tag + "invalid color for " + key + "; using " + last); apply(Hex(last)); }
            };
            read(entry.Value);
            entry.OnChanged += value => { read(value); Touch(); };
        }

        public static void EnsureType(string type)
        {
            if (!Presentation.ProductType(type) || typeColors.ContainsKey(type)) return;
            string initial = DefaultFor(type);
            typeColors[type] = Hex(initial);
            if (config == null) return;
            ColorOption("typeColor_" + type, initial, "Item type colors / 物品类型颜色",
                () => Panel.TypeName(type) + " (" + type + ")", c => typeColors[type] = c);
        }

        public static Color Outline(IList<string> types, bool contents)
        {
            if (contents) return ContainerColor;
            var type = Presentation.SelectType(types, priority);
            if (type == null) return DefaultColor;
            EnsureType(type);
            Color c;
            return typeColors.TryGetValue(type, out c) ? c : DefaultColor;
        }

        public static Color Star(Risk risk)
        {
            return risk == Risk.Both ? BothColor : risk == Risk.Contraband ? ContrabandColor : StolenColor;
        }

        static string DefaultFor(string type)
        {
            switch (type.ToUpperInvariant())
            {
                case "FOOD": case "FRESH_FOOD": case "PROCESSED_FOOD": case "TREAT": return "#F5B66C";
                case "WATER": case "WATER_RATION": case "BREVAGE": return "#60CFFF";
                case "ALCOHOL": return "#DD97EB";
                case "MEDICAL": case "MEDICAL_TOOL": case "MEDICAL_SUPPLIES": case "ORGAN": return "#70E0B4";
                case "TOOL": return "#FFDB76";
                case "WEAPON": case "FIREARM": case "MELEE_WEAPON": case "AMMUNITION": case "MUNITION": return "#ED9690";
                case "MATERIAL": case "BASIC_MATERIAL": case "REFINED_MATERIAL": return "#C0B4A2";
                case "MACHINE": case "MODULE": case "NODE": return "#A3A4FF";
                case "SEED": case "MODIFIED_SEED": case "FARMING_SUPPLIES": return "#A9D778";
                case "ENERGY_CELL": return "#F3F18C";
                case "JUNK": return "#A1A4AF";
                case "ART": case "LUXURY_ITEM": return "#F39ED2";
                default:
                    var palette = new[] { "#83C5EF", "#E5A77B", "#B7A0E8", "#A8CE88", "#E699B7", "#78CEBF", "#D9CD87" };
                    uint hash = 2166136261u;
                    foreach (char c in type.ToUpperInvariant()) hash = unchecked((hash ^ c) * 16777619u);
                    return palette[hash % (uint)palette.Length];
            }
        }
    }
}
