using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickupItems : MonoBehaviour
{
    #region Unity Methods

    private void Awake()
    {
        m_Inventory = transform.parent.GetComponent<PlayerInventory>();
    }

    #endregion

    #region Components

    [Header("Components"), Space(5)]
    private PlayerInventory m_Inventory;

    #endregion

    #region Pickup

    [Header("Pickup"), Space(5)]
    private readonly float m_PickupRange = 4.0f;

    public void OnPickupItem(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, m_PickupRange))
            {
                if (hit.collider.CompareTag("ItemPickup"))
                {
                    // A check  to see if the item is not already in the player's inventory
                    if (hit.collider.transform.parent == null)
                    {
                        m_Inventory.AddItemToInventory(hit.collider.gameObject.GetComponent<ItemBase>());
                    }
                }
            }
        }
    }

    #endregion
}
