using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Doozy.Engine;
using Doozy.Engine.Events;
using UniRx;
using Sirenix.OdinInspector;


namespace SanyoniBMS
{

    [ShowOdinSerializedPropertiesInInspector]
    public class PlayGameScene : SanyoniLib.UnityEngineHelper.SingletonMonoBehaviour<PlayGameScene>
    {
        private const int SceneEndDelay = 6;


        public BMSPlayer m_Player;
        public Image m_BgaImage;
        public RawImage m_BgaRawImage;
        public GameObject m_AutoPlayView;

        protected override void Awake()
        {
            base.EnableDontDestroy = false;
            base.enableLogErrorWhenDuplicated = true;
            base.Awake();
        }

        private IEnumerator Start()
        {
            this.m_AutoPlayView.SetActive(false);

            yield return new WaitUntil(() => (m_Player = BMSPlayer.Instance) != null);
            yield return new WaitUntil(() => m_Player.m_IsPrepared == true && m_Player.m_IsPlaying == false && this.m_Player.m_IsPaused == false);
            m_Player.Play();

            yield return new WaitUntil(() => m_Player.IsBMSPlayFinished == true);
            Debug.LogFormat("Result: {0}", m_Player.m_PlayResult.ToString());
            this.m_BgaImage.color = Color.black;

            yield return new WaitForSeconds(PlayGameScene.SceneEndDelay);
            ResultScene.PlayResult = m_Player.m_PlayResult;
            Doozy.Engine.GameEventMessage.SendEvent(Global.StartResultSceneEventText);
        }

        private void Update()
        {
            UpdateMovespeed();

            UpdateToggleAutoPlay();

            UpdatePlayerInput();
        }

        private void UpdatePlayerInput()
        {
            if (BMSPlayer.Instance != null && BMSPlayer.Instance.m_IsPlaying == true && BMSPlayer.Instance.m_IsPaused == false
                && BMSPlayer.Instance.m_IsAutoPlay == false)
            {
                KeyMode keyModeType = this.m_Player.m_BMSPatternData.KeyType;
                var currentKeySettings = Global.KeySettingsDict[keyModeType];

                // 현재 패턴의 키 모드에 해당되는 키에 대해서만 입력을 업데이트한다.
                foreach (var key in currentKeySettings.GetKeys())
                    this.m_Player.Input(currentKeySettings.GetLaneTypeByKeyCode(key), Input.GetKey(key));

            }

        }

        private void UpdateMovespeed()
        {
            if (BMSPlayer.Instance != null)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) BMSPlayer.Instance.CurrentMovespeedMultiplier -= .25;
                else if (Input.GetKeyDown(KeyCode.Alpha2)) BMSPlayer.Instance.CurrentMovespeedMultiplier += .25;
            }
        }

        private void UpdateToggleAutoPlay()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                this.m_Player.m_IsAutoPlay = !this.m_Player.m_IsAutoPlay;
                if (this.m_AutoPlayView != null) this.m_AutoPlayView.SetActive(this.m_Player.m_IsAutoPlay);
            }
            if (Input.GetKeyDown((KeyCode.F2))) this.m_Player.m_IsAutoScratch = !this.m_Player.m_IsAutoScratch;

        }

        //TODO: Editor에서 Pause될 때 이게 안먹힌다.. editor에서 멈출 때도 이게 호출되게끔 해야한다.
        private void OnApplicationFocus(bool focus)
        {
            //Debug.Log("OnApplicationFocus: " + focus);

            this.m_Player.SetPause(!focus);
        }

        private void OnApplicationPause(bool pause)
        {
            Debug.Log("OnApplicationPause: " + pause);

        }

    }

}