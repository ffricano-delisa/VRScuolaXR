using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class QuadroParlanteController : MonoBehaviour
{
    public VideoPlayer videoPlayerQuadro;

    private void Awake()
    {
        videoPlayerQuadro.Play();
        videoPlayerQuadro.Pause();
    }
}
