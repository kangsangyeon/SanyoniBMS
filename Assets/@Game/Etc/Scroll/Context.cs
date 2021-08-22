using System;

namespace SanyoniBMS
{

    public class Context
    {
        public MusicScrollView MusicScrollViewInstance;
        public bool Prepared = false;
        public int ItemsCount;
        public int SelectedIndex = -1;
        public Action<int> OnCellClicked;
    }

}