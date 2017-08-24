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