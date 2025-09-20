using UnityEngine;
using System.IO;

public class SessionManager : MonoBehaviour
{
    #region Singleton

    private static SessionManager m_Instance;
    public static SessionManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindFirstObjectByType<SessionManager>();
                if (m_Instance == null)
                {
                    GameObject obj = new("SessionManager");
                    m_Instance = obj.AddComponent<SessionManager>();
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
        GenerateSessionId();
    }

    #endregion

    #region Session Data

    [Header("Session Data"), Space(5)]
    public int m_Level = 1;
    public string m_SessionId;
    public int m_PlayerCount = 0;

    #endregion

    #region Session ID Generation

    /// <summary>
    /// Generates a random session ID for making a unique code for saved pictures to avoid overwriting.
    /// If the generated ID already exists in the saved pictures, it generates a new session ID.
    /// </summary>
    private void GenerateSessionId()
    {
        string newSessionId;
        newSessionId = Random.Range(10000, 99999).ToString();

        if (SessionIdCheck(newSessionId))
        {
            GenerateSessionId();
            return;
        }

        m_SessionId = newSessionId;
    }

    private bool SessionIdCheck(string sessionId)
    {
        if (!Directory.Exists(FolderManager.m_SavedPicturesPath))
        {
            return false;
        }

        string[] files = Directory.GetFiles(FolderManager.m_SavedPicturesPath, "*.png");

        if (files.Length == 0)
        {
            return false;
        }

        foreach (string filePath in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            if (fileName.Contains(sessionId))
            {
                return true;
            }
        }
        return false;
    }

    #endregion
}
