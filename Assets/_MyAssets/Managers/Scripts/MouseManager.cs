using UnityEngine;

public class MouseManager : MonoBehaviour
{
    #region Singleton

    private static MouseManager m_Instance;
    public static MouseManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindFirstObjectByType<MouseManager>();
                if (m_Instance == null)
                {
                    GameObject obj = new("FolderManager");
                    m_Instance = obj.AddComponent<MouseManager>();
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
        SetMouseVisibility(m_MouseVisible);
    }

    #endregion

    #region Mouse Settings

    [Header("Mouse Settings"), Space(5)]
    public bool m_MouseVisible = true;

    public void SetMouseVisibility(bool visible)
    {
        m_MouseVisible = visible;
        Cursor.visible = m_MouseVisible;
        Cursor.lockState = m_MouseVisible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    #endregion
}
