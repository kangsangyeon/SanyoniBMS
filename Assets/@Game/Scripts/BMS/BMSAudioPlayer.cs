using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Sirenix.OdinInspector;
using SanyoniLib.UnityEngineHelper;
using System.Linq;

namespace SanyoniBMS
{

    [System.Serializable]
    [ShowOdinSerializedPropertiesInInspector]
    public class BMSAudioPlayer : MonoBehaviour
    {
        private const int AudioSourceCount = 100;

        private Dictionary<int, AudioClip> m_AudioClipDict = new Dictionary<int, AudioClip>();
        private Queue<AudioSource> m_AudioSourceQueue;
        private GameObject m_AudioSourcesParent;

        public bool AnyAudioHasPlaying
        {
            get
            {
                for (int i = 0; i < AudioSourceCount; i++)
                {
                    AudioSource source = this.m_AudioSourceQueue.Dequeue();
                    this.m_AudioSourceQueue.Enqueue(source);

                    if (source.isPlaying) return true;
                }

                return false;
            }
        }

        // 오디오 로딩 관련
        public bool IsPrepared = false;
        public bool IsLoading = false;
        public int AllAudioClipCount;
        public int LoadedAudioClipCount;

        private List<AudioSource> m_PausedAudioSources = new List<AudioSource>();


        public void Initialize()
        {
            this.m_AudioClipDict = new Dictionary<int, AudioClip>();

            if (this.m_AudioSourcesParent != null) Destroy(this.m_AudioSourcesParent);
            this.m_AudioSourcesParent = new GameObject("AudioSources");
            this.m_AudioSourcesParent.transform.SetParent(this.transform);

            this.m_AudioSourceQueue = new Queue<AudioSource>(capacity: AudioSourceCount);
            for (int i = 0; i < AudioSourceCount; i++)
            {
                AudioSource newSource = this.m_AudioSourcesParent.AddComponent<AudioSource>();
                newSource.loop = false;
                newSource.playOnAwake = false;

                this.m_AudioSourceQueue.Enqueue(newSource);
            }
            this.IsPrepared = false;
        }


        public void Prepare(string bmsDir, BMSPatternData pattern)
        {
            StartCoroutine(CLoadBMSAudioClips(bmsDir, pattern));
        }


        public void Play(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetIdleAudioSourceFromQueue();

            // 쉬고있는 AudioSource가 없다면, 그냥 가장 오래된 시간 전에 재생했던 오디오 소스에서 재생한다.
            if (source == null)
            {
                source = this.m_AudioSourceQueue.Dequeue();
                this.m_AudioSourceQueue.Enqueue(source);
            }

            source.clip = clip;
            source.volume = volume;
            source.loop = false;
            source.playOnAwake = false;
            source.Play();
        }

        public void PlayOneshot(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetIdleAudioSourceFromQueue();

            // 쉬고있는 AudioSource가 없다면, 그냥 가장 오래된 시간 전에 재생했던 오디오 소스에서 재생한다.
            if (source == null)
            {
                source = this.m_AudioSourceQueue.Dequeue();
                this.m_AudioSourceQueue.Enqueue(source);
            }

            source.PlayOneShot(clip, volume);

        }

        public void TryPlayInDictionary(int key)
        {
            if (this.m_AudioClipDict.ContainsKey(key))
            {
                AudioClip clip = this.m_AudioClipDict[key];
                if (clip != null) Play(clip);
                else
                {
                    if (DebugSettings.DebugMode) Debug.LogFormat("BMSAudioPlayer.TryPlayDictionary(): {0} is not exist.", key);
                }
            }
        }

        public void TryPlayOneshotInDictionary(int key)
        {
            if (this.m_AudioClipDict.ContainsKey(key))
            {
                AudioClip clip = this.m_AudioClipDict[key];
                if (clip != null) PlayOneshot(clip);
                else
                {
                    if (DebugSettings.DebugMode) Debug.LogFormat("BMSAudioPlayer.TryPlayDictionary(): {0} is not exist.", key);
                }
            }
        }

        public void PauseAll()
        {
            for (int i = 0; i < AudioSourceCount; i++)
            {
                AudioSource source = this.m_AudioSourceQueue.Dequeue();
                this.m_AudioSourceQueue.Enqueue(source);

                if (source.isPlaying == true)
                {
                    this.m_PausedAudioSources.Add(source);
                    source.Pause();
                }

            }
        }

        public void ResumeAll()
        {
            for (int i = 0; i < this.m_PausedAudioSources.Count; i++)
            {
                this.m_PausedAudioSources[i].UnPause();
            }

            this.m_PausedAudioSources.Clear();
        }


        #region Private Methods

        private AudioSource GetIdleAudioSourceFromQueue()
        {
            for (int i = 0; i < AudioSourceCount; i++)
            {
                AudioSource source = this.m_AudioSourceQueue.Dequeue();
                this.m_AudioSourceQueue.Enqueue(source);

                if (source.isPlaying) continue;
                else return source;
            }

            if (DebugSettings.DebugMode) Debug.Log("BMSAudioPlayer.GetIdleAudioSourceFromQueue: 쉬고있는 AudioSource가 없습니다. AudioSource Count를 늘릴 필요가 있어보입니다.");
            return null;
        }

        private IEnumerator CLoadBMSAudioClips(string bmsDir, BMSPatternData pattern)
        {
            IsLoading = true;
            IsPrepared = false;

            int[] keys = new List<int>(pattern.Header.WavDict.Keys).ToArray();
            AllAudioClipCount = keys.Length;

            for (int i = 0; i < keys.Length; i++)
            {
                m_AudioClipDict.Add(keys[i], null);

                string fileName = pattern.Header.WavDict[keys[i]];
                yield return StartCoroutine(CLoadBMSAudioClip(keys[i], bmsDir, fileName));

                LoadedAudioClipCount = i + 1;
            }

            IsLoading = false;
            IsPrepared = true;
        }

        private IEnumerator CLoadBMSAudioClip(int key, string dir, string fileName)
        {
            // 파일의 존재 여부를 확인한다.
            // 이 때, 파라미터로 받은 fileName의 확장자가 실제 파일과 다를 수 있으므로, 
            // 확장자를 바꿔가며 실제 파일로 존재하는지에 대한 여부를 확인한다.
            string realFileName = null;

            try
            {
                realFileName = SanyoniLib.SystemHelper.PathHelper.GuessRealAudioFileName(dir, fileName);

                if (string.IsNullOrWhiteSpace(realFileName) == true) throw new FileNotFoundException();
            }
            // 파일이 없다면 로그를 남긴다.
            catch (FileNotFoundException e)
            {
                DebugHelper.LogErrorFormat(e, "파일이 없거나 지원하지 않는 파일입니다. ogg / wav / mp3만 지원하고 있습니다. \n{0}", Path.Combine(dir, fileName));
                yield break;
            }

            // 실제로 존재한다면 로드한다.
            yield return StartCoroutine(ResourcesHelper.CLoadAudioClip(dir, realFileName, x => this.m_AudioClipDict[key] = x));
        }

    }

    #endregion

}