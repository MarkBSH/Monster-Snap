using UnityEngine;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    #region Singleton

    private static SteamManager m_Instance;
    public static SteamManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindFirstObjectByType<SteamManager>();
                if (m_Instance == null)
                {
                    GameObject obj = new("SteamManager");
                    m_Instance = obj.AddComponent<SteamManager>();
                }
            }

            DontDestroyOnLoad(m_Instance);
            return m_Instance;
        }
    }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        try
        {
            if (!Packsize.Test())
            {
                Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run.");
            }

            if (!DllCheck.Test())
            {
                Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
            }

            m_bInitialized = SteamAPI.Init();
            if (!m_bInitialized)
            {
                Debug.LogError("SteamAPI_Init() failed. Steam must be running, and the game must be launched through Steam.");
            }
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. " + e);
        }
    }

    private void Update()
    {
        if (m_bInitialized)
        {
            SteamAPI.RunCallbacks();
        }
    }

    private void OnDestroy()
    {
        if (!m_bInitialized) return;
        SteamAPI.Shutdown();
    }

    #endregion

    #region Steam Initialization

    [Header("Steam Initialization"), Space(5)]
    private static bool s_EverInitialized = false;
    public static bool Initialized => m_Instance != null && m_Instance.m_bInitialized;
    private bool m_bInitialized = false;

    #endregion
}
