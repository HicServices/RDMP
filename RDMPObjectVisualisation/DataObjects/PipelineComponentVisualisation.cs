using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.Pipelines;
using RDMPObjectVisualisation.Pipelines;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;

namespace RDMPObjectVisualisation.DataObjects
{

    /// <summary>
    /// Shows the state of a single pipeline component within a data flow pipeline (See 'A Brief Overview Of What A Pipeline Is' in UserManual.docx).  This includes the Type of the pipeline
    /// component, whether the software was able to create an instance of the type and whether it passed checking.  Components can be either a Source, Middle (of which there can be any 
    /// number) or Destination.  Objects (usually DataTables) flow through the pipeline and are processed by each component in turn.
    /// 
    /// <para>But first the pipeline must be constructed, this component is used as part of PipelineDiagram to let you adjust the order / add new components etc into your pipeline.  The pipeline
    /// itself is stored as a reusable resource in the Catalogue Database. </para>
    /// </summary>
    [TechnicalUI]
    class PipelineComponentVisualisation:DataFlowComponentVisualisation
    {
        private bool _isSelected;

        private Pen _origFullPen;
        private Exception _exInitialization;

        public bool AllowDrag { get; set; }
        public bool AllowSelection { get; set; }
        public IPipelineComponent PipelineComponent { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                _fullPen = value ? new Pen(new SolidBrush(Color.Blue), 3.0f) : _origFullPen;
                Invalidate(true);
            }
        }

        public Exception ExInitialization
        {
            get { return _exInitialization; }
            set
            {
                _exInitialization = value;

                if (value != null)
                    ragSmiley1.OnCheckPerformed(new CheckEventArgs("Could not initialize object", CheckResult.Fail,_exInitialization));
            }
        }


        public PipelineComponentVisualisation(IPipelineComponent component, Role role, object valueOrNullIfBroken, Exception constructionExceptionIfAny, Func<DragEventArgs, DataFlowComponentVisualisation, DragDropEffects> shouldAllowDrop)
            : base(role, valueOrNullIfBroken, shouldAllowDrop)
        {
            PipelineComponent = component;

            if (constructionExceptionIfAny != null)
                ragSmiley1.OnCheckPerformed(new CheckEventArgs("Failed to construct component", CheckResult.Fail,constructionExceptionIfAny));

            _origFullPen = _fullPen;

            if (component == null)
                return;

            lblText.Text = component.GetClassNameLastPart();
            
            if (valueOrNullIfBroken == null)
                ragSmiley1.OnCheckPerformed(new CheckEventArgs("Could not construct object", CheckResult.Fail));

            this.Width = lblText.PreferredWidth + 80;

            _isEmpty = false;

            MouseDown += Anywhere_MouseDown;
            lblText.MouseDown += Anywhere_MouseDown;
            prongRight1.MouseDown += Anywhere_MouseDown;
            prongRight2.MouseDown += Anywhere_MouseDown;
            prongLeft1.MouseDown += Anywhere_MouseDown;
            prongLeft2.MouseDown += Anywhere_MouseDown;
            pComponent.MouseDown += Anywhere_MouseDown;
        }


        public event PipelineComponentSelectedHandler ComponentSelected;
        
        private void Anywhere_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button != MouseButtons.Left)
                return;

            if (AllowSelection)
            {
                IsSelected = true;
                ComponentSelected(this, PipelineComponent);
            }

            if (AllowDrag)
            {
                DoDragDrop(this, DragDropEffects.Move);
            }
        }
        
        public void Clear()
        {
            ragSmiley1.Reset();
        }
    }
}
