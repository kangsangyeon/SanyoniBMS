using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace SanyoniBMS
{

    // Global클래스에 속해있는 여러 정적변수들을 설정하기 위한 에디터.
    public class GlobalEditor : EditorWindow
    {
        /// <summary>
        /// 에디터로 변경한 변경사항을 바로 실시간으로 게임에 적용할 것인지를 설정하는 변수.
        /// </summary>
        public bool m_ApplyChangesImmediately = true;


        //TODO: 유니티 에디터에서 어떤 메뉴를 통해서 이 함수를 불러오게끔 하는 거 까묵었다.
        public void a()
        {

        }

        private void OnGUI()
        {
            // KeySettingsDict 에디터
        }

    }

}