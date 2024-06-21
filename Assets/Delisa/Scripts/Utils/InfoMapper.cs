using System;
using System.Collections.Generic;


public class InfoMapper
{
    
    public static class WebRequestInfoNameToDeviceInfoName
    {
        // Dizionario associato
        private static readonly Dictionary<WebRequestInfo, string> mappingDictionary = GlobalConfigurationValues.InfoMapperSettings.webRequestInfoNameToDeviceInfoNameMapping;

        // Restituisce il nome della info (corrispondente alla WebRequestInfo) all'interno del DeviceInfoProvider
        public static string GetMappedName(WebRequestInfo webRequestFieldInfo) {
            if (mappingDictionary.ContainsKey(webRequestFieldInfo)) {
                return mappingDictionary[webRequestFieldInfo];
                };
            return null;
        }

        // Restituisce il valore della info (corrispondente alla WebRequestInfo) all'interno del DeviceInfoProvider ed eventualmente aggiorna il "deviceInfoObj"
        public static string GetMappedValue(WebRequestInfo webRequestFieldInfo, bool updateDeviceInfoObj = true) {
            string mappedName = GetMappedName(webRequestFieldInfo);
            if (!string.IsNullOrEmpty(mappedName)) {
                string mappedValue = DeviceInfoProvider.GetInfoValue(mappedName);
                if (updateDeviceInfoObj) {
                    DeviceInfoProvider.SetOrUpdateDeviceInfoObjField(mappedName, mappedValue);
                }
                return mappedValue;
            }
            return null;
        }
    }


    public static class WebRequestEndpointToWebRequestObj 
    {
        // Dizionario associato        
        private static readonly Dictionary<WebRequestEndpoints, WebRequestObj> mappingDictionary = GlobalConfigurationValues.InfoMapperSettings.webRequestEndpointsToWebRequestObjMapping;

        // Restituisce il nome della info (corrispondente alla WebRequestInfo) all'interno del DeviceInfoProvider
        public static WebRequestObj GetMappedWebRequestObj(WebRequestEndpoints webRequestEndpoint) {
            if (mappingDictionary.ContainsKey(webRequestEndpoint)) 
            {
                return mappingDictionary[webRequestEndpoint];
            }
            else
            {
                throw new ArgumentException("Endpoint non valido");
            }
        }     
    }

}
