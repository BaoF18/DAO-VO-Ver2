using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class LogoSceneLoader : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextScene = "TestUI";

    private void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextScene);
    }
}