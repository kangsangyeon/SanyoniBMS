using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

namespace SanyoniBMS
{

    public class Sample_BMSPlayerLauncherUI : MonoBehaviour
    {
        public TextMeshProUGUI TextComponent;

        //public TextMeshProUGUI IsPlayingText;
        //public TextMeshProUGUI CurrentBPMText;
        //public TextMeshProUGUI CurrentMovespeedPerSecondText;
        //public TextMeshProUGUI CurrentMovespeedMultiplierText;

        private void Update()
        {
            if (DebugSettings.DebugMode && BMSPlayer.Instance != null && BMSPlayer.Instance.m_BMSPatternData != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Title: " + BMSPlayer.Instance.m_BMSPatternData.PatternTitle);
                sb.AppendLine("PlayLevel: " + BMSPlayer.Instance.m_BMSPatternData.Header.PlayLevel);
                sb.AppendLine();

                sb.AppendLine("IsPlaying: " + BMSPlayer.Instance.m_IsPlaying);
                sb.AppendLine("CurrentMovespeedMultiplier: " + BMSPlayer.Instance.CurrentMovespeedMultiplier);
                sb.AppendLine("CurrentMovespeedPerSecond: " + BMSPlayer.Instance.m_DestinationMovespeedPerSecond);
                sb.AppendLine("BeatDurationSecond: " + BMSPlayer.Instance.BeatDurationSecond);
                sb.AppendLine("BarDurationSecond: " + BMSPlayer.Instance.BarDurationSecond);

                if (BMSPlayer.Instance.m_IsPrepared == false && BMSPlayer.Instance.m_IsPlaying == false)
                {
                    sb.AppendLine();
                    sb.AppendLine("LoadingPercentage: " + BMSPlayer.Instance.LoadingPercentage);
                }

                if (BMSPlayer.Instance.m_IsPlaying)
                {
                    sb.AppendLine();
                    sb.AppendLine("StartPlayTimeMillis: " + BMSPlayer.Instance.StartPlayTimeMillis);
                    sb.AppendLine("ElapsedPlayingTimeSeconds: " + BMSPlayer.Instance.ElapsedPlayingTimeSeconds);
                    sb.AppendLine("CurrentBarIndex: " + BMSPlayer.Instance.m_CurrentBarIndex);
                }

                TextComponent.text = sb.ToString();
            }
            else
            {
                TextComponent.text = string.Empty;
            }

        }

    }

}