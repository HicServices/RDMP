// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
