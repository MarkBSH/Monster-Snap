using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    #region Unity Methods

    private void Awake()
    {
        m_InventoryUI = GameObject.Find("InventoryBar");
        m_PlayerHands = GameObject.Find("PlayerHands");
    }

    private void Start()
    {
        m_InventoryItems = new List<ItemBase>(5);
        for (int i = 0; i < 5; i++)
        {
            m_InventoryItems.Add(null);
        }
        SelectedItemIndex = 0;
    }

    private void Update()
    {
        if (m_scrollCooldown > 0f)
        {
            m_scrollCooldown -= Time.deltaTime;
        }
    }

    #endregion

    #region Components

    [Header("Components"), Space(5)]
    private GameObject m_InventoryUI;
    private GameObject m_PlayerHands;

    #endregion

    #region Inventory

    [Header("Inventory"), Space(5)]
    public List<ItemBase> m_InventoryItems;
    private ItemBase m_SelectedItem;
    private int m_selectedItemIndex;
    private int SelectedItemIndex
    {
        get => m_selectedItemIndex;
        set
        {
            if (value > 4)
            {
                m_selectedItemIndex = 0;
            }
            else if (value < 0)
            {
                m_selectedItemIndex = 4;
            }
            else
            {
                m_selectedItemIndex = value;
            }

            UpdateInventoryUI();
        }
    }

    public void AddItemToInventory(ItemBase item)
    {
        if (m_InventoryItems[m_selectedItemIndex] == null)
        {
            m_InventoryItems[m_selectedItemIndex] = item;
            UpdateInventoryUI();
            StartCoroutine(ItemLerpToHand(item));
            return;
        }

        for (int i = 0; i < m_InventoryItems.Count; i++)
        {
            if (m_InventoryItems[i] == null)
            {
                m_InventoryItems[i] = item;
                UpdateInventoryUI();
                StartCoroutine(ItemLerpToHand(item));
                return;
            }
        }

        Debug.LogWarning("Inventory is full! Cannot add item: " + item.m_ItemInfo.m_ItemName);
    }

    private IEnumerator ItemLerpToHand(ItemBase item)
    {
        item.GetComponent<Rigidbody>().isKinematic = true;
        item.transform.SetParent(m_PlayerHands.transform);

        Vector3 startPosition = item.transform.position;
        Quaternion startRotation = item.transform.rotation;

        float elapsedTime = 0f;
        float duration = 0.3f;

        while (elapsedTime < duration)
        {
            item.transform.position = Vector3.Lerp(startPosition, m_PlayerHands.transform.position, elapsedTime / duration);
            item.transform.rotation = Quaternion.Lerp(startRotation, m_PlayerHands.transform.rotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

    }

    private void UpdateInventoryUI()
    {
        for (int i = 0; i < 5; i++)
        {
            if (i == m_selectedItemIndex)
            {
                m_InventoryUI.transform.GetChild(i).GetChild(0).gameObject.SetActive(true);
                if (m_InventoryItems[i] != null)
                {
                    m_InventoryItems[i].gameObject.SetActive(true);
                }
            }
            else
            {
                m_InventoryUI.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
                if (m_InventoryItems[i] != null)
                {
                    m_InventoryItems[i].gameObject.SetActive(false);
                }
            }

            if (m_InventoryItems[i] != null)
            {
                m_InventoryUI.transform.GetChild(i).GetChild(1).gameObject.SetActive(true);
                m_InventoryUI.transform.GetChild(i).GetChild(1).GetComponent<Image>().sprite = m_InventoryItems[i].m_ItemInfo.m_ItemIcon;
            }
            else
            {
                m_InventoryUI.transform.GetChild(i).GetChild(1).gameObject.SetActive(false);
            }
        }

        if (m_InventoryItems[m_selectedItemIndex] != null)
        {
            m_SelectedItem = m_InventoryItems[m_selectedItemIndex];
        }
        else
        {
            m_SelectedItem = null;
        }
    }

    #endregion

    #region Inventory Inputs

    [Header("Inventory Inputs"), Space(5)]
    private float m_scrollCooldown = 0f;
    private readonly float m_scrollCooldownTime = 0.1f;

    public void OnScrollWheel(InputAction.CallbackContext _context)
    {
        if (_context.performed && m_scrollCooldown <= 0f)
        {
            float scrollValue = _context.ReadValue<float>();
            if (scrollValue < 0.0f)
            {
                SelectedItemIndex++;
            }
            else if (scrollValue > 0.0f)
            {
                SelectedItemIndex--;
            }

            m_scrollCooldown = m_scrollCooldownTime;
        }
    }

    public void OnSelectSlot(InputAction.CallbackContext _context)
    {
        if (_context.performed)
        {
            if (int.TryParse(_context.control.name, out int index))
            {
                SelectedItemIndex = index - 1;
            }
        }
    }

    public void OnUseItem(InputAction.CallbackContext _context)
    {
        if (_context.performed && m_SelectedItem != null)
        {
            m_SelectedItem.UseItem();
        }
    }

    public void OnAltUseItem(InputAction.CallbackContext _context)
    {
        if (_context.performed && m_SelectedItem != null)
        {
            m_SelectedItem.AltUseItem();
        }
    }

    public void OnDropItem(InputAction.CallbackContext _context)
    {
        if (_context.performed && m_SelectedItem != null)
        {
            m_SelectedItem.DropItem();
            m_InventoryItems[m_selectedItemIndex] = null;
            UpdateInventoryUI();
        }
    }

    #endregion
}
