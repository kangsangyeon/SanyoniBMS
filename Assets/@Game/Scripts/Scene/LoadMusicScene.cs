using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using SanyoniLib.UnityEngineHelper;
using SanyoniLib.UnityEngineExtensions;

namespace SanyoniBMS
{

    public class LoadMusicScene : MonoBehaviour
    {
        [SerializeField] private Doozy.Engine.UI.UIView m_LoadingView;
        [SerializeField] private TextMeshProUGUI m_TitleText;
        [SerializeField] private TextMeshProUGUI m_ArtistText;
        [SerializeField] private TextMeshProUGUI m_LoadingText;
        [SerializeField] private RectTransform m_LoadingBar;
        [SerializeField] private Image m_Thumbnail;

        private IEnumerator Start()
        {
            // UI 초기설정
            m_LoadingBar.SetRight(VideoSettings.TargetResolution.Width);

            // MusicScrollView의 SelectedMusicCellData를 읽어와서 미리 보여준다.
            //TODO: Launcher에서 읽어오도록 바꾸어야하나?
            if (MusicScrollView.SelectedItem.m_ThumbnailTexture == null)
                this.m_Thumbnail.sprite = null;
            else
                this.m_Thumbnail.sprite = ResourcesHelper.CreateSpriteWithTexture2D(MusicScrollView.SelectedItem.m_ThumbnailTexture);

            this.m_TitleText.text = MusicScrollView.SelectedItem.Title;
            m_ArtistText.text = MusicScrollView.SelectedItem.Artist;


            // PlayGameScene 로드
            var operation = SceneManager.LoadSceneAsync(Global.PlayGameSceneText, LoadSceneMode.Additive);
            operation.completed += _ =>
            {
                BMSPlayerLauncher.Instance?.Launch();
            };


            // BMSPlayer의 준비가 끝나면 로딩 뷰를 숨긴다.
            yield return StartCoroutine(UpdateUI());
            this.m_LoadingView.Hide();
        }

        private IEnumerator UpdateUI()
        {
            while (true)
            {
                if (BMSPlayer.Instance != null && BMSPlayer.Instance.m_BMSPatternData != null
                && MusicScrollView.SelectedItem != null)
                {
                    m_TitleText.text = MusicScrollView.SelectedItem.Title;
                    m_ArtistText.text = MusicScrollView.SelectedItem.Artist;
                    m_LoadingText.text = string.Format("LOADING {0:0.00}%", BMSPlayer.Instance.LoadingPercentage * 100);

                    float value = Mathf.Lerp(VideoSettings.TargetResolution.Width, 0, (float)BMSPlayer.Instance.LoadingPercentage);
                    m_LoadingBar.SetRight(value);

                }

                if (BMSPlayer.Instance != null && BMSPlayer.Instance.m_IsPrepared == true) yield break;

                yield return null;
            }

        }

    }

}