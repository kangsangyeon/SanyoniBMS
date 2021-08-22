using UnityEngine;
using UnityEngine.UI;
using FancyScrollView;
using TMPro;
using UniRx;
using UniRx.Triggers;

namespace SanyoniBMS
{

    [System.Serializable]
    public class MusicCellData
    {
        public UnityEngine.Texture2D m_ThumbnailTexture { get; set; }
        public BMSData m_BMSData { get; set; }

        public string Title { get { return this.m_BMSData.Title; } }
        public string Artist { get { return this.m_BMSData.Artist; } }

        public MusicCellData(BMSData _bmsData, UnityEngine.Texture2D _thumbnail = null)
        {
            this.m_BMSData = _bmsData;
            this.m_ThumbnailTexture = _thumbnail;
        }

    }

    public class MusicCell : FancyCell<MusicCellData, Context>
    {
        [SerializeField] private Animator m_Anim = default;
        [SerializeField] private Image m_Overlay = default;
        [SerializeField] private TextMeshProUGUI m_Title = default;
        [SerializeField] private TextMeshProUGUI m_Artist = default;
        [SerializeField] private Image m_Thumbnail = default;
        [SerializeField] private Button m_Button = default;

        private Texture2D m_ThumbnailTexture;

        static class AnimatorHash
        {
            public static readonly int Scroll = Animator.StringToHash("scroll");
        }

        void Start()
        {
            this.m_Button.onClick.AddListener(() => Context.OnCellClicked?.Invoke(Index));
        }

        public override void UpdateContent(MusicCellData itemData)
        {
            // 이미지는 텍스쳐의 데이터가 바뀔 때만 한다.
            if (itemData.m_ThumbnailTexture == null)
            {
                this.m_ThumbnailTexture = null;
                this.m_Thumbnail.sprite = null;
            }
            else if (this.m_ThumbnailTexture != itemData.m_ThumbnailTexture)
            {
                this.m_ThumbnailTexture = itemData.m_ThumbnailTexture;
                Rect textureRect = new Rect(0, 0, itemData.m_ThumbnailTexture.width, itemData.m_ThumbnailTexture.height);
                Sprite newSprite = Sprite.Create(itemData.m_ThumbnailTexture, textureRect, new Vector2(.5f, .5f));
                this.m_Thumbnail.sprite = newSprite;

                this.m_Thumbnail.color = Color.white;
                this.m_Thumbnail.enabled = true;
            }

            this.m_Title.text = itemData.Title;
            this.m_Artist.text = itemData.Artist;

            //TODO fancy scroll view에서는 리스트의 마지막 요소에서 그 다음요소, 즉 가장 맨 처음요소로 돌아올 때
            // Context의 인덱스가 바로 0이 되는것이 아니라, 리스트의 크기로 변경되는 것 같다. 
            // 즉, 인덱스가 처음으로 돌아오지 않고 그저 마지막 요소의 인덱스+1 되어버린다. 
            // 뭐 내부적으로 그렇게 해놨나보다.
            //var selected = Context.SelectedIndex == Index;
            // 따라서 위의 코드로 하게된다면 가장 마지막에서 처음으로 넘어갈 때를 감지하지 못할 때가 간헐적으로 생긴다.
            bool selected = Context.Prepared == false ? false : Context.SelectedIndex % Context.ItemsCount == Index;
            this.m_Overlay.color = selected
                ? new Color32(0, 255, 255, 100)
                : new Color32(255, 255, 255, 0);
        }

        public override void UpdatePosition(float position)
        {
            currentPosition = position;

            if (m_Anim.isActiveAndEnabled)
            {
                m_Anim.Play(AnimatorHash.Scroll, -1, position);
            }

            m_Anim.speed = 0;
        }

        // GameObject が非アクティブになると Animator がリセットされてしまうため
        // 現在位置を保持しておいて OnEnable のタイミングで現在位置を再設定します
        float currentPosition = 0;

        void OnEnable() => UpdatePosition(currentPosition);
    }

}