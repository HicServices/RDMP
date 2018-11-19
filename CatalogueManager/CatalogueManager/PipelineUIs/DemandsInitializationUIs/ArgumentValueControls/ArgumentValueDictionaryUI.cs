using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using ReusableUIComponents;

namespace CatalogueManager.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    /// <summary>
    /// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
    /// 
    /// <para>This Control is for setting Properties that are of Array types TableInfo[], Catalogue[] etc</para>
    /// </summary>
    [TechnicalUI]
    public partial class ArgumentValueDictionaryUI : UserControl, IArgumentValueUI
    {
        private Type _kType;
        private Type _vType;
        private IDictionary _dictionary;


        public ArgumentValueDictionaryUI()
        {
            InitializeComponent();
        }

        public void SetUp(ArgumentValueUIArgs args)
        {
            var concreteType = args.Type;

            //get an IDictionary either from the object or a new empty one (e.g. if Value is null)
            _dictionary = (IDictionary)(args.InitialValue??Activator.CreateInstance(concreteType));

            _kType = concreteType.GenericTypeArguments[0];
            _vType = concreteType.GenericTypeArguments[1];

            foreach (var kvp in _dictionary)
                throw new Exception("Lets cross this bridge later");
        }

        private int numRows = 0;
        private void btnAdd_Click(object sender, EventArgs e)
        {
            int y = numRows*20;
            Label keyUI = new Label(){Text ="Key",Location = new Point(0,y)};
            Label valueUI = new Label() { Text = "Value", Location = new Point(100,y) };

            panel1.Controls.Add(keyUI);
            panel1.Controls.Add(valueUI);
            
            numRows++;
        }

    }
}
