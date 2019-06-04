using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Numerics;

namespace Genetic
{
    class Individual
    {
        public BitArray binaryArray;
        public int age;
        public bool crossovered;
        public bool mutated;

        public Individual(BitArray binaryArray, int age=0)
        {
            this.binaryArray = new BitArray(binaryArray);
            this.age = age;
            this.crossovered = false;
            this.mutated = false;
        }

        public Individual(Individual ind)
        {
            this.binaryArray = new BitArray(ind.binaryArray);
            this.age = ind.age;
            this.crossovered = ind.crossovered;
            this.mutated = ind.mutated;
        }

    }
}
