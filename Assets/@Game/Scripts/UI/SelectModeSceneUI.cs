using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanyoniBMS
{

    public enum SelectModeSceneSequence
    {
        StartView, CasualMode
    }

    public class SelectModeSceneUI : MonoBehaviour
    {
        public GameObject StartView;
        public GameObject CasualModeView;

        private Animator StartViewAnim;
        private Animator CasualModeViewAnim;

        private void Awake()
        {
            this.StartViewAnim = this.StartView.GetComponent<Animator>();
            this.CasualModeViewAnim = this.CasualModeView.GetComponent<Animator>();
        }

        private void Start()
        {

        }

        private void SwitchView(SelectModeSceneSequence origin, SelectModeSceneSequence dest)
        {
            if (origin == SelectModeSceneSequence.StartView && dest == SelectModeSceneSequence.CasualMode)
            {
                StartViewAnim.SetTrigger("hideToTop");
                CasualModeViewAnim.SetTrigger("showFromBottom");
            }
        }

    }

}