using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SanyoniBMS
{

    public class AutoPlayViewUI : MonoBehaviour
    {
        public Image image;
        public TextMeshProUGUI text;

        // Update is called once per frame
        void Update()
        {
            bool active = BMSPlayer.Instance != null && BMSPlayer.Instance.m_IsPlaying == true && BMSPlayer.Instance.m_IsAutoPlay == true;

            image.enabled = active;
            text.enabled = active;
        }
    }

}