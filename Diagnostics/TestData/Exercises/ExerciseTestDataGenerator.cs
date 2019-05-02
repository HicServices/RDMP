// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using MathNet.Numerics.Distributions;

namespace Diagnostics.TestData.Exercises
{
    [InheritedExport(typeof(IExerciseTestDataGenerator))]
    public abstract class ExerciseTestDataGenerator : IExerciseTestDataGenerator
    {
        public event EventHandler<RowsGeneratedEventArgs> RowsGenerated;

        public void GenerateTestDataFile(IExerciseTestIdentifiers cohort, FileInfo target, int numberOfRecords)
        {
            Random r = new Random();
            int totalPeople = cohort.People.Length;

            int linesWritten;
            using(StreamWriter sw = new StreamWriter(target.FullName))
            {
                WriteHeaders(sw);

                Stopwatch stopwatch = new Stopwatch();
                string task = "Populate " + target.Name;
                stopwatch.Start();

                using (var writer = new CsvWriter(sw))
                {
                    for (linesWritten = 0; linesWritten < numberOfRecords; linesWritten++)
                    {
                        foreach (object o in GenerateTestDataRow(cohort.People[r.Next(totalPeople)]))
                            writer.WriteField(o);
                        
                        writer.NextRecord();

                        if (linesWritten % 1000 == 0)
                        {
                            RowsGenerated?.Invoke(this,new RowsGeneratedEventArgs(linesWritten + 1, stopwatch.Elapsed,false));
                            sw.Flush();//flush every 1000
                        }
                    }

                    //tell them about the last line written
                    RowsGenerated?.Invoke(this, new RowsGeneratedEventArgs(linesWritten,stopwatch.Elapsed,true));

                    writer.Dispose();
                }
                
                stopwatch.Stop();
            }
            
        }

        public virtual string GetName()
        {
            var typeName = GetType().Name;
            if (typeName.EndsWith("ExerciseTestData"))
                return typeName.Substring(0, typeName.Length - "ExerciseTestData".Length);

            if (typeName.EndsWith("ExerciseTestDataGenerator"))
                return typeName.Substring(0, typeName.Length - "ExerciseTestDataGenerator".Length);

            return typeName;
        }

        public abstract object[] GenerateTestDataRow(TestPerson p);
        protected abstract void WriteHeaders(StreamWriter sw);
        readonly Normal _normalDist = new Normal(0, 0.3);

        /// <summary>
        /// Concatenates between <paramref name="min"/> and <paramref name="max"/> calls to the <paramref name="generator"/>
        /// </summary>
        /// <param name="r"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="generator"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        protected string Concat(Random r, int min, int max, Func<string> generator, string separator)
        {
            StringBuilder sb = new StringBuilder();

            int to = r.Next(min, max);
            for (int i = 0; i < to; i++)
                sb.Append(generator() + separator);

            return sb.ToString().Trim();
        }

        

        /// <summary>
        /// returns random number between -1 and 1 with normal distribution (more numbers near 0 than near 1/-1).  The standard
        /// deviation is 0.3.  Any values outside the range (5 in 10,000 or so) are adjusted to -1 or 1.
        /// </summary>
        /// <returns></returns>
        public double GetGaussian()
        {
            return Math.Min(Math.Max(-1,_normalDist.Sample()),1);
        }

        /// <summary>
        /// returns random number between lowerBoundary and upperBoundary with a gaussian distribution around the middle
        /// </summary>
        /// <param name="digits">The number of decimal places to have in the number</param>
        /// <returns></returns>
        public double GetGaussian(double lowerBoundary, double upperBoundary, int digits = 2)
        {
            if(upperBoundary< lowerBoundary)
                throw new ArgumentException("lower must be lower than upper boundary");

            double distributionZeroToOne = (GetGaussian() + 1)/2;

            double range = upperBoundary - lowerBoundary;
            return Math.Round((distributionZeroToOne * range) + lowerBoundary,digits);
        }

        protected int GetGaussianInt(double lowerBoundary, double upperBoundary)
        {
            return (int) GetGaussian(lowerBoundary, upperBoundary);
        }


        /// <summary>
        /// returns <paramref name="swapFor"/> if <see cref="swapIfIn"/> contains the input <paramref name="randomInt"/> (otherwise returns the input)
        /// </summary>
        /// <param name="randomInt"></param>
        /// <param name="swapIfIn"></param>
        /// <param name="swapFor"></param>
        /// <returns></returns>
        protected int Swap(int randomInt, IEnumerable<int> swapIfIn, int swapFor)
        {
            return swapIfIn.Contains(randomInt) ? swapFor : randomInt;
        }

        /// <summary>
        /// Returns a random double or string value that represents a double e.g. "2.1".  In future this might return
        /// floats with e specification e.g. "1.7E+3"
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public object GetRandomDouble(Random r)
        {
            switch (r.Next(0, 3))
            {
                case 0:
                    return r.Next(100);
                case 1:
                    return Math.Round(r.NextDouble(),2);
                case 2:
                    return r.Next(10) + "." + r.Next(10);
                default:
                    throw new NotImplementedException();
            }
        }

        public string GetRandomGPCode(Random r)
        {
            return GetRandomLetter(true,r).ToString() + r.Next(0, 999);
        }

        public char GetRandomLetter(bool upperCase,Random r)
        {
            if(upperCase)
                return (char) ('A' + r.Next(0, 26));

            return (char)('a' + r.Next(0, 26));

        }

        public object GetRandomCHIStatus(Random r)
        {
            switch (r.Next(0, 5))
            {
                case 0:return 'C';
                case 1: return 'H';
                case 2:return null;
                case 3: return 'L';
                case 4: return 'R';
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Returns a random sentence.  There are 391 available.  They were created by https://randomwordgenerator.com/sentence.php
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public string GetRandomSentence(Random r)
        {
            switch (r.Next(391))
            {
                case 0: return "A mad prize ghosts the attractive romantic.";
                case 1: return "I often see the time 11:11 or 12:34 on clocks.";
                case 2: return "Malls are great places to shop; I can find everything I need under one roof.";
                case 3: return "Christmas is coming.";
                case 4: return "I will never be this young again. Ever. Oh damn… I just got older.";
                case 5: return "This is a Japanese doll.";
                case 6: return "We have never been to Asia, nor have we visited Africa.";
                case 7: return "She was too short to see over the fence.";
                case 8: return "Hurry!";
                case 9: return "If I don’t like something, I’ll stay away from it.";
                case 10: return "Wednesday is hump day, but has anyone asked the camel if he’s happy about it?";
                case 11: return "She folded her handkerchief neatly.";
                case 12: return "I checked to make sure that he was still alive.";
                case 13: return "He didn’t want to go to the dentist, yet he went anyway.";
                case 14: return "There was no ice cream in the freezer, nor did they have money to go to the store.";
                case 15: return "Sometimes, all you need to do is completely make an ass of yourself and laugh it off to realise that life isn’t so bad after all.";
                case 16: return "If the Easter Bunny and the Tooth Fairy had babies would they take your teeth and leave chocolate for you?";
                case 17: return "Cats are good pets, for they are clean and are not noisy.";
                case 18: return "The body may perhaps compensates for the loss of a true metaphysics.";
                case 19: return "Please wait outside of the house.";
                case 20: return "The mysterious diary records the voice.";
                case 21: return "There were white out conditions in the town; subsequently, the roads were impassable.";
                case 22: return "I love eating toasted cheese and tuna sandwiches.";
                case 23: return "Two seats were vacant.";
                case 24: return "The clock within this blog and the clock on my laptop are 1 hour different from each other.";
                case 25: return "She did her best to help him.";
                case 26: return "We need to rent a room for our party.";
                case 27: return "Someone I know recently combined Maple Syrup & buttered Popcorn thinking it would taste like caramel popcorn. It didn’t and they don’t recommend anyone else do it either.";
                case 28: return "The river stole the gods.";
                case 29: return "Joe made the sugar cookies; Susan decorated them.";
                case 30: return "He told us a very exciting adventure story.";
                case 31: return "He said he was not there yesterday; however, many people saw him there.";
                case 32: return "I really want to go to work, but I am too sick to drive.";
                case 33: return "A glittering gem is not enough.";
                case 34: return "Abstraction is often one floor above you.";
                case 35: return "Sometimes it is better to just walk away from things and go back to them later when you’re in a better frame of mind.";
                case 36: return "Mary plays the piano.";
                case 37: return "She did not cheat on the test, for it was not the right thing to do.";
                case 38: return "I would have gotten the promotion, but my attendance wasn’t good enough.";
                case 39: return "I want more detailed information.";
                case 40: return "It was getting dark, and we weren’t there yet.";
                case 41: return "She borrowed the book from him many years ago and hasn't yet returned it.";
                case 42: return "I was very proud of my nickname throughout high school but today- I couldn’t be any different to what my nickname was.";
                case 43: return "Wow, does that work?";
                case 44: return "When I was little I had a car door slammed shut on my hand. I still remember it quite vividly.";
                case 45: return "The waves were crashing on the shore; it was a lovely sight.";
                case 46: return "If Purple People Eaters are real… where do they find purple people to eat?";
                case 47: return "Where do random thoughts come from?";
                case 48: return "They got there early, and they got really good seats.";
                case 49: return "Everyone was busy, so I went to the movie alone.";
                case 50: return "I am never at home on Sundays.";
                case 51: return "Should we start class now, or should we wait for everyone to get here?";
                case 52: return "The quick brown fox jumps over the lazy dog.";
                case 53: return "A song can make or ruin a person’s day if they let it get to them.";
                case 54: return "I want to buy a onesie… but know it won’t suit me.";
                case 55: return "Italy is my favorite country; in fact, I plan to spend two weeks there next year.";
                case 56: return "I hear that Nancy is very pretty.";
                case 57: return "What was the person thinking when they discovered cow’s milk was fine for human consumption… and why did they do it in the first place!?";
                case 58: return "She advised him to come back at once.";
                case 59: return "He ran out of money, so he had to stop playing poker.";
                case 60: return "My Mum tries to be cool by saying that she likes all the same things that I do.";
                case 61: return "The sky is clear; the stars are twinkling.";
                case 62: return "She works two jobs to make ends meet; at least, that was her reason for not having time to join us.";
                case 63: return "I'd rather be a bird than a fish.";
                case 64: return "He turned in the research paper on Friday; otherwise, he would have not passed the class.";
                case 65: return "The memory we used to share is no longer coherent.";
                case 66: return "Lets all be unique together until we realise we are all the same.";
                case 67: return "I am happy to take your donation; any amount will be greatly appreciated.";
                case 68: return "The old apple revels in its authority.";
                case 69: return "Let me help you with your baggage.";
                case 70: return "Sixty-Four comes asking for bread.";
                case 71: return "I am counting my calories, yet I really want dessert.";
                case 72: return "How was the math test?";
                case 73: return "If you like tuna and tomato sauce- try combining the two. It’s really not as bad as it sounds.";
                case 74: return "Last Friday in three week’s time I saw a spotted striped blue worm shake hands with a legless lizard.";
                case 75: return "She wrote him a long letter, but he didn't read it.";
                case 76: return "Don't step on the broken glass.";
                case 77: return "Check back tomorrow; I will see if the book has arrived.";
                case 78: return "I currently have 4 windows open up… and I don’t know why.";
                case 79: return "Tom got a small piece of pie.";
                case 80: return "Is it free?";
                case 81: return "She only paints with bold colors; she does not like pastels.";
                case 82: return "Yeah, I think it's a good environment for learning English.";
                case 83: return "This is the last random sentence I will be writing and I am going to stop mid-sent";
                case 84: return "We have a lot of rain in June.";
                case 85: return "She always speaks to him in a loud voice.";
                case 86: return "The lake is a long way from here.";
                case 87: return "Writing a list of random sentences is harder than I initially thought it would be.";
                case 88: return "I think I will buy the red car, or I will lease the blue one.";
                case 89: return "A purple pig and a green donkey flew a kite in the middle of the night and ended up sunburnt.";
                case 90: return "The stranger officiates the meal.";
                case 91: return "The shooter says goodbye to his love.";
                case 92: return "The book is in front of the table.";
                case 93: return "Rock music approaches at high velocity.";
                case 94: return "He told us a very exciting adventure story.";
                case 95: return "We have a lot of rain in June.";
                case 96: return "Abstraction is often one floor above you.";
                case 97: return "I am happy to take your donation; any amount will be greatly appreciated.";
                case 98: return "I hear that Nancy is very pretty.";
                case 99: return "I want more detailed information.";
                case 100: return "Sometimes, all you need to do is completely make an ass of yourself and laugh it off to realise that life isn’t so bad after all.";
                case 101: return "Italy is my favorite country; in fact, I plan to spend two weeks there next year.";
                case 102: return "I currently have 4 windows open up… and I don’t know why.";
                case 103: return "The shooter says goodbye to his love.";
                case 104: return "Everyone was busy, so I went to the movie alone.";
                case 105: return "She was too short to see over the fence.";
                case 106: return "I think I will buy the red car, or I will lease the blue one.";
                case 107: return "Yeah, I think it's a good environment for learning English.";
                case 108: return "The book is in front of the table.";
                case 109: return "Writing a list of random sentences is harder than I initially thought it would be.";
                case 110: return "The clock within this blog and the clock on my laptop are 1 hour different from each other.";
                case 111: return "I am never at home on Sundays.";
                case 112: return "The quick brown fox jumps over the lazy dog.";
                case 113: return "I love eating toasted cheese and tuna sandwiches.";
                case 114: return "How was the math test?";
                case 115: return "Rock music approaches at high velocity.";
                case 116: return "She advised him to come back at once.";
                case 117: return "There were white out conditions in the town; subsequently, the roads were impassable.";
                case 118: return "I am counting my calories, yet I really want dessert.";
                case 119: return "She did her best to help him.";
                case 120: return "The waves were crashing on the shore; it was a lovely sight.";
                case 121: return "The lake is a long way from here.";
                case 122: return "Lets all be unique together until we realise we are all the same.";
                case 123: return "Let me help you with your baggage.";
                case 124: return "Someone I know recently combined Maple Syrup & buttered Popcorn thinking it would taste like caramel popcorn. It didn’t and they don’t recommend anyone else do it either.";
                case 125: return "Christmas is coming.";
                case 126: return "The stranger officiates the meal.";
                case 127: return "Joe made the sugar cookies; Susan decorated them.";
                case 128: return "I often see the time 11:11 or 12:34 on clocks.";
                case 129: return "Don't step on the broken glass.";
                case 130: return "The sky is clear; the stars are twinkling.";
                case 131: return "There was no ice cream in the freezer, nor did they have money to go to the store.";
                case 132: return "If you like tuna and tomato sauce- try combining the two. It’s really not as bad as it sounds.";
                case 133: return "If Purple People Eaters are real… where do they find purple people to eat?";
                case 134: return "It was getting dark, and we weren’t there yet.";
                case 135: return "Where do random thoughts come from?";
                case 136: return "The river stole the gods.";
                case 137: return "Last Friday in three week’s time I saw a spotted striped blue worm shake hands with a legless lizard.";
                case 138: return "Sixty-Four comes asking for bread.";
                case 139: return "When I was little I had a car door slammed shut on my hand. I still remember it quite vividly.";
                case 140: return "He turned in the research paper on Friday; otherwise, he would have not passed the class.";
                case 141: return "She works two jobs to make ends meet; at least, that was her reason for not having time to join us.";
                case 142: return "What was the person thinking when they discovered cow’s milk was fine for human consumption… and why did they do it in the first place!?";
                case 143: return "He said he was not there yesterday; however, many people saw him there.";
                case 144: return "This is the last random sentence I will be writing and I am going to stop mid-sent";
                case 145: return "Check back tomorrow; I will see if the book has arrived.";
                case 146: return "I really want to go to work, but I am too sick to drive.";
                case 147: return "Mary plays the piano.";
                case 148: return "Should we start class now, or should we wait for everyone to get here?";
                case 149: return "They got there early, and they got really good seats.";
                case 150: return "A glittering gem is not enough.";
                case 151: return "She only paints with bold colors; she does not like pastels.";
                case 152: return "The memory we used to share is no longer coherent.";
                case 153: return "If I don’t like something, I’ll stay away from it.";
                case 154: return "A song can make or ruin a person’s day if they let it get to them.";
                case 155: return "My Mum tries to be cool by saying that she likes all the same things that I do.";
                case 156: return "She borrowed the book from him many years ago and hasn't yet returned it.";
                case 157: return "Hurry!";
                case 158: return "I checked to make sure that he was still alive.";
                case 159: return "Two seats were vacant.";
                case 160: return "This is a Japanese doll.";
                case 161: return "She folded her handkerchief neatly.";
                case 162: return "He didn’t want to go to the dentist, yet he went anyway.";
                case 163: return "I want to buy a onesie… but know it won’t suit me.";
                case 164: return "Tom got a small piece of pie.";
                case 165: return "Please wait outside of the house.";
                case 166: return "He ran out of money, so he had to stop playing poker.";
                case 167: return "Wow, does that work?";
                case 168: return "I'd rather be a bird than a fish.";
                case 169: return "She wrote him a long letter, but he didn't read it.";
                case 170: return "We need to rent a room for our party.";
                case 171: return "She always speaks to him in a loud voice.";
                case 172: return "Malls are great places to shop; I can find everything I need under one roof.";
                case 173: return "Cats are good pets, for they are clean and are not noisy.";
                case 174: return "We have never been to Asia, nor have we visited Africa.";
                case 175: return "Is it free?";
                case 176: return "I will never be this young again. Ever. Oh damn… I just got older.";
                case 177: return "I was very proud of my nickname throughout high school but today- I couldn’t be any different to what my nickname was.";
                case 178: return "The body may perhaps compensates for the loss of a true metaphysics.";
                case 179: return "The mysterious diary records the voice.";
                case 180: return "I would have gotten the promotion, but my attendance wasn’t good enough.";
                case 181: return "Wednesday is hump day, but has anyone asked the camel if he’s happy about it?";
                case 182: return "If the Easter Bunny and the Tooth Fairy had babies would they take your teeth and leave chocolate for you?";
                case 183: return "Sometimes it is better to just walk away from things and go back to them later when you’re in a better frame of mind.";
                case 184: return "She did not cheat on the test, for it was not the right thing to do.";
                case 185: return "A purple pig and a green donkey flew a kite in the middle of the night and ended up sunburnt.";
                case 186: return "The old apple revels in its authority.";
                case 187: return "Tom got a small piece of pie.";
                case 188: return "I will never be this young again. Ever. Oh damn… I just got older.";
                case 189: return "Should we start class now, or should we wait for everyone to get here?";
                case 190: return "He told us a very exciting adventure story.";
                case 191: return "They got there early, and they got really good seats.";
                case 192: return "The clock within this blog and the clock on my laptop are 1 hour different from each other.";
                case 193: return "Two seats were vacant.";
                case 194: return "What was the person thinking when they discovered cow’s milk was fine for human consumption… and why did they do it in the first place!?";
                case 195: return "Last Friday in three week’s time I saw a spotted striped blue worm shake hands with a legless lizard.";
                case 196: return "Please wait outside of the house.";
                case 197: return "Everyone was busy, so I went to the movie alone.";
                case 198: return "Yeah, I think it's a good environment for learning English.";
                case 199: return "Someone I know recently combined Maple Syrup & buttered Popcorn thinking it would taste like caramel popcorn. It didn’t and they don’t recommend anyone else do it either.";
                case 200: return "There was no ice cream in the freezer, nor did they have money to go to the store.";
                case 201: return "My Mum tries to be cool by saying that she likes all the same things that I do.";
                case 202: return "We have never been to Asia, nor have we visited Africa.";
                case 203: return "Malls are great places to shop; I can find everything I need under one roof.";
                case 204: return "She borrowed the book from him many years ago and hasn't yet returned it.";
                case 205: return "I want more detailed information.";
                case 206: return "It was getting dark, and we weren’t there yet.";
                case 207: return "A purple pig and a green donkey flew a kite in the middle of the night and ended up sunburnt.";
                case 208: return "The body may perhaps compensates for the loss of a true metaphysics.";
                case 209: return "He turned in the research paper on Friday; otherwise, he would have not passed the class.";
                case 210: return "How was the math test?";
                case 211: return "She folded her handkerchief neatly.";
                case 212: return "She only paints with bold colors; she does not like pastels.";
                case 213: return "This is the last random sentence I will be writing and I am going to stop mid-sent";
                case 214: return "The sky is clear; the stars are twinkling.";
                case 215: return "I love eating toasted cheese and tuna sandwiches.";
                case 216: return "Hurry!";
                case 217: return "The old apple revels in its authority.";
                case 218: return "I'd rather be a bird than a fish.";
                case 219: return "If I don’t like something, I’ll stay away from it.";
                case 220: return "I currently have 4 windows open up… and I don’t know why.";
                case 221: return "Abstraction is often one floor above you.";
                case 222: return "Wow, does that work?";
                case 223: return "The book is in front of the table.";
                case 224: return "Writing a list of random sentences is harder than I initially thought it would be.";
                case 225: return "If the Easter Bunny and the Tooth Fairy had babies would they take your teeth and leave chocolate for you?";
                case 226: return "I checked to make sure that he was still alive.";
                case 227: return "She always speaks to him in a loud voice.";
                case 228: return "I am happy to take your donation; any amount will be greatly appreciated.";
                case 229: return "Wednesday is hump day, but has anyone asked the camel if he’s happy about it?";
                case 230: return "Italy is my favorite country; in fact, I plan to spend two weeks there next year.";
                case 231: return "A glittering gem is not enough.";
                case 232: return "Joe made the sugar cookies; Susan decorated them.";
                case 233: return "The stranger officiates the meal.";
                case 234: return "He said he was not there yesterday; however, many people saw him there.";
                case 235: return "Cats are good pets, for they are clean and are not noisy.";
                case 236: return "If you like tuna and tomato sauce- try combining the two. It’s really not as bad as it sounds.";
                case 237: return "Sometimes, all you need to do is completely make an ass of yourself and laugh it off to realise that life isn’t so bad after all.";
                case 238: return "Christmas is coming.";
                case 239: return "Let me help you with your baggage.";
                case 240: return "Sixty-Four comes asking for bread.";
                case 241: return "I hear that Nancy is very pretty.";
                case 242: return "There were white out conditions in the town; subsequently, the roads were impassable.";
                case 243: return "The river stole the gods.";
                case 244: return "He ran out of money, so he had to stop playing poker.";
                case 245: return "I am counting my calories, yet I really want dessert.";
                case 246: return "She did not cheat on the test, for it was not the right thing to do.";
                case 247: return "This is a Japanese doll.";
                case 248: return "She was too short to see over the fence.";
                case 249: return "Check back tomorrow; I will see if the book has arrived.";
                case 250: return "She advised him to come back at once.";
                case 251: return "Don't step on the broken glass.";
                case 252: return "I think I will buy the red car, or I will lease the blue one.";
                case 253: return "Where do random thoughts come from?";
                case 254: return "She did her best to help him.";
                case 255: return "Sometimes it is better to just walk away from things and go back to them later when you’re in a better frame of mind.";
                case 256: return "The quick brown fox jumps over the lazy dog.";
                case 257: return "A song can make or ruin a person’s day if they let it get to them.";
                case 258: return "I am never at home on Sundays.";
                case 259: return "When I was little I had a car door slammed shut on my hand. I still remember it quite vividly.";
                case 260: return "I often see the time 11:11 or 12:34 on clocks.";
                case 261: return "The waves were crashing on the shore; it was a lovely sight.";
                case 262: return "We need to rent a room for our party.";
                case 263: return "He didn’t want to go to the dentist, yet he went anyway.";
                case 264: return "We have a lot of rain in June.";
                case 265: return "The lake is a long way from here.";
                case 266: return "I really want to go to work, but I am too sick to drive.";
                case 267: return "She works two jobs to make ends meet; at least, that was her reason for not having time to join us.";
                case 268: return "I want to buy a onesie… but know it won’t suit me.";
                case 269: return "Mary plays the piano.";
                case 270: return "Is it free?";
                case 271: return "The mysterious diary records the voice.";
                case 272: return "Lets all be unique together until we realise we are all the same.";
                case 273: return "I would have gotten the promotion, but my attendance wasn’t good enough.";
                case 274: return "The memory we used to share is no longer coherent.";
                case 275: return "She wrote him a long letter, but he didn't read it.";
                case 276: return "I was very proud of my nickname throughout high school but today- I couldn’t be any different to what my nickname was.";
                case 277: return "The shooter says goodbye to his love.";
                case 278: return "If Purple People Eaters are real… where do they find purple people to eat?";
                case 279: return "Rock music approaches at high velocity.";
                case 280: return "I often see the time 11:11 or 12:34 on clocks.";
                case 281: return "What was the person thinking when they discovered cow’s milk was fine for human consumption… and why did they do it in the first place!?";
                case 282: return "Christmas is coming.";
                case 283: return "A song can make or ruin a person’s day if they let it get to them.";
                case 284: return "Where do random thoughts come from?";
                case 285: return "We have a lot of rain in June.";
                case 286: return "The memory we used to share is no longer coherent.";
                case 287: return "If the Easter Bunny and the Tooth Fairy had babies would they take your teeth and leave chocolate for you?";
                case 288: return "She advised him to come back at once.";
                case 289: return "The mysterious diary records the voice.";
                case 290: return "Let me help you with your baggage.";
                case 291: return "Mary plays the piano.";
                case 292: return "He ran out of money, so he had to stop playing poker.";
                case 293: return "She only paints with bold colors; she does not like pastels.";
                case 294: return "Everyone was busy, so I went to the movie alone.";
                case 295: return "Sixty-Four comes asking for bread.";
                case 296: return "Check back tomorrow; I will see if the book has arrived.";
                case 297: return "The quick brown fox jumps over the lazy dog.";
                case 298: return "Abstraction is often one floor above you.";
                case 299: return "I want to buy a onesie… but know it won’t suit me.";
                case 300: return "Should we start class now, or should we wait for everyone to get here?";
                case 301: return "Lets all be unique together until we realise we are all the same.";
                case 302: return "The shooter says goodbye to his love.";
                case 303: return "She borrowed the book from him many years ago and hasn't yet returned it.";
                case 304: return "I think I will buy the red car, or I will lease the blue one.";
                case 305: return "This is a Japanese doll.";
                case 306: return "The sky is clear; the stars are twinkling.";
                case 307: return "She wrote him a long letter, but he didn't read it.";
                case 308: return "I was very proud of my nickname throughout high school but today- I couldn’t be any different to what my nickname was.";
                case 309: return "She works two jobs to make ends meet; at least, that was her reason for not having time to join us.";
                case 310: return "If Purple People Eaters are real… where do they find purple people to eat?";
                case 311: return "She folded her handkerchief neatly.";
                case 312: return "She was too short to see over the fence.";
                case 313: return "I am counting my calories, yet I really want dessert.";
                case 314: return "Joe made the sugar cookies; Susan decorated them.";
                case 315: return "A glittering gem is not enough.";
                case 316: return "My Mum tries to be cool by saying that she likes all the same things that I do.";
                case 317: return "I hear that Nancy is very pretty.";
                case 318: return "He turned in the research paper on Friday; otherwise, he would have not passed the class.";
                case 319: return "Please wait outside of the house.";
                case 320: return "The lake is a long way from here.";
                case 321: return "Hurry!";
                case 322: return "He said he was not there yesterday; however, many people saw him there.";
                case 323: return "I checked to make sure that he was still alive.";
                case 324: return "Someone I know recently combined Maple Syrup & buttered Popcorn thinking it would taste like caramel popcorn. It didn’t and they don’t recommend anyone else do it either.";
                case 325: return "Wednesday is hump day, but has anyone asked the camel if he’s happy about it?";
                case 326: return "I will never be this young again. Ever. Oh damn… I just got older.";
                case 327: return "He told us a very exciting adventure story.";
                case 328: return "This is the last random sentence I will be writing and I am going to stop mid-sent";
                case 329: return "They got there early, and they got really good seats.";
                case 330: return "Malls are great places to shop; I can find everything I need under one roof.";
                case 331: return "The waves were crashing on the shore; it was a lovely sight.";
                case 332: return "If you like tuna and tomato sauce- try combining the two. It’s really not as bad as it sounds.";
                case 333: return "She did not cheat on the test, for it was not the right thing to do.";
                case 334: return "Don't step on the broken glass.";
                case 335: return "I currently have 4 windows open up… and I don’t know why.";
                case 336: return "The clock within this blog and the clock on my laptop are 1 hour different from each other.";
                case 337: return "Sometimes it is better to just walk away from things and go back to them later when you’re in a better frame of mind.";
                case 338: return "Is it free?";
                case 339: return "We have never been to Asia, nor have we visited Africa.";
                case 340: return "There were white out conditions in the town; subsequently, the roads were impassable.";
                case 341: return "The old apple revels in its authority.";
                case 342: return "She always speaks to him in a loud voice.";
                case 343: return "We need to rent a room for our party.";
                case 344: return "The river stole the gods.";
                case 345: return "The body may perhaps compensates for the loss of a true metaphysics.";
                case 346: return "The book is in front of the table.";
                case 347: return "Tom got a small piece of pie.";
                case 348: return "Writing a list of random sentences is harder than I initially thought it would be.";
                case 349: return "It was getting dark, and we weren’t there yet.";
                case 350: return "The stranger officiates the meal.";
                case 351: return "I would have gotten the promotion, but my attendance wasn’t good enough.";
                case 352: return "I love eating toasted cheese and tuna sandwiches.";
                case 353: return "I want more detailed information.";
                case 354: return "There was no ice cream in the freezer, nor did they have money to go to the store.";
                case 355: return "He didn’t want to go to the dentist, yet he went anyway.";
                case 356: return "Two seats were vacant.";
                case 357: return "Last Friday in three week’s time I saw a spotted striped blue worm shake hands with a legless lizard.";
                case 358: return "Rock music approaches at high velocity.";
                case 359: return "Yeah, I think it's a good environment for learning English.";
                case 360: return "Sometimes, all you need to do is completely make an ass of yourself and laugh it off to realise that life isn’t so bad after all.";
                case 361: return "Cats are good pets, for they are clean and are not noisy.";
                case 362: return "When I was little I had a car door slammed shut on my hand. I still remember it quite vividly.";
                case 363: return "If I don’t like something, I’ll stay away from it.";
                case 364: return "I really want to go to work, but I am too sick to drive.";
                case 365: return "A purple pig and a green donkey flew a kite in the middle of the night and ended up sunburnt.";
                case 366: return "I am happy to take your donation; any amount will be greatly appreciated.";
                case 367: return "I'd rather be a bird than a fish.";
                case 368: return "How was the math test?";
                case 369: return "Italy is my favorite country; in fact, I plan to spend two weeks there next year.";
                case 370: return "I am never at home on Sundays.";
                case 380: return "Wow, does that work?";
                case 390: return "She did her best to help him.";

                default: return null;
            }
        }
        public static void WriteLookups(DirectoryInfo dir)
        {
            File.WriteAllText(Path.Combine(dir.FullName, "z_Healthboards.csv"),
@"
Code,Description
A,Ayrshire and Arran
B,Borders
C,Argyle and Clyde
D,State Hospital
E,England
F,Fife
G,Greater Glasgow
H,Highland
I,Inverness
J,Junderland
K,Krief
L,Lanarkshire
M,Metropolitan Area
N,Grampian
O,Orkney
P,Pitlochry
Q,Queensferry
R,Retired
S,Lothian
T,Tayside
U,Unknown
V,Forth Valley
W,Western Isles
X,Common Service Agency
Y,Dumfries and Galloway
Z,Shetland");


            File.WriteAllText(Path.Combine(dir.FullName, "z_PCStenosis.csv"),
@"Code,CodeValueDescription
1,Normal
2,Minimal disease
3,30% < 50%
4,50% < 70%
5,70% < 99%
6,Occluded
9,Unsure");

            File.WriteAllText(Path.Combine(dir.FullName, "z_ICStenosisLookup.csv"),
@"Code Type Description,Code,CodeValueDescription
%stenosis Carotid Artery Scan,1,Normal
%stenosis Carotid Artery Scan,2,Minimal disease
%stenosis Carotid Artery Scan,3,30% < 50%
%stenosis Carotid Artery Scan,4,50% < 70%
%stenosis Carotid Artery Scan,5,70% < 99%
%stenosis Carotid Artery Scan,6,Occluded
%stenosis Carotid Artery Scan,8,See report text
%stenosis Carotid Artery Scan,9,Unsure");

            File.WriteAllText(Path.Combine(dir.FullName, "z_VertflowLookup.csv"),
@"Code,CodeValueDescription
1,Cephalad
2,Reversed
3,Not Detected
4,See report text");
            
            File.WriteAllText(Path.Combine(dir.FullName, "z_StenosisLookup.csv"),
@"Code,CodeValueDescription
1,Normal
2,Minimum
3,Moderate
4,Severe
5,Occluded
9,Not seen
6,See report text");
            
            File.WriteAllText(Path.Combine(dir.FullName, "z_PlaqueLookup.csv"),
@"Code,CodeValueDescription
1,I
2,II
3,III
4,IV
9,Nil
8,Not applicable");
        }

    }
}