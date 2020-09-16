using UnityEngine;


namespace SanyoniLib.SystemExtensions
{

    public static class NumberHelper
    {

        public static bool InRange(this int origin, int numA, int numB)
        {
            int min = numA < numB ? numA : numB;
            int max = numA == min ? numB : numA;

            return min <= origin && origin <= max;
        }

    }

}