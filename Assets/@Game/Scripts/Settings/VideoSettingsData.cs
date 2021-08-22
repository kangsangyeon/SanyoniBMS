using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

namespace SanyoniBMS
{

    [System.Serializable]
    public class Resolution
    {
        public readonly string Name;
        private readonly Vector2 Value;
        public int Width { get { return (int)Value.x; } }
        public int Height { get { return (int)Value.y; } }

        public Resolution(string _name, Vector2 _value)
        {
            this.Name = _name;
            this.Value = _value;
        }

    }

    public class VideoSettingsData : ScriptableObject, ISerializable
    {
        private const int MaxTargetFps = 240;
        private const int MinTargetFps = 30;

        [SerializeField] private Resolution m_Resolution;
        [SerializeField] private bool m_Fullscreen;
        [SerializeField] private int m_TargetFps;
        [SerializeField] private bool m_VSync;
        [SerializeField] private int m_AntiAliasing;

        /***** 각 프로퍼티들은 private멤버에 대한 접근을 제공하며, 값을 바꿀 때 Set메소드를 호출해서 값을 교정하도록 한다. *****/
        /// <summary>
        /// 목표 해상도를 저정합니다.
        /// </summary>
        public Resolution Resolution { get { return m_Resolution; } set { m_Resolution = value; Set(); } }
        /// <summary>
        /// 전체화면 모드 여부입니다.
        /// </summary>
        public bool Fullscreen { get { return m_Fullscreen; } set { m_Fullscreen = value; Set(); } }
        /// <summary>
        /// 목표하는 초당 프레임 값입니다.
        /// </summary>
        public int TargetFps { get { return m_TargetFps; } set { m_TargetFps = value; Set(); } }
        /// <summary>
        /// 
        /// </summary>
        public bool VSync { get { return m_VSync; } set { m_VSync = value; Set(); } }
        /// <summary>
        /// 안티앨리어싱 AA 필터링 옵션입니다. 픽셀별 샘플 개수를 지정합니다.
        /// </summary>
        public int AntiAliasing { get { return m_AntiAliasing; } set { m_AntiAliasing = value; Set(); } }


        public VideoSettingsData()
        {
            this.m_Resolution = VideoSettings.SelectableResolutions[4];
            this.m_Fullscreen = true;
#if UNITY_EDITOR || UNITY_STANDALONE
            this.m_TargetFps = MaxTargetFps;
#else
            this.m_TargetFps = -1;  // -1은 플랫폼마다 적절한 기본값으로 초기화해준다.
#endif
            this.m_VSync = false;
            this.m_AntiAliasing = 0;

            Set();
        }

        private void Set()
        {
            m_TargetFps = this.m_TargetFps == -1 ? -1 : Mathf.Clamp(m_TargetFps, MinTargetFps, MaxTargetFps);
            m_AntiAliasing = Mathf.Clamp(m_AntiAliasing, 0, 8);
        }

#region Serialization
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("m_Resolution", m_Resolution, typeof(Resolution));
            info.AddValue("m_Fullscreen", m_Fullscreen, typeof(bool));
            info.AddValue("m_TargetFps", m_TargetFps, typeof(int));
            info.AddValue("m_VSync", m_VSync, typeof(bool));
            info.AddValue("m_AntiAliasing", m_AntiAliasing, typeof(int));
        }

        public VideoSettingsData(SerializationInfo info, StreamingContext context)
        {
            // Reset the property value using the GetValue method.
            this.m_Resolution = (Resolution)info.GetValue("m_Resolution", typeof(Resolution));
            this.m_Fullscreen = (bool)info.GetValue("m_Fullscreen", typeof(bool));
            this.m_TargetFps = (int)info.GetValue("m_TargetFps", typeof(int));
            this.m_VSync = (bool)info.GetValue("m_VSync", typeof(bool));
            this.m_AntiAliasing = (int)info.GetValue("m_AntiAliasing", typeof(int));

            Set();
        }

#endregion

    }

}