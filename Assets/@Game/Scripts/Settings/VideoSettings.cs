using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanyoniBMS
{

    public class VideoSettings
    {

        public static readonly Resolution[] SelectableResolutions =
        {
        new Resolution("1024x576", new Vector2(1024, 576)),
        new Resolution("1280x720 (HD)", new Vector2(1280, 720)),
        new Resolution("1366x768", new Vector2(1366, 768)),
        new Resolution("1600x900 (HD+)", new Vector2(1600, 900)),
        new Resolution("1920x1080 (FHD)", new Vector2(1920, 1080))
        };
        public static readonly Resolution OriginalResolution = SelectableResolutions[0];

        public static Resolution TargetResolution { get { return Global.VideoSettingsData.Resolution; } }
        public static bool Fullscreen { get { return Global.VideoSettingsData.Fullscreen; } }
        public static int TargetFps { get { return Global.VideoSettingsData.TargetFps; } }
        public static bool VSync { get { return Global.VideoSettingsData.VSync; } }
        public static int AntiAliasing { get { return Global.VideoSettingsData.AntiAliasing; } }

        public static void ApplySettings()
        {
            Debug.Log("VideoSettings.ApplySettings(): 비디오 설정을 적용합니다.");

            Screen.SetResolution(TargetResolution.Width, TargetResolution.Height, Fullscreen);
            Application.targetFrameRate = VideoSettings.TargetFps;
            QualitySettings.vSyncCount = VSync == false ? 0 : 1;
            QualitySettings.antiAliasing = AntiAliasing;

        }

    }

}