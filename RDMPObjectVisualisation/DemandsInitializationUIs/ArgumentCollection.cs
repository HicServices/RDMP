using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using RDMPObjectVisualisation.DemandsInitializationUIs.ArgumentValueControls;
using ReusableLibraryCode;
using ReusableUIComponents;
using ContentAlignment = System.Drawing.ContentAlignment;

namespace RDMPObjectVisualisation.DemandsInitializationUIs
{
    /// <summary>
    /// Allows you to specify values for any IArgumentHost class.  This control is used by the user at 'design time' (e.g. when they are building a data load configuration) and the values
    /// are then populated into instantiated runtime instances (not that this control cares about how that happens).  You will see a list of all properties marked with [DemandsInitialization]
    /// on the argument host class.  Selecting the Argument will display the help text associated with the argument (user friendly message telling them what they are supposed to put in for that
    /// property) and an appropriate user control for providing a value (for example an enum will show a dropdown while a string property will show a text box - See ArgumentUI).  
    /// </summary>
    public partial class ArgumentCollection : UserControl 
    {
        public Dictionary<string, DemandsInitialization> DemandDictionary = new Dictionary<string, DemandsInitialization>();
        private Type _argumentsAreFor;
        private IArgumentHost _parent;
        private CatalogueRepository _catalogueRepository;
        private ArgumentValueUIFactory _valueUisFactory;
        
        private int _currentY;
        private int _maxValueUILeft;
        private List<Control> _valueUIs = new List<Control>();

        public ArgumentCollection()
        {
            InitializeComponent();
        }

        public DataTable Preview { get; set; }

        /// <summary>
        /// Reconfigures this UI (can be called multiple times throughout controls lifetime) to facilitate the population of DemandsInitialization
        /// properties on an underlying type (e.g. if your collection is ProcessTask and your argument type is ProcessTaskArgument then your underlying type could
        /// be AnySeparatorFileAttacher or MDFAttacher).  Note that while T is IArgumentHost, it also should be tied to one or more interfaces (e.g. IAttacher) and able to host
        /// any child of that interface of which argumentsAreForUnderlyingType is the currently configured concrete class (e.g. AnySeparatorFileAttacher).
        /// </summary>
        /// <param name="catalogueRepository"></param>
        /// <param name="parent"></param>
        /// <param name="argumentsAreForUnderlyingType"></param>
        public void Setup(CatalogueRepository catalogueRepository, IArgumentHost parent, Type argumentsAreForUnderlyingType)
        {
            _parent = parent;
            _argumentsAreFor = argumentsAreForUnderlyingType;
            _catalogueRepository = catalogueRepository;
            _valueUisFactory = new ArgumentValueUIFactory();

            lblClassName.Text = _argumentsAreFor.FullName;

            helpIcon1.Left = lblClassName.Right;

            btnViewSourceCode.Enabled = ViewSourceCodeDialog.GetSourceForFile(_argumentsAreFor.Name + ".cs") != null;
            btnViewSourceCode.Left = helpIcon1.Right;
            
            helpIcon1.SetHelpText( _argumentsAreFor.Name,GetDescriptionForTypeIncludingBaseTypes(_argumentsAreFor, true));

            RefreshArgumentList();
            
        }
        
        /// <summary>
        /// Parses entire class hierarchy looking for [Description("something")] elements which are all agregated together (recursively) and returned
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetDescriptionForTypeIncludingBaseTypes(Type type, bool isRootClass)
        {
            var descriptions = (DescriptionAttribute[])type.GetCustomAttributes(typeof(DescriptionAttribute), false);

            string message = "";

            if (descriptions.Length == 0)
                message = isRootClass?"No description found for Type:" + type.FullName :"";
            else
            {
                message = descriptions.Single().Description.TrimEnd();

                if (!isRootClass)
                    message = "PARENT CLASS:" + type.Name + Environment.NewLine + Environment.NewLine + message;
            }

            if (type.BaseType != null)
            {
                string baseDescription = GetDescriptionForTypeIncludingBaseTypes(type.BaseType, false);
                if (message == "")
                    return baseDescription;

                return message + Environment.NewLine + Environment.NewLine + baseDescription;
                
            }
            return message;
        }
        private void RefreshArgumentList()
        {
            if (_parent == null)
                return;

            DemandDictionary.Clear();

            if (_argumentsAreFor != null)
            {
                //parameters that already exist
                var existing = _parent.GetAllArguments().Cast<Argument>().ToList();
                List<string> required = new List<string>();

                foreach (PropertyInfo propertyInfo in _argumentsAreFor.GetProperties())
                    foreach (DemandsInitialization attribute in propertyInfo.GetCustomAttributes(typeof(DemandsInitialization), true))
                    {
                        //found a tagged attribute - it might already exist though
                        required.Add(propertyInfo.Name);

                        //record the name of the property and the type it requires
                        DemandDictionary.Add(propertyInfo.Name, attribute);

                        var argument = existing.SingleOrDefault(arg => arg.Name.Equals(propertyInfo.Name));

                        if (argument == null)//it doesnt exist - so create it
                        {
                            var newArgument = (Argument)_parent.CreateNewArgument();

                            newArgument.Name = propertyInfo.Name;
                                
                            try
                            {
                                newArgument.SetType(propertyInfo.PropertyType);
                            }
                            catch (Exception e)
                            {
                                ExceptionViewer.Show("Problem determining argument " + propertyInfo.Name + " on class " + _argumentsAreFor.FullName,e);
                            }

                            newArgument.Description = attribute.Description;

                            if (attribute.DefaultValue != null)
                                try
                                {
                                    newArgument.SetValue(attribute.DefaultValue);
                                }
                                catch (Exception e)
                                {
                                    ExceptionViewer.Show("Problem setting DefaultValue argument " + propertyInfo.Name + " on class " + _argumentsAreFor.FullName + " DefaultValue was '" + attribute.DefaultValue + "' (" + attribute.DefaultValue.GetType().Name+")", e);
                                }
                            newArgument.SaveToDatabase();
                            existing.Add(newArgument);
                        }
                        else
                        {
                            //it does exist but check it hasn't had a type descync
                            if (argument.GetSystemType() != propertyInfo.PropertyType)
                                if (MessageBox.Show("Argument '" + argument.Name + "' is of Type '" +
                                                    argument.GetSystemType() + "' in Catalogue but is of Type '" +
                                                    propertyInfo.PropertyType + "' in underlying class '" +
                                                    _argumentsAreFor.Name +
                                                    "'.  Do you want to resolve this by changing the type of argument " +
                                                    argument.Name + " to Type " + propertyInfo.PropertyType + "?",
                                    "Fix Argument Type Desynchronisation?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    //user wants to fix the problem
                                    argument.SetType(propertyInfo.PropertyType);
                                    argument.SaveToDatabase();
                                }
                        }
                    }

                foreach (var argumentsNotRequired in existing.Where(e => !required.Any(r => r.Equals(e.Name))))
                {
                    if (MessageBox.Show("Argument " + argumentsNotRequired.Name + " is not required, delete it?", "Delete superfluous argument?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        argumentsNotRequired.DeleteInDatabase();
                }
            }

            var args = _parent.GetAllArguments().ToArray();

            lblNoArguments.Visible = !args.Any();
            pArguments.Visible = args.Any();

            if (!args.Any())
                return;

            pArguments.Controls.Clear();
            pArguments.SuspendLayout();

            var headerLabel = GetLabelHeader("Arguments");
            pArguments.Controls.Add(headerLabel);

            _currentY = headerLabel.Height;
            _maxValueUILeft = 0;
            _valueUIs.Clear();

            foreach (var arg in args)
                if (DemandDictionary.ContainsKey(arg.Name))
                    CreateLine(_parent, (Argument) arg, DemandDictionary[arg.Name]);

            foreach (Control control in _valueUIs)
            {
                control.Left = _maxValueUILeft;
                control.Width = control.Parent.ClientRectangle.Width - (_maxValueUILeft);
                control.Parent.MinimumSize = new Size(_maxValueUILeft + control.MinimumSize.Width, control.Parent.Height);
            }

            pArguments.ResumeLayout(true);
        }


        private Label GetLabelHeader(string caption)
        {
            Label label = new Label();
            label.Text = caption;
            label.BackColor = Color.DarkGray;
            label.Width = pArguments.ClientRectangle.Width;
            label.Anchor = AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;

            label.TextAlign = ContentAlignment.MiddleCenter;
            
            return label;
        }

        private void CreateLine(IArgumentHost parent, Argument argument, DemandsInitialization demandsInitialization)
        {

            Label name = new Label();

            HelpIcon helpIcon = new HelpIcon();
            helpIcon.SetHelpText(GetSystemTypeName(argument.GetSystemType()), demandsInitialization.Description);
            helpIcon.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            string spaceSeparatedArgumentName = UsefulStuff.PascalCaseStringToHumanReadable(argument.Name);
            name.Height = helpIcon.Height;
            name.Text = spaceSeparatedArgumentName;
            name.TextAlign = ContentAlignment.MiddleLeft;
            name.AutoSize = true;
            name.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            var valueui = (Control)_valueUisFactory.Create(parent, argument, demandsInitialization, Preview);
            valueui.MinimumSize = new Size(valueui.Width/4, valueui.Height);
            valueui.MaximumSize = new Size(valueui.Width, valueui.Height);
            valueui.Anchor = name.Anchor = AnchorStyles.Top |  AnchorStyles.Left | AnchorStyles.Right;
            _valueUIs.Add(valueui);

            Panel p = new Panel();
            p.Height = Math.Max(Math.Max(lblClassName.Height,helpIcon.Height),valueui.Height);
            p.Width = pArguments.ClientRectangle.Width;
            p.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            p.BorderStyle = BorderStyle.FixedSingle;
            p.Location = new Point(0, _currentY);
            _currentY += p.Height;

            name.Location = new Point(0,0);
            p.Controls.Add(name);

            helpIcon.Left = name.Right;
            p.Controls.Add(helpIcon);

            valueui.Left = helpIcon.Right;
            _maxValueUILeft = Math.Max(_maxValueUILeft, valueui.Left);
            p.Controls.Add(valueui);
            p.MinimumSize = new Size(valueui.Right,p.Height);

            name.Height = p.Height;

            pArguments.Controls.Add(p);
        }

        private string GetSystemTypeName(Type type)
        {
            if (typeof(Enum).IsAssignableFrom(type))
                return "Enum";

            return type.Name;
        }

        /// <summary>
        /// T must be an IArgumentHost e.g.  ProcessTask or PipelineComponent, these two classes act as persistence wrappers for their host type (which can be any type of anything - any plugin)
        /// then you must also pass the underlying type that is being wrapped e.g. basically the constructor arguments to this class :)
        /// </summary>
        /// <param name="catalogueRepository"></param>
        /// <param name="newComp"></param>
        /// <param name="argumentsAreForUnderlyingType"></param>
        /// <param name="previewIfAny">If you have a data table that approximates what the pipeline will look like at the time the component T is reached then pass it in here otherwise pass null</param>
        public static void ShowDialogIfAnyArgs(CatalogueRepository catalogueRepository, IArgumentHost newComp, Type argumentsAreForUnderlyingType,DataTable previewIfAny)
        {
            Form f = new Form();
            var argCollection = new ArgumentCollection();
            argCollection.Setup(catalogueRepository, newComp, argumentsAreForUnderlyingType);
            argCollection.Dock = DockStyle.Fill;
            
            bool areAnyDemandsInitializations =
                //get all properties
                argumentsAreForUnderlyingType.GetProperties()
                //any of them have custom attribute collections which contain a [DemandsInitialization]?
                .Any(p => p.GetCustomAttributes(typeof (CatalogueLibrary.Data.DemandsInitialization)).Any());

            if (!areAnyDemandsInitializations)
                return;

            f.Controls.Add(argCollection);

            Button ok = new Button();
            ok.Text = "Ok";
            ok.Dock = DockStyle.Bottom;
            ok.Click += (s, e) => f.Close();
            f.Controls.Add(ok);
            f.Text = "Set Arguments For Type:" + argumentsAreForUnderlyingType.Name + " (you can always change these later)";

            argCollection.Setup(catalogueRepository, newComp, argumentsAreForUnderlyingType);
            argCollection.Preview = previewIfAny;
            
            f.Width = 800;
            f.Height = 700;
            f.ShowDialog();
        }

        private void btnViewSourceCode_Click(object sender, EventArgs e)
        {
            new ViewSourceCodeDialog(_argumentsAreFor.Name + ".cs").Show();
        }
    }
}
