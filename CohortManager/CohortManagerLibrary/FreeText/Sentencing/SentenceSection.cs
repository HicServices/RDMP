using System;
using System.Linq;
using CohortManagerLibrary.FreeText.Paragraphing;

namespace CohortManagerLibrary.FreeText.Sentencing
{
    public class SentenceSection
    {
        public SentenceSection(int startIndex, int endIndex, SentenceSectionType sentenceSectionType)
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
            SectionType = sentenceSectionType;
        }

        public SentenceSectionType SectionType { get; set; }

        public int StartIndex { get; set; }
        public int EndIndex { get; set; }



        public int Length { get { return (EndIndex - StartIndex )+ 1; }}

        public SentenceSection GetPositionRelativeToParagraphStart(ParagraphSection paragraphSection)
        {
            if(!paragraphSection.CohortSentence.Sections.Contains(this))
                throw new ArgumentException("This SentenceSection is not part of the ParagraphSection you passed in");

            int relativeStart = StartIndex + paragraphSection.StartIndex;
            int relativeEnd = relativeStart + Length-1;

            return new SentenceSection(relativeStart, relativeEnd, SectionType);
        }
    }
}