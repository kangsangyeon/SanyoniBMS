using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanyoniBMS
{

    public static class BMSDataPool
    {

        public static Dictionary<int, BMSPatternData> PatternPool = new Dictionary<int, BMSPatternData>();
        public static Dictionary<int, BMSData> DataPool = new Dictionary<int, BMSData>();

        private static Queue<int> PatternAddedQueue = new Queue<int>();
        private static Queue<int> DataAddedQueue = new Queue<int>();

    }

}