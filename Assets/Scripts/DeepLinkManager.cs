using UnityEngine;
using UnityEngine.UI;
using AppsFlyerSDK;
using System;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class DeepLinkManager : MonoBehaviour, IAppsFlyerConversionData
{
    [Header("AppsFlyer Configuration")]
    [SerializeField] string devKey = "";  // Dev key input field
    [SerializeField] string appleAppId = "";  // Apple App ID input field If iOS
    [SerializeField] bool isDebug = true;  // Debug mode toggle

    [Header("References")]
    [SerializeField] Button shareButton;
    [SerializeField] TMP_Text generatedRoomCode;
    [SerializeField] TMP_Text roomCodeOnDeepLinking;

    [Header("OneLink Configuration")]
    [SerializeField] string baseOneLinkUrl = "https://aryandeeplinking.onelink.me/2giF";
    [SerializeField] string mediaSource = "RummyGame";
    [SerializeField] string campaignName = "roomcode";
    [SerializeField] string fallbackUrl = "deeplinkwithoutgoogleplay://open";

    public class DeepLinkData
    {
        public string roomCode;
        public Dictionary<string, string> parameters;

        public DeepLinkData()
        {
            parameters = new Dictionary<string, string>();
        }
    }

    void Start()
    {
        Debug.Log("[DeepLinkManager] Start called");

        // Validate required fields
        if (string.IsNullOrEmpty(devKey))
        {
            Debug.LogError("[DeepLinkManager] Dev Key not set in Inspector!");
            return;
        }

        if (roomCodeOnDeepLinking == null)
        {
            Debug.LogError("[DeepLinkManager] Room Code Display reference not set in Inspector!");
            return;
        }

        // Initialize AppsFlyer
        AppsFlyer.initSDK(
            devKey,
            appleAppId,  // Will be empty string for Android-only
            this);

        // Set debug mode
        AppsFlyer.setIsDebug(isDebug);

        AppsFlyer.startSDK();

        Debug.Log("[DeepLinkManager] AppsFlyer SDK initialized");

        if (shareButton != null)
        {
            shareButton.onClick.AddListener(OnShareButtonClicked);
            Debug.Log("[DeepLinkManager] Share button listener added");
        }

        // Clear display text on start
        if (roomCodeOnDeepLinking != null)
        {
            roomCodeOnDeepLinking.text = "";
        }
    }

    // Handle Conversion Data on App Starting/Unpausing
    public void onConversionDataSuccess(string conversionData)
    {
        Debug.Log($"Raw Data: {conversionData}");
        ParseAndDisplayParameters(conversionData);
    }

    public void onConversionDataFail(string error)
    {
        Debug.Log($"[DeepLinkManager] Conversion Data Failed: {error}");
    }

    //Handle AppOpenAttribution Data On Deep Linking / Deferred Deep Linking
    public void onAppOpenAttribution(string attributionData)
    {
        Debug.Log("----------------------------------------");
        Debug.Log("[DeepLinkManager] App Open Attribution");
        Debug.Log("----------------------------------------");
        Debug.Log($"Raw Data: {attributionData}");
        ParseAndDisplayParameters(attributionData);
    }

    public void onAppOpenAttributionFailure(string error)
    {
        Debug.Log($"[DeepLinkManager] App Open Attribution Failed: {error}");
    }

    // Parsing and Debug Purposes , Remove Debugs In Dev Builds
    void ParseAndDisplayParameters(string data)
    {
        try
        {
            Debug.Log("[DeepLinkManager] Starting Parameter Parsing");
            Debug.Log("========================================");
            Debug.Log($"Raw Data Received: {data}");
            Debug.Log("----------------------------------------");

            string roomCodeForInput = "";

            // Split the data string by & to get individual parameters
            string[] pairs = data.Split('&');
            DeepLinkData parsedData = new DeepLinkData();

            Debug.Log("Parsing Individual Parameters:");
            foreach (string pair in pairs)
            {
                // Split each pair by = to get key and value
                string[] keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    string key = Uri.UnescapeDataString(keyValue[0]);
                    string value = Uri.UnescapeDataString(keyValue[1]);

                    // Store in dictionary
                    parsedData.parameters[key] = value;

                    // Print each parameter on a new line with clear formatting
                    Debug.Log($"Parameter Found:");
                    Debug.Log($"    Key: {key}");
                    Debug.Log($"    Value: {value}");
                    Debug.Log("----------------");

                    // Special handling for room code
                    if (key == "deep_link_sub1" || key == "myroomcode")
                    {
                        // Extract only the 6 digits if present in the value
                        string sixDigitCode = new string(value.Where(char.IsDigit).Take(6).ToArray());
                        if (sixDigitCode.Length == 6)
                        {
                            roomCodeForInput = sixDigitCode;
                            Debug.Log($"[ROOM CODE] 6-Digit Code Found: {roomCodeForInput}");
                            Debug.Log("----------------");
                        }
                    }
                }
            }

            Debug.Log("Parameter Parsing Complete");
            Debug.Log("----------------------------------------");

            // Update Room Code Display Text with only the 6-digit code if found
            if (!string.IsNullOrEmpty(roomCodeForInput))
            {
                if (roomCodeOnDeepLinking != null)
                {
                    roomCodeOnDeepLinking.text = roomCodeForInput;
                    OnRoomCodeReceived(roomCodeForInput);
                    Debug.Log($"[Room Code Display Text] Updated with room code: {roomCodeForInput}");
                }
                else
                {
                    Debug.LogError("[DeepLinkManager]Room Code Display Text reference is missing!");
                }
            }
            else
            {
                Debug.Log("[ROOM CODE] No valid 6-digit room code found in parameters");
            }
            Debug.Log("========================================");
        }
        catch (Exception e)
        {
            Debug.LogError($"[DeepLinkManager] Error parsing parameters: {e.Message}");
            Debug.LogError($"[DeepLinkManager] Raw data: {data}");
        }
    }

    // On Share Button Clicked
    void OnShareButtonClicked()
    {
        string randomRoomCode = GenerateRandomRoomCode();
        Debug.Log($"[DeepLinkManager] Generated random code: {randomRoomCode}");
        ShareRoomCode(randomRoomCode);
    }

    // Random Code Generation Logic
    string GenerateRandomRoomCode()
    {
        return UnityEngine.Random.Range(100000, 999999).ToString();
    }

    void ShareRoomCode(string roomCode)
    {
        generatedRoomCode.text = roomCode;
        string generatedLink = GenerateDeepLink(roomCode);
        ShareLink(generatedLink, roomCode);
    }

    // Deep Link Generation which will be shared
    string GenerateDeepLink(string roomCode)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>
        {
            {"af_xp", "custom"},
            {"pid", mediaSource},
            {"c", campaignName},
            {"deep_link_value", "joinroomcode"},
            {"deep_link_sub1", roomCode},
            {"af_dp", fallbackUrl},
            {"myroomcode", roomCode}
        };

        string paramString = "";
        foreach (var param in parameters)
        {
            string encodedKey = Uri.EscapeDataString(param.Key);
            string encodedValue = Uri.EscapeDataString(param.Value);
            paramString += $"{encodedKey}={encodedValue}&";
        }

        paramString = paramString.TrimEnd('&');
        string finalUrl = $"{baseOneLinkUrl}?{paramString}";

        Debug.Log($"[DeepLinkManager] Generated deep link: {finalUrl}");
        return finalUrl;
    }

    // Share Link With Native Share
    void ShareLink(string link, string roomCode)
    {
        string shareMessage = $"Join my game room with code: {roomCode}\n{link}";

        new NativeShare()
            .SetSubject("Join My Game Room")
            .SetText(shareMessage)
            .Share();
    }

    void OnRoomCodeReceived(string roomCode)
    {
        Debug.LogError($"[DeepLinkManager] Room code received: {roomCode}");
        // Implement your room joining logic here
    }

    void OnDestroy()
    {
        if (shareButton != null)
        {
            shareButton.onClick.RemoveListener(OnShareButtonClicked);
        }
    }
}