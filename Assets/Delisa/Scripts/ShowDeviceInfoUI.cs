using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;


public class ShowDeviceInfoUI : MonoBehaviour
{

    // VARIABLES
    // ********************************************************************************************

    public bool showUI = GlobalConfigurationValues.ShowDeviceInfoUISettings.SHOW_DEVICE_INFO_UI;
    public Canvas deviceInfoCanvas;
    public TMP_Text deviceInfoTextField;

    private JObject deviceInfoObj;
    private static readonly int deviceInvoValueMaxLength = GlobalConfigurationValues.ShowDeviceInfoUISettings.DEVICE_INFO_VALUE_MAX_LENGTH;


    // UNITY LIFECYCLE
    // ********************************************************************************************

    void Update()
    {
        deviceInfoObj = DeviceInfoProvider.GetDeviceInfoObj();
        if(showUI) {
            deviceInfoTextField.text = BuildInfoMessage(deviceInfoObj);
            deviceInfoCanvas.enabled = true;
        }
        else {
            deviceInfoCanvas.enabled = false;
        }
    }


    // FUNCIONS
    // ********************************************************************************************

    // Costruisce il messaggio contenente tutte le info del dispositivo
    public string BuildInfoMessage(JObject deviceInfoObj) {
        string message = "";
        foreach (var property in deviceInfoObj.Properties())
        {
            string propertyName = property.Name;
            string propertyValue = property.Value.ToString();
            string showedPropertyValue = (propertyName == "Authorization" || propertyName == "access_token") ? propertyValue.Substring(0, deviceInvoValueMaxLength) + "..." : propertyValue;
            message += propertyName + ": " + showedPropertyValue + "\n";
        }
        return message;
    }

}
