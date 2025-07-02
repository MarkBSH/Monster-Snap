using UnityEngine;

public class FlashlightDimming : MonoBehaviour
{
    #region Unity Methods

    private void Awake()
    {
        m_Flashlight = GetComponent<Light>();
        m_Camera = GameObject.Find("PlayerCamera");
    }

    private void Start()
    {
        m_BaseIntensity = m_Flashlight.intensity;
        m_MinIntensity = m_BaseIntensity * 0.001f;
    }

    private void Update()
    {
        FlashlightIntesityControl();
    }

    #endregion

    #region Components

    [Header("Components"), Space(5)]
    private Light m_Flashlight;
    private GameObject m_Camera;

    #endregion

    #region Flashlight Intesity

    [Header("Flashlight Intensity"), Space(5)]
    public LayerMask m_FlashlightLayerMask;
    private float m_BaseIntensity;
    private float m_MinIntensity;
    private readonly float m_CloseRange = 5.0f;

    private void FlashlightIntesityControl()
    {
        float _targetIntensity = m_BaseIntensity;
        if (Physics.Raycast(m_Camera.transform.position, m_Camera.transform.forward, out RaycastHit hit, m_CloseRange, m_FlashlightLayerMask))
        {
            float _hitDistance = hit.distance - 0.7f;
            _targetIntensity = Mathf.Lerp(m_MinIntensity, m_BaseIntensity, Mathf.Clamp(_hitDistance / m_CloseRange, 0.0f, 1.0f));
        }

        m_Flashlight.intensity = Mathf.Lerp(m_Flashlight.intensity, _targetIntensity, Time.deltaTime * 2.0f);
    }

    #endregion
}
