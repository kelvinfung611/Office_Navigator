using UnityEngine;

public class DogPicture : MonoBehaviour
{
    // Reference to the main camera (player camera)
    private Camera playerCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get reference to the main camera
        playerCamera = Camera.main;
        
        // If Camera.main is null, try to find a camera with "MainCamera" tag
        if (playerCamera == null)
        {
            playerCamera = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>();
        }
        
        // If still null, find any active camera
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        if (playerCamera == null)
        {
            Debug.LogWarning("DogPicture: No camera found! The dog picture won't be able to look at the player.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Only proceed if we have a valid camera reference
        if (playerCamera != null)
        {
            // Make the dog picture look at the player camera
            // We keep the Y position the same to avoid tilting up/down
            Vector3 targetPosition = new Vector3(playerCamera.transform.position.x,
                                                transform.position.y,
                                                playerCamera.transform.position.z);
            
            // Make the object look at the target position
            transform.LookAt(targetPosition);
        }
    }
}
