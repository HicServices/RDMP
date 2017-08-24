using System.Collections.Generic;

namespace Diagnostics.TestData.Exercises
{
    public interface IExerciseTestIdentifiers
    {
        TestPerson[] People { get; }
        void GeneratePeople(int numberOfUniqueIndividuals);
        
    }
}