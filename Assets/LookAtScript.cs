using UnityEngine;

public class LookAtScript : MonoBehaviour
{
    [Header("Look At Settings")]
    [Tooltip("Keep Y position same to avoid tilting up/down (billboard effect)")]
    public bool constrainYAxis = true;
    
    [Tooltip("Enable debug logging")]
    public bool enableDebugLogs = false;
    
    // Reference to the main camera (player camera)
    private Camera playerCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get reference to the main camera using the same logic as DogPicture
        InitializeCamera();
    }

    // Update is called once per frame
    void Update()
    {
        // Only proceed if we have a valid camera reference
        if (playerCamera != null)
        {
            LookAtPlayer();
        }
    }
    
    private void InitializeCamera()
    {
        // Get reference to the main camera
        playerCamera = Camera.main;
        
        // If Camera.main is null, try to find a camera with "MainCamera" tag
        if (playerCamera == null)
        {
            GameObject mainCameraObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (mainCameraObject != null)
            {
                playerCamera = mainCameraObject.GetComponent<Camera>();
            }
        }
        
        // If still null, find any active camera
        if (playerCamera == null)
        {
            playerCamera = FindFirstObjectByType<Camera>();
        }
        
        if (playerCamera == null)
        {
            Debug.LogWarning($"LookAtScript: No camera found! '{gameObject.name}' won't be able to look at the player.");
        }
        else if (enableDebugLogs)
        {
            Debug.Log($"LookAtScript: '{gameObject.name}' will look at camera '{playerCamera.name}'.");
        }
    }
    
    private void LookAtPlayer()
    {
        Vector3 targetPosition;
        
        if (constrainYAxis)
        {
            // Keep the Y position the same to avoid tilting up/down (billboard effect)
            // This is the same behavior as DogPicture
            targetPosition = new Vector3(playerCamera.transform.position.x,
                                        transform.position.y,
                                        playerCamera.transform.position.z);
        }
        else
        {
            // Look at the exact camera position (full 3D rotation)
            targetPosition = playerCamera.transform.position;
        }
        
        // Make the object look at the target position
        transform.LookAt(targetPosition);
        
        if (enableDebugLogs)
        {
            Debug.Log($"LookAtScript: '{gameObject.name}' looking at position {targetPosition}");
        }
    }
}
