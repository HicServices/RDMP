// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CohortManagerLibrary.FreeText.Paragraphing;

namespace CohortManagerLibrary.FreeText
{
    public class FreeTextCompiler
    {
        public List<CohortParagraph> InclusionCriteria { get;private set; }
        public List<CohortParagraph> ExclusionCriteria { get;private set; }
        
        public FreeTextCompiler(CohortIdentificationConfiguration configuration)
        {
            InclusionCriteria = new List<CohortParagraph>();
            ExclusionCriteria = new List<CohortParagraph>();

            var container = configuration.RootCohortAggregateContainer;
            
            if (container.Operation != SetOperation.EXCEPT)
                throw new NotSupportedException("Root container must be an EXCEPT to work with User Friendly Mode");

            var contents = container.GetOrderedContents().ToArray();

            if (contents.Length == 0)
                return;

            //first entity is always the inclusion criteria 
            if(contents[0] is AggregateConfiguration)
                InclusionCriteria.Add(new CohortParagraph((AggregateConfiguration)contents[0]));
            else
                InclusionCriteria.Add(new CohortParagraph((CohortAggregateContainer)contents[0]));

            //everything else is an exclusion criteria
            for (int i = 1; i < contents.Length; i++)
                if (contents[i] is AggregateConfiguration)
                    ExclusionCriteria.Add(new CohortParagraph((AggregateConfiguration)contents[i]));
                else
                    ExclusionCriteria.Add(new CohortParagraph((CohortAggregateContainer)contents[i]));
        }

        public void Syntax()
        {
            
        }
    }
}
