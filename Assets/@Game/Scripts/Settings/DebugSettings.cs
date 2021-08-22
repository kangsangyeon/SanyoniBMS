using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SanyoniBMS
{

    public class DebugSettings : SanyoniLib.UnityEngineHelper.SingletonMonoBehaviour<DebugSettings>
    {
        [SerializeField] private bool _DebugMode = false;
        public static bool DebugMode
        {
            get { return Instance._DebugMode; }
            set { Instance._DebugMode = value; }
        }

        private void OnEnable()
        {
            base.EnableDontDestroy = true;
        }

        private void Update()
        {
            // `키로 디버그모드 on/off 제어
            if (Input.GetKeyDown(KeyCode.BackQuote)) DebugMode = !DebugMode;

        }

    }

}