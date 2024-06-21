using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;


public class BundleSceneLoader : MonoBehaviour
{

    // VARIABLES
    // ********************************************************************************************

    public string bundleName;
    public string sceneName;
    public bool autoLoadAtStart;
    public int autoLoadTimeDelay;


    // UNITY LIFECYCLE
    // ********************************************************************************************

    void Start()
    {
        if (autoLoadAtStart)
        {
            LoadAssetBundleScene();
        }
    }


    // FUNCIONS
    // ********************************************************************************************

    // Caricamento asincono di una scena contenuta nell'AssetBundle
    public void LoadAssetBundleScene()
    {
        StartCoroutine(LoadAssetBundleSceneCoroutine());
    }


    // Coroutine per il caricamento asincono di una scena contenuta nell'AssetBundle
    IEnumerator LoadAssetBundleSceneCoroutine()
    {
        // TEMPO DI ATTESA
        if (autoLoadAtStart && autoLoadTimeDelay > 0)
        {
            yield return new WaitForSeconds(autoLoadTimeDelay);
        }

        // CARICAMENTO ASSETBUNDLE (ASYNC)
        string bundlePath = Path.Combine(Application.streamingAssetsPath, bundleName);
        AssetBundleCreateRequest asyncBundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
        yield return asyncBundleRequest;

        // VERIFICA ASSETBUNDLE
        AssetBundle localAssetBundle = asyncBundleRequest.assetBundle;
        if (localAssetBundle == null)
        {
            Debug.LogError("Failed to load AssetBundle!");
            yield break;
        }

        // CARICAMENTO SCENA (ASYNC)
        string[] scenePaths = localAssetBundle.GetAllScenePaths();
        if (scenePaths.Length > 0)
        {
            string scenePath = scenePaths[0];
            yield return LoadSceneAsync(scenePath);
        }

        // RILASCIO RISORSA
        localAssetBundle.Unload(false);
    }


    // Caricamento asincrono di una scena presente nella build, utilizzando lo SceneManager
    IEnumerator LoadSceneAsync(string scenePath)
    {
        AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
        yield return loadSceneOperation;
    }

}
