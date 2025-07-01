using UnityEngine;

public class PlayerHands : MonoBehaviour
{
    #region Unity Methods

    private void Awake()
    {
        m_Camera = GameObject.Find("PlayerCamera");
    }

    private void Update()
    {
        MoveHandsToCamera();
    }

    #endregion

    #region Components

    [Header("Components"), Space(5)]
    private GameObject m_Camera;

    #endregion

    #region Hand Movement

    [Header("Hand Movement"), Space(5)]
    private readonly float m_CameraToHandRotation = 0.5f;

    private void MoveHandsToCamera()
    {
        if (m_Camera.transform.rotation.eulerAngles.x > 180.0f)
        {
            transform.localRotation = Quaternion.Euler(m_Camera.transform.rotation.eulerAngles.x * m_CameraToHandRotation - 180.0f, 0.0f, 0.0f);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(m_Camera.transform.rotation.eulerAngles.x * m_CameraToHandRotation, 0.0f, 0.0f);
        }
    }

    #endregion
}
