using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanyoniBMS
{

    public class BMSParserTest : MonoBehaviour
    {

        public static string BMSRootPath = @"D:\BMSFiles\";

        // Use this for initialization
        void Start()
        {
            BMSData[] datas;
            BMSParser.ParseAllBMSBelowRootDirectory(BMSRootPath, out datas);

            Debug.Log("");
        }

    }

}