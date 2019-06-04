using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Numerics;

namespace Genetic
{
    public static class Converter
    {
        public static bool[] ConvertIntToBitStr(int x, int length)
        {
            bool[] boolArray = Convert.ToString(x, 2).PadLeft(length, '0').Select(c => c == '1').ToArray();
            return boolArray;
        }

        public static int ConvertBitToInt(BitArray x)
        {
            int newX = 0;
            for (int i = 0; i < x.Count; i++)
            {
                if (x[x.Count - i - 1])
                {
                    newX += (int)(Math.Pow(2, i));
                }
            }
            return newX;
        }

        public static string ConvertBitToString(BitArray bitArray)
        {
            string binary = "";
            for (int i = 0; i < bitArray.Count; i++)
            {
                bool bit = bitArray.Get(i);
                binary+=bit ? 1 : 0;
            }
            return binary;
        }

        public static int BitArrayMaxLength(double a, double b)
        {
            return (int)Math.Log((b - a) * Math.Pow(10, 6), 2) + 1;
        }

        public static double ConvertBitToInterval(double a, double b, BitArray x)
        {
            return (b - a) / (Math.Pow(2, x.Count) - 1) * ConvertBitToInt(x) + a;
        }

        public static double[] ConvertBitToMultiIntervals(double[] intervalStarts, double[] intervalEnds, int[] maxLengthArr, BitArray individual)
        {
            double[] x = new double[maxLengthArr.Length];

            for (int i = 0; i < x.Length; i++)
            {
                if (i == 0)
                {
                    x[i] = ConvertBitToInterval(intervalStarts[i], intervalEnds[i], new BitArray(individual.Cast<bool>().Take(maxLengthArr[i]).ToArray()));
                }
                else
                {
                    x[i] = ConvertBitToInterval(intervalStarts[i], intervalEnds[i], new BitArray(individual.Cast<bool>().Skip(maxLengthArr[i - 1]).Take(maxLengthArr[i]).ToArray()));
                }
            }
            return x;
        }

    }
}
