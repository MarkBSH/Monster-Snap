using System.Collections;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CameraItem : ItemBase
{
    #region Unity Methods

    protected override void Awake()
    {
        base.Awake();
        m_RenderCamera = GetComponent<Camera>();
        m_CameraFlash = transform.Find("CameraFlash").gameObject;
        m_CameraFlash.SetActive(false);
        m_PictureDropPoint = transform.Find("PictureDropPoint");
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        UpdateCooldown();
    }

    #endregion

    #region Components

    [Header("Components"), Space(5)]
    public GameObject m_PictureItemPrefab;
    public RenderTexture m_RenderTexture;
    private Camera m_RenderCamera;
    private GameObject m_CameraFlash;
    private Transform m_PictureDropPoint;
    private readonly string m_OutputFileName = "Picture";

    #endregion

    #region Item

    [Header("Item Info"), Space(5)]
    private int m_PictureCounter = 0;
    private float m_CooldownTime = 1.0f;

    public override void UseItem()
    {
        base.UseItem();

        if (m_CooldownTime > 0f)
        {
            return;
        }

        StartCoroutine(TakePicture());
    }

    public override void AltUseItem()
    {
        base.AltUseItem();
    }

    private void UpdateCooldown()
    {
        if (m_CooldownTime > 0f)
        {
            m_CooldownTime -= Time.deltaTime;
        }
        else
        {
            m_CooldownTime = 0f;
        }
    }

    // TODO - less laggy way to take pictures

    private IEnumerator TakePicture()
    {
        m_CameraFlash.SetActive(true);

        yield return new WaitForEndOfFrame();

        m_RenderCamera.targetTexture = m_RenderTexture;
        m_RenderCamera.Render();
        RenderTexture.active = m_RenderTexture;

        int width = m_RenderTexture.width;
        int height = m_RenderTexture.height;
        Texture2D image = new(width, height, TextureFormat.RGBA32, false);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        yield return null;

        // Optimized linear to gamma correction for linear color space in Unity
        // Only use if your textures appear too dark

        Color32[] pixels = image.GetPixels32();
        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            c.r = Mathf.LinearToGammaSpace(c.r);
            c.g = Mathf.LinearToGammaSpace(c.g);
            c.b = Mathf.LinearToGammaSpace(c.b);
            pixels[i] = c;
        }
        image.SetPixels32(pixels);
        image.Apply();

        byte[] bytes = image.EncodeToPNG();

        yield return null;

        m_PictureCounter++;

        string fileName = "/" + m_OutputFileName + m_PictureCounter + ".png";
        string fullPath = FolderManager.m_CurrentPicturePath + fileName;
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        File.WriteAllBytes(fullPath, bytes);

        yield return null;

        GameObject TempPicture = Instantiate(m_PictureItemPrefab, m_PictureDropPoint.position, m_PictureDropPoint.rotation);

        yield return null;

        TempPicture.GetComponent<PictureItem>().SetPictureTexture(fullPath);

        yield return null;

        RenderTexture.active = null;
        m_RenderCamera.targetTexture = null;
        Destroy(image);

        yield return new WaitForSeconds(0.1f);
        m_CameraFlash.SetActive(false);
        m_CooldownTime = 1.0f;
    }

    #endregion
}
