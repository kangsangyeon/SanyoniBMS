using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SanyoniLib.UnityEngineExtensions;
using SanyoniLib.UnityEngineHelper;

namespace SanyoniBMS
{

    public class BMSPlayerLauncher : SingletonMonoBehaviour<BMSPlayerLauncher>
    {
        public static BMSData BMSData { get; set; }
        public static int PatternIndex { get; set; }
        public static Sprite ThumbnailSprite { get; set; }

        [SerializeField] private GameObject m_BMSPlayerPrefab;

        protected override void Awake()
        {
            base.EnableDontDestroy = false;
            base.enableDestroyGameobject = true;
            base.enableLogErrorWhenDuplicated = true;
            base.Awake();
        }

        public void Launch()
        {

            if (BMSPlayerLauncher.BMSData != null)
            {
                if (BMSPlayer.Instance == null)
                {
                    GameObject playerGO = Instantiate(this.m_BMSPlayerPrefab);
                    playerGO.transform.parent = this.transform;
                }

                if (MusicScrollView.SelectedItem.m_ThumbnailTexture == null)
                    BMSPlayerLauncher.ThumbnailSprite = null;
                else
                    BMSPlayerLauncher.ThumbnailSprite = ResourcesHelper.CreateSpriteWithTexture2D(MusicScrollView.SelectedItem.m_ThumbnailTexture);


                BMSPatternData pattern = BMSPlayerLauncher.BMSData.BMSPatternDatas[BMSPlayerLauncher.PatternIndex];
                string patternFilePath = System.IO.Path.Combine(BMSPlayerLauncher.BMSData.Directory, pattern.FileName);
                BMSParser.ParseBMSPatternData(patternFilePath, out pattern, true, true, true);

                BMSPlayer.Instance.Initialize();
                StartCoroutine(BMSPlayer.Instance.CPrepare(BMSData.Directory, pattern));

                if (DebugSettings.DebugMode) Debug.Log("Launch BMSPlayer.");
            }
            else
            {
                if (DebugSettings.DebugMode) Debug.Log("Could not launch BMSPlayer. BMSData is empty.");
            }
        }

    }

}