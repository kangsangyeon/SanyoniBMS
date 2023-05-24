using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using UniRx;
using DG.Tweening;
using Michsky.MUIP;
using SanyoniLib.UnityEngineWrapper;

namespace SanyoniBMS
{

    [SerializeField]
    [ShowOdinSerializedPropertiesInInspector]
    public class SelectMusicScene : SanyoniLib.UnityEngineHelper.SingletonMonoBehaviour<SelectMusicScene>
    {
        private readonly Color COLOR_EASY = new Color32(141, 255, 0, 255);  // 초록
        private readonly Color COLOR_NORMAL = new Color32(255, 155, 0, 255);    // 주황
        private readonly Color COLOR_HARD = new Color32(240, 0, 0, 255);    // 빨강
        private readonly Color COLOR_SUPERHARD = new Color32(255, 0, 57, 255);    // 빨강

        public string m_CurrentDirectory;
        public List<BMSData> BMSDataListInCurrentDirectory { get { return Global.BMSDataListByDictionary[this.m_CurrentDirectory]; } }

        private int m_SelectedMusicIndex = -1;      // 씬 진입 최초에 한해 OnUpdateSelectedMusicIndex()를 호출했을 때 UI가 제대로 갱신될 수 있도록  초기값을 -1로 설정한다.
        private int m_SelectedPatternIndex = 0;
        private bool m_SelectMusic;

        // 배경
        [SerializeField] private VideoManager m_VideoManager;
        [SerializeField] private Image m_BackgroundImage;
        [SerializeField] private CanvasGroup m_BackgroundRawImageGroup;
        [SerializeField] private CanvasGroup m_BackgroundImageGroup;

        // 왼쪽 UI
        [SerializeField] private Image m_ThumbnailImage;
        [SerializeField] private TextMeshProUGUI m_TitleText;
        [SerializeField] private TextMeshProUGUI m_ArtistText;
        [SerializeField] private HorizontalSelector m_PatternSelector;
        [SerializeField] private TextMeshProUGUI m_PlayLevelText;
        [SerializeField] private Image m_PlayLevelColorImage;

        // 오른쪽 UI
        [SerializeField] private MusicScrollView m_MusicScrollView;

        // 모달 UI
        [SerializeField] private Michsky.MUIP.ModalWindowManager m_MW_QuitToMainMenu;

        private void Awake()
        {
            base.EnableDontDestroy = false;
            base.enableDestroyGameobject = false;
            base.Awake();
        }

        private void Start()
        {
            TryFindComponents();

            Initialize();
            Prepare();
        }

        private void TryFindComponents()
        {
            if (m_MusicScrollView == null)
            {
                MusicScrollView[] managers = FindObjectsOfType<MusicScrollView>();
                if (managers.Length == 1) m_MusicScrollView = managers[0];
                else Debug.LogFormat("여러개의 MusicScrollViewManager가 씬에 로드되어 있습니다.");
            }

        }

        private void Initialize()
        {
            // 처음 보여줄 bms리스트는 Global.BMSRootDirectories배열의 가장 첫 번째 디렉토리로 설정한다.
            this.m_CurrentDirectory = Global.BMSRootDirectories[0];

            this.m_VideoManager.Initialize();
        }

        public void Prepare()
        {
            this.m_MusicScrollView.Prepare(Global.BMSDataListByDictionary[m_CurrentDirectory]);

            // 선택 음악 변경 이벤트
            this.ObserveEveryValueChanged(x => MusicScrollView.SelectedItemIndex).Subscribe(x => OnUpdateSelectedMusicIndex(x, 0));

            // 패턴 셀렉터 이벤트
            this.m_PatternSelector.onValueChanged.AddListener(x => OnUpdateSelectedMusicIndex(this.m_SelectedMusicIndex, x));

            // 모달 UI 이벤트
            this.m_MW_QuitToMainMenu.onConfirm.AddListener(() =>
            {
                Debug.Log("OnClickQuitToMainMenu");
            });

            OnUpdateSelectedMusicIndex(0, 0);
        }

        public IEnumerator ShowBackgroundWithVideo(string url)
        {
            this.m_BackgroundImageGroup.DOFade(0f, 1f);
            this.m_BackgroundRawImageGroup.DOFade(1f, 1f);

            // TODO
            // this.m_VideoManager.PauseVideo();
            yield return this.m_VideoManager.CPrepareVideoUrl(url);
            this.m_VideoManager.PlayVideo(true);
        }

        public void ShowBackgroundWithSprite(Sprite sprite)
        {
            this.m_BackgroundRawImageGroup.DOFade(0f, 1f);
            this.m_BackgroundImageGroup.DOFade(1f, 1f);

            // TODO
            // this.m_VideoManager.PauseVideo();

            this.m_BackgroundImage.sprite = sprite;
        }

        private void Update()
        {
            if (this.m_SelectMusic == false)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    this.m_SelectMusic = true;
                    SelectMusic();
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow)) this.m_PatternSelector.ForwardClick();
                else if (Input.GetKeyDown(KeyCode.LeftArrow)) this.m_PatternSelector.PreviousClick();
                if (this.m_MusicScrollView.m_IsPrepared == true)
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow)) this.m_MusicScrollView.SelectPrevCell();
                    else if (Input.GetKeyDown(KeyCode.DownArrow)) this.m_MusicScrollView.SelectNextCell();
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape) == true)
            {
                this.m_MW_QuitToMainMenu.AnimateWindow();
            }


        }

        private void SelectMusic()
        {
            BMSData selectedBms = BMSDataListInCurrentDirectory[MusicScrollView.SelectedItemIndex];

            BMSPlayerLauncher.BMSData = selectedBms;
            BMSPlayerLauncher.PatternIndex = this.m_SelectedPatternIndex;

            // 애니메이션 재생
            Doozy.Engine.GameEventMessage.SendEvent(Global.StartLoadMusicSceneEventText);

            if (DebugSettings.DebugMode)
            {
                Debug.LogFormat("SelectMusicScene.SelectMusic(): Selected BMS: {0}", selectedBms.ToString());
                Debug.LogFormat("\t\tSelected Pattern: {0}", selectedBms.BMSPatternDatas[this.m_SelectedPatternIndex].ToString());
                Debug.LogFormat("\t\tPattern Index: {0}", m_SelectedPatternIndex);
            }
        }

        private void OnUpdateSelectedMusicIndex(int musicIndex, int patternIndex)
        {
            MusicCellData musicData = MusicScrollView.SelectedItem;

            if (this.m_SelectedMusicIndex != musicIndex)
            {
                this.m_SelectedMusicIndex = musicIndex;

                musicData = MusicScrollView.SelectedItem;
                if (musicData != null)
                {
                    this.m_TitleText.text = musicData.Title;
                    this.m_ArtistText.text = musicData.Artist;

                    this.m_PatternSelector.items.Clear();
                    foreach (var item in musicData.m_BMSData.BMSPatternDatas) this.m_PatternSelector.CreateNewItem(item.PatternTitle);
                    this.m_PatternSelector.SetupSelector();


                    if (musicData.m_ThumbnailTexture != null) this.m_ThumbnailImage.sprite = SanyoniLib.UnityEngineHelper.ResourcesHelper.CreateSpriteWithTexture2D(musicData.m_ThumbnailTexture);
                    else this.m_ThumbnailImage.sprite = null;


                    // bga가 동영상으로 제공되면 동영상을 재생해 배경을 채우고, 그렇지 않다면 썸네일 이미지를 크게 띄운다.
                    //if(musicData.m_BMSData.BMSPatternDatas[patternIndex].Header.BmpDict)
                    //else
                    //TODO: 동영상이 있을 경우 동영상을 재생하고, 그렇지 않으면 이미지를 출력하는 코드를 작성해야 한다.
                    ShowBackgroundWithSprite(this.m_ThumbnailImage.sprite);
                }

            }

            this.m_SelectedPatternIndex = patternIndex;
            if (musicData != null)
            {
                int playLevel = musicData.m_BMSData.BMSPatternDatas[this.m_SelectedPatternIndex].Header.PlayLevel;

                this.m_PlayLevelText.text = playLevel.ToString();

                if (playLevel <= 6) this.m_PlayLevelColorImage.color = COLOR_EASY;
                else if (playLevel <= 9) this.m_PlayLevelColorImage.color = COLOR_NORMAL;
                else if (playLevel <= 12) this.m_PlayLevelColorImage.color = COLOR_HARD;
                else this.m_PlayLevelColorImage.color = COLOR_SUPERHARD;

            }

        }

    }

}