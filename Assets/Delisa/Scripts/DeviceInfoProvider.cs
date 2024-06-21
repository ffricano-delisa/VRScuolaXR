using System;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;


public static class DeviceInfoProvider
{

    // VARIABLES
    // ********************************************************************************************

    private static readonly string _logPrefix = "DeviceInfoProvider";
    private static JObject deviceInfoObj = new();


    // FUNCIONS ("deviceInfoObject")
    // ********************************************************************************************

    // Verifica l'esistenza e la valorizzazione del campo "fieldName" all'interno dell'oggetto "deviceInfoObj"
    public static bool IsDeviceInfoObjFieldSet(string fieldName)
    {
        return !string.IsNullOrEmpty(fieldName) && deviceInfoObj.ContainsKey(fieldName);
    }


    // Ritorna il valore del campo "fieldName" (se presente) all'interno dell'oggetto "deviceInfoObj"
    public static string GetDeviceInfoObjField(string fieldName)
    {
        string logPrefix = _logPrefix + " => GetDeviceInfoObjField";
        if (IsDeviceInfoObjFieldSet(fieldName))
        {
            string fieldValue = deviceInfoObj[fieldName].ToString();
            PrintDebug.Log(logPrefix, "Valore recuperato: \"" + fieldName + "\"= \"" + fieldValue + "\"", Color.cyan);
            return fieldValue;
        }
        else
        {
            PrintDebug.LogWarning(logPrefix, $"Errore durante il recupero della info. Il campo '{fieldName}' non è presente nell'oggetto \"deviceInfoObj\".");
            return null;
        }
    }


    // Aggiunge o aggiorna (se già esiste) una coppia chiave-valore all'interno dell'oggetto "deviceInfoObj"
    public static void SetOrUpdateDeviceInfoObjField(string fieldName, string fieldValue)
    {
        string logPrefix = _logPrefix + " => SetOrUpdateDeviceInfoObjField";
        if (!string.IsNullOrEmpty(fieldName) && fieldValue != null)
        {
            string currentValue;
            bool isNewValue;
            if (IsDeviceInfoObjFieldSet(fieldName))
            {
                currentValue = deviceInfoObj[fieldName].ToString();
                isNewValue = currentValue != fieldValue;
                if (isNewValue)
                {
                    deviceInfoObj[fieldName] = fieldValue;
                    PrintDebug.Log(logPrefix, $"Aggiornato elemento: \"{fieldName}\" = \"{fieldValue}\"", Color.cyan);
                }
            }
            else
            {
                isNewValue = true;
                deviceInfoObj[fieldName] = fieldValue;
                PrintDebug.Log(logPrefix, $"Aggiunto elemento: \"{fieldName}\" = \"{fieldValue}\"", Color.cyan);
            }
            if (fieldName == "subject_message" && !string.IsNullOrEmpty(fieldValue) && isNewValue)
            {
                // INVOCAZIONE EVENTO LEGATO AL SUBJECT (CAMBIO SCENA)
                CustomEventHandler.InvokeSubjectChange(fieldValue);
            }
        }
        else
        {
            throw new ArgumentException("I campi \"fieldName\" e \"fieldValue\" non possono essere NULL. Il campo \"fieldName\" non può essere vuoto.");
        }
    }


    // Rimuove una coppia chiave-valore dall'oggetto "deviceInfoObj"
    public static void DeleteDeviceInfoObjField(string fieldName)
    {
        string logPrefix = _logPrefix + " => DeleteDeviceInfoObjField";
        if (IsDeviceInfoObjFieldSet(fieldName))
        {
            deviceInfoObj.Remove(fieldName);
        }
        else
        {
            PrintDebug.LogWarning(logPrefix, $"Errore durante l'eliminazione della info. Il campo '{fieldName}' non è presente nell'oggetto \"deviceInfoJsonObj\".");
        }
    }


    // Resetta l'oggetto "deviceInfoObj"
    public static void ResetDeviceInfoObj()
    {
        deviceInfoObj = new JObject();
    }


    // Ritorna l'oggetto "deviceInfoObj"
    public static JObject GetDeviceInfoObj()
    {
        return deviceInfoObj;
    }


    // FUNCIONS (Info Values)
    // ********************************************************************************************


    // Ritorna il valore associato al campo "fieldName"
    public static string GetInfoValue(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName))
        {
            throw new ArgumentException("Il campo \"fieldName\" non può essere vuoto o NULL.");
        }
        string fieldValue;
        // CAMPI RECUPERATI PRECENDENTEMENTE E MEMORIZZATI NEL "deviceInfoObj"
        if (IsDeviceInfoObjFieldSet(fieldName))
        {
            fieldValue = GetDeviceInfoObjField(fieldName);
        }
        // CAMPI DA RECUPERARE
        else {
            switch (fieldName)
            {
                // VALORI COSTANTI, CALCOLATI O RECUPERATI DAL SISTEMA
                case "Content-Type":
                    fieldValue = GetHeaderContentTypeWwwForm();
                    break;
                case "Authorization":
                    fieldValue = GetValueOfBearerToken();
                    break;
                case "macAddress":
                    fieldValue = GetValueOfDeviceMacAddress();
                    break;
                case "batteryLevel":
                    fieldValue = GetValueOfDeviceBatteryLevel();
                    break;
                case "grant_type":
                    fieldValue = GetValueOfUserGrantType();
                    break;
                case "client_id":
                    fieldValue = GetValueOfUserClientId();
                    break;
                case "password":
                    fieldValue = GetValueOfUserPassword();
                    break;
                case "classe":
                    fieldValue = GetValueOfClass();
                    break;
                case "sezione":
                    fieldValue = GetValueOfSection();
                    break;
                default:
                    fieldValue = "";
                    PrintDebug.LogWarning(_logPrefix, "Campo non valido: " + fieldName, Color.red);
                    // throw new ArgumentException("Campo non valido: " + fieldName);
                    break;
            }
        }
        return fieldValue;
    }


    // Restituisce il Content-Type (WwwForm)
    private static string GetHeaderContentTypeWwwForm()
    {
        return GlobalConfigurationValues.WebRequestClientSettings.CONTENT_TYPE_WWW_FORM;
    }


    // Restituisce il MAC Address del dispositivo
    private static string GetValueOfDeviceMacAddress()
    {
        string macAddress = "N/A";
        string macAddressSimplePattern = @"^[0-9A-Fa-f]{12}$";
        string macAddressFullPattern = @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$";

        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context");
            string wifiService = contextClass.GetStatic<string>("WIFI_SERVICE");
            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject wifiManager = activity.Call<AndroidJavaObject>("getSystemService", wifiService);
            AndroidJavaObject wifiInfo = wifiManager.Call<AndroidJavaObject>("getConnectionInfo");
            macAddress = wifiInfo.Call<string>("getMacAddress");
        }
        else
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in nics)
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    macAddress = adapter.GetPhysicalAddress().ToString();

                }
            }
        };
        if (Regex.IsMatch(macAddress, macAddressFullPattern))
        {
            return macAddress;
        }
        else if (Regex.IsMatch(macAddress, macAddressSimplePattern))
        {
            return Regex.Replace(macAddress, "(.{2})", "$1:").TrimEnd(':');
        }
        else
        {
            throw new ArgumentException("Valore non valido per il campo \"macAddress\": " + macAddress);
        }
    }


    // Restituisce il livello di carica della batteria del dispositivo
    private static string GetValueOfDeviceBatteryLevel()
    {
        float batteryLevel = SystemInfo.batteryLevel * 100;
        if (SystemInfo.batteryStatus == BatteryStatus.Charging) batteryLevel *= -1;
        return batteryLevel.ToString();
    }


    // Restituisce il grant_type
    private static string GetValueOfUserGrantType() {
        return GlobalConfigurationValues.WebRequestClientSettings.GRANT_TYPE;
    }


    // Restituisce il client_id
    private static string GetValueOfUserClientId()
    {
        return GlobalConfigurationValues.WebRequestClientSettings.CLIENT_ID;
    }


    // Restituisce la password associata all'utente
    private static string GetValueOfUserPassword()
    {
        string username = GetDeviceInfoObjField("username");
        string pattern = @"^\d+-[a-zA-Z]-[a-zA-Z]+-[a-zA-Z]+$";
        if (!string.IsNullOrEmpty(username))
        {
            // Verifica se la stringa di input corrisponde al pattern regex
            if (Regex.IsMatch(username, pattern))
            {
                string[] nameSurname = username.Split("-").Skip(2).ToArray();
                string password = string.Join("-", nameSurname);
                return password;
            }
            else
            {
                throw new ArgumentException("Valore non valido per il campo \"username\": " + username);
            }
        }
        else
        {
            throw new ArgumentException("Lo username non può essere NULL.");
        }
    }


    // Restituisce Il Bearer token associato all'utente
    private static string GetValueOfBearerToken()
    {
        return "Bearer " + GetDeviceInfoObjField("access_token");
    }


    // Restituisce lo stato di carica della batteria del dispositivo
    private static string GetValueOfDeviceBatteryStatus()
    {
        // Questo è un esempio; l'implementazione reale dipenderà dalle API specifiche dell'Oculus
        switch (SystemInfo.batteryStatus)
        {
            case BatteryStatus.Charging:
                return "In carica";
            case BatteryStatus.Discharging:
            case BatteryStatus.NotCharging:
                return "Non in carica";
            case BatteryStatus.Full:
                return "Carica completa";
            default:
                return "Sconosciuto";
        }
    }


    // Restituisce la classe corrente
    private static string GetValueOfClass() {
        string username = GetDeviceInfoObjField("username");
        string pattern = @"^\d+-[a-zA-Z]-[a-zA-Z]+-[a-zA-Z]+$";
        if (!string.IsNullOrEmpty(username))
        {
            // Verifica se la stringa di input corrisponde al pattern regex
            if (Regex.IsMatch(username, pattern))
            {
                return username.Split("-")[0];
            }
            else
            {
                throw new ArgumentException("Valore non valido per il campo \"username\": " + username);
            }
        }
        else
        {
            throw new ArgumentException("Lo username non può essere NULL.");
        }
    }


    // Restituisce la sezione corrente
    private static string GetValueOfSection() {
        string username = GetDeviceInfoObjField("username");
        string pattern = @"^\d+-[a-zA-Z]-[a-zA-Z]+-[a-zA-Z]+$";
        if (!string.IsNullOrEmpty(username))
        {
            // Verifica se la stringa di input corrisponde al pattern regex
            if (Regex.IsMatch(username, pattern))
            {
                return username.Split("-")[1];
            }
            else
            {
                throw new ArgumentException("Valore non valido per il campo \"username\": " + username);
            }
        }
        else
        {
            throw new ArgumentException("Lo username non può essere NULL.");
        }
    }


    // Restituisce l'ID univoco del dispositivo (md5 in caso di dispositivi Android)
    private static string GetValueOfDeviceUniqueIdentifier()
    {
        return SystemInfo.deviceUniqueIdentifier;
    }


    // Restituisce il nome del dispositivo
    private static string GetValueOfDeviceName()
    {
        return SystemInfo.deviceName;
    }


    // Restituisce il nome prodotto del dispositivo
    private static string GetvalueOfDeviceProductName()
    {
        return OVRPlugin.productName;
    }


    // Restituisce il modello del dispositivo
    private static string GetValueOfDeviceModel()
    {
        return SystemInfo.deviceModel;
    }


    // Restituisce il tipo di headset (es: Oculus_Quest_2)
    private static string GetValueOfDeviceHeadsetType()
    {
        return OVRPlugin.GetSystemHeadsetType().ToString();
    }

}
