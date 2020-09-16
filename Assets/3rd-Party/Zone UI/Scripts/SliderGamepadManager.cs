using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.Zone
{
    public class SliderGamepadManager : MonoBehaviour
    {
        [Header("SLIDER")]
        public Slider sliderObject;
        public float changeValue = 0.5f;

        [Header("INPUT")]
        public string horizontalAxis = "Xbox Right Stick Horizontal";

        void Update()
        {
            float h = Input.GetAxis(horizontalAxis);

            if (h == 1)
            {
                sliderObject.value += changeValue;
            }

            else if (h == -1)
            {
                sliderObject.value -= changeValue;
            }
        }
    }
}