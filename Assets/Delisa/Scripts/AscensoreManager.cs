using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class AscensoreManager : MonoBehaviour
{
    public Transform doorSx;
    public Transform doorDx;
    public VideoPlayer videoplayer;

    public Transform wall;

    public Material ascensoreMat;
    public Material gateMat;
    public Material wallMat;

    public AudioSource movingDoorsSFX;
    public AudioSource movingElevatorSFX;
    public AudioSource dingSFX;

    private bool ascensoreIsMoving = false;
    private int currentFloor;

    public void Start()
    {
        currentFloor = 0;

        // Define the path to the folder
        string folderPath = Path.Combine(Application.persistentDataPath, "videos");

        // Check if the folder exists
        if (!Directory.Exists(folderPath))
        {
            // If it doesn't exist, create the folder
            Directory.CreateDirectory(folderPath);
            Debug.Log("Folder created: " + folderPath);
        }
        else
        {
            Debug.Log("Folder already exists: " + folderPath);
        }
        // ChiamaAscensore(3);
    }

    public void ChiamaAscensore(int floor)
    {
        if (ascensoreIsMoving) return;
        int deltaFloor = floor - currentFloor; // deltaFloor positivo=UP, negativo=DOWN
        if (deltaFloor == 0) return;
        StartCoroutine(MuoviAscensore(deltaFloor, floor));
    }

    private IEnumerator MuoviAscensore(int delta, int floor)
    {
        videoplayer.Stop();
        ascensoreIsMoving = true;
        StartCoroutine(FadeMaterial(ascensoreMat, true));
        StartCoroutine(FadeMaterial(gateMat, true));
        StartCoroutine(FadeMaterial(wallMat, true));

        yield return StartCoroutine(CloseDoors());
        PlayVideo(floor);

        yield return StartCoroutine(MoveWall(delta));

        dingSFX.Play();
        yield return StartCoroutine(OpenDoors());

        StartCoroutine(FadeMaterial(ascensoreMat, false));
        StartCoroutine(FadeMaterial(wallMat, false));
        yield return StartCoroutine(FadeMaterial(gateMat, false));
        ascensoreIsMoving = false;     
        currentFloor = floor;

        yield break;
    }


    private void PlayVideo(int id)
    {
        string idPath = id.ToString();
        videoplayer.url = Application.persistentDataPath + "/videos/" + idPath + ".mp4";
        // Debug.Log("loading video in path: " + Application.dataPath + "/" + idPath + ".mp4");
        StartCoroutine(PlayVideoDelay(4f));
    }

    private IEnumerator PlayVideoDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        videoplayer.Play();
    }

    private IEnumerator OpenDoors()
    {
        Debug.Log("opening doors");
        movingDoorsSFX.Play();

        float elapsedTime = 0f;
        float openDuration = 2f; // Adjust the time it takes to open the doors

        Vector3 initialPosSx = doorSx.position;
        Vector3 finalPosSx = new Vector3(-1.120f, initialPosSx.y, initialPosSx.z);

        Vector3 initialPosDx = doorDx.position;
        Vector3 finalPosDx = new Vector3(0.560f, initialPosDx.y, initialPosDx.z);

        while (elapsedTime < openDuration)
        {
            doorSx.position = Vector3.Lerp(initialPosSx, finalPosSx, elapsedTime / openDuration);
            doorDx.position = Vector3.Lerp(initialPosDx, finalPosDx, elapsedTime / openDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        movingDoorsSFX.Stop();       
    }

    private IEnumerator CloseDoors()
    {
        Debug.Log("closing doors");
        movingDoorsSFX.Play();

        float elapsedTime = 0f;
        float closeDuration = 2f; // Adjust the time it takes to close the doors

        Vector3 initialPosSx = doorSx.position;
        Vector3 finalPosSx = new Vector3(-0.555f, initialPosSx.y, initialPosSx.z);

        Vector3 initialPosDx = doorDx.position;
        Vector3 finalPosDx = new Vector3(0.022f, initialPosDx.y, initialPosDx.z);

        while (elapsedTime < closeDuration)
        {
            doorSx.position = Vector3.Lerp(initialPosSx, finalPosSx, elapsedTime / closeDuration);
            doorDx.position = Vector3.Lerp(initialPosDx, finalPosDx, elapsedTime / closeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        movingDoorsSFX.Stop();

        yield return new WaitForSeconds(0.5f);
    }
    private IEnumerator MoveWall(int deltaFl)
    {
        Debug.Log("moving wall");
        float wallMoveDuration = 6f; // Adjust the time it takes to move the wall
        float wallMoveDistance = 25.75f; // Adjust the distance the wall moves (prev=22.75)
        movingElevatorSFX.Play();
        Vector3 initialPos;
        Vector3 finalPos;

        if (deltaFl < 0)
        {
            // Moving up: -8.75 -> 14
            initialPos = new Vector3(wall.position.x, -8.75f , wall.position.z);
            finalPos = new Vector3(initialPos.x, initialPos.y + wallMoveDistance, initialPos.z);
        }
        else
        {
            // Moving down: 14 -> -8.75
            initialPos = new Vector3(wall.position.x, 14f, wall.position.z);
            finalPos = new Vector3(initialPos.x, initialPos.y - wallMoveDistance, initialPos.z);
        }

        float elapsedTime = 0f;

        while (elapsedTime < wallMoveDuration)
        {
            wall.position = Vector3.Lerp(initialPos, finalPos, elapsedTime / wallMoveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        movingElevatorSFX.Stop();
    }

    private IEnumerator FadeMaterial(Material mat, bool fadeIn)
    {
        float fadeDuration = 1.0f; // You can adjust the duration as needed
        float elapsedTime = 0f;

        float startAlpha = mat.color.a;
        float targetAlpha = fadeIn ? 1f : 0f;

        while (elapsedTime < fadeDuration)
        {
            // Calculate the new alpha value based on the elapsed time
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);

            // Set the new color with the updated alpha value
            Color newColor = mat.color;
            newColor.a = alpha;
            mat.color = newColor;

            // Update the elapsed time
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }

        // Ensure the final state is set
        Color finalColor = mat.color;
        finalColor.a = targetAlpha;
        mat.color = finalColor;
    }

}
