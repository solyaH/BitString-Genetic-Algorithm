using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Numerics;


namespace Genetic
{
    class GeneticBasic
    {
        private Random rand = new Random();
        double[] intervalStarts;
        double[] intervalEnds;
        int n;
        double Pm;
        double Pc;
        int maxIteration = 10;
        public int iteration;
        public double[] maxArgument;
        public double absMaxFunction;
        public List<double> maxFuncEachIteration;

        public GeneticBasic(double[] intervalStarts, double[] intervalEnds, int n, double Pm, double Pc)
        {
            this.intervalStarts = intervalStarts;
            this.intervalEnds = intervalEnds;
            this.n = n;
            this.Pm = Pm;
            this.Pc = Pc;
            this.iteration = 0;
            this.maxFuncEachIteration = new List<double>();
            Calculate();
        }

        private void Calculate()
        {
            int[] maxLengthArr = new int[intervalStarts.Length];
            for (int i = 0; i < maxLengthArr.Length; i++)
            {
                maxLengthArr[i] = Converter.BitArrayMaxLength(intervalStarts[i], intervalEnds[i]);
            }
            int maxLength = maxLengthArr.Sum();

            List<BitArray> population = GeneratePopulation(n, maxLengthArr);

            int counter = 0;
            double constAdd = 0;
            absMaxFunction = int.MinValue;
            do
            {
                constAdd = ConstantAdd(population, intervalStarts, intervalEnds, maxLengthArr) + 1;
                List<double> calculateFunction = population.Select(ind => FitnessFunctionMulti(Converter.ConvertBitToMultiIntervals(intervalStarts, intervalEnds, maxLengthArr, ind))).ToList<double>();
                double newIterMaxFunction = calculateFunction.Max();
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
                    maxArgument = Converter.ConvertBitToMultiIntervals(intervalStarts, intervalEnds, maxLengthArr, population[indexOfMax]);
                    //maxFunction = FitnessFunctionMulti(maxArgument);
                }

                double[] eval = EvaluatedFunctionArr(population, constAdd, intervalStarts, intervalEnds, maxLengthArr);
                double[] probabilitiesRange = ProbabilitiesRange(eval);

                List<BitArray> newPopulation = new List<BitArray>();
                do
                {
                    List<BitArray> newIndividuals = Crossover(Select(population, probabilitiesRange), Select(population, probabilitiesRange), Pc);
                    newIndividuals[0] = Mutation(newIndividuals[0], Pm);
                    newIndividuals[1] = Mutation(newIndividuals[1], Pm);
                    newPopulation.AddRange(newIndividuals);
                }
                while (newPopulation.Count < population.Count);
                population = new List<BitArray>(newPopulation);
                iteration++;
            }
            while (counter <maxIteration);
            //population = population.OrderByDescending(ind => FitnessFunctionMulti(Converter.ConvertBitToMultiIntervals(intervalStarts, intervalEnds, maxLengthArr, ind))).ToList();
            //maxArgument = Converter.ConvertBitToMultiIntervals(intervalStarts, intervalEnds, maxLengthArr, population[0]);
            //maxFunction = FitnessFunctionMulti(maxArgument);
        }

        double FitnessFunctionMulti(double[] x)
        {
            return -(Math.Pow(x[0], 2) + Math.Pow(x[1], 2));
        }
        //(Math.Abs(Math.Pow(x[0], 2) + Math.Pow(x[1], 2)-1)+ Math.Abs(x[0] + x[1] - 1))
        //-(Math.Pow(x[0], 2) + Math.Pow(x[1], 2)+10) 
        //Math.Abs(x[0]) - 1

        double ConstantAdd(List<BitArray> population,double[] intervalStarts, double[] intervalEnds, int[] maxLengthArr)
        {
            return Math.Abs(population.Select(ind => FitnessFunctionMulti(Converter.ConvertBitToMultiIntervals(intervalStarts, intervalEnds, maxLengthArr, ind))).Min());
        }

        List<BitArray> GeneratePopulation(int n, int[] maxLengthArr)
        {
            int maxLength = maxLengthArr.Sum();
            List<BitArray> population = new List<BitArray>();
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
                population.Add(newInd);
            }

            return population;
        }

        BitArray Select(List<BitArray> population, double[] probabilitiesRange)
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

        double[] EvaluatedFunctionArr(List<BitArray> population, double maxFunc, double[] intervalStarts, double[] intervalEnds, int[] maxLengthArr)//(List<BitArray> population, double maxFunc, double a, double b)
        {
            double[] evaluatedFunction = new double[population.Count];
            //double[] function = new double[population.Count];


            for (int i = 0; i < population.Count; i++)
            {
                //function[i] = FitnessFunctionMulti(ConvertBitToMultiIntervals(intervalStarts, intervalEnds, maxLengthArr, population[i]));
                evaluatedFunction[i] = FitnessFunctionMulti(Converter.ConvertBitToMultiIntervals(intervalStarts, intervalEnds, maxLengthArr, population[i])) + maxFunc;//-FitnessFunction(ConvertBitToInterval(a, b, population[i])) + maxFunc
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

        List<BitArray> Crossover(BitArray individual1, BitArray individual2, double Pc)
        {
            List<BitArray> newIndividuals = new List<BitArray>();

            double crossoverProb = rand.NextDouble();
            if (crossoverProb <= Pc)
            {
                int crossIndex = rand.Next(0, individual1.Length - 1);
                newIndividuals.Add(new BitArray(individual1.Cast<bool>().Take(crossIndex).Concat(individual2.Cast<bool>().Skip(crossIndex).Take(individual1.Length)).ToArray()));
                newIndividuals.Add(new BitArray(individual2.Cast<bool>().Take(crossIndex).Concat(individual1.Cast<bool>().Skip(crossIndex).Take(individual1.Length)).ToArray()));
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
                newIndividuals.Add(individual1);
                newIndividuals.Add(individual2);
            }

            return newIndividuals;
        }

        BitArray Mutation(BitArray individual, double Pm)
        {
            BitArray newIndividual = new BitArray(individual);
            double mutationProb = rand.NextDouble();
            if (mutationProb <= Pm)
            {
                int mutIndex = rand.Next(0, individual.Length - 1);
                newIndividual[mutIndex] = individual[mutIndex] ? false : true;
                //Console.WriteLine("Mutation {0}", mutIndex);
                //Console.WriteLine("before {0}", individual);
                //Console.WriteLine("after {0}", newIndividual);
            }

            return newIndividual;
        }
    }
}
