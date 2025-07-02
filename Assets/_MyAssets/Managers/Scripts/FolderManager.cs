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
        m_BasePath = Path.GetDirectoryName(Application.dataPath) + "/Storage";
        m_PicturesPath = m_BasePath + "/Pictures";
        m_SavedPicturesPath = m_PicturesPath + "/Saved";
        m_CurrentPicturePath = m_PicturesPath + "/Current";

        CreateFolders();

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#else
        Application.quitting += OnApplicationQuitHandler;
#endif
    }

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
        DeleteCurrentPicturesOnly();
    }
#endif

    #endregion

    #region Paths

    [Header("Paths"), Space(5)]
    public static string m_BasePath;
    public static string m_PicturesPath;
    public static string m_SavedPicturesPath;
    public static string m_CurrentPicturePath;

    private void CreateFolders()
    {
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
        if (!Directory.Exists(m_CurrentPicturePath))
        {
            Directory.CreateDirectory(m_CurrentPicturePath);
        }
    }

    private static void ClearFolders()
    {
        Directory.Delete(m_BasePath, true);
    }

    private static void DeleteCurrentPicturesOnly()
    {
        string[] files = Directory.GetFiles(m_CurrentPicturePath, "*.png");
        for (int i = 0; i < files.Length; i++)
        {
            File.Delete(files[i]);
        }
    }

    #endregion
}
