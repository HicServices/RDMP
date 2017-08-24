using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CohortManagerLibrary.FreeText.Sentencing;

namespace CohortManagerLibrary.FreeText.Paragraphing
{
    public class CohortParagraph
    {
        public string Text { get; set; }

        public List<ParagraphSection> Sections { get; set; }
        
        public ParagraphSectionType ParagraphSectionType { get; set; }


        public CohortParagraph(CohortAggregateContainer cohortAggregateContainer) : this()
        {
            Text = "";
            if(cohortAggregateContainer.GetSubContainers().Any())
                throw new NotSupportedException("Container has subcontainers, ahh confuse!");

            var configurations = cohortAggregateContainer.GetAggregateConfigurations().ToArray();
            for (int i = 0; i < configurations.Length; i++)
            {
                AddSentenceAtEnd(configurations[i]);

                if (i + 1 < configurations.Length)
                    AddSetOperation(cohortAggregateContainer.Operation);

            }
        }

        public CohortParagraph(AggregateConfiguration aggregateConfiguration):this()
        {
            AddSentenceAtEnd(aggregateConfiguration);
        }

        private CohortParagraph()
        {
            Text = "";
            Sections = new List<ParagraphSection>();
        }

        private void AddSetOperation(SetOperation operation)
        {
            string userFriendlyText = "";
            switch (operation)
            {
                case SetOperation.UNION:
                    userFriendlyText = " or ";
                    break;
                case SetOperation.INTERSECT:
                    userFriendlyText = " and ";
                    break;
                case SetOperation.EXCEPT:
                    userFriendlyText = " except ";
                    break;
                default:
                    throw new ArgumentOutOfRangeException("operation");
            }
            
            int startIndex = Text.Length;
            int endIndex = startIndex + userFriendlyText.Length -1;

            Text += userFriendlyText;
            Sections.Add(new ParagraphSection(operation,startIndex,endIndex));
        }

        private void AddSentenceAtEnd(AggregateConfiguration aggregateConfiguration)
        {
            var sentence = new CohortSentence(aggregateConfiguration);

            int startIndex = Text.Length;
            int endIndex = startIndex + sentence.Text.Length - 1;

            Text += sentence.Text;

            Sections.Add(new ParagraphSection(sentence,startIndex,endIndex));
        }
    }
}
