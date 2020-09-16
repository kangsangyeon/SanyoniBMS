using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.Zone
{
    public class ScrollGamepadManager : MonoBehaviour
    {
        [Header("SLIDER")]
        public Scrollbar scrollbarObject;
        public float changeValue = 0.05f;

        [Header("INPUT")]
        public string verticalAxis = "Xbox Right Stick Vertical";

        void Update()
        {
            float h = Input.GetAxis(verticalAxis);

            if (h == 1)
            {
                scrollbarObject.value -= changeValue;
            }

            else if (h == -1)
            {
                scrollbarObject.value += changeValue;
            }
        }
    }
}