using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SanyoniBMS
{

    public class Sample_BMSPlayerLauncher : MonoBehaviour
    {
        public string BMSPath = @"D:\BMSFiles\[COLORTRONICS2017] LoveSketchNote2017Mix_OGG";
        BMSData data;

        // Start is called before the first frame update
        void Start()
        {
            BMSParser.ParseBMSData(BMSPath, out data, true, true, true);
            BMSPlayerLauncher.BMSData = data;
            BMSPlayerLauncher.PatternIndex = this.data.BMSPatternDatas.Length - 1;

            SceneManager.LoadScene(Global.PlayGameSceneText, LoadSceneMode.Additive);

            // 자동 플레이모드
            //BMSPlayer.Instance.m_IsAutoPlay = true;
        }

    }

}