// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CatalogueLibrary.Data.DataLoad;
using Rdmp.Core.CatalogueLibrary.Repositories;
using Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;

namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs
{
    /// <summary>
    /// Allows you to specify values for any IArgumentHost class.  This control is used by the user at 'design time' (e.g. when they are building a data load configuration) and the values
    /// are then populated into instantiated runtime instances (not that this control cares about how that happens).  You will see a list of all properties marked with [DemandsInitialization]
    /// on the argument host class.  Selecting the Argument will display the help text associated with the argument (user friendly message telling them what they are supposed to put in for that
    /// property) and an appropriate user control for providing a value (for example an enum will show a dropdown while a string property will show a text box - See ArgumentUI).  
    /// </summary>
    public partial class ArgumentCollectionUI : UserControl 
    {
        public Dictionary<IArgument, RequiredPropertyInfo> DemandDictionary;
        private Type _argumentsAreFor;
        private IArgumentHost _parent;
        private ArgumentValueUIFactory _valueUisFactory;
        
        private int _currentY;
        private int _maxValueUILeft;
        private List<Control> _valueUIs = new List<Control>();

        public ArgumentCollectionUI()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Reconfigures this UI (can be called multiple times throughout controls lifetime) to facilitate the population of DemandsInitialization
        /// properties on an underlying type (e.g. if your collection is ProcessTask and your argument type is ProcessTaskArgument then your underlying type could
        /// be AnySeparatorFileAttacher or MDFAttacher).  Note that while T is IArgumentHost, it also should be tied to one or more interfaces (e.g. IAttacher) and able to host
        /// any child of that interface of which argumentsAreForUnderlyingType is the currently configured concrete class (e.g. AnySeparatorFileAttacher).
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="argumentsAreForUnderlyingType"></param>
        /// <param name="catalogueRepository"></param>
        public void Setup(IArgumentHost parent, Type argumentsAreForUnderlyingType, ICatalogueRepository catalogueRepository)
        {
            _parent = parent;
            _argumentsAreFor = argumentsAreForUnderlyingType;

            lblTypeUnloadable.Visible = _argumentsAreFor == null;

            _valueUisFactory = new ArgumentValueUIFactory();

            if (_argumentsAreFor != null)
                lblClassName.Text = _argumentsAreFor.FullName;

            helpIcon1.Left = lblClassName.Right;
            
            if (_argumentsAreFor != null)
            {
                btnViewSourceCode.Enabled = ViewSourceCodeDialog.GetSourceForFile(_argumentsAreFor.Name + ".cs") != null;
                btnViewSourceCode.Left = helpIcon1.Right;

                var summary = catalogueRepository.CommentStore.GetTypeDocumentationIfExists(argumentsAreForUnderlyingType);
                
                if(summary != null)
                    helpIcon1.SetHelpText(_argumentsAreFor.Name, summary);

                RefreshArgumentList();
            }

        }
        
        private void RefreshArgumentList()
        {
            var argumentFactory = new ArgumentFactory();
            DemandDictionary = argumentFactory.GetDemandDictionary(_parent, _argumentsAreFor);
            
            lblNoArguments.Visible = !DemandDictionary.Any();
            pArguments.Visible = DemandDictionary.Any();

            if (!DemandDictionary.Any())
                return;

            pArguments.Controls.Clear();
            pArguments.SuspendLayout();

            var headerLabel = GetLabelHeader("Arguments");
            pArguments.Controls.Add(headerLabel);

            _currentY = headerLabel.Height;
            _maxValueUILeft = 0;
            _valueUIs.Clear();

            foreach (var kvp in DemandDictionary)
                CreateLine(_parent, kvp.Key, kvp.Value);

            foreach (Control control in _valueUIs)
            {
                control.Left = _maxValueUILeft;
                control.Width = pArguments.Width - (_maxValueUILeft + 25);
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

        private void CreateLine(IArgumentHost parent, IArgument argument, RequiredPropertyInfo required)
        {

            Label name = new Label();

            HelpIcon helpIcon = new HelpIcon();
            helpIcon.SetHelpText(GetSystemTypeName(argument.GetSystemType())??"Unrecognised Type:" + argument.Type, required.Demand.Description);
            helpIcon.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            string spaceSeparatedArgumentName = UsefulStuff.PascalCaseStringToHumanReadable(argument.Name);
            name.Height = helpIcon.Height;
            name.Text = spaceSeparatedArgumentName;
            name.TextAlign = ContentAlignment.MiddleLeft;
            name.AutoSize = true;
            name.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            RAGSmiley ragSmiley = new RAGSmiley();

            if (required.Demand.Mandatory && string.IsNullOrWhiteSpace(argument.Value))
                ragSmiley.Fatal(new Exception("Property " + argument.Name + " is Mandatory"));

            var args = new ArgumentValueUIArgs();
            args.Parent = parent;
            args.Type = argument.GetSystemType();

            try
            {

                args.InitialValue = argument.GetValueAsSystemType();
            }
            catch (Exception e)
            {
                
                //add the text value value and report the error
                if(_valueUisFactory.CanHandleInvalidStringData(args.Type))
                    args.InitialValue = argument.Value;
                else
                    args.InitialValue = null;

                ragSmiley.Fatal(e);
            }

            
            args.Required = required;
            args.CatalogueRepository = (ICatalogueRepository)argument.Repository;
            args.Setter = (v) =>
            {
                ragSmiley.Reset();
                
                try
                {
                    argument.SetValue(v);
                    argument.SaveToDatabase();

                    argument.GetValueAsSystemType();

                    if(required.Demand.Mandatory && (v == null  || string.IsNullOrWhiteSpace(v.ToString())))
                        ragSmiley.Fatal(new Exception("Property " + argument.Name + " is Mandatory"));
                }
                catch (Exception ex)
                {
                    ragSmiley.OnCheckPerformed(new CheckEventArgs("Failed to set property properly",CheckResult.Fail,ex));
                }
            };
            args.Fatal = ragSmiley.Fatal;

            var valueui = (Control)_valueUisFactory.Create(args);
            
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

            ragSmiley.Left = p.Width - ragSmiley.Width;
            ragSmiley.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            p.Controls.Add(ragSmiley);

            valueui.Left = helpIcon.Right;
            valueui.Width = p.Width - (helpIcon.Right + ragSmiley.Left);
            _maxValueUILeft = Math.Max(_maxValueUILeft, valueui.Left);
            p.Controls.Add(valueui);
            p.MinimumSize = new Size(ragSmiley.Right,p.Height);

            name.Height = p.Height;

            pArguments.Controls.Add(p);
        }

        private string GetSystemTypeName(Type type)
        {
            if (typeof(Enum).IsAssignableFrom(type))
                return "Enum";

            if (type == null)
                return null;

            return type.Name;
        }

        private void btnViewSourceCode_Click(object sender, EventArgs e)
        {
            new ViewSourceCodeDialog(_argumentsAreFor.Name + ".cs").Show();
        }
    }
}
