using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using System;
using Sirenix.Serialization;

namespace SanyoniBMS
{
    [System.Serializable]
    public class GearLane
    {
        public float PositionX;
        public BMSNoteSkin NoteSkin;
        public BMSGearLaneButtonSkin LaneButtonSkin;
        public Animator LaneBackgroundAnim;
        public Image LaneButtonImage;
    }

    [System.Serializable]
    [ShowOdinSerializedPropertiesInInspector]
    public class BMSPlayerSkin : SerializedMonoBehaviour
    {
        private const string AnimComboTriggerName = "AnimateCombo";
        private const string PressParamName = "Press";
        private const float GuageAnimValue = .05f;
        private const float GuageAnimSpeed = 6f;

        [Header("Skin Info")]
        public string m_SkinName;
        public string m_Author;
        public string m_Version;
        [SerializeField] private KeyMode m_KeySettingsType;
        [SerializeField] private bool m_ExistScratch;

        // 콤보를 BMFont 또는 TMPro중에 하나로 선택해서 띄우도록 하자.
        [Header("Components")]
        [SerializeField] private GameObject m_JudgementLine;
        [SerializeField] private Animator m_JudgementAnim;
        [SerializeField] private Animator m_Anim;
        [SerializeField] private bool m_EnableTMProCombo = true;
        [SerializeField] private Text m_CurrentComboText;
        [SerializeField] private TextMeshProUGUI m_CurrentComboTMPro;
        [SerializeField] private bool m_EnableTMProJudgement = true;
        [SerializeField] private TextMeshProUGUI m_JudgementTMPro;
        [SerializeField] private TextMeshProUGUI m_MaxComboTMPro;
        [SerializeField] private TextMeshProUGUI m_ScoreTMPro;
        [SerializeField] private TextMeshProUGUI m_SpeedTMPro;
        [SerializeField] private Image m_HealthBar;

        [Header("Prefabs")]
        [SerializeField] public Dictionary<LaneType, GearLane> m_GearLaneDict;

        private RectTransform m_SkinCanvasRect;


        #region Properties
        public KeyMode KeySettingsType { get { return this.m_KeySettingsType; } }
        public bool ExistScratch { get { return this.m_ExistScratch; } }
        public float JudgementPositionY { get { return m_JudgementLine == null ? 0f : m_JudgementLine.transform.position.y; } }
        public Vector2 Resolution
        {
            get
            {
                if (this.m_SkinCanvasRect != null) return this.m_SkinCanvasRect.rect.size;
                else return Vector2.zero;
            }
        }
        #endregion


        #region Getters
        public GameObject GetNotePrefabByLane(LaneType lane) { return this.m_GearLaneDict.ContainsKey(lane) ? this.m_GearLaneDict[lane].NoteSkin.NotePrefab : null; }
        public GameObject GetLNPrefabByLane(LaneType lane) { return this.m_GearLaneDict.ContainsKey(lane) ? this.m_GearLaneDict[lane].NoteSkin.LNBodyPrefab : null; }
        public float GetNotePositionXByLane(LaneType lane) { return this.m_GearLaneDict.ContainsKey(lane) ? this.m_GearLaneDict[lane].PositionX : -1000; }
        #endregion


        #region Public Methods
        public void Initialize()
        {
            if (this.m_CurrentComboTMPro != null)
            {
                this.m_CurrentComboTMPro.text = string.Empty;
                this.m_CurrentComboTMPro.gameObject.SetActive(false);
            }
            else if (this.m_CurrentComboText != null)
            {
                this.m_CurrentComboText.text = string.Empty;
                this.m_CurrentComboText.gameObject.SetActive(false);
            }

            if (this.m_JudgementTMPro != null)
            {
                this.m_JudgementTMPro.text = string.Empty;
                this.m_JudgementTMPro.gameObject.SetActive(false);
            }

            this.m_SkinCanvasRect = GetComponentInChildren<Canvas>().GetComponent<RectTransform>();


            this.m_ScoreTMPro.text = "0";
            this.m_MaxComboTMPro.text = "0";

        }

        //TODO: 애니메이션 만들 것
        public void AnimateComboAndJudge(int _currentCombo, int _maxCombo, int _score, JudgementType _judgeType)
        {

            if (_currentCombo != 0)
            {
                if (this.m_CurrentComboTMPro != null && this.m_EnableTMProCombo == true)
                {
                    this.m_CurrentComboTMPro.text = _currentCombo.ToString();
                    this.m_CurrentComboTMPro.gameObject.SetActive(true);
                    if (this.m_CurrentComboText != null) this.m_CurrentComboText.gameObject.SetActive(false);
                }
                else if (this.m_CurrentComboText != null && this.m_EnableTMProCombo == false)
                {
                    this.m_CurrentComboText.text = _currentCombo.ToString();
                    this.m_CurrentComboText.gameObject.SetActive(true);
                    this.m_CurrentComboTMPro.gameObject.SetActive(false);
                }

                // 애니메이션 재생한다. 콤보 크게 딱 뜨고 서서히 흐려지거나 작아져가는 그런 ...
                if (this.m_Anim != null)
                    this.m_Anim.SetTrigger(AnimComboTriggerName);

            }
            else
            {
                // 틀렷으면 콤보 표시 즉시 가림
                if (this.m_CurrentComboText != null) this.m_CurrentComboText.gameObject.SetActive(false);
                if (this.m_CurrentComboTMPro != null) this.m_CurrentComboTMPro.gameObject.SetActive(false);
            }

            if (this.m_MaxComboTMPro != null) this.m_MaxComboTMPro.text = _maxCombo.ToString();
            if (this.m_ScoreTMPro != null) this.m_ScoreTMPro.text = _score.ToString();


            if (this.m_JudgementTMPro != null && this.m_EnableTMProJudgement == true)
            {
                this.m_JudgementTMPro.text = _judgeType.ToString();
                this.m_JudgementTMPro.gameObject.SetActive(true);
            }
            if (this.m_JudgementAnim != null) this.m_JudgementAnim.SetTrigger(_judgeType.ToString());
        }

        public void AnimateLanes(Dictionary<LaneType, KeyPressState> _keyPressStateByLane)
        {
            foreach (var item in _keyPressStateByLane)
            {
                bool press = item.Value == KeyPressState.DOWN || item.Value == KeyPressState.HOLD;

                if (this.m_GearLaneDict != null && this.m_GearLaneDict.ContainsKey(item.Key))
                {
                    this.m_GearLaneDict[item.Key].LaneBackgroundAnim.SetBool(PressParamName, press);
                    this.m_GearLaneDict[item.Key].LaneButtonImage.sprite = press ? this.m_GearLaneDict[item.Key].LaneButtonSkin.PressSprite : this.m_GearLaneDict[item.Key].LaneButtonSkin.IdleSprite;
                }

            }

        }

        public void AnimateHealthGuage(float _normalizedGuage)
        {
            _normalizedGuage = Mathf.Clamp01(_normalizedGuage);

            float currentHealthGuage;

            if (Mathf.Approximately(_normalizedGuage, 0) == true) currentHealthGuage = 0;
            else if ((Mathf.Approximately(_normalizedGuage, 1) == true)) currentHealthGuage = 1;
            else currentHealthGuage = _normalizedGuage + BMSPlayerSkin.GuageAnimValue * Mathf.Sin(Time.time * Mathf.PI * BMSPlayerSkin.GuageAnimSpeed);

            if (this.m_HealthBar != null) this.m_HealthBar.fillAmount = currentHealthGuage;
        }

        public void AnimateSpeed(float _speed)
        {
            this.m_SpeedTMPro.text = string.Format("{0:f2}", _speed);
        }

        #endregion

    }

}