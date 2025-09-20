using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    #region Unity Methods

    private void Awake()
    {
        m_Player = GameObject.FindGameObjectWithTag("Player");
        m_Camera = GetComponent<Camera>();
    }

    private void Start()
    {
        m_MouseSensitivity = m_BaseMouseSensitivity;
        m_ClampedY = 0.0f;
    }

    private void Update()
    {
        HandleCameraRotation();
    }

    #endregion

    #region Components

    [Header("Components"), Space(5)]
    private GameObject m_Player;
    private Camera m_Camera;

    #endregion

    #region Camera

    [Header("Camera Settings"), Space(5)]
    public float m_MouseSensitivity;
    private readonly float m_BaseMouseSensitivity = 10.0f;
    private Vector2 m_LookInput;
    private float m_ClampedY;
    private readonly float m_MaxYAngle = -40.0f;
    private readonly float m_MinYAngle = 50.0f;

    public void OnLook(InputAction.CallbackContext _context)
    {
        m_LookInput = _context.ReadValue<Vector2>();
    }

    private void HandleCameraRotation()
    {
        m_ClampedY -= m_LookInput.y * m_MouseSensitivity * Time.deltaTime;
        m_ClampedY = Mathf.Clamp(m_ClampedY, m_MaxYAngle, m_MinYAngle);
        m_Camera.transform.localRotation = Quaternion.Euler(m_ClampedY, 0.0f, 0.0f);
        m_Player.transform.Rotate(Vector3.up * m_LookInput.x * m_MouseSensitivity * Time.deltaTime);
    }

    #endregion
}
