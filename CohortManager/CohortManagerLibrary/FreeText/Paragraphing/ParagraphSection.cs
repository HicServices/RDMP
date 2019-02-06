// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using CatalogueLibrary.Data.Cohort;
using CohortManagerLibrary.FreeText.Sentencing;

namespace CohortManagerLibrary.FreeText.Paragraphing
{
    public class ParagraphSection
    {
        public CohortSentence CohortSentence { get; set; }
        public SetOperation SetOperation { get; set; }
        
        public ParagraphSectionType SectionType { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public int Length { get { return (EndIndex - StartIndex) +1; } }
         
        public ParagraphSection(CohortSentence sentence, int startIndex, int endIndex)
        {
            CohortSentence = sentence;
            SectionType = ParagraphSectionType.Sentence;
            StartIndex = startIndex;
            EndIndex = endIndex;
        }

        public ParagraphSection(SetOperation operation, int startIndex, int endIndex)
        {
            SetOperation = operation;
            SectionType = ParagraphSectionType.SetOperation;
            StartIndex = startIndex;
            EndIndex = endIndex;
        }
    }
}