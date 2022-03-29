using System;
using System.IO;

namespace Slayer_Task_Simulator
{
    struct Task
    {
        public Task(string name, int weight, int lowerBound, int upperBound, int exp)
        {
            Name = name;
            Weight = weight;
            LowerBound = lowerBound;
            UpperBound = upperBound;
            Exp = exp;
        }

        public string Name { get; }
        public int Weight { get; }
        public int LowerBound { get; }
        public int UpperBound { get; }
        public int Exp { get; }
    }

    class Program
    {

        static void Main(string[] args)
        {
            int numSimulations = 100000;
            int numSuccesses = 0;
            int numFailures = 0;

            int longestChain = 0;
            string longestChainString = "";
            int longestFailureChain = 0;
            string longestFailureChainString = "";


            Random rng = new Random();
            Task[] KrystiliaTaskList = new Task[26];
            Task[] TuraelTaskList = new Task[22];
            int[] taskDistribution = new int[50];

            using (var reader = new StreamReader(@"KrystiliaTasks.csv"))
            {
                int i = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    KrystiliaTaskList[i] = new Task(values[0], Int32.Parse(values[1]), Int32.Parse(values[2]), Int32.Parse(values[3]), Int32.Parse(values[4]));

                    i++;
                }
            }

            using (var reader = new StreamReader(@"TuraelTasks.csv"))
            {
                int i = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');

                    TuraelTaskList[i] = new Task(values[0], Int32.Parse(values[1]), Int32.Parse(values[2]), Int32.Parse(values[3]), Int32.Parse(values[4]));

                    i++;
                }
            }

            Console.ReadLine();

            while (true)
            {
                int currentSlayerExp = 1275;
                int previousTaskIndex = -1;
                bool isExpediting = true;
                int taskStreak = 0;
                int slayerPoints = 0;

                int currentChain = 0;
                string currentChainString = "";

                while (slayerPoints < 50 && slayerPoints != -1)
                {
                    previousTaskIndex = GenerateKrystiliaTask(KrystiliaTaskList, rng, previousTaskIndex);
                    currentChain++;
                    currentChainString += KrystiliaTaskList[previousTaskIndex].Name + " - ";

                    if (previousTaskIndex == 0)
                    {

                        int turaelTask = GenerateTuraelTask(TuraelTaskList, rng, currentSlayerExp);
                        if (turaelTask == 1 || turaelTask == 8 || turaelTask == 13 || turaelTask == 14 || turaelTask == 18 || turaelTask == 20 || turaelTask == 21)
                        {
                            //FAIL STATE
                            slayerPoints = -1;
                            currentChain++;
                            currentChainString += "Turael: " + TuraelTaskList[turaelTask].Name + " - ";
                        }
                        else
                        {
                            //TURAEL TASK COMPLETE
                            currentChain++;
                            currentChainString += "Turael: " + TuraelTaskList[turaelTask].Name + " - ";
                            float gainedExp = rng.Next(TuraelTaskList[turaelTask].LowerBound, TuraelTaskList[turaelTask].UpperBound);
                            if (isExpediting)
                                gainedExp *= .8f;
                            currentSlayerExp += (int)gainedExp * TuraelTaskList[turaelTask].Exp;

                            taskStreak = 0;
                            previousTaskIndex = -1;
                        }
                    }
                    else
                    {
                        //KRYSTILIA TASK COMPLETE
                        
                        float gainedExp = rng.Next(KrystiliaTaskList[previousTaskIndex].LowerBound, KrystiliaTaskList[previousTaskIndex].UpperBound);
                        if (isExpediting)
                            gainedExp *= .8f;
                        currentSlayerExp += (int)gainedExp * KrystiliaTaskList[previousTaskIndex].Exp;
                        taskStreak++;
                        if (taskStreak >= 5)
                            slayerPoints += 25;
                    }
                }

                if (slayerPoints == 50)
                {
                    
                    if (currentChain > longestChain)
                    {
                        longestChain = currentChain;
                        longestChainString = currentChainString;
                    }

                    taskDistribution[currentChain]++;
                    numSuccesses++;
                    numSimulations--;
                }
                else if (slayerPoints == -1)
                {
                    
                    if (currentChain > longestFailureChain)
                    {
                        longestFailureChain = currentChain;
                        longestFailureChainString = currentChainString;
                    }
                    numFailures++;
                    numSimulations--;

                }

                if (numSimulations == 0)
                {
                    Console.WriteLine("Number of Simulations: " + (100000 - numSimulations));
                    Console.WriteLine();
                    Console.WriteLine("Number of Successes: " + numSuccesses);
                    Console.WriteLine("Number of Failures: " + numFailures);
                    Console.WriteLine("Success Rate: " + (float)numSuccesses / 100000.0f);
                    Console.WriteLine();
                    Console.WriteLine("Longest Successful Task Chain: " + longestChain + " Tasks.");
                    Console.WriteLine(longestChainString);
                    Console.WriteLine();
                    Console.WriteLine("Longest Failure Task Chain: " + longestFailureChain + " Tasks.");
                    Console.WriteLine(longestFailureChainString);
                    Console.ReadLine();
                    Console.WriteLine();
                    for (int i = 6; i < longestChain + 1; i++) {
                        Console.WriteLine(i + " tasks: " + taskDistribution[i]);
                    }
                    Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("Number of Simulations: " + (100000 - numSimulations));
                }
            }
        }



        static int GenerateKrystiliaTask(Task[] tasklist, Random rng, int previousTaskIndex)
        {
            int taskRoll = rng.Next(140);
            int taskIndex = -1;

            while (taskRoll >= 0)
            {
                taskIndex++;
                taskRoll -= tasklist[taskIndex].Weight;
            }

            if (taskIndex == previousTaskIndex)
            {
                taskIndex = GenerateKrystiliaTask(tasklist, rng, previousTaskIndex);
            }
            return taskIndex;
        }

        static int GenerateTuraelTask(Task[] tasklist, Random rng, int currentSlayerExp)
        {
            int taskRoll;
            int taskIndex = -1;

            if (currentSlayerExp >= 5624)
            {
                taskRoll = rng.Next(156);
            }
            else if (currentSlayerExp >= 3115)
            {
                taskRoll = rng.Next(148);
            }
            else
            {
                taskRoll = rng.Next(140);
            }

            while (taskRoll >= 0)
            {
                taskIndex++;
                taskRoll -= tasklist[taskIndex].Weight;
            }

            return taskIndex;
        }
    }
}
