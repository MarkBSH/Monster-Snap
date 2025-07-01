using System.Collections;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;

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
    }

    #endregion

    #region Components

    [Header("Components"), Space(5)]
    public GameObject m_PictureItemPrefab;
    public RenderTexture m_RenderTexture;
    public string m_OutputFileName = "Picture";
    private Camera m_RenderCamera;
    private GameObject m_CameraFlash;
    private Transform m_PictureDropPoint;
    private int m_PictureCounter = 0;

    #endregion

    #region Item

    public override void UseItem()
    {
        base.UseItem();

        StartCoroutine(TakePicture());
    }

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

        yield return new Null();

        // Optional: Convert to gamma if you're using linear color space
        // Uncomment if your images look too dark

        Color[] pixels = image.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i].r = Mathf.LinearToGammaSpace(pixels[i].r);
            pixels[i].g = Mathf.LinearToGammaSpace(pixels[i].g);
            pixels[i].b = Mathf.LinearToGammaSpace(pixels[i].b);
        }
        Color32[] pixels32 = new Color32[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels32[i] = pixels[i];
        }
        image.SetPixels32(pixels32);
        image.Apply();

        byte[] bytes = image.EncodeToPNG();

        yield return new Null();

        // Increment counter before using it
        m_PictureCounter++;

        string fileName = m_OutputFileName + "_" + m_PictureCounter + ".png";
        string fullPath = Application.dataPath + "/Resources/RenderPictures/" + fileName;
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        File.WriteAllBytes(fullPath, bytes);

        yield return new Null();

#if UNITY_EDITOR
        // Refresh the AssetDatabase so Unity recognizes the new file
        AssetDatabase.Refresh();
#endif

        // Wait a frame to ensure the file is recognized by Unity
        yield return null;

        GameObject TempPicture = Instantiate(m_PictureItemPrefab, m_PictureDropPoint.position, m_PictureDropPoint.rotation);

        yield return new Null();

        // Load the texture from Resources
        Texture2D loadedTexture = Resources.Load<Texture2D>("RenderPictures/" + m_OutputFileName + "_" + m_PictureCounter);
        if (loadedTexture != null)
        {
            TempPicture.GetComponent<PictureItem>().SetPictureTexture(loadedTexture);
        }
        else
        {
            Debug.LogError("Failed to load texture: " + "RenderPictures/" + m_OutputFileName + "_" + m_PictureCounter);
        }

        yield return new Null();

        RenderTexture.active = null;
        m_RenderCamera.targetTexture = null;
        Destroy(image);

        Debug.Log("Picture taken and saved to: " + fullPath);

        yield return new WaitForSeconds(0.1f);
        m_CameraFlash.SetActive(false);
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

    #endregion
}
