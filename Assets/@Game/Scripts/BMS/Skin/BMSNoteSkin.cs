using UnityEngine;
using Sirenix.OdinInspector;

namespace SanyoniBMS
{

    [CreateAssetMenu(menuName = "SanyoniBMS/Skin/Note")]
    public class BMSNoteSkin : SerializedScriptableObject
    {
        [AssetsOnly] public GameObject NotePrefab;
        [AssetsOnly] public GameObject LNHeadPrefab;
        [AssetsOnly] public GameObject LNBodyPrefab;
    }

}