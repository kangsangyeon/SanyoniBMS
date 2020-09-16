using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Michsky.UI.Zone
{
    public class ContentSizeFitterFix : MonoBehaviour
    {
        private ContentSizeFitter ctf;

        void Start()
        {
            ctf = gameObject.GetComponent<ContentSizeFitter>();

            // BECAUSE UNITY UI IS BUGGY AND NEEDS REFRESHING :P
            StartCoroutine(ExecuteAfterTime(0.01f));
        }

        IEnumerator ExecuteAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            ctf.enabled = false;
            ctf.enabled = true;
            StopCoroutine(ExecuteAfterTime(0.01f));
            Destroy(this);
        }
    }
}