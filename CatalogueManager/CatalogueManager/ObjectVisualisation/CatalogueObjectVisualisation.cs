using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Reports;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconProvision;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.Checks;
using ReusableUIComponents.Dependencies.Models;
using Sharing.Dependency.Gathering;

namespace CatalogueManager.ObjectVisualisation
{
    public class CatalogueObjectVisualisation : IObjectVisualisation
    {
        private readonly ICoreIconProvider _coreIconProvider;
        private Dictionary<Type, string> _summaries;

        public CatalogueObjectVisualisation(ICoreIconProvider coreIconProvider)
        {
            _coreIconProvider = coreIconProvider;

            var documentation = new DocumentationReportMapsDirectlyToDatabase(typeof(Catalogue).Assembly,typeof(ExtractionConfiguration).Assembly);
            documentation.Check(new IgnoreAllErrorsCheckNotifier());
            _summaries = documentation.Summaries;
        }

        public string[] GetNameAndType(object toRender)
        {
            var gatheredObject = toRender as GatheredObject;

            //don't render the gathered object
            if (gatheredObject != null)
                toRender = gatheredObject.Object;

            var idPropertyInfo = toRender.GetType().GetProperty("ID");
            string idAsString = null;
            if (idPropertyInfo != null)
                idAsString = idPropertyInfo.GetValue(toRender).ToString();

            string[] nameAndType = new string[3];
            nameAndType[0] = toRender.ToString();
            nameAndType[1] = " (" + toRender.GetType().Name + (idAsString != null ? " ID=" + idAsString : "") + ")";
            nameAndType[2] = toRender.GetType().Name;
            return nameAndType;

        }

        //The dictionary has space for 3 segments of information. The first entry in the dictionary
        //is placed in the Rich Textbox (hence why description is almost always the first entry)
        public OrderedDictionary EntityInformation(object toRender)
        {
            var gatheredObject = toRender as GatheredObject;

            //don't render the gathered object
            if (gatheredObject != null)
                toRender = gatheredObject.Object;
            
            OrderedDictionary informationToReturn = new OrderedDictionary();

            if (_summaries != null && _summaries.ContainsKey(toRender.GetType()))
                informationToReturn.Add("Type Purpose: ", _summaries[toRender.GetType()]);
            else
                informationToReturn.Add("Type Purpose: ", "Unknown");


            if (toRender.GetType() == typeof(Catalogue))
            {
                informationToReturn.Add("Description: ", ((Catalogue)toRender).Description);
            }
            if (toRender.GetType() == typeof(CatalogueItem))
            {
                informationToReturn.Add("Description: ", ((CatalogueItem)toRender).Description);
                informationToReturn.Add("Comments: ", ((CatalogueItem)toRender).Comments);
            }
            if (toRender.GetType() == typeof(TableInfo))
            {
                informationToReturn.Add("Store Type:  ", ((TableInfo)toRender).DatabaseType);
            }

            if (toRender.GetType() == typeof(ColumnInfo))
            {
                informationToReturn.Add("Description: ", ((ColumnInfo)toRender).Description);
            }

            if (toRender.GetType() == typeof(ExtractionInformation))
            {
                informationToReturn.Add("Select SQL: ", ((ExtractionInformation)toRender).SelectSQL);
                informationToReturn.Add("Extraction Category: ", ((ExtractionInformation)toRender).ExtractionCategory.ToString());
            }

            if (toRender.GetType() == typeof(LoadMetadata))
            {
                informationToReturn.Add("Description: ", ((LoadMetadata)toRender).Description);
            }

            if (toRender.GetType() == typeof(ExtractionFilter))
            {
                informationToReturn.Add("Description: ", ((ExtractionFilter)toRender).Description);
            }

            if (toRender.GetType() == typeof(ExtractionFilterParameter))
            {
                informationToReturn.Add("Parameter SQL: ", ((ExtractionFilterParameter)toRender).ParameterSQL);
                informationToReturn.Add("Parameter Name: ", ((ExtractionFilterParameter)toRender).ParameterName);
            }

            if (toRender.GetType() == typeof(Lookup))
            {
                informationToReturn.Add("Description: ", ((Lookup)toRender).Description.ToString());
                informationToReturn.Add("Primary Key: ", ((Lookup)toRender).PrimaryKey.ToString());
                informationToReturn.Add("Foreign Key: ", ((Lookup)toRender).ForeignKey.ToString());
            }

            return informationToReturn;
        }

        public ColorResponse GetColor(object toRender, ColorRequest request)
        {
            var gatheredObject = toRender as GatheredObject;

            //don't render the gathered object
            if (gatheredObject != null)
                toRender = gatheredObject.Object;

            if (request.IsHighlighted)
                return new ColorResponse(KnownColor.LightPink, KnownColor.White);

            if (toRender is ExtractionInformation)
                if (((ExtractionInformation)toRender).IsProperTransform())
                    return new ColorResponse(KnownColor.LawnGreen, KnownColor.White);

            return new ColorResponse(KnownColor.LightBlue, KnownColor.White);
        }


        public Bitmap GetImage(object toRender)
        {
            var gatheredObject = toRender as GatheredObject;

            //don't render the gathered object
            if (gatheredObject != null)
                toRender = gatheredObject.Object;

            var img = (Bitmap)_coreIconProvider.GetImage(toRender);
            
            if (img == null)
                throw new NotSupportedException("Did not know what image to serve for object of type " + toRender.GetType().FullName + " (Icon provider was of Type '" + _coreIconProvider.GetType().Name +"')");

            return img;
        }
    }


}
