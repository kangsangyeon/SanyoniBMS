using SanyoniBMS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Doozy_ShowView : MonoBehaviour
{
    bool m_Show = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) == true)
        {
            this.m_Show = !this.m_Show;

            if (this.m_Show == true)
                StartCoroutine(Doozy.Engine.UI.UIView.ShowViewNextFrame(Global.PlayGameSceneCategoryName, Global.PlayGameScenePauseViewName));
            else
                StartCoroutine(Doozy.Engine.UI.UIView.HideViewNextFrame(Global.PlayGameSceneCategoryName, Global.PlayGameScenePauseViewName));

        }
    }
}
