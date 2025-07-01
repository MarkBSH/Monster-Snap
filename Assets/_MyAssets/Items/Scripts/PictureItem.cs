using UnityEngine;

public class PictureItem : ItemBase
{
    #region Unity Methods

    protected override void Awake()
    {
        base.Awake();
    }

    #endregion

    #region Components

    [Header("Components"), Space(5)]
    public Texture2D m_PictureTexture;

    #endregion

    #region Item Info

    public override void UseItem()
    {
        base.UseItem();
    }

    public override void AltUseItem()
    {
        base.AltUseItem();
    }

    public override void EquipItem()
    {
        base.EquipItem();
    }

    public override void DropItem()
    {
        base.DropItem();
    }

    public void SetPictureTexture(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.LogError("Texture is null in SetPictureTexture");
            return;
        }

        m_PictureTexture = texture;

        // Get the first child (quad) and apply the texture
        Transform childTransform = transform.GetChild(0).GetChild(0);
        if (childTransform != null)
        {
            Renderer renderer = childTransform.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.mainTexture = m_PictureTexture;
            }
        }
    }

    #endregion
}
