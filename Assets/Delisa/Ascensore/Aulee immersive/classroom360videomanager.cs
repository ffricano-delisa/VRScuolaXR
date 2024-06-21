using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class classroom360videomanager : MonoBehaviour
{
    public GameObject OVRCameraRig;

    private Transform[] videoIndexTransform;

    private void Start()
    {
        Component[] components = GetComponentsInChildren<Transform>(includeInactive: true);
        videoIndexTransform = new Transform[components.Length];
        for (int i = 0; i < components.Length; i++)
        {
            videoIndexTransform[i] = (Transform)components[i];
        }
    }
    public void Moveto360Video(int indice)
    {
        OVRCameraRig.transform.position = videoIndexTransform[indice].position;
        videoIndexTransform[indice].gameObject.SetActive(true);
    }
}
