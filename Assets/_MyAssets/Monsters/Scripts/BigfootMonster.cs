using UnityEngine;
using UnityEngine.AI;

public class BigfootMonster : MonsterBase
{
    #region Unity Methods

    protected override void Awake()
    {
        base.Awake();
        m_MonsterName = "Bigfoot";
        m_BaseScore = 500f;
        m_AttackPower = 20;
        m_EyeTransform = transform.Find("BigfootEyes");
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        // Check if monster is scared of any players
        if (m_CurrentState != MonsterState.Fleeing)
        {
            DetectPlayerScare();
        }
    }

    #endregion

    #region Components

    [Header("Components"), Space(5)]
    private Transform m_EyeTransform;

    #endregion

    #region Monster Actions

    [Header("Monster Actions"), Space(5)]
    private GameObject m_RoamTarget;
    private GameObject m_StalkTarget;
    private GameObject m_Scaredof;
    private float m_SightRange = 50f;

    public override void StartRoaming()
    {
        base.StartRoaming();

        // Check if monster sees any players and should start stalking
        if (CheckForVisiblePlayers())
        {
            StartStalking();
            return;
        }

        CheckClosestPlayer();

        Vector3 roamPosition = m_RoamTarget.transform.position;
        roamPosition.x += Random.Range(-70, 70);
        roamPosition.z += Random.Range(-70, 70);

        Debug.Log($"{m_MonsterName} is roaming around the forest.");

        NavMeshDestination(roamPosition, StartRoaming);
    }

    public override void StartStalking()
    {
        // TODO - Implement stalking behavior
        Debug.Log($"{m_MonsterName} is stalking {m_StalkTarget.name}.");
        // Implement stalking behavior here, such as moving towards the target
    }

    public override void StartFleeing()
    {
        // TODO - Flee via trees or other obstacles
        base.StartFleeing();
        Debug.Log($"{m_MonsterName} is fleeing from {m_Scaredof.name}.");

        // Calculate a position away from the scaring player
        Vector3 directionAway = (transform.position - m_Scaredof.transform.position).normalized;
        Debug.Log($"Direction away from {m_Scaredof.name}: {directionAway}");
        Vector3 fleePosition = transform.position + directionAway * 49f;
        Debug.Log($"Flee position: {fleePosition}");

        // Add some randomness to the fleeing direction
        fleePosition += new Vector3(Random.Range(-20f, 20f), 0f, Random.Range(-20f, 20f));
        fleePosition.y = 0f; // Keep the monster on the ground

        // Set the NavMesh destination
        NavMeshDestination(fleePosition, StartRoaming);

        // After some time, go back to roaming
        Invoke(nameof(ResetScaredState), 15f);
    }

    private void ResetScaredState()
    {
        if (m_CurrentState == MonsterState.Fleeing)
        {
            m_Scaredof = null;
            StartRoaming();
        }
    }

    private bool CheckForVisiblePlayers()
    {
        float closestDistance = float.MaxValue;
        GameObject closestVisiblePlayer = null;

        foreach (GameObject player in m_Players)
        {
            Vector3 directionToPlayer = player.transform.position - m_EyeTransform.position;
            float distanceToPlayer = Vector3.Distance(m_EyeTransform.position, player.transform.position);

            if (distanceToPlayer > m_SightRange)
                continue;

            if (Physics.Raycast(m_EyeTransform.position, directionToPlayer.normalized, out RaycastHit hit, m_SightRange))
            {
                if (hit.transform.gameObject == player)
                {
                    if (distanceToPlayer < closestDistance)
                    {
                        closestDistance = distanceToPlayer;
                        closestVisiblePlayer = player;
                    }
                }
            }
        }

        if (closestVisiblePlayer != null)
        {
            m_StalkTarget = closestVisiblePlayer;
            return true;
        }

        return false;
    }

    private void CheckClosestPlayer()
    {
        float closestDistance = float.MaxValue;
        GameObject closestPlayer = null;

        foreach (GameObject player in m_Players)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer < closestDistance)
            {
                closestDistance = distanceToPlayer;
                closestPlayer = player;
            }
        }

        if (closestPlayer != null)
        {
            m_RoamTarget = closestPlayer;
        }
    }

    private void DetectPlayerScare()
    {
        GameObject scariestPlayer = null;
        float scareIntensity = 0f;

        // Scared detection thresholds
        float maxScareDistance = 30f;
        float flashlightScareDistance = 20f;
        float cameraFlashScareDistance = 15f;
        float runningScareDistance = 10f;
        float microphoneScareDistance = 25f;  // Distance for microphone detection

        foreach (GameObject player in m_Players)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            // Only consider players within max detection range
            if (distanceToPlayer > maxScareDistance)
                continue;

            // Calculate base scare factor based on distance (closer = more scary)
            float currentScareIntensity = 1f - (distanceToPlayer / maxScareDistance);

            // Check for flashlight
            GameObject flashlightObj = player.transform.Find("Flashlight").gameObject;
            if (flashlightObj != null && flashlightObj.activeSelf && distanceToPlayer < flashlightScareDistance)
            {
                // Check if flashlight is pointing towards monster
                Transform flashlightTransform = flashlightObj.transform;
                Vector3 directionToMonster = (transform.position - flashlightTransform.position).normalized;
                float flashlightDot = Vector3.Dot(flashlightTransform.forward, directionToMonster);

                // If flashlight is pointing at monster (dot product > 0.7 means within ~45 degrees)
                if (flashlightDot > 0.7f)
                {
                    currentScareIntensity += 0.4f;
                    Debug.Log($"{m_MonsterName} is scared by {player.name}'s flashlight!");
                }
            }

            // Check for camera flash
            CameraItem cameraItem = player.GetComponentInChildren<CameraItem>();
            if (cameraItem != null && distanceToPlayer < cameraFlashScareDistance)
            {
                GameObject cameraFlash = cameraItem.transform.Find("CameraFlash").gameObject;
                if (cameraFlash != null && cameraFlash.activeSelf)
                {
                    currentScareIntensity += 0.6f;
                    Debug.Log($"{m_MonsterName} is startled by {player.name}'s camera flash!");
                }
            }

            // Check for player running (making noise)
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null && distanceToPlayer < runningScareDistance)
            {
                // Check if player is moving fast by comparing current speed to base speed
                if (playerMovement.m_CurrentMoveSpeed > playerMovement.m_BaseMoveSpeed * 1.2f)
                {
                    currentScareIntensity += 0.3f;
                    Debug.Log($"{m_MonsterName} hears {player.name} running!");
                }
            }

            // Check for microphone usage (TO BE IMPLEMENTED)
            if (distanceToPlayer < microphoneScareDistance)
            {
                // This is a placeholder for future microphone implementation
                // When implemented, this should check if the player is using a microphone
                // and how loud they are speaking

                // Example of future implementation:
                // MicrophoneHandler micHandler = player.GetComponent<MicrophoneHandler>();
                // if (micHandler != null && micHandler.IsActive && micHandler.CurrentVolume > micHandler.ScareThreshold)
                // {
                //     float volumeFactor = Mathf.Clamp01(micHandler.CurrentVolume / micHandler.MaxVolume);
                //     currentScareIntensity += 0.5f * volumeFactor;
                //     Debug.Log($"{m_MonsterName} hears {player.name} speaking through microphone!");
                // }
            }

            // Update scariest player if this one is scarier
            if (currentScareIntensity > scareIntensity)
            {
                scareIntensity = currentScareIntensity;
                scariestPlayer = player;
                float closestScareDistance = distanceToPlayer;
            }
        }

        // If scared enough, start fleeing
        if (scariestPlayer != null && scareIntensity > 0.7f)
        {
            m_Scaredof = scariestPlayer;
            StartFleeing();
        }
    }

    #endregion
}
