using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using System.Collections;
using UnityEngine.UI;

public class DownloadAndPlay360Video : MonoBehaviour
{
    public string videoUrl;
    public VideoPlayer videoPlayer;
    public Slider progressBar;

    IEnumerator Start()
    {
        // Create a UnityWebRequest object to download the video file
        UnityWebRequest www = UnityWebRequest.Get(videoUrl);
        www.downloadHandler = new DownloadHandlerBuffer();

        // Start the download and track the progress
        var asyncOperation = www.SendWebRequest();
        while (!asyncOperation.isDone)
        {
            progressBar.value = asyncOperation.progress;
            yield return null;
        }

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to download video file: " + www.error);
            yield break;
        }

        // Save the downloaded video file to disk
        string videoPath = Application.persistentDataPath + "/video.mp4";
        System.IO.File.WriteAllBytes(videoPath, www.downloadHandler.data);

        // Set the VideoPlayer properties
        videoPlayer.url = videoPath;
        videoPlayer.playOnAwake = false;

        // Prepare the video for playback
        videoPlayer.Prepare();

        // Wait until the video is prepared
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        // Play the video
        videoPlayer.Play();

        // Hide the progress bar
        progressBar.gameObject.SetActive(false);
    }
}
