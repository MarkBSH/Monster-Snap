using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using System; // Added for Math.Round

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

        // TODO - show camera view on screen
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

    private IEnumerator TakePicture()
    {
        m_CameraFlash.SetActive(true);
        yield return new WaitForEndOfFrame(); // Wait until camera finishes rendering

        // Ensure the render texture is set up
        m_RenderCamera.targetTexture = m_RenderTexture;
        m_RenderCamera.Render();
        RenderTexture.active = m_RenderTexture;

        // Get the width and height of the render texture
        int width = m_RenderTexture.width;
        int height = m_RenderTexture.height;

        // Read from camera view
        Texture2D image = new(width, height, TextureFormat.RGBA32, false);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        RenderTexture.active = null;
        m_RenderCamera.targetTexture = null;

        // Flash off after a short delay
        yield return new WaitForSeconds(0.1f);
        m_CameraFlash.SetActive(false);
        m_CooldownTime = 1.0f;

        int score = CalculatePictureScore();

        m_PictureCounter++;

        // Save the picture to a file
        string playerName = GetComponentInParent<PlayerMovement>().m_PlayerName;
        string sessionId = SessionManager.Instance.m_SessionId;
        string fileName = $"Picture_{playerName}_{sessionId}_{m_PictureCounter}.png";
        string fullPath = Path.Combine(FolderManager.m_CurrentPicturePath, fileName);

        // Get PNG data from the texture
        byte[] pngData = image.EncodeToPNG();

        // Write the PNG data to a file
        yield return TaskToCoroutine(() => File.WriteAllBytes(fullPath, pngData));

        // Save the picture data to a JSON file
        PictureData data = new()
        {
            filename = "/" + fileName,
            score = score,
            whyScore = whyScoreEntries
        };
        string json = JsonUtility.ToJson(data, true);
        string jsonFileName = $"Picture_{playerName}_{sessionId}_{m_PictureCounter}.json";
        string jsonPath = Path.Combine(FolderManager.m_CurrentJsonPath, jsonFileName);
        File.WriteAllText(jsonPath, json);

        // Make a new picture item
        GameObject tempPicture = Instantiate(m_PictureItemPrefab, m_PictureDropPoint.position, m_PictureDropPoint.rotation);
        tempPicture.GetComponent<PictureItem>().SetPictureTexture(fullPath);
        tempPicture.GetComponent<PictureItem>().m_PictureScore = score;

        Destroy(image);
    }

    private IEnumerator TaskToCoroutine(System.Action action)
    {
        Task task = Task.Run(action);
        while (!task.IsCompleted) yield return null;

        if (task.Exception != null)
            Debug.LogException(task.Exception);
    }

    #endregion

    #region Calculate Picture Score

    [Header("Calculate Picture Score"), Space(5)]
    private readonly float m_MaxRenderDistance = 50.0f;
    private readonly float m_MinScreenFraction = 0.1f;
    private readonly float m_ScreenFractionMultiplier = 3.0f;

    [Header("Picture Score info"), Space(5)]
    private readonly List<WhyScoreEntry> whyScoreEntries = new();

    private int CalculatePictureScore()
    {
        whyScoreEntries.Clear();

        MonsterBase[] monsters = FindObjectsByType<MonsterBase>(FindObjectsSortMode.None); // List of monsters in the scene
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(m_RenderCamera); // Frustum planes for the camera
        List<string> detectedMonsters = new(); // List to store detected monsters

        float totalScore = 0f;

        foreach (var monster in monsters)
        {
            // Get all Renderers on the monster
            Renderer[] monsterRenderers = monster.GetComponentsInChildren<Renderer>();

            // Check if any of the monster's colliders are within the camera's view
            bool isVisible = false;
            Bounds combinedBounds = new();
            bool boundsInitialized = false;

            // Combine bounds of all monster renderers
            foreach (Renderer render in monsterRenderers)
            {
                // Check if the monster in on the picture and close to the camera
                if (GeometryUtility.TestPlanesAABB(planes, render.bounds) && Vector3.Distance(m_RenderCamera.transform.position, monster.transform.position) < m_MaxRenderDistance)
                {
                    isVisible = true;
                }

                // Combine bounds of all colliders
                if (!boundsInitialized)
                {
                    combinedBounds = render.bounds;
                    boundsInitialized = true;
                }
                else
                {
                    combinedBounds.Encapsulate(render.bounds);
                }
            }

            // If no renderers were found, skip this monster
            if (isVisible && boundsInitialized)
            {
                // Get 0-1 screen coordinates of the monster's bounds
                Bounds bounds = combinedBounds;
                Vector3 screenMin = m_RenderCamera.WorldToScreenPoint(bounds.min);
                Vector3 screenMax = m_RenderCamera.WorldToScreenPoint(bounds.max);

                // Clamp to screen size
                screenMin = ClampToScreen(screenMin);
                screenMax = ClampToScreen(screenMax);

                // Calculate screen fraction
                float screenFraction = Mathf.Abs(screenMax.x - screenMin.x) * Mathf.Abs(screenMax.y - screenMin.y) / (Screen.width * Screen.height);
                screenFraction *= m_ScreenFractionMultiplier;
                screenFraction = Mathf.Clamp(screenFraction, m_MinScreenFraction, 1f);
                // Round screen fraction to 2 decimal places
                screenFraction = MathF.Round(screenFraction, 2);

                // Check if monster is fully visible on screen before clamping
                bool fullyVisible = true;
                for (int i = 0; i < monsterRenderers.Length; i++)
                {
                    if (fullyVisible == false)
                    {
                        continue;
                    }
                    fullyVisible = IsFullyVisible(m_RenderCamera, monsterRenderers[i]);
                }

                // Score calculation
                float visibilityMultiplier = screenFraction * (fullyVisible ? 1f : 0.5f);
                float finalScore = monster.m_BaseScore * visibilityMultiplier * monster.m_ActionMultiplier;

                totalScore += finalScore;

                detectedMonsters.Add($"{monster.m_MonsterName} (+{Mathf.RoundToInt(finalScore)})");

                // Create a WhyScoreEntry for this monster
                WhyScoreEntry entry = new()
                {
                    monsterName = monster.m_MonsterName,
                    baseScore = monster.m_BaseScore,
                    screenFraction = screenFraction.ToString(),
                    isFullyVisible = fullyVisible,
                    actionMultiplier = monster.m_ActionMultiplier,
                    action = monster.m_CurrentState.ToString()
                };

                whyScoreEntries.Add(entry);
            }
        }

        // Apply a multiplier for each additional monster detected
        for (int i = 1; i < detectedMonsters.Count; i++)
        {
            totalScore *= 1.2f;
        }

        // Return the total score rounded to the nearest integer
        return Mathf.RoundToInt(totalScore);
    }

    private Vector3 ClampToScreen(Vector3 screenPoint)
    {
        // Clamp to see the monster size on screen
        return new Vector3(Mathf.Clamp(screenPoint.x, 0, Screen.width), Mathf.Clamp(screenPoint.y, 0, Screen.height), screenPoint.z);
    }

    public static bool IsFullyVisible(Camera camera, Renderer renderer)
    {
        if (camera == null || renderer == null)
            return false;

        Bounds bounds = renderer.bounds;

        Vector3 min = bounds.min;
        Vector3 max = bounds.max;

        Vector3[] corners = new Vector3[8];

        // Generate 8 corners of the bounding box
        corners[0] = new Vector3(min.x, min.y, min.z);
        corners[1] = new Vector3(min.x, min.y, max.z);
        corners[2] = new Vector3(min.x, max.y, min.z);
        corners[3] = new Vector3(min.x, max.y, max.z);
        corners[4] = new Vector3(max.x, min.y, min.z);
        corners[5] = new Vector3(max.x, min.y, max.z);
        corners[6] = new Vector3(max.x, max.y, min.z);
        corners[7] = new Vector3(max.x, max.y, max.z);

        foreach (Vector3 corner in corners)
        {
            Vector3 viewportPoint = camera.WorldToViewportPoint(corner);

            // Check if each corner is within the camera's viewport
            if (viewportPoint.z < 0 || viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1)
            {
                return false;
            }
        }

        return true;
    }

    #endregion
}
