using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;


namespace SanyoniBMS
{

    [CreateAssetMenu(menuName = "SanyoniBMS/Skin/GearLaneButton")]
    public class BMSGearLaneButtonSkin : SerializedScriptableObject
    {
        [AssetsOnly] public Sprite IdleSprite;
        [AssetsOnly] public Sprite PressSprite;
    }

}