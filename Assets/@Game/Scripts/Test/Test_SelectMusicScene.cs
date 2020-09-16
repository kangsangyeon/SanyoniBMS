using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SanyoniBMS
{

    public class Test_SelectMusicScene : MonoBehaviour
    {
        private void Start()
        {
            Scene testScene = SceneManager.GetActiveScene();

            var process = SceneManager.LoadSceneAsync(Global.PersistentSceneText);
            process.completed += _ =>
            {

            };

            SceneManager.LoadSceneAsync(Global.SelectMusicSceneText);
        }
    }

}