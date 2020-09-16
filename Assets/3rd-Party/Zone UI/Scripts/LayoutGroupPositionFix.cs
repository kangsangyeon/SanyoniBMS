using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Michsky.UI.Zone
{
    public class LayoutGroupPositionFix : MonoBehaviour
    {    
        void Start()
        {
            // BECAUSE UNITY UI IS BUGGY AND NEEDS REFRESHING :P
            StartCoroutine(ExecuteAfterTime(0.01f));
        }

        IEnumerator ExecuteAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            StopCoroutine(ExecuteAfterTime(0.01f));
            Destroy(this);
        }
    }
}