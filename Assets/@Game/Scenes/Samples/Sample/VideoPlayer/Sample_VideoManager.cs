using System.Collections;
using SanyoniLib.UnityEngineWrapper;
using UnityEngine;

public class Sample_VideoManager : MonoBehaviour
{
    public string videoUrl;
    [SerializeField] private VideoManager videoManager;
    [SerializeField] private RenderTexture renderTexture;

    private IEnumerator Start()
    {
        // TODO
        // videoManager.Initialize(renderTexture);
        yield return videoManager.CPrepareVideoUrl(videoUrl);

        videoManager.PlayVideo(true);
    }

}
