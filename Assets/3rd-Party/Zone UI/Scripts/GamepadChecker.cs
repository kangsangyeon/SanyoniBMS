using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Michsky.UI.Zone
{
    public class GamepadChecker : MonoBehaviour
    {
        [Header("RESOURCES")]
        public GameObject virtualCursor;
        public GameObject virtualCursorContent;
        public GameObject tooltipDesktop;
        public GameObject eventSystem;

        [Header("OBJECTS")]
        [Tooltip("Objects in this list will be active when gamepad is un-plugged.")]
        public List<GameObject> keyboardObjects = new List<GameObject>();
        [Tooltip("Objects in this list will be active when gamepad is plugged.")]
        public List<GameObject> gamepadObjects = new List<GameObject>();

        [Header("SETTINGS")]
        [Tooltip("Always update input device. If you turn off this feature, you won't able to change the input device after start, but it might increase the performance.")]
        public bool alwaysSearch = true;

        private TooltipManagerDesktop tooltipDesktopScript;
        private GamepadChecker checkerScript;
        private int GamepadConnected = 0;
        private Vector3 startMousePos;
        private Vector3 startPos;

        bool gamepadEnabled;

        void Start()
        {
            tooltipDesktopScript = this.GetComponent<TooltipManagerDesktop>();
            checkerScript = gameObject.GetComponent<GamepadChecker>();

            SwitchToKeyboard();

            if (alwaysSearch == false)
            {
                checkerScript.enabled = false;
            }

            else
            {
                checkerScript.enabled = true;
                Debug.Log("Always Search is on. Input device will be updated in case of disconnecting/connecting.");
            }
        }

        void Update()
        {
            string[] names = Input.GetJoystickNames();

            for (int x = 0; x < names.Length; x++)
            {
                // print(names[x].Length); Just for testing stuff

                if (names[x].Length >= 1)
                {
                    GamepadConnected = 1;
                }

                else if (names[x].Length == 0)
                {
                    GamepadConnected = 0;
                }
            }

            if (GamepadConnected == 1 && gamepadEnabled == false)
            {
                SwitchToController();
            }

            else if (GamepadConnected == 0 && gamepadEnabled == true)
            {
                SwitchToKeyboard();
            }
        }

        public void SwitchToController()
        {
            for (int i = 0; i < keyboardObjects.Count; i++)
            {
                keyboardObjects[i].SetActive(false);
            }

            for (int i = 0; i < gamepadObjects.Count; i++)
            {
                gamepadObjects[i].SetActive(true);
            }

            gamepadEnabled = true;
            eventSystem.SetActive(false);
            virtualCursor.SetActive(true);
            virtualCursorContent.SetActive(true);
            tooltipDesktop.SetActive(false);
            tooltipDesktopScript.enabled = false;
            Debug.Log("Gamepad detected. Switching to gamepad input.");
        }

        public void SwitchToKeyboard()
        {
            for (int i = 0; i < keyboardObjects.Count; i++)
            {
                keyboardObjects[i].SetActive(true);
            }

            for (int i = 0; i < gamepadObjects.Count; i++)
            {
                gamepadObjects[i].SetActive(false);
            }

            gamepadEnabled = false;
            virtualCursor.SetActive(false);
            virtualCursorContent.SetActive(false);
            tooltipDesktop.SetActive(true);
            tooltipDesktopScript.enabled = true;
            eventSystem.SetActive(true);
            Debug.Log("No gamepad detected. Switching to keyboard input.");
        }
    }
}