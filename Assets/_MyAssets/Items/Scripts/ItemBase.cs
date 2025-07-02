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

    }

    public virtual void AltUseItem()
    {

    }

    public virtual void EquipItem()
    {

    }

    public virtual void DropItem()
    {
        transform.SetParent(null);
        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Rigidbody>().AddForce(transform.forward * 5f, ForceMode.Impulse);
    }

    #endregion
}
