using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FolderManager : MonoBehaviour
{
    #region Singleton

    private static FolderManager m_Instance;
    public static FolderManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindFirstObjectByType<FolderManager>();
                if (m_Instance == null)
                {
                    GameObject obj = new("FolderManager");
                    m_Instance = obj.AddComponent<FolderManager>();
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
        // Setting up the paths for the folders
        m_BasePath = Path.GetDirectoryName(Application.dataPath) + "/Storage";
        m_PicturesPath = m_BasePath + "/Pictures";
        m_SavedPicturesPath = m_PicturesPath + "/Saved";
        m_SavedJsonPath = m_SavedPicturesPath + "/Json";
        m_CurrentPicturePath = m_PicturesPath + "/Current";
        m_CurrentJsonPath = m_CurrentPicturePath + "/Json";

        CreateFolders();

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#else
        Application.quitting += OnApplicationQuitHandler;
#endif
    }

    /// <summary>
    /// If in the editor all the folders will be cleared when exiting play mode.
    /// If in a build, only the current pictures will be deleted when the application quits.
    /// why only the current pictures? Because the saved pictures and JSON files are meant to persist across sessions.
    /// This is to prevent cluttering the storage with temporary files.
    /// </summary>
#if UNITY_EDITOR
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            ClearFolders();
        }
    }
#else
    private static void OnApplicationQuitHandler()
    {
        DeleteCurrentOnly();
    }
#endif

    #endregion

    #region Paths

    [Header("Paths"), Space(5)]
    public static string m_BasePath;
    public static string m_PicturesPath;
    public static string m_SavedPicturesPath;
    public static string m_SavedJsonPath;
    public static string m_CurrentPicturePath;
    public static string m_CurrentJsonPath;

    private void CreateFolders()
    {
        // Create the necessary directories if they do not exist
        if (!Directory.Exists(m_BasePath))
        {
            Directory.CreateDirectory(m_BasePath);
        }
        if (!Directory.Exists(m_PicturesPath))
        {
            Directory.CreateDirectory(m_PicturesPath);
        }
        if (!Directory.Exists(m_SavedPicturesPath))
        {
            Directory.CreateDirectory(m_SavedPicturesPath);
        }
        if (!Directory.Exists(m_SavedJsonPath))
        {
            Directory.CreateDirectory(m_SavedJsonPath);
        }
        if (!Directory.Exists(m_CurrentPicturePath))
        {
            Directory.CreateDirectory(m_CurrentPicturePath);
        }
        if (!Directory.Exists(m_CurrentJsonPath))
        {
            Directory.CreateDirectory(m_CurrentJsonPath);
        }
    }

    private static void ClearFolders()
    {
        Directory.Delete(m_BasePath, true);
    }

    private static void DeleteCurrentOnly()
    {
        string[] files = Directory.GetFiles(m_CurrentPicturePath, "*.png");
        for (int i = 0; i < files.Length; i++)
        {
            File.Delete(files[i]);
        }

        files = Directory.GetFiles(m_CurrentJsonPath, "*.json");
        for (int i = 0; i < files.Length; i++)
        {
            File.Delete(files[i]);
        }
    }

    #endregion
}
