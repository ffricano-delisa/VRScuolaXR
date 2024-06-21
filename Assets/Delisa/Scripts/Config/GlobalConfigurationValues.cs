using System.Collections.Generic;
using System.IO;
using UnityEngine;


public static class GlobalConfigurationValues
{

    public static readonly bool IS_DEBUG_MODE = true;

    public static class AssetBundlesSetting 
    {
        public static readonly string DEFAULT_ASSET_NAME = "RootEnviroment";
        public static readonly string BUNDLE_TO_IGNORE = "introduzione";
        public static readonly string ASSET_BUNDLES_DIRECTORY_NAME = "AssetBundles";
        public static readonly string ASSET_BUNDLES_STREAMING_PATH = Path.Combine(Application.streamingAssetsPath, ASSET_BUNDLES_DIRECTORY_NAME);
        public static readonly string ASSET_BUNDLES_PERSISTENT_PATH = Path.Combine(Application.persistentDataPath, ASSET_BUNDLES_DIRECTORY_NAME);
        public static readonly string ASSET_BUNDLES_CUSTOM_PATH = Path.Combine("Assets/App", ASSET_BUNDLES_DIRECTORY_NAME);
        public static readonly string ASSET_BUNDLES_CURRENT_PATH = ASSET_BUNDLES_CUSTOM_PATH;
    }

    public static class WebRequestManagerSettings
    {
        public static List<WebRequestObj> baseWorkflowRequestList = new()
        {
            // WebRequestClientSettings.Endpoints.username,
            WebRequestClientSettings.Endpoints.token,
            WebRequestClientSettings.Endpoints.userinfo,
            WebRequestClientSettings.Endpoints.subject,
            WebRequestClientSettings.Endpoints.arguments,
            // TODO: Inserisci ulteriori WebRequestObj se necessario
        };

        // TODO: inserisci ulteriori liste per definire nuove sequenze di chiamate
    };

    public static class WebRequestClientSettings
    {
        public static readonly string GRANT_TYPE = "password";
        public static readonly string CLIENT_ID = "client";
        public static readonly string CONTENT_TYPE_WWW_FORM = "application/x-www-form-urlencoded";
        public static readonly int POLLING_COROUTINE_DELAY_TIME = 5;
        public static readonly int WORKFLOW_COROUTINE_DELAY_TIME = 3;

        // ENDPOINTS ------------------------------------------------------------------------------
        public static class Endpoints
        {
            // public static readonly string BASE_PATH_URL_DEV = "http://11.0.200.101";
            public static readonly string BASE_PATH_URL_DEV = "http://192.168.200.3";
            // public static readonly string BASE_PATH_URL_TEST = "https://vrscuola.delisagroup.it"; // TODO: da eliminare
            public static readonly string BASE_PATH_URL = BASE_PATH_URL_DEV;

            // LISTA DEGLI ENDPOINT -----------------------------------------------------------------------------------
            // NB:
            // - endpointName => Label univoca dell'endpoint
            // - method => Metodo da utilizzare nella request
            // - url => URL da invocare nella request
            // - requestHeaders => Lista degli Headers da inserire nella request
            // - urlParams => Lista dei parametri da accodare come stringa all'URL della request
            // - bodyRequestFields => Lista delle informazioni da inserire nel wwwForm della request
            // - bodyResponseFields => Lista dei campi della response che verranno memorizzati in locale
            // --------------------------------------------------------------------------------------------------------
            // FUNZIONI PER IL RECUPERO E LA MEMORIZZAZIONE DATI => Vedi "infoMapper" e "DeviceInfoProvider"
            // --------------------------------------------------------------------------------------------------------

            public static readonly WebRequestObj username = new()
            {
                endpointName = WebRequestEndpoints.username,
                method = WebRequestMethods.POST,
                url = BASE_PATH_URL + "/connectivity-devices/username",
                requestHeaders = null,
                urlParams = new WebRequestInfo[] {
                    WebRequestInfo.macAddress,
                    WebRequestInfo.batteryLevel
                },
                bodyRequestFields = null,
                bodyResponseFields = new WebRequestInfo[]
                {
                    WebRequestInfo.state,
                    WebRequestInfo.sec,
                    WebRequestInfo.username,
                    WebRequestInfo.avatar,
                    WebRequestInfo.lab
                }
            };

            public static readonly WebRequestObj token = new()
            {
                endpointName = WebRequestEndpoints.token,
                method = WebRequestMethods.POST,
                url = BASE_PATH_URL + ":8080/realms/scuola/protocol/openid-connect/token",
                requestHeaders = new WebRequestInfo[]
                {
                    WebRequestInfo.content_type
                },
                urlParams = null,
                bodyRequestFields = new WebRequestInfo[]
                {
                    WebRequestInfo.sec,
                    WebRequestInfo.username,
                    WebRequestInfo.password,
                    WebRequestInfo.grant_type,
                    WebRequestInfo.client_id
                },
                bodyResponseFields = new WebRequestInfo[]
                {
                    WebRequestInfo.access_token,
                }
            };

            public static readonly WebRequestObj userinfo = new()
            {
                endpointName = WebRequestEndpoints.userinfo,
                method = WebRequestMethods.GET,
                url = BASE_PATH_URL + "/userinfo",
                requestHeaders = new WebRequestInfo[]
                {
                    WebRequestInfo.content_type,
                    WebRequestInfo.authorization
                },
                urlParams = null,
                bodyRequestFields = null,
                bodyResponseFields = new WebRequestInfo[]
                {
                    WebRequestInfo.name
                }
            };

            public static readonly WebRequestObj subject = new()
            {
                endpointName = WebRequestEndpoints.subject,
                method = WebRequestMethods.POST,
                url = BASE_PATH_URL + "/connectivity-devices/subject",
                requestHeaders = new WebRequestInfo[]
                {
                    WebRequestInfo.content_type,
                    WebRequestInfo.authorization
                },
                urlParams = new WebRequestInfo[] {
                    WebRequestInfo.macAddress
                },
                bodyRequestFields = null,
                bodyResponseFields = new WebRequestInfo[]
                {
                    WebRequestInfo.message
                }
            };

            public static readonly WebRequestObj arguments = new()
            {
                endpointName = WebRequestEndpoints.arguments,
                method = WebRequestMethods.GET,
                url = BASE_PATH_URL + "/argomenti/all",
                requestHeaders = null,
                urlParams = new WebRequestInfo[] {
                    WebRequestInfo.lab,
                    WebRequestInfo.class_number,
                    WebRequestInfo.class_section
                },
                bodyRequestFields = null,
                bodyResponseFields = new WebRequestInfo[]
                {
                    WebRequestInfo.message
                }
            };

            // TODO: Aggiungere altri WebRequestObj per configurare ulteriori chiamate
        }
    }
    
    public static class CustomSceneManagerSettings
    {
        public static readonly string START_SCENE_NAME = "HUB";
        public static readonly string EMPTY_SCENE_NAME = "BASE_EMPTY";
        public static readonly string ELEVATOR_SCENE_NAME = "ASCENSORE";

    }

    public static class InfoMapperSettings {

        // Associa ad ogni Enum di tipo WebRequestInfo, il corrispondente nome della proprietà all'interno di DeviceInfoProvide
        public static readonly Dictionary<WebRequestInfo, string> webRequestInfoNameToDeviceInfoNameMapping = new()
        {
            { WebRequestInfo.state, "state" },
            { WebRequestInfo.content_type, "Content-Type" },
            { WebRequestInfo.authorization, "Authorization" },
            { WebRequestInfo.macAddress, "macAddress" },
            { WebRequestInfo.batteryLevel, "batteryLevel" },
            { WebRequestInfo.grant_type, "grant_type" },
            { WebRequestInfo.client_id, "client_id" },
            { WebRequestInfo.sec, "client_secret" },
            { WebRequestInfo.username, "username" },
            { WebRequestInfo.password, "password" },
            { WebRequestInfo.access_token, "access_token" },
            { WebRequestInfo.name, "name" },
            { WebRequestInfo.avatar, "avatar" },
            { WebRequestInfo.message, "message" },
            { WebRequestInfo.lab, "lab" },
            { WebRequestInfo.class_number, "classe" },
            { WebRequestInfo.class_section, "sezione" },
            // TODO: Aggiungere altri mapping se necessario
        };
        
        // Associa ad ogni WebRequestEndpoint il corrispondente WebRequestObj
        public static readonly Dictionary<WebRequestEndpoints, WebRequestObj> webRequestEndpointsToWebRequestObjMapping = new()
        {
            { WebRequestEndpoints.username, WebRequestClientSettings.Endpoints.username },
            { WebRequestEndpoints.token, WebRequestClientSettings.Endpoints.token },
            { WebRequestEndpoints.userinfo, WebRequestClientSettings.Endpoints.userinfo },
            { WebRequestEndpoints.subject, WebRequestClientSettings.Endpoints.subject },
            { WebRequestEndpoints.arguments, WebRequestClientSettings.Endpoints.arguments },
            // TODO: Aggiungere altri mapping se necessario
        };

    }

    public static class ShowDeviceInfoUISettings {
        public static readonly bool SHOW_DEVICE_INFO_UI = true;
        public static readonly int DEVICE_INFO_VALUE_MAX_LENGTH = 50;
    }
    
}