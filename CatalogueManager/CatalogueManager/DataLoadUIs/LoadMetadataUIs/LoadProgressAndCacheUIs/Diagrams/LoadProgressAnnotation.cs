using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using CatalogueLibrary.Data;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs.Diagrams
{
    public class LoadProgressAnnotation
    {
        private readonly LoadProgress _lp;
        private readonly DataTable _dt;

        public  LineAnnotation LineAnnotationOrigin { get; private set; }
        public TextAnnotation TextAnnotationOrigin { get; private set; }

        public LineAnnotation LineAnnotationFillProgress { get; private set; }
        public TextAnnotation TextAnnotationFillProgress { get; private set; }

        public LineAnnotation LineAnnotationCacheProgress { get; private set; }
        public TextAnnotation TextAnnotationCacheProgress { get; private set; }
        
        public LoadProgressAnnotation(LoadProgress lp,DataTable dt, Chart chart)
        {
            _lp = lp;
            _dt = dt;

            LineAnnotation line;
            TextAnnotation text;
            GetAnnotations("OriginDate",0.9,lp.OriginDate, chart, out line, out text);
            LineAnnotationOrigin = line;
            TextAnnotationOrigin = text;


            LineAnnotation line2;
            TextAnnotation text2;
            GetAnnotations("Progress", 0.7, lp.DataLoadProgress, chart, out line2, out text2);
            LineAnnotationFillProgress = line2;
            TextAnnotationFillProgress = text2;

            var cp = lp.CacheProgress;

            if(cp != null)
            {
                LineAnnotation line3;
                TextAnnotation text3;
                GetAnnotations("Cache Fill", 0.50, cp.CacheFillProgress, chart, out line3, out text3);
                LineAnnotationCacheProgress = line3;
                TextAnnotationCacheProgress = text3;
            }


        }

        private void GetAnnotations(string label, double fractionalHeightOfLabel,DateTime? date, Chart chart, out LineAnnotation line, out TextAnnotation text)
        {
            string originText;
            double anchorX;
            double maxY = GetMaxY(_dt);

            //display the text labels half way allong the chart
            double textAnchorY = maxY * fractionalHeightOfLabel;

            if (date == null)
            {
                originText = "unknown";
                anchorX = 1;
            }
            else
            {
                originText = date.Value.ToString("yyyy-MM-dd");
                anchorX = GetXForDate(_dt, date.Value) + 1;
            }

            line = new LineAnnotation();
            line.IsSizeAlwaysRelative = false;
            line.AxisX = chart.ChartAreas[0].AxisX;
            line.AxisY = chart.ChartAreas[0].AxisY;
            line.AnchorX = anchorX;
            line.AnchorY = 0;
            line.Height = maxY * 2;
            line.LineWidth = 1;
            line.LineDashStyle = ChartDashStyle.Dot;
            line.Width = 0;
            line.LineWidth = 2;
            line.StartCap = LineAnchorCapStyle.None;
            line.EndCap = LineAnchorCapStyle.None;
            line.AllowSelecting = true;
            line.AllowMoving = true;

            text = new TextAnnotation();

            text.Text = label +":" + Environment.NewLine + originText;
            text.IsSizeAlwaysRelative = false;
            text.AxisX = chart.ChartAreas[0].AxisX;
            text.AxisY = chart.ChartAreas[0].AxisY;
            text.AnchorX = anchorX;
            text.AnchorY = textAnchorY;

            text.ForeColor = date == null ? Color.Red : Color.Green;
            text.Font = new Font(text.Font, FontStyle.Bold);


        }

        private int GetXForDate(DataTable dt, DateTime value)
        {
            var year = value.Year;
            var month = value.Month;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                int currentYear = Convert.ToInt32(dt.Rows[i]["Year"]);
                int currentMonth = Convert.ToInt32(dt.Rows[i]["Month"]);

                //we have overstepped
                if (year < currentYear)
                    return i;
                
                //if we are on the month or have overstepped it (axis can have gaps)
                if (year == currentYear && month <= currentMonth)
                    return i;
                
                //if there is no more data, return the last row
                if (i + 1 == dt.Rows.Count)
                    return i;
            }

            throw new Exception("Should have returned the last row or the first row by now");
        }

        private double GetMaxY(DataTable dt)
        {
            int max = 0;
            int colCount = dt.Columns.Count;
            foreach (DataRow r in dt.Rows)
            {
                int totalForRow = 0;
                for (int i = 3; i < colCount; i++)
                    totalForRow += Convert.ToInt32(r[i]);

                max = Math.Max(max, totalForRow);
            }

            return max;
        }

        public void OnAnnotationPositionChanged(object sender, EventArgs eventArgs)
        {
            if (sender == LineAnnotationOrigin)
            {
                var newDate = GetDateFromX((int)LineAnnotationOrigin.X);

                if (MessageBox.Show("Set new LoadProgress Origin date to " + newDate + "?", "Change Origin Date?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _lp.OriginDate = newDate;
                    _lp.SaveToDatabase();
                }
            }

            if (sender == LineAnnotationFillProgress)
            {
                var newDate = GetDateFromX((int)LineAnnotationFillProgress.X);

                if (MessageBox.Show("Set new LoadProgress Fill date to " + newDate + "?", "Change Fill Date?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _lp.DataLoadProgress = newDate;
                    _lp.SaveToDatabase();
                }
            }

            if (sender == LineAnnotationCacheProgress)
            {
                var cp = _lp.CacheProgress;

                if(cp == null)
                    return;

                var newDate = GetDateFromX((int)LineAnnotationCacheProgress.X);

                if (MessageBox.Show("Set new CacheProgress date to " + newDate + "?", "Change Cache Progress Date?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    cp.CacheFillProgress = newDate;
                    cp.SaveToDatabase();
                }
            }
        }

        private DateTime? GetDateFromX(int x)
        {
            x = Math.Max(0, x - 1);//subtract 1 because X axis on chart starts at 1 but data table starts at 0, also prevents them dragging it super negative
            x = Math.Min(x, _dt.Rows.Count - 1);

            int year = Convert.ToInt32(_dt.Rows[x]["Year"]);
            int month = Convert.ToInt32(_dt.Rows[x]["Month"]);

            return  new DateTime(year, month, 1);

        }
    }
}
