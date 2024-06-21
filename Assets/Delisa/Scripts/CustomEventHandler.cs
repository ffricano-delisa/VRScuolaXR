using UnityEngine;


public static class CustomEventHandler
{

    // VARIABLES
    // ********************************************************************************************

    private static readonly string _logPrefix = "CustomEventHandler";

    // LISTA EVENTI E DELEGATI
    public delegate void SubjectChangeAction(string newSubject);
    public static event SubjectChangeAction OnSubjectChange;


    // FUNCIONS
    // ********************************************************************************************

    // Invocazione evento per il cambio subject
    public static void InvokeSubjectChange(string newSubject)
    {
        PrintDebug.Log(_logPrefix, "Invocazione evento \"OnSubjectChange\"", Color.blue);
        OnSubjectChange?.Invoke(newSubject);
    }

}