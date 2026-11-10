using System;
using DemandHighlight;

static class PresentationTest
{
    static int failed;
    static void Check(string label, bool value)
    {
        Console.WriteLine((value ? "ok    " : "FAIL  ") + label);
        if (!value) failed++;
    }
    static int Main()
    {
        Check("ordinary item: no star", Presentation.RiskOf(false, false) == Risk.None);
        Check("stolen only: yellow-star category", Presentation.RiskOf(true, false) == Risk.Stolen);
        Check("contraband only: green-star category", Presentation.RiskOf(false, true) == Risk.Contraband);
        Check("both statuses: distinct red-star category", Presentation.RiskOf(true, true) == Risk.Both);
        foreach (var mark in new[] { Mark.Normal, Mark.Stolen, Mark.Contraband })
        {
            var direct = Presentation.DemandStyle(new Style(mark, false));
            var inside = Presentation.DemandStyle(new Style(mark, true));
            Check("demand outline ignores legal status " + mark, direct.Mark == Mark.Normal && !direct.Contents);
            Check("container keeps corner style " + mark, inside.Mark == Mark.Normal && inside.Contents);
        }
        Check("unwanted item stays without an outline", Presentation.DemandStyle(default(Style)).Mark == Mark.None);
        var order = new[] { "MEDICAL", "FOOD", "TOOL" };
        Check("multiple types use configured priority", Presentation.SelectType(new[] { "FOOD", "TOOL", "MEDICAL" }, order) == "MEDICAL");
        Check("type-list ordering cannot change a colour", Presentation.SelectType(new[] { "TOOL", "FOOD" }, order) ==
            Presentation.SelectType(new[] { "FOOD", "TOOL" }, order));
        Check("legal categories never choose frame colour", Presentation.SelectType(new[] { "CONTRABAND", "RESTRICTED_1", "FOOD" }, order) == "FOOD");
        Check("only legal categories fall back", Presentation.SelectType(new[] { "CONTRABAND", "STOLEN" }, order) == null);
        Check("unknown mod types have stable ordering", Presentation.SelectType(new[] { "Z_NEW", "A_NEW" }, order) == "A_NEW");
        Check("empty type list falls back", Presentation.SelectType(null, order) == null);
        Check("priority is case insensitive", Presentation.SelectType(new[] { "food" }, order) == "food");

        var buyer = new ClientView { Intent = Intent.Buy, Token = "1", BuyingTags = new[] { "FOOD" } };
        Check("buyers still use the game's acceptance oracle", Presentation.DemandQuery(buyer, null).Kind == Query.Op.Want);
        Check("no customer means no demand outline", Presentation.DemandQuery(null, null).Kind == Query.Op.Nothing);
        Check("inspection adds no legal-status outlines", Presentation.DemandQuery(new ClientView { Intent = Intent.Inspection }, null).Kind == Query.Op.Nothing);
        Check("seller goods do not become demand outlines", Presentation.DemandQuery(new ClientView { Intent = Intent.Sell }, null).Kind == Query.Op.Nothing);
        Check("manual filter still works without a customer", Presentation.DemandQuery(null, new FilterView { Expression = Pred.Type("FOOD") }).Kind == Query.Op.Filter);
        Check("manual filter still works during inspection", Presentation.DemandQuery(new ClientView { Intent = Intent.Inspection },
            new FilterView { Expression = Pred.Stolen }).Kind == Query.Op.Filter);
        MarkerColor color;
        Check("six-digit hex defaults to opaque", MarkerColor.TryParse("#FFD84A", out color) && color.Hex == "#FFD84AFF");
        Check("eight-digit hex preserves opacity", MarkerColor.TryParse("66ed8080", out color) && color.Hex == "#66ED8080");
        Check("bad hex rejected", !MarkerColor.TryParse("#GG0000", out color));
        Check("short hex rejected", !MarkerColor.TryParse("#FF0", out color));
        Check("null hex rejected", !MarkerColor.TryParse(null, out color));
        var fill = Presentation.StarMask(false);
        var edge = Presentation.StarMask(true);
        int n = Presentation.StarSide, filled = 0, outlined = 0;
        bool symmetric = true, disjoint = true, padding = true;
        for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
        {
            int i = y * n + x;
            if (fill[i] != 0) filled++;
            if (edge[i] != 0) outlined++;
            symmetric &= fill[i] == fill[y * n + n - x - 1] && edge[i] == edge[y * n + n - x - 1];
            disjoint &= fill[i] == 0 || edge[i] == 0;
            if (x == 0 || x == n - 1 || y == 0 || y == n - 1) padding &= fill[i] == 0;
        }
        Check("pixel star is nonempty and symmetric", filled > 20 && symmetric);
        Check("pixel outline is separate and padded", outlined > 0 && disjoint && padding);
        return failed == 0 ? 0 : 1;
    }
}
