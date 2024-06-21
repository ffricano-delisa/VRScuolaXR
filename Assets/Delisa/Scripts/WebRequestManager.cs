using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ALIAS
using CustomSettings = GlobalConfigurationValues.WebRequestClientSettings;
using DeviceInfoMapper = InfoMapper.WebRequestInfoNameToDeviceInfoName;
using EndpointMapper = InfoMapper.WebRequestEndpointToWebRequestObj;


public class WebRequestManager : MonoBehaviour
{

    // VARIABLES
    // ********************************************************************************************

    private static readonly string _logPrefix = "WebRequestManager ";
    private static readonly int _pollingDelayTime = CustomSettings.POLLING_COROUTINE_DELAY_TIME;
    private static readonly int _workflowDelayTime = CustomSettings.WORKFLOW_COROUTINE_DELAY_TIME;
    private static List<WorkflowRequest> _workflowRequestList;


    // UNITY LIFECYCLE
    // ********************************************************************************************

    void Awake()
    {
        InitWorkflowRequestList();
    }


    void Start()
    {
        // Polling Coroutine su singolo endpoint
        DeviceInfoProvider.SetOrUpdateDeviceInfoObjField("POLLING_ITERATION_DELAY_TIME", _pollingDelayTime.ToString() + " secondi");
        StartCoroutine(PollingCoroutine(CustomSettings.Endpoints.username, _pollingDelayTime));
        // Workflow Coroutine su una lista di endpoints, con polling abilitato
        DeviceInfoProvider.SetOrUpdateDeviceInfoObjField("WORKFLOW_ITERATION_DELAY_TIME", _workflowDelayTime.ToString() + " secondi");
        StartCoroutine(WorkflowCoroutine(GlobalConfigurationValues.WebRequestManagerSettings.baseWorkflowRequestList, _workflowDelayTime, true));
    }

    void Update()
    {
        // DEBUG
        // PrintWorkflowRequestList();
    }


    // FUNCIONS
    // ********************************************************************************************

    // Inizializza la lista di richieste da gestire, recuperando gli endpoint configurati dal relativo Enum
    // Lo stato di tutte le possibili richieste viene impostato di default a "PENDING"
    private void InitWorkflowRequestList()
    {
        if (_workflowRequestList == null)
        {
            _workflowRequestList = new List<WorkflowRequest>();
            // Recupera tutti gli enpoint configurati e, per ciascuno di essi, aggiunge un nuovo elemento alla lista
            Array endpointsEnum = Enum.GetValues(typeof(WebRequestEndpoints));
            if (endpointsEnum != null && endpointsEnum.Length > 0)
            {
                foreach (WebRequestEndpoints endpoint in endpointsEnum)
                {
                    WebRequestObj webRequestObj = EndpointMapper.GetMappedWebRequestObj(endpoint);
                    _workflowRequestList.Add(new WorkflowRequest(endpoint, WorkflowRequestState.PENDING, GetStartCheckFunction(webRequestObj)));
                }
            }
        }
    }


    // Esegue iterativamente la WebRequestCoroutine relativa al WebRequestObj passato come argomento
    // La richiesta viene eseguita intervalli di "delayTime" secondi, in seguito alla verifica della "currentStartCondition()()" (valutata in fase di runtime)
    public IEnumerator PollingCoroutine(WebRequestObj webRequestObj, int delayTime = 0)
    {
        Func<Func<bool>> currentStartCondition = GetStartCheckFunction(webRequestObj);
        int iterationCounter = 0;
        while (true)
        {
            iterationCounter++;
            DeviceInfoProvider.SetOrUpdateDeviceInfoObjField("POLLING_ITERATION_COUNTER", iterationCounter.ToString());
            string logPrefix = _logPrefix + "=> PollingCoroutine (endpoint: \"" + webRequestObj.endpointName + "\"; iterationCounter: " + iterationCounter + ")";
            if (currentStartCondition()())
            {
                PrintDebug.Log(logPrefix, "START", Color.white);
                IEnumerator currentWebRequestCoroutine = BuildWebRequestCoroutine(webRequestObj);
                yield return StartCoroutine(currentWebRequestCoroutine);
                PrintDebug.Log(logPrefix, "END", Color.white);
            }
            PrintDebug.Log(logPrefix, "Avvio prossima iterazione tra " + delayTime + "secondi...", Color.white);
            yield return new WaitForSeconds(delayTime);
        }
    }


    // Esegue in sequenza la lista di WebRequestCoroutine, corrispondenti alla lista di WebRequestObj passata come argomento
    // Ogni richiesta viene eseguita intervalli di "delayTime" secondi, in seguito alla verifica della "currentStartCondition()()" (valutata in fase di runtime)
    // Per ogni sub-coroutine in lista, se la "currentStartCondition()()" non è verificata, viene riavviata l'intera sequenza
    // Se "isPollingCoroutine" è vera, la sequenza verrà riavviata anche in caso di completamento
    private IEnumerator WorkflowCoroutine(List<WebRequestObj> webRequestObjList, int delayTime = 0, bool isPollingCoroutine = false)
    {
        int iterationCounter = 0;
        int restartCounter = -1;
        bool isCompleted = false;
        DeviceInfoProvider.SetOrUpdateDeviceInfoObjField("WORKFLOW_ENDPOINTS_NUMBER", webRequestObjList.Count.ToString());
        while (!isCompleted)
        {
            restartCounter++;
            foreach (WebRequestObj webRequestObj in webRequestObjList)
            {
                iterationCounter++;
                DeviceInfoProvider.SetOrUpdateDeviceInfoObjField("WORKFLOW_ITERATION_COUNTER", iterationCounter.ToString());
                DeviceInfoProvider.SetOrUpdateDeviceInfoObjField("WORKFLOW_RESTART_COUNTER", restartCounter.ToString());
                string logPrefix = _logPrefix + " => WorkflowCoroutine (endpoint: \"" + webRequestObj.endpointName + "\"; iterationCounter: " + iterationCounter + ")";
                Func<Func<bool>> currentStartCondition = GetStartCheckFunction(webRequestObj);
                if (currentStartCondition()())
                {
                    PrintDebug.Log(logPrefix, "START", Color.red);
                    // Nel caso di endpoint = "token", se ho già un token passo alla chiamata successiva
                    if (webRequestObj.endpointName == WebRequestEndpoints.token && !string.IsNullOrEmpty(DeviceInfoMapper.GetMappedValue(WebRequestInfo.access_token, false)))
                    {
                        PrintDebug.Log(logPrefix, "Access Token già presente in locale. la chiamata non verrà eseguita", Color.red);
                    }
                    // Tutti gli altri casi
                    else
                    {
                        // COSTRUZIONE E AVVIO COROUTINE
                        IEnumerator currentWebRequestCoroutine = BuildWebRequestCoroutine(webRequestObj);
                        yield return StartCoroutine(currentWebRequestCoroutine);
                        PrintDebug.Log(logPrefix, "END", Color.red);
                    }
                }
                else
                {
                    PrintDebug.Log(logPrefix, "Coroutine non avviata, condizione di avvio non verificata.", Color.red);
                    PrintDebug.Log(_logPrefix + " => WorkflowCoroutine", "RIAVVIO DELLA SEQUENZA TRA " + delayTime + " SECONDI...", Color.red);
                    yield return new WaitForSeconds(delayTime);
                    iterationCounter = 0;
                    restartCounter++;
                    break; // Esci dal foreach
                };
                if (webRequestObjList.IndexOf(webRequestObj) < webRequestObjList.Count - 1)
                {
                    PrintDebug.Log(logPrefix, "Avvio prossima coroutine tra " + delayTime + " secondi...", Color.red);
                    yield return new WaitForSeconds(delayTime);
                }
                else
                {
                    PrintDebug.Log(logPrefix, "SEQUENZA COMPLETATA CON SUCCESSO!", Color.red);
                    if (!isPollingCoroutine)
                    {
                        isCompleted = true;
                    }
                    else
                    {
                        iterationCounter = 0;
                        restartCounter++;
                        PrintDebug.Log(_logPrefix + " => WorkflowCoroutine", "RIAVVIO DELLA SEQUENZA TRA " + delayTime + " SECONDI...", Color.red);
                        yield return new WaitForSeconds(delayTime);
                    }
                };
            }
        }
    }


    // Restituisce una funzione che verifica l'esistenza in locale di tutti i dati necessari ad avviare la WebRequestCoroutine, relativa al webRequestObj passato come argomento
    // Quando tale funzione viene eseguita (in fase di runtime): se tutti i dati risultano presenti restituisce "true", altrimenti restituisce "false" e vengono stampate nei log le info mancanti
    public Func<Func<bool>> GetStartCheckFunction(WebRequestObj webRequestObj)
    {
        string logPrefix = _logPrefix + " => GetStartCheckFunction";
        return () =>
        {
            bool previousRequestConditions = false;
            bool deviceInfoConditions = false;
            switch (webRequestObj.endpointName)
            {
                case WebRequestEndpoints.username:
                    previousRequestConditions = true;
                    deviceInfoConditions = true;
                    break;
                case WebRequestEndpoints.token:
                    previousRequestConditions = GetWorkflowRequestState(WebRequestEndpoints.username) == WorkflowRequestState.SUCCESS;
                    deviceInfoConditions = DeviceInfoMapper.GetMappedValue(WebRequestInfo.state, false) == "ok";
                    break;
                case WebRequestEndpoints.userinfo:
                case WebRequestEndpoints.subject:
                case WebRequestEndpoints.arguments:
                    previousRequestConditions = GetWorkflowRequestState(WebRequestEndpoints.token) == WorkflowRequestState.SUCCESS;
                    deviceInfoConditions = CheckAllInfoExistance(webRequestObj);
                    break;
            }
            return () => previousRequestConditions && deviceInfoConditions;
        };


        bool CheckAllInfoExistance(WebRequestObj webRequestObj)
        {
            // Recupero di tutti i dati necessari ad eseguire la chiamata
            WebRequestInfo[] requestHeaders = webRequestObj.requestHeaders;
            WebRequestInfo[] urlParams = webRequestObj.urlParams;
            WebRequestInfo[] bodyRequestFields = webRequestObj.bodyRequestFields;

            // Unione dei dati recuperati in un unico array
            List<WebRequestInfo> infoToCheckList = new();
            if (requestHeaders != null)
            {
                infoToCheckList.AddRange(requestHeaders);
            }
            if (urlParams != null)
            {
                infoToCheckList.AddRange(urlParams);
            }
            if (bodyRequestFields != null)
            {
                infoToCheckList.AddRange(bodyRequestFields);
            }
            WebRequestInfo[] infoToCheck = infoToCheckList.Distinct().ToArray();
            // Controllo esistenza/reperibilità dei dati:
            bool missingDataDetected = false;
            List<string> missingDataNameList = new();
            if (infoToCheck != null && infoToCheck.Length > 0)
            {
                for (int i = 0; i < infoToCheck.Length; i++)
                {
                    if (string.IsNullOrEmpty(DeviceInfoMapper.GetMappedValue(infoToCheck[i])))
                    {
                        missingDataNameList.Add(infoToCheck[i].ToString());
                        PrintDebug.Log(logPrefix, "@#@#@ " + infoToCheck[i].ToString(), Color.black);
                        missingDataDetected = true;
                    }
                }
            }
            if (missingDataDetected)
            {
                PrintDebug.Log(logPrefix, "Condizione di avvio non verificata. Info mancanti: " + missingDataNameList, Color.black);
            }
            return !missingDataDetected;
        }
    }


    // Restituisce lo stato della richiesta relativa all'endpoint passato come argomento
    public WorkflowRequestState GetWorkflowRequestState(WebRequestEndpoints endpoint)
    {
        WorkflowRequest currentRequest = _workflowRequestList.Find(request => request.endpoint == endpoint);
        return currentRequest.requestState;
    }


    // Imposta lo stato della richiesta relativa all'endpoint passato come argomento
    public void SetWorkflowRequestState(WebRequestEndpoints endpoint, WorkflowRequestState stateValue)
    {
        string logPrefix = _logPrefix + " => SetWorkflowRequestState";
        int currentRequestIndex = _workflowRequestList.FindIndex(request => request.endpoint == endpoint);
        if (currentRequestIndex != -1)
        {
            WorkflowRequest currentRequest = _workflowRequestList[currentRequestIndex];
            currentRequest.requestState = stateValue;
            _workflowRequestList[currentRequestIndex] = currentRequest;
            PrintDebug.Log(logPrefix, "Stato della richiesta aggiornato. endpoint: \"" + _workflowRequestList[currentRequestIndex].endpoint + "\"; requestState: " + _workflowRequestList[currentRequestIndex].requestState, Color.white);
        }
    }


    // Costruisce e ritorna una Web Request Coroutine e le relative funzioni di callback, a partire da un "WebRequestObj"
    private IEnumerator BuildWebRequestCoroutine(WebRequestObj webRequestObj)
    {
        return WebRequestClient.SendRequestCoroutine(
            webRequestObj,
            // SUCCESS CALLBACK
            (request, responseSuccess) =>
            {
                return HandleResponseSuccess(webRequestObj, request, responseSuccess, true);
            },
            // ERROR CALLBACK
            (request, responseError) =>
            {
                return HandleResponseError(request, responseError, true);
            }
        );
    }


    // Success Callback per il ritorno e la gestione delle response di tipo "Success"
    private string HandleResponseSuccess(WebRequestObj webRequestObj, UnityWebRequest request, string responseSuccess, bool updateDeviceInfoObj = true)
    {
        string logPrefix = _logPrefix + " => HandleResponseSuccess";
        PrintDebug.Log(logPrefix, "START", Color.magenta);
        PrintDebug.Log(logPrefix, "responseSuccess: " + responseSuccess, Color.green);
        try
        {
            JObject responseJsonObj = JObject.Parse(responseSuccess);
            if (updateDeviceInfoObj)
            {
                // MEMORIZZAZIONE INFO
                UpdateResponseDeviceInfo(webRequestObj, responseJsonObj);
            }
            // GESTIONE WORKFLOW
            UpdateResponseWorkflowInfo(webRequestObj, responseJsonObj);
        }
        catch (JsonReaderException exception)
        {
            PrintDebug.LogError(logPrefix, "Errore durante la gestione della response: " + exception.Message);
            throw exception;
        }
        finally
        {
            UpdateWorkflowRequestState(webRequestObj, request);
            PrintDebug.Log(logPrefix, "END", Color.magenta);
        }
        return responseSuccess;
    }


    // Error Callback per il ritorno e la gestione delle response di tipo diverso da "Success"
    private string HandleResponseError(UnityWebRequest request, string responseError, bool updateDeviceInfoObj = true)
    {
        string logPrefix = _logPrefix + " => HandleResponseError";
        PrintDebug.Log(logPrefix, "START.", Color.magenta);
        PrintDebug.Log(logPrefix, "requestError: " + request.error + "; responseError: " + responseError, Color.green);
        JObject responseErrorObj = new();
        if (request.error != null)
        {
            responseErrorObj["requestError"] = request.error;
            if (updateDeviceInfoObj)
            {
                DeviceInfoProvider.SetOrUpdateDeviceInfoObjField("requestError", request.error);
            }
        }
        if (responseError != null)
        {
            responseErrorObj["responseError"] = responseError;
            if (updateDeviceInfoObj)
            {
                DeviceInfoProvider.SetOrUpdateDeviceInfoObjField("responseError", responseError);
            }
        }
        string responseErrorObjString = responseErrorObj.ToString();
        PrintDebug.LogError(logPrefix, "Eccezione durante l'esecuzione della chiamata. responseObjString: " + responseErrorObjString);
        PrintDebug.Log(logPrefix, "Error Callback END.", Color.magenta);
        return responseErrorObjString;
    }


    // Operazioni per la gestione delle info utente/dispositivo presenti nella successResponse, valide per tutti gli endpoints
    // recupera tutti i campi della "responseSuccessJsonObj" censiti in "WebRequestObj.bodyResponseFields" e li inserisce nell'oggetto "deviceInfoObj" della classe "DeviceInfoProvider"
    private void UpdateResponseDeviceInfo(WebRequestObj webRequestObj, JObject responseSuccessJsonObj)
    {
        if (webRequestObj.bodyResponseFields?.Length > 0 && responseSuccessJsonObj.Properties().Any())
        {
            for (int i = 0; i < webRequestObj.bodyResponseFields.Length; i++)
            {
                WebRequestInfo currentField = webRequestObj.bodyResponseFields[i];
                string currentFieldName = currentField.ToString();
                if (responseSuccessJsonObj.ContainsKey(currentFieldName))
                {
                    string currentInfoName = DeviceInfoMapper.GetMappedName(currentField);
                    string currentInfoValue = responseSuccessJsonObj[currentFieldName].ToString();
                    if (currentInfoName == "message")
                    {
                        // Il dato "message" è attualmente presente nelle response di più endpoint
                        // per evitare la sovrascrittura in locale, aggiungo il nome dell'endpoint come prefisso
                        string currentMessageName = webRequestObj.endpointName.ToString() + "_message";
                        DeviceInfoProvider.SetOrUpdateDeviceInfoObjField(currentMessageName, currentInfoValue);
                    }
                    else
                    {
                        DeviceInfoProvider.SetOrUpdateDeviceInfoObjField(currentInfoName, currentInfoValue);
                    }
                }
            }
        }
    }


    // Operazioni per la gestione delle info relative al workflow presenti nella successResponse e relative al particolare endpoint chiamato
    // Memorizza in variabili locali le informazioni relative alla gestione del flusso di lavoro
    private void UpdateResponseWorkflowInfo(WebRequestObj webRequestObj, JObject responseSuccessJsonObj)
    {
        switch (webRequestObj.endpointName)
        {
            case WebRequestEndpoints.username:
                string mappedStateName = DeviceInfoMapper.GetMappedName(WebRequestInfo.state);
                DeviceInfoProvider.SetOrUpdateDeviceInfoObjField(mappedStateName, responseSuccessJsonObj[mappedStateName].ToString());
                // deviceConnectionState = responseSuccessJsonObj[mappedStateName].ToString();
                break;
            // TODO: aggiungere ulteriori endpoint e relative operazioni
            default:
                return;
        }
    }


    // Aggiorna il flag di stato della WorkflowRequest corrispondente a "webRequestObj.endpoint", in base allo statusCode della Response
    private void UpdateWorkflowRequestState(WebRequestObj webRequestObj, UnityWebRequest request)
    {
        string logPrefix = _logPrefix + " => updateWorkflowFlags";
        string regexResponseCodePattern = @"^\d{3}$";
        string regexResponseCodeSuccess = @"^2\d{2}$";
        WebRequestEndpoints currentEndpoint = webRequestObj.endpointName;
        long responseCode = request.responseCode;
        string responseCodeString = responseCode.ToString();
        PrintDebug.Log(logPrefix, "endpoint: " + webRequestObj.endpointName + "; responseCode: " + responseCodeString, Color.green);
        WorkflowRequestState currentWorkflowRequestState;
        // VERIFICA GENERICA
        if (Regex.IsMatch(responseCodeString, regexResponseCodePattern))
        {
            // RESPONSE CODE DEL TIPO 2XX
            if (Regex.IsMatch(responseCodeString, regexResponseCodeSuccess))
            {
                currentWorkflowRequestState = WorkflowRequestState.SUCCESS;
            }
            // ALTRI TIPI DI RESPONSE CODE
            else
            {
                currentWorkflowRequestState = WorkflowRequestState.ERROR;
            }
        }
        else
        {
            currentWorkflowRequestState = WorkflowRequestState.EXCEPTION;
            PrintDebug.LogError(logPrefix, "Ottenuto un responseCode non valido. responseCode: " + responseCodeString);
        }
        SetWorkflowRequestState(webRequestObj.endpointName, currentWorkflowRequestState);
    }


    // DEBUG
    public void PrintWorkflowRequestList()
    {
        _workflowRequestList.ForEach(
            (workflowRequest) => PrintDebug.Log(_logPrefix, "endpoint: " + workflowRequest.endpoint + "; requestState: " + workflowRequest.requestState, Color.white)
        );
    }

}
