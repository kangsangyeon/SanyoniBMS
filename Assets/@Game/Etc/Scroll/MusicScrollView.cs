using EasingCore;
using FancyScrollView;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace SanyoniBMS
{

    public class MusicScrollView : FancyScrollView<MusicCellData, Context>
    {
        [SerializeField] Scroller scroller = default;
        [SerializeField] GameObject cellPrefab = default;


        public bool m_IsPrepared;
        public static int SelectedItemIndex;
        public static MusicCellData SelectedItem;


        protected override GameObject CellPrefab => cellPrefab;

        private MusicCellData[] m_CellDatas;

        public void Prepare(IList<BMSData> _bmsDatas)
        {
            this.m_IsPrepared = false;
            Context.Prepared = false;
            Context.ItemsCount = _bmsDatas.Count;


            this.m_CellDatas = Enumerable.Range(0, _bmsDatas.Count)
                    .Select(i => new MusicCellData(_bmsDatas[i], null))
                    .ToArray();

            foreach (var item in this.m_CellDatas)
            {
                string thumbnailFileName;
                // 가장 먼저 "__thumbnail.xxx" 이미지 파일이 있는지 검사한다.
                if ((thumbnailFileName = SanyoniLib.SystemHelper.PathHelper.GuessRealImageFileName(item.m_BMSData.Directory, "__thumbnail.img")) != null) { }
                // "__thumbnail" 이미지 파일이 없을 시 가장 첫 번째 패턴의 stage파일을 썸네일로 쓴다.
                else if ((thumbnailFileName = SanyoniLib.SystemHelper.PathHelper.GuessRealImageFileName(item.m_BMSData.Directory, item.m_BMSData.BMSPatternDatas[0].Header.StageFile)) != null) { }
                // 없을 시 건너뛴다
                else continue;

                // 있을 시 로드하고 스크롤뷰를 새로고침한다.
                SanyoniLib.UnityEngineHelper.ResourcesHelper.LoadTexture(item.m_BMSData.Directory, thumbnailFileName, x =>
                {
                    item.m_ThumbnailTexture = x;
                    ForceRefresh(); ///
                });

            }

            UpdateData(this.m_CellDatas);

            this.m_IsPrepared = true;
            Context.Prepared = true;

            SelectCell(0);
        }

        public void SelectNextCell()
        {
            int nextIndex = (Context.SelectedIndex + 1) % ItemsSource.Count;
            SelectCell(nextIndex);
        }

        public void SelectPrevCell()
        {
            int prevIndex = (Context.SelectedIndex - 1) < 0 ? ItemsSource.Count - 1 : Context.SelectedIndex - 1;
            SelectCell(prevIndex);
        }

        public void SelectCell(int index)
        {
            if (index < 0 || index >= ItemsSource.Count || index == Context.SelectedIndex)
            {
                return;
            }

            UpdateSelection(index);
            scroller.ScrollTo(index, 0.35f, Ease.OutCubic);

            MusicScrollView.SelectedItemIndex = index;
            MusicScrollView.SelectedItem = this.m_CellDatas[index];
        }

        public void ForceRefresh()
        {
            base.Refresh();
        }

        #region Private & Protected Methods

        protected override void Initialize()
        {
            base.Initialize();

            Context.MusicScrollViewInstance = this;
            Context.OnCellClicked = SelectCell;

            scroller.OnValueChanged(UpdatePosition);
            scroller.OnSelectionChanged(UpdateSelection);
        }

        private void UpdateData(IList<MusicCellData> _cellDatas)
        {
            UpdateContents(_cellDatas);
            scroller.SetTotalCount(_cellDatas.Count);
        }

        private void UpdateSelection(int index)
        {
            if (Context.SelectedIndex == index)
            {
                return;
            }

            Context.SelectedIndex = index;
            Refresh();
        }

        #endregion

    }

}