using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReusableUIComponents;

namespace RDMPObjectVisualisation.Pipelines.Models
{
    /// <summary>
    /// Used in PipelineDiagram to indicate an area you can drag and drop a 'Middle' flow component into your Pipeline.  Also allows you to drag and drop 'Middle' flow components that
    /// are already in the pipeline to reorder them.
    /// </summary>
    [TechnicalUI]
    public partial class DividerLineControl : UserControl
    {
        private readonly Func<DragEventArgs, DragDropEffects> _shouldAllowDrop;
        private Pen _pen;

        public DividerLineControl(Func<DragEventArgs, DragDropEffects> shouldAllowDrop)
        {
            _shouldAllowDrop = shouldAllowDrop;
            InitializeComponent();
            _pen = new Pen(new SolidBrush(Color.Black));
            _pen.DashPattern = new float[] { 4.0F, 2.0F, 1.0F, 3.0F };
            
            pbDividerLine.Visible = false;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            e.Graphics.DrawRectangle(_pen, 2,2,Width -4,Height -4);
        }

        private void DividerLineControl_DragOver(object sender, DragEventArgs e)
        {
            
        }

        private void DividerLineControl_DragLeave(object sender, EventArgs e)
        {
            pbDividerLine.Visible = false;
            pbDropPrompt.Visible = true;
        }

        private void DividerLineControl_DragEnter(object sender, DragEventArgs e)
        {
            var shouldAllow = _shouldAllowDrop(e);

            if (shouldAllow != DragDropEffects.None)
            {
                pbDividerLine.Visible = true;
                pbDropPrompt.Visible = false;
            }

            e.Effect = shouldAllow;
        }

    }
}
