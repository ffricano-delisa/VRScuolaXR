using System.Collections;
using System.IO;
using System.Linq;
using Chiligames.MetaFusionTemplate;
using UnityEngine;
using UnityEngine.SceneManagement;

// Alias
using CustomSettings = GlobalConfigurationValues.CustomSceneManagerSettings;


public class CustomSceneManager : MonoBehaviour
{

    // VARIABLES
    // ********************************************************************************************

    private readonly string _logPrefix = "CustomSceneManager";
    private string _bundleBasePath;
    private string _startSceneName;
    private string _elevatorSceneName;

    private string _emptySceneName;
    private static CustomSceneManager _instance;


    // Classe Singletone
    public static CustomSceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CustomSceneManager>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(CustomSceneManager).Name);
                    _instance = singletonObject.AddComponent<CustomSceneManager>();
                }
            }
            return _instance;
        }
    }


    // UNITY LIFECYCLE
    // ********************************************************************************************

    private void Awake()
    {
        _bundleBasePath = GlobalConfigurationValues.AssetBundlesSetting.ASSET_BUNDLES_PERSISTENT_PATH;
        _startSceneName = CustomSettings.START_SCENE_NAME;
        _elevatorSceneName = CustomSettings.ELEVATOR_SCENE_NAME;
        _emptySceneName = CustomSettings.EMPTY_SCENE_NAME;
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        // SOTTOSCRIZIONE EVENTI
        CustomEventHandler.OnSubjectChange += FadeOutAndLoadSceneAndAssetFromSubjectName;
    }


    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        // DEBUG SU PC
        if (Input.GetKeyUp(KeyCode.P))
        {
            LoadEmptySceneWithRandomAsset();
        }
#endif
    }


    private void OnDestroy()
    {
        // DESOTTOSCIZIONE EVENTI
        CustomEventHandler.OnSubjectChange -= FadeOutAndLoadSceneAndAssetFromSubjectName;
    }


    // FUNCIONS
    // ********************************************************************************************

    // Funzione per il cambio di scena (ed eventuale caricamento di assetBundle) in base al "subject" passato come argomento
    // NB: DA UTILIZZARE COME CALLBACK PER L"EVENTO "OnSubjectChange"
    public void FadeOutAndLoadSceneAndAssetFromSubjectName(string newSubjectName)
    {
        string logPrefix = _logPrefix + " => FadeOutAndLoadScene";
        PrintDebug.Log(logPrefix, "START", Color.yellow);
        string newSceneName = newSubjectName switch
        {
            "introduzione" => _startSceneName,
            "ascensore" => _elevatorSceneName,
            _ => _emptySceneName,
        };
        StartCoroutine(LoadNewSceneWithAssetCoroutine(newSceneName, newSubjectName));
        PrintDebug.Log(logPrefix, "END", Color.yellow);
    }


    // Funzione per il cambio di scena casuale (ed eventuale caricamento di assetBundle)
    public void LoadEmptySceneWithRandomAsset()
    {
        string logPrefix = _logPrefix + " => FadeOutAndLoadEmptySceneWithRandomAsset";
        string[] availableAssets = GetAssetBundleNames(_bundleBasePath);
        int currentAssetIndex = new System.Random().Next(1, availableAssets.Length + 1);
        string currentSubjectName = availableAssets[currentAssetIndex];
        PrintDebug.LogWarning(logPrefix, "START; currentSubjectName: " + currentSubjectName, Color.yellow);
        if (string.IsNullOrEmpty(currentSubjectName) && AssetExists(_bundleBasePath, currentSubjectName))
        {
            // Verifica se esiste l'asset
            AssetBundle localAssetBundle = AssetBundle.LoadFromFile(currentSubjectName);
            if (localAssetBundle == null)
            {
                PrintDebug.LogError(logPrefix, "L'asset non esiste!", Color.red);
                return;
            }
        }
        StartCoroutine(LoadNewSceneWithAssetCoroutine(_emptySceneName, currentSubjectName));
        PrintDebug.LogWarning(logPrefix, "END; currentSubjectName: " + currentSubjectName, Color.yellow);
    }


    // Coroutine per il cambio scena e caricamento di assetBundle
    private IEnumerator LoadNewSceneWithAssetCoroutine(string newSceneName, string assetName)
    {
        string logPrefix = _logPrefix + " => FadeOutAndLoadNewSceneAndAssetCoroutine";
        PrintDebug.Log(logPrefix, "START. newSceneName " + newSceneName + "; assetName: " + assetName, Color.magenta);
        // Verifica condizione di cambio scena
        if (SceneExists(newSceneName) && IsNewScene(newSceneName))
        {
            yield return LoadNewSceneCoroutine(newSceneName);
            // Caricamento asset
            if (newSceneName != CustomSettings.START_SCENE_NAME && newSceneName != CustomSettings.ELEVATOR_SCENE_NAME)
            {
                LoadAsset(_bundleBasePath, assetName);
            }
        }
        else
        {
            if (!SceneExists(newSceneName)) PrintDebug.LogError(logPrefix, "Scena non trovata", Color.red);
            else PrintDebug.Log(logPrefix, "Scena o stanza già attiva, caricamento interrotto", Color.magenta);
            yield return null;
        }
        PrintDebug.Log(logPrefix, "END. Scena caricata correttamente", Color.magenta);
    }


    // Coroutine per il caricamento (asincrono) di una nuova scena
    private IEnumerator LoadNewSceneCoroutine(string newSceneName)
    {
        string logPrefix = _logPrefix + " => LoadSceneCoroutine";
        PrintDebug.Log(logPrefix, "Caricamento nuova scena: " + newSceneName, Color.magenta);
        NetworkRunnerHandler _networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        yield return StartCoroutine(_networkRunnerHandler.LoadNewScene(newSceneName));
        // Attendi un frame per visualizzare le modifiche
        yield return null;
        PrintDebug.Log(logPrefix, "Caricamento nuova scena completato: " + newSceneName, Color.magenta);
    }


    private void LoadAsset(string bundlePath, string assetName)
    {
        string logPrefix = _logPrefix + " => LoadAsset";
        if (AssetExists(bundlePath, assetName))
        {
            BundledAssetLoader bundledObjectLoader = FindObjectOfType<BundledAssetLoader>();
            bundledObjectLoader.LoadAsset(assetName);
        }
        else
        {
            PrintDebug.LogError(logPrefix, "Asset \"" + assetName + "\"+ non trovato nella directory: " + bundlePath, Color.red);
        }
    }


    // Verifica l'esistenza della scena all'interno del set di scene della build
    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (name.Equals(sceneName))
            {
                return true;
            }
        }
        return false;
    }


    // Verifica se la prossima scena è una nuova scena
    private bool IsNewScene(string newSceneName)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        bool isNewScene = currentSceneName != newSceneName;
        bool isNewRoom = currentSceneName == newSceneName && currentSceneName == CustomSettings.EMPTY_SCENE_NAME;
        return isNewScene || isNewRoom;
    }


    // Verifica se esiste il nome dell'asset all'interno della esiste all'interno della directory _bundleBasePath
    private bool AssetExists(string bundlePath, string assetName)
    {
        string[] assetList = GetAssetBundleNames(bundlePath);
        return assetList.Contains(assetName);
    }


    // Restituisce tutti i nomi dei file senza estensioni (senza il carattere punto) presenti nel directoryPath, ad eccezione del file "AssetBundles"
    private string[] GetAssetBundleNames(string bundlePath)
    {
        string logPrefix = _logPrefix + "GetAssetBundleNames";
        string assetBundleDirectoryName = GlobalConfigurationValues.AssetBundlesSetting.ASSET_BUNDLES_DIRECTORY_NAME;
        if (Directory.Exists(bundlePath))
        {
            // Ottieni tutti i file nella directory
            string[] files = Directory.GetFiles(bundlePath, "*")
                // Il cui nome sia diverso dal nome della directory e che non contengano il carattere punto (file con estensione)
                .Where(filePath => !Path.GetFileName(filePath).Contains(assetBundleDirectoryName) && !Path.GetFileName(filePath).Contains(".")).ToArray();
            // Ottieni solo i nomi dei file (senza il percorso completo)
            string[] fileNames = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                fileNames[i] = Path.GetFileName(files[i]);
            }
            return fileNames;
        }
        else
        {
            Debug.LogError("Directory does not exist: " + bundlePath);
            return new string[0];
        }
    }


    // Coroutine per il fade-in/fade-out della scena
    private IEnumerator SceenFadeCoroutine(bool isFadeIn)
    {
        string logPrefix = _logPrefix + " => SceenFadeCoroutine";
        OVRScreenFade screenFade = Camera.main.GetComponent<OVRScreenFade>();
        // Avvia il fade in base al parametro in input
        if (isFadeIn)
        {
            screenFade.FadeIn();
        }
        else
        {
            screenFade.FadeOut();
        }
        // Attendi il completamento del fade
        while (screenFade.currentAlpha != (isFadeIn ? 0f : 1f))
        {
            yield return null;
        }
        PrintDebug.Log(logPrefix, "Fade " + (isFadeIn ? "in" : "out") + " completato", Color.magenta);
    }


}