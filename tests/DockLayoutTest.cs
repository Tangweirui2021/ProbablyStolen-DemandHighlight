using System;
using DemandHighlight;

static class DockLayoutTest
{
    static int failed;
    static void Check(string name, bool value) { Console.WriteLine((value ? "ok    " : "FAIL  ") + name); if (!value) failed++; }
    static int Main()
    {
        var d = DockLayout.Fit(320,20,0,0,true,230,68);
        Check("dock occupies the gap below trash and left of inventory", d.Right == 316 && d.Bottom == 20 && d.Width == 94 && d.Height == 44);
        Check("normal gap has two checkbox columns", d.Visible && d.Columns == 2);
        d = DockLayout.Fit(300,20,0,0,true,230,68);
        Check("narrow gap switches to one column", d.Visible && d.Columns == 1 && d.Width == 74);
        d = DockLayout.Fit(500,-10,0,0,false,0,0);
        Check("dock stays above the viewport bottom", d.Bottom == 4 && d.Width == 100 && d.Height == 56);
        d = DockLayout.Fit(320,20,0,0,true,230,40);
        Check("insufficient height never covers the trashcan", !d.Visible);
        d = DockLayout.Fit(30,20,0,0,false,0,0);
        Check("insufficient width never covers the inventory", !d.Visible);
        d = DockLayout.Fit(0,-160,-320,-180,true,-90,-112);
        Check("centered canvas coordinates also fit", d.Visible && d.Right == -4 && d.Bottom == -160 && d.Height == 44);
        return failed == 0 ? 0 : 1;
    }
}
