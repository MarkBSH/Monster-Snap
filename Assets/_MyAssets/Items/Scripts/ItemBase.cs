using UnityEngine;

public class ItemBase : MonoBehaviour
{
    #region Unity Methods

    protected virtual void Awake()
    {

    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }

    #endregion

    #region Components

    [Header("Components"), Space(5)]

    #endregion

    #region Item

    [Header("Item"), Space(5)]
    public ItemInfo m_ItemInfo;

    public virtual void UseItem()
    {
        Debug.Log($"Using item: {m_ItemInfo.m_ItemName}");
    }

    public virtual void AltUseItem()
    {
        Debug.Log($"Alt Using item: {m_ItemInfo.m_ItemName}");
    }

    public virtual void EquipItem()
    {
        Debug.Log($"Equipping item: {m_ItemInfo.m_ItemName}");
    }

    public virtual void DropItem()
    {
        Debug.Log($"Dropping item: {m_ItemInfo.m_ItemName}");
        transform.SetParent(null);
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().AddForce(transform.forward * 5f, ForceMode.Impulse);
    }

    #endregion
}
