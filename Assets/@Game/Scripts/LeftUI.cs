using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;

namespace SanyoniBMS
{
    public class LeftUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_ButtonText;
        [SerializeField] private Image m_Thumbnail;
        [SerializeField] private TextMeshProUGUI m_Title;

        private void Start()
        {
            this.ObserveEveryValueChanged(x => MusicScrollView.SelectedItem).Subscribe(x =>
             {
                 // TODO: 몇키 정보도 파싱해서 여기에 집어넣을 것.
                 //m_ButtonText.text = x.
                 //ResourcesHelper.LoadImageIfExist()

                 //ResourcesHelper.LoadImage(x.m_BMSData.Directory, x.m_BMSData.BMSPatternDatas[0].Header.StageFile, 
                 //    sprite => this.m_Thumbnail.sprite = sprite);
             });
        }


    }

}