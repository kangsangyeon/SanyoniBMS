using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Michsky.UI.Zone
{
    public class HorizontalSelector : MonoBehaviour
    {
        [Header("KEYS")]
        public KeyCode previousKey;
        public KeyCode forwardKey;

        [Header("GAMEPAD")]
        public bool useGamepadButtons = true;
        public KeyCode previousButton;
        public KeyCode forwardButton;

        [Header("SETTINGS")]
        private int index = 0;
        public int defaultIndex = 0;
        public bool invokeEventAtStart = false;
        public bool disableAtStart = true;

        [Header("ELEMENTS")]
        public List<string> elements = new List<string>();

        [Header("EVENT")]
        public UnityEvent onValueChanged;

        private TextMeshProUGUI label;
        private TextMeshProUGUI labeHelper;
        private Animator selectorAnimator;

        void Awake()
        {
            selectorAnimator = gameObject.GetComponent<Animator>();
            label = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            labeHelper = transform.Find("Text Helper").GetComponent<TextMeshProUGUI>();
            label.text = elements[defaultIndex];
            labeHelper.text = label.text;

            if(invokeEventAtStart == true)
            {
                onValueChanged.Invoke();
            }

            if(disableAtStart == true)
            {
                this.enabled = false;
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(previousKey))
            {
                PreviousClick();
            }

            if (Input.GetKeyDown(forwardKey))
            {
                ForwardClick();
            }

            if (useGamepadButtons == true)
            {
                if (Input.GetKeyDown(previousButton))
                {
                    PreviousClick();
                }

                else if (Input.GetKeyDown(forwardButton))
                {
                    ForwardClick();
                }
            }
        }

        public void PreviousClick()
        {
            labeHelper.text = label.text;

            if (index == 0)
            {
                index = elements.Count - 1;
            }

            else
            {
                index--;
            }

            onValueChanged.Invoke();
            label.text = elements[index];

            selectorAnimator.Play(null);
            selectorAnimator.StopPlayback();
            selectorAnimator.Play("Previous");
        }

        public void ForwardClick()
        {
            labeHelper.text = label.text;

            if ((index + 1) >= elements.Count)
            {
                index = 0;
            }

            else
            {
                index++;
            }

            onValueChanged.Invoke();
            label.text = elements[index];

            selectorAnimator.Play(null);
            selectorAnimator.StopPlayback();
            selectorAnimator.Play("Forward");
        }
    }
}