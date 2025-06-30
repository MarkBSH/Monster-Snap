using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{
    #region Unity Methods

    private void Awake()
    {
        m_Flashlight = transform.Find("Flashlight").gameObject;
        m_Flashlight.SetActive(false);
        m_Camera = transform.Find("PlayerCamera").gameObject;
    }

    private void Update()
    {
        FollowCameraRotation();
    }

    #endregion

    #region Components

    [Header("Components"), Space(5)]
    private GameObject m_Flashlight;
    private GameObject m_Camera;

    private void FollowCameraRotation()
    {
        m_Flashlight.transform.localRotation = Quaternion.Euler(m_Camera.transform.rotation.eulerAngles.x, 0.0f, 0.0f);
    }

    #endregion

    #region Flashlight

    public void ToggleFlashlight(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            m_Flashlight.SetActive(!m_Flashlight.activeSelf);
        }
    }

    #endregion
}
