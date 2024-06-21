#nullable enable


// Oggetto contenente tutte le info necessarie per eseguire la richiesta
public struct WebRequestObj
{
  public WebRequestEndpoints endpointName;
  public WebRequestMethods method;
  public string url;
  public WebRequestInfo[]? requestHeaders;            // Lista Headers
  public WebRequestInfo[]? urlParams;                 // Lista campi info da accodare all'URL
  public WebRequestInfo[]? bodyRequestFields;         // Lista campi info da inserire nel bodyRaw
  public WebRequestInfo[]? bodyResponseFields;        // Lista campi info della response da memorizzare in locale
}

// Entpoint configurati
public enum WebRequestEndpoints
{
  username,
  token,
  userinfo,
  subject,
  arguments
  // TODO: Aggiungere altri endpoint se necessario
}

// Metodi abilitati
public enum WebRequestMethods
{
  GET,
  POST,
  PUT,
  // TODO: Aggiungere altri metodi se necessario
}

// Parametri censiti
public enum WebRequestInfo
{
  state,
  content_type,
  authorization,
  macAddress,
  batteryLevel,
  sec,
  username,
  grant_type,
  client_id,
  password,
  access_token,
  name,
  avatar,
  message,
  lab,
  class_number,
  class_section
  // TODO: Aggiungere altri campi se necessario
}
