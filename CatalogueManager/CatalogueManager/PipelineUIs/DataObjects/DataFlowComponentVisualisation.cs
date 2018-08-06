using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;

namespace CatalogueManager.PipelineUIs.DataObjects
{
    /// <summary>
    /// TECHNICAL: Base class for PipelineComponentVisualisation but can also include empty components (no type selected yet)
    /// </summary>
    [TechnicalUI]
    public partial class DataFlowComponentVisualisation : UserControl
    {
        public object Value { get; set; }
        private readonly Role _role;
        private ICheckable _checkable;
        public bool IsLocked
        {
            get { return pbPadlock.Visible; }
            set { pbPadlock.Visible = value; }
        }
        private readonly Func<DragEventArgs, DataFlowComponentVisualisation, DragDropEffects> _shouldAllowDrop;

        public DataFlowComponentVisualisation()
            : this(Role.Middle, null,null)
        {
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime) //dont connect to database in design mode unless they passed in a fist full of nulls
                throw new NotSupportedException("Do not use this constructor, it is for use by Visual Studio Designer");
        }

        public Role GetRole()
        {
            return _role;
        }

        public DataFlowComponentVisualisation(Role role, object value, Func<DragEventArgs, DataFlowComponentVisualisation, DragDropEffects> shouldAllowDrop)
        {
            Value = value;
            _role = role;
            _shouldAllowDrop = shouldAllowDrop;
            InitializeComponent();

            _fullPen.Width = 2;
            _emptyPen.Width = 2;
            _emptyPen.DashPattern = new float[] { 4.0F, 2.0F, 1.0F, 3.0F };

            if (value == null)
            {
                IsLocked = false;//cannot be locked AND empty!
                _isEmpty = true;
                lblText.Text = "Empty";
                AllowDrop = _shouldAllowDrop != null;
            }
            else
            {
                lblText.Text = value.ToString();//.GetType().Name;
                GenerateToolTipBasedOnProperties(value);
            }

            switch (role)
            {
                case Role.Source:
                    prongLeft1.Visible = false;
                    prongLeft2.Visible = false;
                    break;
                case Role.Middle:
                    break;
                case Role.Destination:
                    prongRight1.Visible = false;
                    prongRight2.Visible = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("role");
            }

            
            _checkable = value as ICheckable;
            this.Width = lblText.PreferredWidth + 80;
        }

        ToolTip _toolTip = new ToolTip();

        private void GenerateToolTipBasedOnProperties(object value)
        {
            StringBuilder toolTip = new StringBuilder();

            //get all the properties that must be set on AnySeparatorFileAttacher (Those marked with the attribute DemandsInitialization
            var propertiesWeHaveToSet =
                value.GetType().GetProperties()
                    .Where(p => p.GetCustomAttributes(typeof(DemandsInitializationAttribute), true).Any())
                    .ToArray();
            foreach (PropertyInfo p in propertiesWeHaveToSet)
            {
                object propValue = p.GetValue(value);

                if (propValue == null || string.IsNullOrWhiteSpace(propValue.ToString()))
                    propValue = "<NotSet>";

                toolTip.AppendLine(p.Name + ":" + propValue);
            }

            string result = toolTip.ToString();

            if(!string.IsNullOrWhiteSpace(result))
                _toolTip.SetToolTip(lblText,
                    "Arguments:" + Environment.NewLine + 
                    result);
        }

        protected bool _isEmpty ;
        Pen _emptyPen = new Pen(new SolidBrush(Color.Black));
        protected Pen _fullPen = new Pen(new SolidBrush(Color.Black));
    
        public enum Role
        {
            Source,
            Middle,
            Destination
        }
        
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            
            if (_isEmpty)
                e.Graphics.DrawRectangle(_emptyPen, pComponent.Bounds);
            else
                e.Graphics.DrawRectangle(_fullPen, pComponent.Bounds);
        }

        private void DataFlowComponentVisualisation_DragEnter(object sender, DragEventArgs e)
        {
            if(_shouldAllowDrop == null)
                return;

            var shouldAllow = _shouldAllowDrop(e,this);

            if (shouldAllow != DragDropEffects.None)
                pbInsertHere.Visible = true;

            e.Effect = shouldAllow;
        }

        private void DataFlowComponentVisualisation_DragLeave(object sender, EventArgs e)
        {
            pbInsertHere.Visible = false;
        }

        public void Check()
        {
            if (_checkable != null)
                ragSmiley1.StartChecking(_checkable);
            else
                ragSmiley1.SetVisible(false);//it isn't checkable
        }

        public static Role GetRoleFor(Type componentType)
        {
            if (IsGenericType(componentType, typeof(IDataFlowSource<>)))
                return  Role.Source;
            
            if (IsGenericType(componentType, typeof(IDataFlowDestination<>)))
                return Role.Destination;

            if (IsGenericType(componentType, typeof(IDataFlowComponent<>)))
                return Role.Middle;
            
            throw new ArgumentException("Object must be an IDataFlowComponent<> but was " + componentType);
        }

        private static bool IsGenericType(Type toCheck, Type genericType)
        {
            return toCheck.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericType);
        }
    }
}
