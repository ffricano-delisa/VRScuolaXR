using UnityEngine;


public static class PrintDebug
{
    private static readonly bool enablePrint = GlobalConfigurationValues.IS_DEBUG_MODE;

    public static void Log(string prefix, string message, Color? textColor = null) {
        if (enablePrint) {
            string logMessage = BuildLogMessage(prefix, message, textColor);
            Debug.Log(logMessage);
        }
    }

    public static void LogWarning(string prefix, string message, Color? textColor = null) {
        if (enablePrint) {
            string logMessage = BuildLogMessage(prefix, message, textColor);
            Debug.LogWarning(logMessage);
        }
    }

    public static void LogError(string prefix, string message, Color? textColor = null) {
        if (enablePrint) {
            string logMessage = BuildLogMessage(prefix, message, textColor);
            Debug.LogError(logMessage);
        }
    }

    private static string BuildLogMessage(string prefix, string message, Color? textColor = null) {
        string fullPrefix = "[" + prefix + "] ";
        string colorTag = textColor != null ? "<color=#" + ColorUtility.ToHtmlStringRGB((Color)textColor) + ">" : "";
        string fullMessage = colorTag + fullPrefix + message + (textColor != null ? "</color>" : "");
        return fullMessage;
    }
    
}
