using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Networking;

public class VideoManager : SanyoniLib.UnityEngineHelper.SingletonMonoBehaviour<VideoManager>
{
    public bool IsInitialized { get; private set; } = false;

    //Raw Image to Show Video Images [Assign from the Editor]
    [SerializeField] private RenderTexture m_TargetTexture;

    private VideoPlayer m_VideoPlayer;
    private AudioSource m_AudioSource;

    private void Start()
    {
        if (IsInitialized == false && this.m_TargetTexture != null)
            Initialize(this.m_TargetTexture);
    }

    public void Initialize(RenderTexture _targetTexture)
    {
        if (IsInitialized == true)
            return;

        if (this.m_VideoPlayer == null) this.m_VideoPlayer = GetComponent<VideoPlayer>();
        if (this.m_VideoPlayer == null) this.m_VideoPlayer = gameObject.AddComponent<VideoPlayer>();

        if (this.m_AudioSource == null) this.m_AudioSource = GetComponent<AudioSource>();
        if (this.m_AudioSource == null) this.m_AudioSource = gameObject.AddComponent<AudioSource>();

        m_VideoPlayer.playOnAwake = false;
        m_AudioSource.playOnAwake = false;

        m_VideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        m_VideoPlayer.EnableAudioTrack(0, true);
        m_VideoPlayer.SetTargetAudioSource(0, m_AudioSource);

        m_VideoPlayer.targetTexture = _targetTexture;

        IsInitialized = true;

    }

    public void Initialize()
    {
        if (IsInitialized == true)
            return;

        if (this.m_TargetTexture != null)
            Initialize(this.m_TargetTexture);
    }

    public IEnumerator CPrepareVideoUrl(string _url)
    {
        Debug.Log("Try preparing video.");

        m_VideoPlayer.source = VideoSource.Url;
        m_VideoPlayer.url = _url;

        m_VideoPlayer.Prepare();

        yield return new WaitUntil(() => m_VideoPlayer.isPrepared);

        Debug.Log("Preparing Video is done.");
    }

    public void PlayVideo(bool _withAudio)
    {
        m_VideoPlayer.Play();
        if (_withAudio == true) m_AudioSource.Play();

    }

    public void PauseVideo()
    {
        this.m_VideoPlayer.Pause();
    }

}
