using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Michsky.UI.Zone
{
    public class PointerEnterEvents : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("EVENTS")]
        public UnityEvent enterEvent;
        public UnityEvent exitEvent;

        public void OnPointerEnter(PointerEventData eventData)
        {
            enterEvent.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            exitEvent.Invoke();
        }
    }
}