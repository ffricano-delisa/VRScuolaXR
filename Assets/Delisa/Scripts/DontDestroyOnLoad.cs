using UnityEngine;


// CLASSE SINGLETONE
public class DontDestroyOnLoad : MonoBehaviour
{

    // VARIABLES
    // ********************************************************************************************

    // Classe Singletone
    private static DontDestroyOnLoad instance;


    // UNITY LIFECYCLE
    // ********************************************************************************************

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);
    }
}
