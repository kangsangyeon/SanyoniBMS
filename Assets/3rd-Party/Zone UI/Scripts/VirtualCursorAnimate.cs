using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Michsky.UI.Zone
{
    public class VirtualCursorAnimate : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("RESOURCES")]
        public VirtualCursor virtualCursor;

        void Start()
        {
            if (virtualCursor == null)
            {
                Debug.Log("Looking for Virtual Cursor automatically.");
                virtualCursor = GameObject.Find("Virtual Cursor").GetComponent<VirtualCursor>();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            virtualCursor.AnimateCursorIn();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            virtualCursor.AnimateCursorOut();
        }
    }
}