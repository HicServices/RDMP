using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diagnostics.TestData.Exercises
{
    public class ExerciseTestIdentifiers:IExerciseTestIdentifiers
    {
        Random r = new Random();

        public TestPerson[] People { get; private set; }

        public void GeneratePeople(int numberOfUniqueIndividuals)
        {
            People = new TestPerson[numberOfUniqueIndividuals];

            for (int i = 0; i < numberOfUniqueIndividuals; i++)
                People[i]=new TestPerson(r);
        }
    }
}
