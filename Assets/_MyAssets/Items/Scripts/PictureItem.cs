using System.Collections;
using System.IO;
using UnityEngine;

// TODO - Center of Gravity tweaking

public class PictureItem : ItemBase
{
    #region Unity Methods

    protected override void Awake()
    {
        base.Awake();
        m_DownPosition = transform.GetChild(0).localPosition;
        m_UpPosition = transform.GetChild(1).localPosition;
    }

    #endregion

    #region Components

    [Header("Components"), Space(5)]
    public string m_TexturePath;
    private Texture2D m_PictureTexture;

    #endregion

    #region Item Info

    [Header("Item Info"), Space(5)]
    public int m_PictureScore;
    public bool m_ShowPicture = false;
    private bool m_AltUseCooldown = false;
    private Vector3 m_DownPosition;
    private Vector3 m_UpPosition;

    public override void UseItem()
    {
        base.UseItem();
    }

    public override void AltUseItem()
    {
        base.AltUseItem();
        if (m_AltUseCooldown)
        {
            return;
        }
        m_ShowPicture = !m_ShowPicture;
        StartCoroutine(LerpToShowPicture(0.5f));
    }

    public override void EquipItem()
    {
        base.EquipItem();
    }

    public override void DropItem()
    {
        base.DropItem();
    }

    public void SetPictureTexture(string texturePath)
    {
        m_TexturePath = texturePath;

        if (File.Exists(m_TexturePath))
        {
            byte[] imageData = File.ReadAllBytes(m_TexturePath);
            m_PictureTexture = new Texture2D(2, 2);
            m_PictureTexture.LoadImage(imageData);

            Transform childTransform = transform.GetChild(0).GetChild(0);
            if (childTransform != null)
            {
                childTransform.GetComponent<Renderer>().material.mainTexture = m_PictureTexture;
            }
        }
    }

    public IEnumerator LerpToShowPicture(float duration)
    {
        m_AltUseCooldown = true;

        float elapsedTime = 0.0f;

        Transform startTransform = transform.GetChild(0);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            if (m_ShowPicture)
            {
                transform.GetChild(0).localPosition = Vector3.Lerp(startTransform.localPosition, m_UpPosition, elapsedTime / duration);
            }
            else
            {
                transform.GetChild(0).localPosition = Vector3.Lerp(startTransform.localPosition, m_DownPosition, elapsedTime / duration);
            }
            yield return null;
        }

        m_AltUseCooldown = false;
    }

    #endregion
}
