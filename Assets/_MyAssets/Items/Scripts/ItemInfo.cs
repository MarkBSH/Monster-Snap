using UnityEngine;

[CreateAssetMenu(fileName = "ItemInfo", menuName = "Scriptable Objects/ItemInfo")]
public class ItemInfo : ScriptableObject
{
    [Header("Item Info")]
    public string m_ItemName;
    [TextArea(3, 10)]
    public string m_ItemDescription;
    public Sprite m_ItemIcon;
    public GameObject m_ItemPrefab;
}
