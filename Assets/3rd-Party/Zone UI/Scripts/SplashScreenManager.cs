using UnityEngine;

namespace Michsky.UI.Zone
{
    public class SplashScreenManager : MonoBehaviour
    {
        [Header("RESOURCES")]
        public GameObject splashScreen;
        public GameObject mainPanels;
        public GameObject homePanel;
        public Animator backgroundAnimator;

        private Animator splashScreenAnimator;
        private Animator mainPanelsAnimator;
        private Animator homePanelAnimator;
        private BlurManager bManager;
        private TimedAction ssTimedAction;

        [Header("SETTINGS")]
        public bool disableSplashScreen;
        public bool enableLoginScreen;

        void Start()
        {
            bManager = gameObject.GetComponent<BlurManager>();
            splashScreenAnimator = splashScreen.GetComponent<Animator>();
            ssTimedAction = splashScreen.GetComponent<TimedAction>();
            mainPanelsAnimator = mainPanels.GetComponent<Animator>();
            homePanelAnimator = homePanel.GetComponent<Animator>();

            if (enableLoginScreen == false)
            {
                if (disableSplashScreen == true)
                {
                    splashScreen.SetActive(false);
                    mainPanels.SetActive(true);

                    mainPanelsAnimator.Play("Start");
                    homePanelAnimator.Play("Panel In");
                    backgroundAnimator.Play("Switch");
                    bManager.BlurInAnim();
                }

                else
                {
                    mainPanelsAnimator.Play("Invisible");
                    bManager.BlurOutAnim();
                    splashScreen.SetActive(true);
                }
            }

            else
            {
                splashScreen.SetActive(true);
                mainPanelsAnimator.Play("Invisible");
                homePanelAnimator.Play("Wait");
                bManager.BlurOutAnim();
            }
        }

        public void LoginScreenCheck()
        {
            if (enableLoginScreen == true)
                splashScreenAnimator.Play("Login Screen In");

            else
            {
                splashScreenAnimator.Play("Loading");
                ssTimedAction.StartIEnumerator();
            }
        }
    }
}