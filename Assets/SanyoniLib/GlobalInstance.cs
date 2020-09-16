using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanyoniLib
{

    public class GlobalInstance : SanyoniLib.UnityEngineHelper.SingletonMonoBehaviour<GlobalInstance>
    {

        protected override void Awake()
        {
            base.EnableDontDestroy = true;
            base.enableLogErrorWhenDuplicated = true;
            base.Awake();
        }

    }

}