using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.Zone
{
    public class ScrollForMore : MonoBehaviour
    {
        [Header("RESOURCES")]
        public Scrollbar listScrollbar;
        private Animator SFMAnimator;

        [Header("SETTINGS")]
        public float fadeOutValue;

        void Start()
        {
            SFMAnimator = gameObject.GetComponent<Animator>();

            if (listScrollbar.value >= fadeOutValue)
            {
                SFMAnimator.Play("SFM In");
            }

            else
            {
                SFMAnimator.Play("SFM Out");
            }
        }

        public void CheckValue()
        {
            if (SFMAnimator != null && listScrollbar.value >= fadeOutValue)
            {
                SFMAnimator.Play("SFM In");
            }

            else if (SFMAnimator != null && listScrollbar.value <= fadeOutValue)
            {
                SFMAnimator.Play("SFM Out");
            }
        }
    }
}
