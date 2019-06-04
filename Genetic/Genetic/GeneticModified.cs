using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Numerics;

namespace Genetic
{
    class GeneticModified
    {
        private Random rand = new Random();
        int MinLT;
        int MaxLT;
        int p;
        double eta;
        double[] intervalStarts;
        double[] intervalEnds;
        int n;
        double Pm;
        double Pc;
        int maxIteration = 10;
        public int iteration;
        public double[] maxArgument;
        public double absMaxFunction;
        //public double absMinFunction;
        public List<double> maxFuncEachIteration;

        public GeneticModified(double[] intervalStarts, double[] intervalEnds, int n, double Pm, double Pc ,int MinLT,int MaxLT , int p)
        {
            this.intervalStarts = intervalStarts;
            this.intervalEnds = intervalEnds;
            this.n = n;
            this.Pm = Pm;
            this.Pc = Pc;
            this.iteration = 0;
            this.MinLT = MinLT;
            this.MaxLT = MaxLT;
            this.p = p;
            this.eta = (MaxLT - MinLT) / 2.0;
            this.maxFuncEachIteration = new List<double>();
            Calculate();
        }

        private int[] Lifetime(List<Individual> population, int[] maxLengthArr, double constAdd)
        {
            double[] evaluatedFunction = EvaluatedFunctionArr(population, constAdd, maxLengthArr);
            double maxFit = evaluatedFunction.Max();
            double minFit = evaluatedFunction.Min();
            double avgFit = evaluatedFunction.Average();
            int[] lifetime = new int[population.Count];
            for(int j = 0; j < population.Count; j++)
            {
                if (avgFit >= evaluatedFunction[j])
                {
                    lifetime[j]=(int)(MinLT + eta * (evaluatedFunction[j] - minFit) / (avgFit - minFit));
                }
                else
                {
                    lifetime[j]= (int)((MinLT +MaxLT)/2+ eta * (evaluatedFunction[j] - avgFit) / (maxFit - avgFit));
                }
            }
            return lifetime;
        }

        private void Calculate()
        {
            int[] maxLengthArr = new int[intervalStarts.Length];
            for (int i = 0; i < maxLengthArr.Length; i++)
            {
                maxLengthArr[i] = Converter.BitArrayMaxLength(intervalStarts[i], intervalEnds[i]);
            }
            int maxLength = maxLengthArr.Sum();

            List<Individual> population = GeneratePopulation(n, maxLengthArr);

            int counter = 0;
            double constAdd = 0;
            absMaxFunction = int.MinValue;
            double newIterMaxFunction = int.MinValue;
            do
            {
                constAdd = ConstantAdd(population, intervalStarts, intervalEnds, maxLengthArr) + 1;
                List<double> calculateFunction = population.Select(ind => FitnessFunctionMulti(Converter.ConvertBitToMultiIntervals(intervalStarts, intervalEnds, maxLengthArr, ind.binaryArray))).ToList<double>();
                newIterMaxFunction = calculateFunction.Max();

                maxFuncEachIteration.Add(newIterMaxFunction);//population.Select(ind => FitnessFunctionMulti(Converter.ConvertBitToMultiIntervals(intervalStarts, intervalEnds, maxLengthArr, ind))).Max();
                if (newIterMaxFunction <= absMaxFunction)
                {
                    counter++;
                }
                else
                {
                    counter = 0;
                    absMaxFunction = newIterMaxFunction;
                    var indexOfMax = calculateFunction.IndexOf(newIterMaxFunction);
                    maxArgument = Converter.ConvertBitToMultiIntervals(intervalStarts, intervalEnds, maxLengthArr, population[indexOfMax].binaryArray);
                    //maxFunction = FitnessFunctionMulti(maxArgument);
                }

                double[] eval = EvaluatedFunctionArr(population, constAdd, maxLengthArr);
                double[] probabilitiesRange = ProbabilitiesRange(eval);

                List<Individual> newPopulation = new List<Individual>();
                do
                {
                    List<Individual> newIndividuals = Crossover(Select(population, probabilitiesRange), Select(population, probabilitiesRange), Pc);

                    newIndividuals[0] = Mutation(newIndividuals[0], Pm);               
                    newIndividuals[1] = Mutation(newIndividuals[1], Pm);                                 
                    
                    newPopulation.AddRange(newIndividuals);
                }
                while (newPopulation.Count < population.Count * p);

                for (int i = 0; i < newPopulation.Count; i++)
                {
                    if (!newPopulation[i].crossovered && !newPopulation[i].mutated)
                    {
                        newPopulation[i].age++;
                    }
                    else
                    {
                        newPopulation[i].crossovered = false;
                        newPopulation[i].mutated = false;
                    }
                }

                int[] lifetime = Lifetime(newPopulation, maxLengthArr, constAdd);
                List<int> ages = newPopulation.Select(ind => ind.age).ToList();

                List<Individual> newPopulationWithLT = new List<Individual>();
                for (int i = 0; i < lifetime.Length; i++)
                {
                    if (newPopulation[i].age < lifetime[i])
                    {
                        newPopulationWithLT.Add(newPopulation[i]);
                    }
                }
                population = new List<Individual>(newPopulationWithLT);

                iteration++;
            }
            while (counter<maxIteration);//counter

        }

        double FitnessFunctionMulti(double[] x)
        {
            return -(Math.Pow(x[0], 2) + Math.Pow(x[1], 2));
        }
        //(Math.Abs(Math.Pow(x[0], 2) + Math.Pow(x[1], 2)-1)+ Math.Abs(x[0] + x[1] - 1))
        //-(Math.Pow(x[0], 2) + Math.Pow(x[1], 2)+10) 
        //Math.Abs(x[0]) - 1

        double ConstantAdd(List<Individual> population, double[] intervalStarts, double[] intervalEnds, int[] maxLengthArr)
        {
            return Math.Abs(population.Select(ind => FitnessFunctionMulti(Converter.ConvertBitToMultiIntervals(intervalStarts, intervalEnds, maxLengthArr, ind.binaryArray))).Min());
        }

        List<Individual> GeneratePopulation(int n, int[] maxLengthArr)
        {
            int maxLength = maxLengthArr.Sum();
            List<Individual> population = new List<Individual>();
            for (int j = 0; j < n; j++)
            {
                bool[] newIndBool = new bool[0];
                for (int i = 0; i < maxLengthArr.Length; i++)
                {
                    int range = (int)Math.Pow(2, maxLengthArr[i]) - 1;
                    int randomIndividual = rand.Next(0, range);
                    newIndBool = newIndBool.Concat(Converter.ConvertIntToBitStr(randomIndividual, maxLengthArr[i])).ToArray();
                    //newInd.Cast<bool>().Skip(maxLengthArr[i - 1]).Take(maxLengthArr[i]).Concat(individual2.Cast<bool>().Skip(crossIndex).Take(individual1.Length)).ToArray());
                }
                BitArray newInd = new BitArray(newIndBool);
                population.Add(new Individual(newInd));
            }

            return population;
        }

        Individual Select(List<Individual> population, double[] probabilitiesRange)
        {
            double probability = rand.NextDouble();
            //double[] probabilitiesRange = ProbabilitiesRange(population, previousMax, a, b);
            int indIndex = -1;
            for (int i = 1; i < probabilitiesRange.Length; i++)
            {
                if (probability <= probabilitiesRange[i] && probability > probabilitiesRange[i - 1])
                {
                    indIndex = i - 1;
                    break;
                }
            }
            //Console.WriteLine(indIndex);
            return population[indIndex];
        }

        double[] ProbabilitiesRange(double[] evaluatedFunction)
        {
            double[] probabilitiesRange = new double[evaluatedFunction.Length + 1];
            double[] prob = new double[evaluatedFunction.Length];

            double sum = evaluatedFunction.Sum();

            for (int i = 1; i < probabilitiesRange.Length; i++)
            {
                prob[i - 1] = evaluatedFunction[i - 1] / sum;
                probabilitiesRange[i] = evaluatedFunction[i - 1] / sum + probabilitiesRange[i - 1];
            }

            return probabilitiesRange;
        }

        double[] EvaluatedFunctionArr(List<Individual> population, double maxFunc, int[] maxLengthArr)//(List<BitArray> population, double maxFunc, double a, double b)
        {
            double[] evaluatedFunction = new double[population.Count];
            //double[] function = new double[population.Count];


            for (int i = 0; i < population.Count; i++)
            {
                //function[i] = FitnessFunctionMulti(ConvertBitToMultiIntervals(intervalStarts, intervalEnds, maxLengthArr, population[i]));
                evaluatedFunction[i] = FitnessFunctionMulti(Converter.ConvertBitToMultiIntervals(intervalStarts, intervalEnds, maxLengthArr, population[i].binaryArray)) + maxFunc;//-FitnessFunction(ConvertBitToInterval(a, b, population[i])) + maxFunc
            }
            return evaluatedFunction;
        }

        double MaxFitnessFunction(List<BitArray> population, double previousMax, double[] intervalStarts, double[] intervalEnds, int[] maxLengthArr)//(List<BitArray> population, double previousMax, double a, double b)
        {
            double maxFunc = previousMax;//previousMax
            foreach (BitArray ind in population)
            {
                double func = FitnessFunctionMulti(Converter.ConvertBitToMultiIntervals(intervalStarts, intervalEnds, maxLengthArr, ind));// FitnessFunction(ConvertBitToInterval(a, b, ind))
                if (func > maxFunc)
                {
                    maxFunc = func;
                }
            }
            return maxFunc;
        }

        List<Individual> Crossover(Individual individual1, Individual individual2, double Pc)
        {
            List<Individual> newIndividuals = new List<Individual>();

            double crossoverProb = rand.NextDouble();
            if (crossoverProb <= Pc)
            {
                int crossIndex = rand.Next(0, individual1.binaryArray.Length - 1);
                Individual newInd1 = new Individual(new BitArray(individual1.binaryArray.Cast<bool>().Take(crossIndex).Concat(individual2.binaryArray.Cast<bool>().Skip(crossIndex).Take(individual1.binaryArray.Length)).ToArray()));
                Individual newInd2 = new Individual(new BitArray(individual2.binaryArray.Cast<bool>().Take(crossIndex).Concat(individual1.binaryArray.Cast<bool>().Skip(crossIndex).Take(individual1.binaryArray.Length)).ToArray()));
                newInd1.crossovered = true;
                newInd2.crossovered = true;
                newIndividuals.Add(newInd1);
                newIndividuals.Add(newInd2);
                //Console.WriteLine("Crossover {0}", crossIndex);
                //Console.WriteLine("before");
                //DisplayBitArray(individual1);
                //DisplayBitArray(individual2);
                //Console.WriteLine("after");
                //DisplayBitArray(newIndividuals[0]);
                //DisplayBitArray(newIndividuals[1]);
            }
            else
            {
                newIndividuals.Add(new Individual(individual1));
                newIndividuals.Add(new Individual(individual2));
            }

            return newIndividuals;
        }

        Individual Mutation(Individual individual, double Pm)
        {
            Individual newIndividual=new Individual(individual);
            double mutationProb = rand.NextDouble();
            if (mutationProb <= Pm)
            {
                int mutIndex = rand.Next(0, individual.binaryArray.Length - 1);
                BitArray newArr = individual.binaryArray;
                newArr[mutIndex]=newArr[mutIndex] ? false : true;
                newIndividual = new Individual(newArr);
                newIndividual.mutated = true;
                //Console.WriteLine("Mutation {0}", mutIndex);
                //Console.WriteLine("before {0}", individual);
                //Console.WriteLine("after {0}", newIndividual);
            }

            return newIndividual;
        }
    }
}
