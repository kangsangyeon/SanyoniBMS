using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Michsky.UI.Zone
{
    public class MainPanelManager : MonoBehaviour
    {
        [Header("PANEL LIST")]
        public List<GameObject> panels = new List<GameObject>();

        [Header("BUTTON LIST")]
        public List<GameObject> buttons = new List<GameObject>();

        private GameObject currentPanel;
        private GameObject nextPanel;

        private GameObject currentButton;
        private GameObject nextButton;

        [Header("TITLE")]
        public bool enableTitleAnim;
        public GameObject titleObject;
        public List<string> titles = new List<string>();
        private Animator titleAnimator;
        private TextMeshProUGUI title;
        private TextMeshProUGUI titleHelper;

        [Header("SETTINGS")]
        public int currentPanelIndex = 0;
        private int currentButtonlIndex = 0;

        private Animator currentPanelAnimator;
        private Animator nextPanelAnimator;

        private Animator currentButtonAnimator;
        private Animator nextButtonAnimator;

        string panelFadeIn = "Panel In";
        string panelFadeOut = "Panel Out";
        string buttonFadeIn = "Hover to Pressed";
        string buttonFadeOut = "Pressed to Normal";

        void Start()
        {
            currentButton = buttons[currentPanelIndex];
            currentButtonAnimator = currentButton.GetComponent<Animator>();
            currentButtonAnimator.Play(buttonFadeIn);

            currentPanel = panels[currentPanelIndex];
            currentPanelAnimator = currentPanel.GetComponent<Animator>();
            currentPanelAnimator.Play(panelFadeIn);

            if(enableTitleAnim == true)
            {
                titleAnimator = titleObject.GetComponent<Animator>();
                title = titleObject.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                titleHelper = titleObject.transform.Find("Helper").GetComponent<TextMeshProUGUI>();
            }
        }

        public void OpenFirstTab()
        {
            currentPanel = panels[currentPanelIndex];
            currentPanelAnimator = currentPanel.GetComponent<Animator>();
            currentPanelAnimator.Play(panelFadeIn);

            currentButton = buttons[currentPanelIndex];
            currentButtonAnimator = currentButton.GetComponent<Animator>();
            currentButtonAnimator.Play(buttonFadeIn);

            if (enableTitleAnim == true)
            {
                title.text = titles[currentPanelIndex];
            }
        }

        public void PanelAnim(int newPanel)
        {
            if (newPanel != currentPanelIndex)
            {
                currentPanel = panels[currentPanelIndex];

                if (enableTitleAnim == true)
                {
                    titleHelper.text = titles[currentPanelIndex];
                    titleAnimator.Play("Switch");
                }

                currentPanelIndex = newPanel;
                nextPanel = panels[currentPanelIndex];

                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                nextPanelAnimator = nextPanel.GetComponent<Animator>();

                currentPanelAnimator.Play(panelFadeOut);
                nextPanelAnimator.Play(panelFadeIn);

                if (enableTitleAnim == true)
                {
                    title.text = titles[currentPanelIndex];
                }

                currentButton = buttons[currentButtonlIndex];

                currentButtonlIndex = newPanel;
                nextButton = buttons[currentButtonlIndex];

                currentButtonAnimator = currentButton.GetComponent<Animator>();
                nextButtonAnimator = nextButton.GetComponent<Animator>();

                currentButtonAnimator.Play(buttonFadeOut);
                nextButtonAnimator.Play(buttonFadeIn);
            }
        }

        public void NextPage()
        {
            if (currentPanelIndex <= panels.Count - 2)
            {
                if (enableTitleAnim == true)
                {
                    titleHelper.text = titles[currentPanelIndex];
                    titleAnimator.Play("Switch");
                }

                currentPanel = panels[currentPanelIndex];
                currentButton = buttons[currentButtonlIndex];
                nextButton = buttons[currentButtonlIndex + 1];

                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                currentButtonAnimator = currentButton.GetComponent<Animator>();

                currentButtonAnimator.Play(buttonFadeOut);
                currentPanelAnimator.Play(panelFadeOut);

                currentPanelIndex += 1;
                currentButtonlIndex += 1;
                nextPanel = panels[currentPanelIndex];

                nextPanelAnimator = nextPanel.GetComponent<Animator>();
                nextButtonAnimator = nextButton.GetComponent<Animator>();
                nextPanelAnimator.Play(panelFadeIn);
                nextButtonAnimator.Play(buttonFadeIn);

                if (enableTitleAnim == true)
                {
                    title.text = titles[currentPanelIndex];
                }
            }
        }

        public void PrevPage()
        {
            if (currentPanelIndex >= 1)
            {
                if (enableTitleAnim == true)
                {
                    titleHelper.text = titles[currentPanelIndex];
                    titleAnimator.Play("Switch");
                }

                currentPanel = panels[currentPanelIndex];
                currentButton = buttons[currentButtonlIndex];
                nextButton = buttons[currentButtonlIndex - 1];

                currentPanelAnimator = currentPanel.GetComponent<Animator>();
                currentButtonAnimator = currentButton.GetComponent<Animator>();

                currentButtonAnimator.Play(buttonFadeOut);
                currentPanelAnimator.Play(panelFadeOut);

                currentPanelIndex -= 1;
                currentButtonlIndex -= 1;
                nextPanel = panels[currentPanelIndex];

                nextPanelAnimator = nextPanel.GetComponent<Animator>();
                nextButtonAnimator = nextButton.GetComponent<Animator>();
                nextPanelAnimator.Play(panelFadeIn);
                nextButtonAnimator.Play(buttonFadeIn);

                if (enableTitleAnim == true)
                {
                    title.text = titles[currentPanelIndex];
                }
            }
        }
    }
}