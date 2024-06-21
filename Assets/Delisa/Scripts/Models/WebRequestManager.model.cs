using System;


public struct WorkflowRequest
{
    public readonly WebRequestEndpoints endpoint;
    public WorkflowRequestState requestState;               
    public Func<Func<bool>> CanBeStarted;    // Delegato per determinare se la richiesta può essere avviata

    // Costruttore
    public WorkflowRequest(WebRequestEndpoints endpoint, WorkflowRequestState requestState = WorkflowRequestState.PENDING, Func<Func<bool>> canBeStartedFunc = null)
    {
        this.endpoint = endpoint;
        this.requestState = requestState;
        CanBeStarted = canBeStartedFunc ?? DefaultCanBeStartedCheck;
    }

    private static readonly Func<Func<bool>> DefaultCanBeStartedCheck = () => () => true;
}


public enum WorkflowRequestState 
{
    PENDING,    // Richiesta ancora da eseguire
    RUNNING,    // Richiesta in esecuzione
    SUCCESS,    // Richiesta eseguita con successo (ottenuta successRespone)
    ERROR,      // Richiesta eseguita con errori (ottenuta errorResponse)
    EXCEPTION   // Richiesta non eseguita (sollevamento eccezioni)
}
