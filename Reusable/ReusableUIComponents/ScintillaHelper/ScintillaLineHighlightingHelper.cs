using System.Drawing;
using ScintillaNET;

namespace ReusableUIComponents.ScintillaHelper
{
    public class ScintillaLineHighlightingHelper
    {
        public void HighlightLine(Scintilla editor, int i, Color color)
        {
            Marker marker = editor.Markers[0];
            marker.Symbol = MarkerSymbol.Background;
            marker.SetBackColor(color);
            editor.Lines[i].MarkerAdd(0);
        }

        public void ClearAll(Scintilla editor)
        {
            editor.MarkerDeleteAll(-1);
        }
    }
}
