using UnityEngine;
using AppsFlyerSDK;
using System;

public class GameManager : MonoBehaviour
{
    [SerializeField] AppsFlyerObjectScript appsFlyerObj;
    void Awake()
    {
        Debug.Log("[GameManager] Awake called");
    }

    void Start()
    {
        Debug.Log("[GameManager] Start called");

        // Check if AppsFlyerObject exists in scene
        // var appsFlyerObj = FindObjectOfType<AppsFlyerObjectScript>();
        if (appsFlyerObj != null)
        {
            Debug.Log("[GameManager] AppsFlyerObject found in scene");
            // Subscribe to deep link events
            AppsFlyer.OnDeepLinkReceived += OnDeepLinkReceived;
            Debug.Log("[GameManager] Subscribed to AppsFlyer deep link events");
        }
        else
        {
            Debug.LogError("[GameManager] AppsFlyerObject not found in scene!");
        }
    }

    void OnDeepLinkReceived(object sender, EventArgs args)
    {
        Debug.Log("[GameManager] Deep link received in GameManager");
        try
        {
            if (args != null)
            {
                string deepLinkValue = args.ToString();
                Debug.Log($"[GameManager] Received deep link data: {deepLinkValue}");
                Debug.Log($"[GameManager] Full deep link content: {deepLinkValue}");
            }
            else
            {
                Debug.LogWarning("[GameManager] Deep link args were null");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameManager] Error processing deep link: {e.Message}\nStackTrace: {e.StackTrace}");
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus) // App resuming
        {
            Debug.Log("[GameManager] App resumed - checking for deep links");
        }
    }

    void OnDestroy()
    {
        Debug.Log("[GameManager] OnDestroy called - unsubscribing from deep link events");
        AppsFlyer.OnDeepLinkReceived -= OnDeepLinkReceived;
    }
}