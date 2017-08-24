using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.QueryBuilding;
using CohortManagerLibrary.QueryBuilding;

namespace CohortManagerLibrary.FreeText.Sentencing
{
    public class CohortSentence
    {
        public string Text { get; private set; }
        public List<SentenceSection> Sections { get; private set; }

        public AggregateConfiguration AggregateConfiguration { get; set; }

        public CohortSentence(AggregateConfiguration aggregateConfiguration)
        {
            Text = "";
            Sections = new List<SentenceSection>();

            AggregateConfiguration = aggregateConfiguration;
            Text = GenerateSentence();
        }

        private string GenerateSentence()
        {
            Append(AggregateConfiguration.Catalogue.Name, SentenceSectionType.Catalogue);

            if(AggregateConfiguration.RootFilterContainer_ID != null)
                AppendFiltersRecursively(AggregateConfiguration.RootFilterContainer);

            return Text;
        }

        private void AppendFiltersRecursively(AggregateFilterContainer filterContainer)
        {
            FilterContainerOperation operation = filterContainer.Operation;
            AggregateFilter[] filters = (AggregateFilter[]) filterContainer.GetFilters();
            AggregateFilterContainer[] subcontainers = filterContainer.GetSubContainers().Cast<AggregateFilterContainer>().ToArray();
            
            //Handle filters at this level
            for (int i = 0; i < filters.Length; i++)
            {
                if (i > 0)
                    Append(operation.ToString(), SentenceSectionType.FilterContainerOperation);

                Append(filters[i].Name,SentenceSectionType.Filter);

                foreach (AggregateFilterParameter parameter in filters[i].AggregateFilterParameters)
                {
                    Append(parameter.ParameterName, SentenceSectionType.FilterParameter);
                    Append(parameter.Value, SentenceSectionType.FilterParameterValue);
                }
            }

            //Handle subcontainers
            if (subcontainers.Any())
            {
                //if we have just written out at least one filter on this level?
                if(filters.Any())
                    Append(operation.ToString(), SentenceSectionType.FilterContainerOperation); //then before we write the subcontainer we must add the operator AND/OR

                bool first = true;
                foreach (AggregateFilterContainer container in subcontainers)
                {
                    //if it is not the first subcontainer we must separate it with the current container operation e.g. root operation is OR and there are 2 subcontainers then we would get (...) OR (...) 
                    if (!first)
                        Append(operation.ToString(), SentenceSectionType.FilterContainerOperation);

                    //also the subcontainer will need to go in brackets
                    Append("(", SentenceSectionType.FilterSubContainerBoundaryBracket);

                    AppendFiltersRecursively(container);

                    Append(")", SentenceSectionType.FilterSubContainerBoundaryBracket);

                    first = false;    
                }
            }
            
            return;
        }

        private void Append(string toAppendText, SentenceSectionType sentenceSectionType)
        {
            //space separate them with after the first, the space is considered the next part
            if (!string.IsNullOrWhiteSpace(Text))
                toAppendText = " " + toAppendText.Trim();

            int startIndex = Text.Length;
            int endIndex = Text.Length + toAppendText.Length - 1;
            
            Text += toAppendText;
            Sections.Add(new SentenceSection(startIndex,endIndex,sentenceSectionType));
        }
    }
}