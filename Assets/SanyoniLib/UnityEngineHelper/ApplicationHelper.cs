using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanyoniLib.UnityEngineHelper
{

    public static class ApplicationHelper
    {

        public static void ManualGarbageCollect()
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

    }

}