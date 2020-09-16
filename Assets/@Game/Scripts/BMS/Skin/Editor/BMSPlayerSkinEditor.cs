using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Sirenix.OdinInspector;
using SanyoniLib.UnityEngineExtensions;

namespace SanyoniBMS
{

    public class BMSPlayerSkinEditor : Sirenix.OdinInspector.Editor.OdinEditorWindow
    {
        public bool ShowSetupOptions { get { return this.m_EdittedSkin != null; } }

        [LabelText("Editted Skin: ")] public BMSPlayerSkin m_EdittedSkin;

        [BoxGroup("-Setup Options-")] [ShowIf("ShowSetupOptions", true)] public string m_LaneBackgroundFormatText = "LaneBackground_{0}";
        [BoxGroup("-Setup Options-")] [ShowIf("ShowSetupOptions", true)] public string m_LaneButtonFormatText = "LaneButton_{0}";
        [BoxGroup("-Setup Options-")] [ShowIf("ShowSetupOptions", true)] [AssetsOnly] public BMSNoteSkin m_ScratchNoteSkin;
        [BoxGroup("-Setup Options-")] [ShowIf("ShowSetupOptions", true)] [AssetsOnly] public BMSNoteSkin m_OddNoteSkin;
        [BoxGroup("-Setup Options-")] [ShowIf("ShowSetupOptions", true)] [AssetsOnly] public BMSNoteSkin m_EvenNoteSkin;
        [BoxGroup("-Setup Options-")] [ShowIf("ShowSetupOptions", true)] [AssetsOnly] public BMSGearLaneButtonSkin m_ScratchButtonSkin;
        [BoxGroup("-Setup Options-")] [ShowIf("ShowSetupOptions", true)] [AssetsOnly] public BMSGearLaneButtonSkin m_OddButtonSkin;
        [BoxGroup("-Setup Options-")] [ShowIf("ShowSetupOptions", true)] [AssetsOnly] public BMSGearLaneButtonSkin m_EvenButtonSkin;
        [BoxGroup("-Setup Options-/Override")] [ShowIf("ShowSetupOptions", true)] public Dictionary<LaneType, BMSNoteSkin> m_OverrideNoteSkin = new Dictionary<LaneType, BMSNoteSkin>();
        [BoxGroup("-Setup Options-/Override")] [ShowIf("ShowSetupOptions", true)] [AssetsOnly] public Dictionary<LaneType, BMSGearLaneButtonSkin> m_OverrideButtonSkin = new Dictionary<LaneType, BMSGearLaneButtonSkin>();
        [BoxGroup("-Setup Options-")] [ShowIf("ShowSetupOptions", true)] public bool m_OverwriteLanePositionX = true;


        [EnableIf("ShowSetupOptions", true)]
        [Button("Run")]
        private void RunSetup()
        {
            Debug.Log("RunSetup()");
        }



        [MenuItem("SanyoniBMS/Skin Editor")]
        private static void OpenWindow()
        {
            GetWindow<BMSPlayerSkinEditor>().Show();
        }

        public void OpenWindowWithSkin(BMSPlayerSkin skin)
        {
            OpenWindow();

            this.m_EdittedSkin = skin;
        }

        private void AttemptSetup()
        {
            if (this.m_EdittedSkin == null) return;

            LaneType[] lanes = null;
            if (m_EdittedSkin.KeySettingsType == KeyMode.SP7) lanes = new LaneType[] { LaneType.SCRATCH, LaneType.NOTE1, LaneType.NOTE2, LaneType.NOTE3, LaneType.NOTE4, LaneType.NOTE5, LaneType.NOTE6, LaneType.NOTE7 };
            else if (m_EdittedSkin.KeySettingsType == KeyMode.SP5) lanes = new LaneType[] { LaneType.SCRATCH, LaneType.NOTE1, LaneType.NOTE2, LaneType.NOTE3, LaneType.NOTE4, LaneType.NOTE5 };

            if (m_EdittedSkin.m_GearLaneDict == null) m_EdittedSkin.m_GearLaneDict = new Dictionary<LaneType, GearLane>();

            foreach (LaneType lane in lanes)
            {
                string laneName = lane.ToString();
                Transform laneBackground = m_EdittedSkin.transform.FindRecursive(string.Format(m_LaneBackgroundFormatText, laneName));
                Animator laneAnim = laneBackground.GetComponent<Animator>();
                Transform laneButton = m_EdittedSkin.transform.FindRecursive(string.Format(m_LaneButtonFormatText, laneName));
                Image laneButtonImange = laneButton.GetComponent<Image>();
                float lanePositionX = laneBackground.position.x;


                if (m_EdittedSkin.m_GearLaneDict.ContainsKey(lane) == true && m_EdittedSkin.m_GearLaneDict[lane] != null)
                {
                    m_EdittedSkin.m_GearLaneDict[lane].LaneBackgroundAnim = laneAnim;
                    m_EdittedSkin.m_GearLaneDict[lane].LaneButtonImage = laneButtonImange;
                    m_EdittedSkin.m_GearLaneDict[lane].PositionX = lanePositionX;
                }
                else
                {
                    GearLane gearLane = new GearLane();
                    gearLane.LaneBackgroundAnim = laneAnim;
                    gearLane.LaneButtonImage = laneButtonImange;
                    gearLane.PositionX = lanePositionX;

                    m_EdittedSkin.m_GearLaneDict[lane] = gearLane;
                }

            }

        }

    }

}
