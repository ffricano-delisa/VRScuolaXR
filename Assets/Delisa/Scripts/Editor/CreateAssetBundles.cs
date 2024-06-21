using UnityEditor;
using System.IO;
using UnityEngine;

// Alias
using CustomSettings = GlobalConfigurationValues.AssetBundlesSetting;
using System.Drawing;

public class CreateAssetBundles
{

    private static readonly string _logPrefix = "CreateAssetBundles";

    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string logPrefix = _logPrefix + " => BuildAllAssetBundles";
        string assetBundleDirectory = CustomSettings.ASSET_BUNDLES_CUSTOM_PATH;
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(
            assetBundleDirectory,
            // BuildAssetBundleOptions.None, 
            BuildAssetBundleOptions.UncompressedAssetBundle,
            // BuildTarget.Android
            EditorUserBuildSettings.activeBuildTarget
        );
        PrintDebug.Log(logPrefix, "AssetBundles aggiunti correttamente nella directory: " + assetBundleDirectory, UnityEngine.Color.yellow);
    }

}