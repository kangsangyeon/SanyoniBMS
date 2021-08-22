using System;
using System.Collections.Generic;

namespace SanyoniLib.SystemHelper
{
    public static class ArrayHelper
    {
        public static int CalculateLoopedArrayIndex(int arrayLength, int index)
        {
            // 배열의 길이가 0이하라면 처리할 수 없다.
            if (arrayLength <= 0) return -1;

            if (index < 0) return (int)(arrayLength * Math.Ceiling((float)Math.Abs(index) / arrayLength) + index);
            else return index % arrayLength;
        }
    }
}
