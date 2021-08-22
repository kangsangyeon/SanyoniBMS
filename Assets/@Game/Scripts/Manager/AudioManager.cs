using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using SanyoniLib.UnityEngineHelper;

namespace SanyoniBMS
{

    public class AudioManager : SingletonMonoBehaviour<AudioManager>
    {
        private AudioSource[] m_AudioSources;

        protected void Start()
        {
            Initialize();

        }
        public void Initialize()
        {
            if (this.m_AudioSources != null)
            {
                foreach (var source in this.m_AudioSources) Destroy(source);
            }
            this.m_AudioSources = null;

        }

        public void Prepare()
        {

        }


    }

}