using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    #region Unity Methods

    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Animator = GetComponent<Animator>();
        m_StaminaBar = GameObject.Find("StaminaBar").GetComponent<Image>();
        m_GroundCheck = transform.Find("GroundCheck");
    }

    private void Start()
    {
        m_CurrentMoveSpeed = m_BaseMoveSpeed;
        m_CurrentStamina = m_MaxStamina;
        m_StaminaBarMaxWidth = m_StaminaBar.rectTransform.sizeDelta.x;
        m_OriginalHeight = m_CharacterController.height;
    }

    private void Update()
    {
        RegenerateStamina();
        UpdateStaminaBar();
        MoveCharacter();
        CheckGroundStatus();
        ApplyGravity();
    }

    #endregion

    #region Components

    [Header("Components"), Space(5)]
    private CharacterController m_CharacterController;
    private Animator m_Animator;

    #endregion

    #region Movement

    [Header("Movement"), Space(5)]
    public float m_BaseMoveSpeed = 6.0f;
    public float m_CurrentMoveSpeed;
    private Vector2 m_MoveInput;

    [Header("Sprinting"), Space(5)]
    public float m_SprintMultiplier = 1.6f;
    public float m_MaxStamina = 15.0f;
    public float m_CurrentStamina;
    private bool m_IsSprinting = false;
    private readonly float m_StaminaRegenRate = 0.5f;
    private readonly float m_SprintStaminaCost = 2.0f;

    [Header("Stamina UI"), Space(5)]
    private Image m_StaminaBar;
    private float m_StaminaBarMaxWidth;

    [Header("Crouching"), Space(5)]
    public float m_CrouchMultiplier = 0.6f;
    private readonly float m_CrouchHeight = 1.2f;
    private float m_OriginalHeight;

    private bool m_IsCrouching = false;

    public void OnMove(InputAction.CallbackContext _context)
    {
        m_MoveInput = _context.ReadValue<Vector2>();
        if (m_MoveInput.magnitude > 1)
        {
            m_MoveInput.Normalize();
        }
    }

    public void OnSprint(InputAction.CallbackContext _context)
    {
        if (_context.performed && m_CurrentStamina > 0.0f && m_IsOnAnyGround)
        {
            if (m_IsCrouching)
            {
                m_CharacterController.height = m_OriginalHeight;
                m_CharacterController.center = new Vector3(0, 0, 0);
                m_IsCrouching = false;
                m_Animator.SetBool("IsCrouching", false);
                return;
            }
            m_CurrentMoveSpeed = m_BaseMoveSpeed * m_SprintMultiplier;
            m_IsSprinting = true;
        }
        else if (_context.canceled || !m_IsOnAnyGround)
        {
            m_CurrentMoveSpeed = m_BaseMoveSpeed;
            m_IsSprinting = false;
        }
    }

    private void RegenerateStamina()
    {
        if (!m_IsSprinting && m_CurrentStamina < m_MaxStamina)
        {
            if (m_IsCrouching)
            {
                m_CurrentStamina += m_StaminaRegenRate * 2.0f * Time.deltaTime;
            }
            else
            {
                m_CurrentStamina += m_StaminaRegenRate * Time.deltaTime;
            }
            if (m_CurrentStamina > m_MaxStamina)
            {
                m_CurrentStamina = m_MaxStamina;
            }
        }
        else if (m_IsSprinting && m_CurrentStamina > 0.0f)
        {
            m_CurrentStamina -= m_SprintStaminaCost * Time.deltaTime;
            if (m_CurrentStamina < 0.0f)
            {
                m_CurrentStamina = 0.0f;
                m_IsSprinting = false;
                m_CurrentMoveSpeed = m_BaseMoveSpeed;
            }
        }
    }

    public void OnCrouch(InputAction.CallbackContext _context)
    {
        if (_context.performed && m_IsOnAnyGround)
        {
            if (!m_IsCrouching)
            {
                m_CurrentMoveSpeed = m_BaseMoveSpeed * m_CrouchMultiplier;
                m_CharacterController.height = m_CrouchHeight;
                m_CharacterController.center = new Vector3(0, 0 - (2 - m_CrouchHeight) / 2, 0);
                m_IsCrouching = true;
                m_Animator.SetTrigger("StartCrouch");
                m_Animator.SetBool("IsCrouching", true);
            }
            else
            {
                m_CurrentMoveSpeed = m_BaseMoveSpeed;
                m_CharacterController.height = m_OriginalHeight;
                m_CharacterController.center = new Vector3(0, 0, 0);
                m_IsCrouching = false;
                m_Animator.SetBool("IsCrouching", false);
            }
        }
    }

    private void MoveCharacter()
    {
        Vector3 _move = transform.forward * m_MoveInput.y + transform.right * m_MoveInput.x;
        m_CharacterController.Move(_move * m_CurrentMoveSpeed * Time.deltaTime);

        if (m_MoveInput.magnitude > 0.1f)
        {
            if (m_IsSprinting && m_CurrentStamina > 0.0f)
            {
                m_Animator.SetBool("IsSprinting", true);
            }
            else
            {
                m_Animator.SetBool("IsSprinting", false);
            }
            m_Animator.SetBool("IsWalking", true);
        }
        else
        {
            m_Animator.SetBool("IsWalking", false);
            m_Animator.SetBool("IsSprinting", false);
        }
    }

    private void UpdateStaminaBar()
    {
        if (m_StaminaBar != null)
        {
            float _staminaWidth = m_CurrentStamina / m_MaxStamina * m_StaminaBarMaxWidth;
            m_StaminaBar.rectTransform.sizeDelta = new Vector2(_staminaWidth, m_StaminaBar.rectTransform.sizeDelta.y);
            m_StaminaBar.rectTransform.anchoredPosition = new Vector2(-m_StaminaBarMaxWidth / 2 + _staminaWidth / 2, m_StaminaBar.rectTransform.anchoredPosition.y);
        }
    }

    #endregion

    #region Jumping

    [Header("Jumping"), Space(5)]
    public float m_JumpForce = 8.0f;
    public LayerMask m_JumpableGroundLayer;
    public LayerMask m_AnyGroundLayer;
    private readonly float m_GroundCheckRadius = 0.1f;
    private bool m_IsOnJumpableGround;
    private bool m_IsOnAnyGround;
    private Transform m_GroundCheck;

    [Header("Gravity"), Space(5)]
    private readonly float m_FallMultiplier = 1.2f;
    private Vector3 m_Velocity;

    public void OnJump(InputAction.CallbackContext _context)
    {
        if (_context.performed && m_IsOnJumpableGround && !m_IsCrouching)
        {
            m_Velocity.y = m_JumpForce;
            m_Animator.SetTrigger("StartJump");
        }
    }

    private void CheckGroundStatus()
    {
        m_IsOnJumpableGround = Physics.CheckSphere(m_GroundCheck.position, m_GroundCheckRadius, m_JumpableGroundLayer);
        m_IsOnAnyGround = Physics.CheckSphere(m_GroundCheck.position, m_GroundCheckRadius, m_AnyGroundLayer);

        if (!m_IsOnAnyGround)
        {
            m_Animator.SetBool("IsFalling", true);
        }
        else
        {
            m_Animator.SetBool("IsFalling", false);
        }
    }

    private void ApplyGravity()
    {
        if (m_IsOnAnyGround)
        {
            if (m_Velocity.y < 0.0f)
            {
                m_Velocity.y = -2.0f;
            }
        }
        else
        {
            m_Velocity.y += Physics.gravity.y * m_FallMultiplier * Time.deltaTime;
        }

        m_CharacterController.Move(m_Velocity * Time.deltaTime);
    }

    #endregion
}
