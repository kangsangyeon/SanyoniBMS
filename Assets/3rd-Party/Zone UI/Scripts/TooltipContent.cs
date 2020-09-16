using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace Michsky.UI.Zone
{
    public class TooltipContent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("CONTENT")]
        public string title;
        [TextArea] public string description;

        [Header("RESOURCES")]
        public GameObject mouseTooltipObject;
        public TextMeshProUGUI mouseTitle;
        public TextMeshProUGUI mouseDescription;
        public GameObject virtualTooltipObject;
        public TextMeshProUGUI virtualTitle;
        public TextMeshProUGUI virtualDescription;

        [Header("SETTINGS")]
        public bool enableMouse = true;
        public bool enableVirtualCursor = true;

        private Animator mouseAnimator;
        private Animator virtualAnimator;
        private BlurManager mouseBlur;
        private BlurManager virtualBlur;

        void Start()
        {
            if (enableMouse == true)
            {
                mouseAnimator = mouseTooltipObject.GetComponent<Animator>();
                mouseBlur = mouseTooltipObject.GetComponent<BlurManager>();
            }

            if (enableVirtualCursor == true)
            {
                virtualAnimator = virtualTooltipObject.GetComponent<Animator>();
                virtualBlur = virtualTooltipObject.GetComponent<BlurManager>();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (enableMouse == true && mouseTooltipObject.activeSelf)
            {
                mouseTitle.text = title;
                mouseDescription.text = description;
                mouseAnimator.Play("In");
                mouseBlur.BlurInAnim();
            }

            if (enableVirtualCursor == true && virtualTooltipObject.activeSelf)
            {
                virtualTitle.text = title;
                virtualDescription.text = description;
                virtualAnimator.StopPlayback();
                virtualAnimator.Play("In");
                virtualBlur.BlurInAnim();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (enableMouse == true && mouseTooltipObject.activeSelf)
            {
                mouseAnimator.Play("Out");
                mouseBlur.BlurOutAnim();
            }

            if (enableVirtualCursor == true && virtualTooltipObject.activeSelf)
            {
                virtualAnimator.Play("Out");
                virtualBlur.BlurOutAnim();
            }
        }
    }
}