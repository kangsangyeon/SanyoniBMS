using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Test_VideoManager : MonoBehaviour
{
    public string videoUrl;
    [SerializeField] private VideoManager videoManager;
    [SerializeField] private RenderTexture renderTexture;

    private IEnumerator Start()
    {
        videoManager.Initialize(renderTexture);
        yield return videoManager.CPrepareVideoUrl(videoUrl);

        videoManager.PlayVideo(true);
    }

}
