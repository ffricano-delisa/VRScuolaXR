using System.Collections;
using Chiligames.MetaFusionTemplate;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CustomSceneLoader : MonoBehaviour
{

    // VARIABLES
    // ********************************************************************************************

    public NetworkRunnerHandler networkRunnerHandler;
    public TMP_Text m_Text;
    public string sceneName;

    private OVRScreenFade screenFade;
    private bool asyncModeChosen = false;


    // UNITY LIFECYCLE
    // ********************************************************************************************

    void Awake()
    {
        screenFade = FindObjectOfType<OVRScreenFade>();
    }


    void Start()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        m_Text.text = "<color=blue>Press 'T' for LoadSceneSync()</color>" + "\n" + "<color=yellow>Press 'L' for LoadSceneAsync()</color>";
#else
            m_Text.enabled = false;
#endif
    }


    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (Input.GetKeyDown(KeyCode.T))
        {
            LoadSceneSync(sceneName);
        }
        if (Input.GetKeyDown(KeyCode.L) && !asyncModeChosen)
        {
            asyncModeChosen = true;
            LoadSceneAsync(sceneName);
        }
#endif
    }


    // FUNCIONS
    // ********************************************************************************************


    // Caricamento sincrono di una scena utilizzando il networkRunnerHandler
    void LoadSceneSync(string sceneName)
    {
        StartCoroutine(networkRunnerHandler.LoadNewScene(sceneName));
    }


    // Caricamento asincrono di una scena con trigger manuali
    void LoadSceneAsync(string sceneName)
    {
        StartCoroutine(LoadSceneAsyncCoroutine(sceneName));
    }


    // Coroutine per il caricamento asincrono di una scena con trigger manuali
    IEnumerator LoadSceneAsyncCoroutine(string sceneName)
    {
        yield return null;

        //Begin to load the Scene you specify
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        //Don't let the Scene activate until you allow it to
        asyncOperation.allowSceneActivation = false;
        Debug.Log("Pro :" + asyncOperation.progress);
        //When the load is still in progress, output the Text and progress bar
        while (!asyncOperation.isDone)
        {
            //Output the current progress
            m_Text.text = "<color=yellow>Loading progress: " + (asyncOperation.progress * 100) + "%</color>";
            // Check if the load has finished
            if (asyncOperation.progress >= 0.9f)
            {
                //Change the Text to show the Scene is ready
                m_Text.text = "<color=yellow>Press 'L' again to switch to new Scene</color>";
                //Wait to you press the space key to activate the Scene
                if (Input.GetKeyDown(KeyCode.L))
                {
                    //Activate the Scene
                    screenFade.FadeOut();
                    yield return new WaitForSeconds(screenFade.fadeTime);
                    asyncOperation.allowSceneActivation = true;
                    Destroy(gameObject);
                }
            }
            yield return null;
        }
    }

}