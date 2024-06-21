using System;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Text;

// ALIAS
using Mapper = InfoMapper.WebRequestInfoNameToDeviceInfoName;


public class WebRequestClient : MonoBehaviour
{

    // VARIABLES
    // ********************************************************************************************
    
    private static readonly string _logPrefix = "WebRequestClient";


    // CLASSES
    // ********************************************************************************************

    // Classe per bypassare la verifica dei certificati
    private class BypassCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            // Accetta tutti i certificati (per scopi di sviluppo/testing)
            return true;
        }
    }


    // FUNCIONS
    // ********************************************************************************************

    // Esegue la coroutine
    public void SendRequest(WebRequestObj webRequestObj, Func<UnityWebRequest, string, dynamic> successCallback = null, Func<UnityWebRequest, string, dynamic> errorCallback = null)
    {
        StartCoroutine(SendRequestCoroutine(webRequestObj, successCallback, errorCallback));
    }


    // Coroutine per la costruzione e l'invio di una singola richiesta con funzioni di callback dinamiche
    // Le callbacks operano su un singolo parametro di input (ovvero la "response" di tipo string) e restituiscono un tipo dinamico valutato in fase di chiamata
    public static IEnumerator SendRequestCoroutine(WebRequestObj webRequestObj, Func<UnityWebRequest, string, dynamic> successCallback = null, Func<UnityWebRequest, string, dynamic> errorCallback = null)
    {
        string logPrefix = _logPrefix + " => SendRequestCoroutine";
        PrintDebug.Log(logPrefix, "START, endpoint: " + webRequestObj.endpointName, Color.green);

        // INIZIALIZZAZIONE CALLBACK DI DEFAULT
        successCallback ??= DefaultSuccessResponseCallback;
        errorCallback ??= DefaultErrorResponseCallback;

        // COSTRUZIONE REQUEST
        UnityWebRequest request = BuildRequest(webRequestObj);

        // INVIO REQUEST E GESTIONE RESPONSE
        UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
        try
        {
            string response = request.downloadHandler.text;
            if (request.result == UnityWebRequest.Result.Success)
            {
                successCallback(request, response);
            }
            else
            {
                errorCallback(request, response);
            }
        }
        catch (Exception exception)
        {
            PrintDebug.LogError(logPrefix, "Eccezione durante la gestione della risposta: " + exception.Message);
            throw exception;
        }
        finally
        {
            request.Dispose();
        }
    }


    // Ritorna la Web Request da eseguire
    private static UnityWebRequest BuildRequest(WebRequestObj webRequestObj)
    {
        string logPrefix = _logPrefix + " => BuildRequest";
        PrintDebug.Log(logPrefix, "Costruzione della richiesta, endpoint: " + webRequestObj.endpointName, Color.yellow);
        try
        {
            UnityWebRequest request;
            // COSTRUZIONE URL
            string url = webRequestObj.url + BuildUrlParamString(webRequestObj);
            PrintDebug.Log(logPrefix, "url: " + url, Color.yellow);
            // COSTRUZIONE BODY
            byte[] bodyRaw = BuildBodyRequest(webRequestObj);
            // SCELTA METODO
            switch (webRequestObj.method)
            {
                case WebRequestMethods.GET:
                    request = UnityWebRequest.Get(url);
                    break;
                case WebRequestMethods.POST:
                    request = new UnityWebRequest(url, "POST")
                    {
                        uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw),
                        downloadHandler = (DownloadHandler)new DownloadHandlerBuffer()
                    };
                    break;
                case WebRequestMethods.PUT:
                    request = UnityWebRequest.Put(url, bodyRaw);
                    break;
                // TODO: Aggiungere altri metodi se necessario
                default:
                    request = null;
                    throw new ArgumentException("Metodo HTTP non supportato: " + webRequestObj.method);
            }
            // AGGIUNTA HEADERS
            if (webRequestObj.requestHeaders?.Length > 0)
            {
                for (int i = 0; i < webRequestObj.requestHeaders.Length; i++)
                {
                    WebRequestInfo currentHeader = webRequestObj.requestHeaders[i];
                    string currentHeaderName = Mapper.GetMappedName(currentHeader);
                    string currentHeaderValue = Mapper.GetMappedValue(currentHeader);
                    request.SetRequestHeader(currentHeaderName, currentHeaderValue);
                    PrintDebug.Log(logPrefix, $"Aggiunto header: \"{currentHeaderName}\" = \"{currentHeaderValue}\"", Color.yellow);
                }
            }
            // BYPASS CERTIFICATI
            // request.certificateHandler = new BypassCertificateHandler(); 
            return request;
        }
        catch (Exception exception)
        {
            PrintDebug.LogError(logPrefix, "Eccezione durante la costruzione della richiesta: " + exception.Message);
            throw exception;
        }
    }


    // Costruisce e ritorna la stringa di parametri da concatenare all'URL
    private static string BuildUrlParamString(WebRequestObj webRequestObj)
    {
        if (webRequestObj.urlParams?.Length > 0)
        {
            string urlParameters = "?";
            for (int i = 0; i < webRequestObj.urlParams.Length; i++)
            {
                WebRequestInfo currentParam = webRequestObj.urlParams[i];
                string currentDeviceInfoName = Mapper.GetMappedName(currentParam);
                string currentDeviceInfoValue = Mapper.GetMappedValue(currentParam);
                urlParameters += currentDeviceInfoName + "=" + currentDeviceInfoValue;
                if (i < webRequestObj.urlParams.Length - 1)
                {
                    urlParameters += "&";
                }
            }
            return urlParameters;
        }
        else return "";
    }


    // Costruisce e ritorna il bodyRaw della richiesta
    private static byte[] BuildBodyRequest(WebRequestObj webRequestObj)
    {
        string logPrefix = _logPrefix + " => BuildBodyRequest";
        if (webRequestObj.bodyRequestFields?.Length > 0)
        {
            string bodyString = "";
            for (int i = 0; i < webRequestObj.bodyRequestFields.Length; i++)
            {
                WebRequestInfo currentField = webRequestObj.bodyRequestFields[i];
                string currentDeviceInfoName = Mapper.GetMappedName(currentField);
                string currentDeviceInfoValue = Mapper.GetMappedValue(currentField);
                bodyString += currentDeviceInfoName + "=" + currentDeviceInfoValue;
                if (i < webRequestObj.bodyRequestFields.Length - 1)
                {
                    bodyString += "&";
                }
            }
            PrintDebug.Log(logPrefix, "bodyString: " + bodyString, Color.yellow);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyString);
            return bodyRaw;
        }
        return null;
    }


    // Callback di default che ritorna la Success Response
    private static string DefaultSuccessResponseCallback(UnityWebRequest request, string responseSuccess)
    {
        string logPrefix = _logPrefix + " => DefaultSuccessResponseCallback";
        PrintDebug.Log(logPrefix, "START", Color.magenta);
        PrintDebug.Log(logPrefix, "request: " + request.ToString(), Color.yellow);
        PrintDebug.Log(logPrefix, "responseSuccess: " + responseSuccess, Color.yellow);
        PrintDebug.Log(logPrefix, "END", Color.magenta);
        return responseSuccess;
    }


    // Callback di default che ritorna la Error Response
    private static string DefaultErrorResponseCallback(UnityWebRequest request, string responseError)
    {
        string logPrefix = _logPrefix + " => DefaultErrorResponseCallback";
        PrintDebug.Log(logPrefix, "START", Color.magenta);
        JObject responseObj = new()
        {
            ["requestError"] = request.error,
            ["responseError"] = responseError
        };
        string responseObjString = responseObj.ToString();
        PrintDebug.Log(logPrefix, "responseObjString: " + responseObjString, Color.yellow);
        PrintDebug.Log(logPrefix, "END", Color.magenta);
        return responseObjString;
    }

}
