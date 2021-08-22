using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SanyoniBMS
{

    public class ResultScene : SanyoniLib.UnityEngineHelper.SingletonMonoBehaviour<ResultScene>
    {
        public static BMSPlayResult PlayResult { get; set; }

        public TextMeshProUGUI m_TitleText;
        public TextMeshProUGUI m_PatternText;
        public Image m_Thumbnail;

        public TextMeshProUGUI m_ScoreText;
        public TextMeshProUGUI m_PGreatCountText;
        public TextMeshProUGUI m_GreatCountText;
        public TextMeshProUGUI m_GoodCountText;
        public TextMeshProUGUI m_BadCountText;
        public TextMeshProUGUI m_PoorCountText;
        public TextMeshProUGUI m_RankText;

        public TextMeshProUGUI m_AutoPlayText;

        private void Awake()
        {
            base.EnableDontDestroy = false;
            base.enableDestroyGameobject = false;
            base.Awake();
        }

        private void Update()
        {
            if (this.m_TitleText != null) this.m_TitleText.text = BMSPlayerLauncher.BMSData.Title;
            if (this.m_PatternText != null && BMSPlayerLauncher.BMSData != null) this.m_PatternText.text = BMSPlayerLauncher.BMSData.BMSPatternDatas[BMSPlayerLauncher.PatternIndex].PatternTitle;
            if (this.m_Thumbnail != null) this.m_Thumbnail.sprite = BMSPlayerLauncher.ThumbnailSprite;

            //  플레이 기록 텍스트
            if (this.m_ScoreText != null) this.m_ScoreText.text = PlayResult.CurrentScore.ToString();
            if (this.m_PGreatCountText != null) this.m_PGreatCountText.text = PlayResult.PgreatCount.ToString();
            if (this.m_GreatCountText != null) this.m_GreatCountText.text = PlayResult.GreatCount.ToString();
            if (this.m_GoodCountText != null) this.m_GoodCountText.text = PlayResult.GoodCount.ToString();
            if (this.m_BadCountText != null) this.m_BadCountText.text = PlayResult.BadCount.ToString();
            if (this.m_PoorCountText != null) this.m_PoorCountText.text = PlayResult.PoorCount.ToString();
            //if(this.m_RankText != null) 

            if (this.m_AutoPlayText != null) this.m_AutoPlayText.gameObject.SetActive(PlayResult.IsAutoPlay);
        }

        public void OnClickHome()
        {
            Doozy.Engine.GameEventMessage.SendEvent(Global.StartSelectMusicSceneEventText);
        }

    }

}