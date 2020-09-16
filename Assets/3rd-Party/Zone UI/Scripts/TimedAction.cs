using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Michsky.UI.Zone
{
    public class TimedAction : MonoBehaviour
    {
        [Header("TIMING (SECONDS)")]
        public float timer = 4;

        [Header("TIMER EVENT")]
        public UnityEvent timerAction;

        IEnumerator TimedEvent()
        {
            yield return new WaitForSeconds(timer);
            timerAction.Invoke();
        }

        public void StartIEnumerator ()
        {
            StartCoroutine("TimedEvent");
        }

        public void StopIEnumerator ()
        {
            StopCoroutine("TimedEvent");
        }
    }
}
