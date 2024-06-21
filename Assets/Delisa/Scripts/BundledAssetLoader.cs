using System.IO;
using UnityEngine;

// Alias
using CustomSettings = GlobalConfigurationValues.AssetBundlesSetting;


public class BundledAssetLoader : MonoBehaviour
{

    // VARIABLES
    // ********************************************************************************************

    // [Header("Asset Spawn Position")]
    // public float posx = 0;
    // public float posy = 0;
    // public float posz = 0;

    [Header("Editor Debug Only")]
#if UNITY_EDITOR
    public bool autoLoadEnabled = false;
    [Tooltip("Enable auto-load of specific asset")]
    public string assetNameToAutoLoad;
    [Tooltip("The name of specific asset to load")]
#endif


    private string _logPrefix = "BundledObjectLoader";
    private string _bundleBasePath;
    private string _bundleToIgnore;


    // UNITY LIFECYCLE
    // ********************************************************************************************

    void Awake()
    {
#if UNITY_EDITOR
        _bundleBasePath = CustomSettings.ASSET_BUNDLES_CUSTOM_PATH;
        // DEBUG MANUAL LOAD
        if (autoLoadEnabled && !string.IsNullOrEmpty(assetNameToAutoLoad))
        {
            LoadAsset(assetNameToAutoLoad);
        }
#else
        _bundleBasePath = CustomSettings.ASSET_BUNDLES_PERSISTENT_PATH;
#endif
        _bundleToIgnore = CustomSettings.BUNDLE_TO_IGNORE;
    }


    // FUNCIONS
    // ********************************************************************************************

    // Carica un asset di default presente all'interno dell'assetBundle passato come argomento
    public void LoadAsset(string bundleName)
    {
        string logPrefix = _logPrefix + " => LoadAsset";
        if (bundleName != _bundleToIgnore)
        {
            string bundlePath = Path.Combine(_bundleBasePath, bundleName);
            string assetToLoad = CustomSettings.DEFAULT_ASSET_NAME;
            PrintDebug.Log(logPrefix, "START. bundlePath: " + bundlePath + "; assetToLoad: " + assetToLoad, Color.cyan);
            AssetBundle localAssetBundle = AssetBundle.LoadFromFile(bundlePath);
            if (localAssetBundle == null)
            {
                PrintDebug.LogError(logPrefix, "Failed to load AssetBundle!", Color.red);
                return;
            }
            GameObject asset = localAssetBundle.LoadAsset<GameObject>(assetToLoad);
            // asset.transform.position = new Vector3(posx, posy, posz);
            // asset.transform.position = Camera.main.transform.position;
            // AlignToCameraCenter(asset, Camera.main.gameObject);
            Instantiate(asset);
            PrintDebug.Log(logPrefix, "ASSET LOADED! bundlePath: " + bundlePath + "; asset: " + assetToLoad, Color.cyan);
            localAssetBundle.Unload(false);
        }
        else
        {
            PrintDebug.Log(logPrefix, "The bundle \"" + bundleName + "\" is marked as ignore, asset not loaded", Color.cyan);
        }
    }

    private void AlignToCameraCenter(GameObject assetEnviroment, GameObject camera)
    {
        AlignToObjectCenter.Align(assetEnviroment, camera);
    }

}