// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Windows.Forms;


namespace Rdmp.UI.PipelineUIs.Pipelines.Models
{
    /// <summary>
    /// Used in <see cref="PipelineDiagramUI"/> to indicate an area you can drag and drop a 'Middle' flow component into your Pipeline.  Also allows you to drag and drop 'Middle' flow components that
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
