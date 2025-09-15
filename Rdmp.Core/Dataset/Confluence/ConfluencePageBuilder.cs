using Rdmp.Core.Curation.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Dataset.Confluence
{
    public class ConfluencePageBuilder
    {

        private List<Catalogue> _catalogues;

        private string _repositoryName = "";
        //HIC
        private string _repositoryDescription = "";
        //A HIC dataset offers a theme-specific database of residents in the Tayside region.

        public ConfluencePageBuilder(List<Catalogue> catalogues, string name,string description) { 
            _catalogues = catalogues;
            _repositoryName = name;
            _repositoryDescription = description;
        }

        public string BuildRootPage() { return BuildContainerPage(); }

        private static string BuildCatalogueOverviewHTML(Catalogue catalogue)
        {
            return $"""
                <h2>{catalogue.Name}</h2>
                <table>
                <tr>
                    <th>
                        Dataset Name
                    </th>
                    <th>
                        Description
                    </th>
                    <th>
                        Tags
                    </th>
                </tr>
                <tr>
                    <td>
                        {catalogue.Name} - todo make link
                    </td>
                    <td>
                        {catalogue.Description}
                    </td>
                    <td>
                        {string.Join(", ",catalogue.Search_keywords)}
                    </td>
                </tr>
                </table>
                """;
        }

        private string BuildContainerPage() {
            return $"""
           
            <h2>{_repositoryName} Catalogues</h2>
            <p>{_repositoryDescription}</p>
            <p><ac:structured-macro ac:name='toc'/></p>
            {string.Join("",_catalogues.Select(c => BuildCatalogueOverviewHTML(c)))}
            """;
        }

        private static string BuildDataVariableRecord(CatalogueItem catalogueItem)
        {
            var lookups = catalogueItem.CatalogueRepository.GetAllObjectsWhere<Lookup>("ForeignKey_ID", catalogueItem.ColumnInfo.ID);
            return $"""
                <tr>
                    <td>{catalogueItem.Name}</td>
                    <td>{catalogueItem.ColumnInfo.Data_type}</td>
                    <td>{catalogueItem.ExtractionInformation.IsPrimaryKey}</td>
                    <td>{catalogueItem.Description}</td>
                    <td>{catalogueItem.ExtractionInformation.IsExtractionIdentifier}</td>
                    <td>{lookups.Any()}</td>
                </tr>
                """;
        }

        public string BuildHTMLForCatalogue(Catalogue catalogue)
        {
            return $"""
            <h2>Dataset Summary</h2>
            <p>{catalogue.Description}</p>
            <p><ac:structured-macro ac:name='toc'/></p>
            <h2>Data Details</h2>
            <table>
                <tr>
                    <th>Resource Type</th>
                    <td>{Enum.GetName(catalogue.Type)}</td>
                </tr>
                <tr>
                    <th>Dataset Purpose</th>
                    <td>{Enum.GetName(catalogue.Purpose)}</td>
                </tr>
                <tr>
                    <th>Dataset Type</th>
                    <td>{catalogue.DataType}</td>
                </tr>
                <tr>
                    <th>Dataset SubType</th>
                    <td>{catalogue.DataSubType}</td>
                </tr>
                <tr>
                    <th>Dataset Source</th>
                    <td>{catalogue.DataSource}</td>
                </tr>
                <tr>
                    <th>Dataset Source Setting</th>
                    <td>{catalogue.DataSourceSetting}</td>
                </tr>
                <tr>
                    <th>Dataset Purpose</th>
                    <td>{Enum.GetName(catalogue.Purpose)}</td>
                </tr>
            <tr>
                    <th>Dataset Keywords</th>
                    <td>{catalogue.Search_keywords}</td>
                </tr>
            </table>
            <h2> Geospatial and Temporal Details</h2>
            <table>
                <tr>
                    <th>Geographic Coverage</th>
                    <td>{catalogue.Geographical_coverage}</td>
                </tr>
                <tr>
                    <th>Granularity</th>
                    <td>{catalogue.Granularity}</td>
                </tr>
                <tr>
                    <th>Start Date</th>
                    <td>{catalogue.StartDate}</td>
                </tr>
                <tr>
                    <th>End Date</th>
                    <td>{catalogue.EndDate}</td>
                </tr>
            </table>
            <h2>Access Details</h2>
            <table>
                <tr>
                    <th>Access Contact</th>
                    <td>{catalogue.Contact_details}</td>
                </tr>
                <tr>
                    <th>Data Controller</th>
                    <td>{catalogue.DataController}</td>
                </tr>
                <tr>
                    <th>Data Processor</th>
                    <td>{catalogue.DataProcessor}</td>
                </tr>
                <tr>
                    <th>Jurisdiction</th>
                    <td>{catalogue.Juristiction}</td>
                </tr>
            </table>
            <h2>Attribution</h2>
            <table>
                <tr>
                    <th>DOI</th>
                    <td>{catalogue.Doi}</td>
                </tr>
                <tr>
                    <th>Controlled Vocabulary</th>
                    <td>{catalogue.ControlledVocabulary}</td>
                </tr>
                <tr>
                    <th>Associated People</th>
                    <td>{catalogue.AssociatedPeople}</td>
                </tr>
                <tr>
                    <th>Jurisdiction</th>
                    <td>{catalogue.Juristiction}</td>
                </tr>
            </table>
            <h2>Data Updates</h2>
            <table>
                <tr>
                    <th>Update Frequency</th>
                    <td>{Enum.GetName(catalogue.Update_freq)}</td>
                </tr>
                <tr>
                    <th>Initial Release Date</th>
                    <td>{catalogue.DatasetReleaseDate.ToString()}</td>
                </tr>
                <tr>
                    <th>Update Lag</th>
                    <td>{Enum.GetName(catalogue.UpdateLag)}</td>
                </tr>
            </table>
            <h2>Dataset Variables</h2>
            <table>
                <tr>
                    <th>
                        Variable Name
                    </th>
                    <th>
                        Type
                    </th>
                    <th>
                        Null Possible (Y/N)
                    </th>
                    <th>
                        Description
                    </th>
                    <th>
                        Identifier
                    </th>
                    <th>
                        Has Lookups
                    </th>
                </tr>
                {string.Join("",catalogue.CatalogueItems.Where(ci => ci.ExtractionInformation is not null).Select(ci => BuildDataVariableRecord(ci)))}
            </table>
            """;
        }
    }
}
