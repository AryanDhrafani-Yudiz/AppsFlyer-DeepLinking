using UnityEngine;
using UnityEngine.UI;
using AppsFlyerSDK;
using System;
using System.Collections.Generic;
using TMPro;
using System.Linq;

/// <summary>
/// Configuration class for AppsFlyer integration settings.
/// Contains all necessary parameters for deep linking and attribution.
/// </summary>
[Serializable]
public class AppsFlyerSettings
{
    [Header("AppsFlyer Basic Config")]
    public string devKey = "";                    // Developer key from AppsFlyer dashboard
    public string appleAppId = "";                // Apple App ID for iOS builds
    public bool isDebug = true;                   // Enable debug logging
    public string outOfStoreSource = "Dropbox";   // Source for non-store installations

    [Header("OneLink URL Config")]
    public string baseOneLinkUrl = "";            // Base URL for OneLink deep links
    public string mediaSource = "";               // Source of the traffic (e.g., "Facebook")
    public string campaignName = "";              // Name of the marketing campaign
    public string fallbackUrl = "";               // Fallback URL if deep linking fails
    public string oneLinkDomain = "";             // Custom domain for OneLink

    [Header("Deep Link Parameters")]
    public string deepLinkValueParam = "joinroomcode";      // Parameter for join room action
    public string deepLinkSubParam = "deep_link_sub1";      // Sub-parameter for additional data
    public string customRoomCodeParam = "myroomcode";       // Custom parameter for room code
    public string experienceParam = "af_xp";                // Experience parameter name
    public string experienceValue = "custom";               // Experience value identifier
    public string fallbackScheme = "deeplinkwithoutgoogleplay";  // URL scheme for fallback
    public string fallbackHost = "open";                    // Host for fallback URL
}

/// <summary>
/// Data structure to hold deep link information including room code and additional parameters.
/// </summary>
public class DeepLinkData
{
    public string roomCode;
    public Dictionary<string, string> parameters;

    public DeepLinkData()
    {
        parameters = new Dictionary<string, string>();
    }
}

/// <summary>
/// Manages deep linking functionality using AppsFlyer SDK.
/// Handles room code sharing and deep link processing for multiplayer game rooms.
/// </summary>
public class DeepLinkManager : MonoBehaviour, IAppsFlyerConversionData
{
    [Header("Configuration")]
    [SerializeField] AppsFlyerSettings settings;

    [Header("UI References")]
    [SerializeField] Button shareButton;              // Button to trigger room sharing
    [SerializeField] TMP_Text generatedRoomCode;      // Display for generated room code
    [SerializeField] TMP_Text roomCodeOnDeepLinking;  // Display for received room code

    private DeepLinkData deepLinkData;

    /// <summary>
    /// Initializes the deep link data and AppsFlyer SDK on Awake.
    /// </summary>
    void Awake()
    {
        deepLinkData = new DeepLinkData();
        InitializeAppsFlyer();
    }

    /// <summary>
    /// Sets up initial AppsFlyer configuration including out-of-store source and OneLink domain.
    /// </summary>
    void InitializeAppsFlyer()
    {
        AppsFlyer.setOutOfStore(settings.outOfStoreSource);
        AppsFlyer.setOneLinkCustomDomain(new string[] { settings.oneLinkDomain });
    }

    /// <summary>
    /// Validates configuration and sets up AppsFlyer SDK and UI elements.
    /// </summary>
    void Start()
    {
        if (ValidateConfiguration())
        {
            SetupAppsFlyer();
            SetupUI();
        }
    }

    /// <summary>
    /// Validates that required configuration parameters are set.
    /// </summary>
    /// <returns>True if configuration is valid, false otherwise.</returns>
    bool ValidateConfiguration()
    {
        if (string.IsNullOrEmpty(settings.devKey))
        {
            Debug.LogError("[DeepLinkManager] Dev Key not set in Inspector!");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Initializes AppsFlyer SDK with configuration parameters and starts tracking.
    /// </summary>
    void SetupAppsFlyer()
    {
        AppsFlyer.initSDK(settings.devKey, settings.appleAppId, this);
        AppsFlyer.setIsDebug(settings.isDebug);
        AppsFlyer.setResolveDeepLinkURLs(new string[] { settings.oneLinkDomain });
        AppsFlyer.startSDK();
        Debug.Log("[DeepLinkManager] AppsFlyer SDK initialized");
    }

    /// <summary>
    /// Sets up UI elements and their event listeners.
    /// </summary>
    void SetupUI()
    {
        if (shareButton != null)
            shareButton.onClick.AddListener(OnShareButtonClicked);

        if (roomCodeOnDeepLinking != null)
            roomCodeOnDeepLinking.text = "";
    }

    /// <summary>
    /// Callback for successful conversion data reception.
    /// Processes deep link data if present and it's the first launch.
    /// </summary>
    /// <param name="conversionData">JSON string containing conversion data</param>
    public void onConversionDataSuccess(string conversionData)
    {
        Debug.Log($"Raw Data: {conversionData}");

        if (conversionData.Contains(settings.deepLinkValueParam) &&
            conversionData.Contains("\"is_first_launch\":true"))
        {
            ParseAndDisplayParameters(conversionData);
        }
        else
        {
            Debug.Log("[DeepLinkManager] No deep link data or not first launch");
        }
    }

    /// <summary>
    /// Parses deep link data and updates UI with room code if present.
    /// </summary>
    /// <param name="data">Raw deep link data string</param>
    void ParseAndDisplayParameters(string data)
    {
        try
        {
            Debug.Log($"[DeepLinkManager] Parsing data: {data}");
            string roomCode = ParseRoomCode(data);

            if (!string.IsNullOrEmpty(roomCode))
            {
                UpdateUIWithRoomCode(roomCode);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[DeepLinkManager] Parse error: {e.Message}\nData: {data}");
        }
    }

    /// <summary>
    /// Extracts room code from deep link data.
    /// </summary>
    /// <param name="data">Raw deep link data</param>
    /// <returns>Room code string if found, null otherwise</returns>
    string ParseRoomCode(string data)
    {
        var parameters = ParseParameters(data);
        return ExtractRoomCode(parameters);
    }

    /// <summary>
    /// Parses deep link data into key-value pairs.
    /// Handles both JSON and URL parameter formats.
    /// </summary>
    /// <param name="data">Raw deep link data</param>
    /// <returns>Dictionary of parsed parameters</returns>
    Dictionary<string, string> ParseParameters(string data)
    {
        var parameters = new Dictionary<string, string>();

        if (data.StartsWith("{")) // JSON format
        {
            ParseJsonFormat(data, parameters);
        }
        else // URL parameter format
        {
            ParseUrlFormat(data, parameters);
        }

        return parameters;
    }

    /// <summary>
    /// Parses JSON formatted deep link data.
    /// </summary>
    /// <param name="data">JSON string</param>
    /// <param name="parameters">Dictionary to store parsed parameters</param>
    void ParseJsonFormat(string data, Dictionary<string, string> parameters)
    {
        data = data.Replace("\\/", "/").Trim('{', '}');
        foreach (string pair in data.Split(','))
        {
            string[] splitPair = pair.Split(':');
            if (splitPair.Length == 2)
            {
                string key = splitPair[0].Trim('"', ' ');
                string value = splitPair[1].Trim('"', ' ');
                if (value.ToLower() != "null")
                {
                    parameters[key] = value;
                }
            }
        }
    }

    /// <summary>
    /// Parses URL parameter formatted deep link data.
    /// </summary>
    /// <param name="data">URL parameter string</param>
    /// <param name="parameters">Dictionary to store parsed parameters</param>
    void ParseUrlFormat(string data, Dictionary<string, string> parameters)
    {
        foreach (string pair in data.Split('&'))
        {
            string[] keyValue = pair.Split('=');
            if (keyValue.Length == 2)
            {
                parameters[Uri.UnescapeDataString(keyValue[0])] =
                    Uri.UnescapeDataString(keyValue[1]);
            }
        }
    }

    /// <summary>
    /// Extracts 6-digit room code from parameters dictionary.
    /// Checks both deep link sub parameter and custom room code parameter.
    /// </summary>
    /// <param name="parameters">Dictionary of parsed parameters</param>
    /// <returns>6-digit room code if found, null otherwise</returns>
    string ExtractRoomCode(Dictionary<string, string> parameters)
    {
        foreach (var param in parameters)
        {
            if (param.Key == settings.deepLinkSubParam ||
                param.Key == settings.customRoomCodeParam)
            {
                string sixDigitCode = new string(
                    param.Value.Where(char.IsDigit).Take(6).ToArray());
                if (sixDigitCode.Length == 6)
                    return sixDigitCode;
            }
        }
        return null;
    }

    /// <summary>
    /// Updates UI with received room code and triggers room code processing.
    /// </summary>
    /// <param name="roomCode">Valid room code</param>
    void UpdateUIWithRoomCode(string roomCode)
    {
        if (roomCodeOnDeepLinking != null)
        {
            roomCodeOnDeepLinking.text = roomCode;
            OnRoomCodeReceived(roomCode);
            Debug.Log($"[DeepLinkManager] Room code set: {roomCode}");
        }
    }

    /// <summary>
    /// Generates a deep link URL with room code and additional parameters.
    /// </summary>
    /// <param name="roomCode">Room code to include in deep link</param>
    /// <returns>Formatted deep link URL</returns>
    string GenerateDeepLink(string roomCode)
    {
        var parameters = new Dictionary<string, string>
        {
            { settings.experienceParam, settings.experienceValue },
            { "pid", settings.mediaSource },
            { "c", settings.campaignName },
            { settings.deepLinkValueParam, settings.deepLinkValueParam },
            { settings.deepLinkSubParam, roomCode },
            { "af_dp", $"{settings.fallbackUrl}?roomcode={roomCode}" },
            { settings.customRoomCodeParam, roomCode },
            { "is_retargeting", "true" }
        };

        string paramString = string.Join("&", parameters.Select(p =>
            $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

        return $"{settings.baseOneLinkUrl}?{paramString}";
    }

    /// <summary>
    /// Callback for failed conversion data reception.
    /// </summary>
    public void onConversionDataFail(string error) =>
        Debug.Log($"[DeepLinkManager] Conversion Data Failed: {error}");

    /// <summary>
    /// Callback for successful app open attribution.
    /// </summary>
    public void onAppOpenAttribution(string attributionData)
    {
        Debug.Log($"[DeepLinkManager] App Open Attribution Data: {attributionData}");
        ParseAndDisplayParameters(attributionData);
    }

    /// <summary>
    /// Callback for failed app open attribution.
    /// </summary>
    public void onAppOpenAttributionFailure(string error) =>
        Debug.Log($"[DeepLinkManager] App Open Attribution Failed: {error}");

    /// <summary>
    /// Handler for share button click. Generates and shares a random room code.
    /// </summary>
    void OnShareButtonClicked() =>
        ShareRoomCode(GenerateRandomRoomCode());

    /// <summary>
    /// Generates a random 6-digit room code.
    /// </summary>
    /// <returns>Random 6-digit room code</returns>
    string GenerateRandomRoomCode() =>
        UnityEngine.Random.Range(100000, 999999).ToString();

    /// <summary>
    /// Updates UI with generated room code and initiates sharing.
    /// </summary>
    /// <param name="roomCode">Generated room code</param>
    void ShareRoomCode(string roomCode)
    {
        if (generatedRoomCode != null)
            generatedRoomCode.text = roomCode;
        ShareLink(GenerateDeepLink(roomCode), roomCode);
    }

    /// <summary>
    /// Shares the room code and deep link using native sharing functionality.
    /// </summary>
    /// <param name="link">Generated deep link URL</param>
    /// <param name="roomCode">Room code to share</param>
    void ShareLink(string link, string roomCode)
    {
        string shareMessage = $"Join my game room with code: {roomCode}\n{link}";
        new NativeShare()
            .SetSubject("Join My Game Room")
            .SetText(shareMessage)
            .Share();
    }

    /// <summary>
    /// Handles received room code. Implement game-specific room joining logic here.
    /// </summary>
    /// <param name="roomCode">Received room code</param>
    void OnRoomCodeReceived(string roomCode)
    {
        Debug.Log($"[DeepLinkManager] Room code received: {roomCode}");
        // Implement your room joining logic here
    }

    /// <summary>
    /// Cleanup method to remove event listeners.
    /// </summary>
    void OnDestroy()
    {
        if (shareButton != null)
            shareButton.onClick.RemoveListener(OnShareButtonClicked);
    }
}