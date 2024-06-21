using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpscaleEyeResolution : MonoBehaviour
{
    public float TextureScale = 1.0f;
    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale = TextureScale;
    }


}
