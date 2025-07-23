using UnityEngine;

public class tolietRaycasting : MonoBehaviour
{
    [Header("URL Settings")]
    [Tooltip("URL to open when this toilet video is clicked")]
    public string websiteURL = "https://www.google.com";
    
    [Header("Raycast Settings")]
    [Tooltip("Maximum distance for raycast detection")]
    public float maxRaycastDistance = 100f;
    
    [Tooltip("Layer mask for raycast (optional - leave as 'Everything' for all layers)")]
    public LayerMask raycastLayerMask = -1;
    
    [Header("Feedback Settings")]
    [Tooltip("Enable visual feedback when clicking on toilet video")]
    public bool enableClickFeedback = true;
    
    [Tooltip("Duration of click feedback effect")]
    public float feedbackDuration = 0.5f;
    
    // References
    private Camera playerCamera;
    private bool isProcessingClick = false;
    private Collider thisCollider;

    void Start()
    {
        // Get reference to the main camera by tag
        InitializeCamera();
        
        // Get this object's collider
        InitializeCollider();
        
        // Validate setup
        ValidateSetup();
    }

    void Update()
    {
        // Handle input for both mouse (editor/desktop) and touch (mobile/AR)
        HandleInput();
    }
    
    private void InitializeCamera()
    {
        // Find camera by MainCamera tag first (as requested)
        GameObject mainCameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCameraObject != null)
        {
            playerCamera = mainCameraObject.GetComponent<Camera>();
            Debug.Log($"ToiletRaycasting: Found camera '{playerCamera.name}' using MainCamera tag.");
        }
        
        // Fallback to Camera.main if MainCamera tag didn't work
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera != null)
            {
                Debug.Log($"ToiletRaycasting: Using Camera.main '{playerCamera.name}' as fallback.");
            }
        }
        
        // Final fallback: find any active camera
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
            if (playerCamera != null)
            {
                Debug.Log($"ToiletRaycasting: Using found camera '{playerCamera.name}' as final fallback.");
            }
        }
        
        if (playerCamera == null)
        {
            Debug.LogError("ToiletRaycasting: No camera found! Please ensure there's a camera with 'MainCamera' tag.");
        }
    }
    
    private void InitializeCollider()
    {
        thisCollider = GetComponent<Collider>();
        if (thisCollider == null)
        {
            Debug.LogError($"ToiletRaycasting: No Collider found on '{gameObject.name}'! Please add a collider component.");
        }
        else
        {
            Debug.Log($"ToiletRaycasting: Using collider on '{gameObject.name}' for raycast detection.");
        }
    }
    
    private void ValidateSetup()
    {
        bool setupValid = true;
        
        if (playerCamera == null)
        {
            Debug.LogError("ToiletRaycasting: No camera found! Please ensure there's a camera with 'MainCamera' tag.");
            setupValid = false;
        }
        
        if (thisCollider == null)
        {
            Debug.LogError($"ToiletRaycasting: No Collider on '{gameObject.name}'! Please add a collider component.");
            setupValid = false;
        }
        else if (!thisCollider.enabled)
        {
            Debug.LogWarning($"ToiletRaycasting: Collider on '{gameObject.name}' is disabled. Raycast detection will not work.");
        }
        
        if (string.IsNullOrEmpty(websiteURL))
        {
            Debug.LogWarning("ToiletRaycasting: Website URL is empty. Please set the URL to open.");
            setupValid = false;
        }
        
        if (setupValid)
        {
            Debug.Log($"ToiletRaycasting: Setup complete for '{gameObject.name}' - ready to detect clicks!");
        }
    }
    
    private void HandleInput()
    {
        if (playerCamera == null || isProcessingClick) return;
        
        Vector3 inputPosition = Vector3.zero;
        bool inputDetected = false;
        
        // Handle mouse input (for editor/desktop testing)
        if (Input.GetMouseButtonDown(0))
        {
            inputPosition = Input.mousePosition;
            inputDetected = true;
        }
        // Handle touch input (for mobile/AR)
        else if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                inputPosition = touch.position;
                inputDetected = true;
            }
        }
        
        if (inputDetected)
        {
            PerformRaycast(inputPosition);
        }
    }
    
    private void PerformRaycast(Vector3 screenPosition)
    {
        // Convert screen position to world ray
        Ray ray = playerCamera.ScreenPointToRay(screenPosition);
        
        // Perform the raycast
        if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, raycastLayerMask))
        {
            // Check if we hit this toilet video object
            if (hit.collider == thisCollider || hit.collider.transform.IsChildOf(transform))
            {
                OnToiletVideoClicked(hit);
            }
        }
    }
    
    private void OnToiletVideoClicked(RaycastHit hit)
    {
        Debug.Log($"ToiletRaycasting: Toilet video '{gameObject.name}' clicked! Opening URL: {websiteURL}");
        
        // Set processing flag to prevent multiple rapid clicks
        isProcessingClick = true;
        
        // Provide visual feedback if enabled
        if (enableClickFeedback)
        {
            StartCoroutine(PlayClickFeedback(hit.point));
        }
        
        // Open the website
        OpenWebsite();
        
        // Reset processing flag after a short delay
        Invoke(nameof(ResetClickProcessing), 0.5f);
    }
    
    private void OpenWebsite()
    {
        if (string.IsNullOrEmpty(websiteURL))
        {
            Debug.LogError("ToiletRaycasting: Cannot open website - URL is empty!");
            return;
        }
        
        try
        {
            Debug.Log($"ToiletRaycasting: Opening website: {websiteURL}");
            Application.OpenURL(websiteURL);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ToiletRaycasting: Failed to open URL '{websiteURL}'. Error: {e.Message}");
        }
    }
    
    private System.Collections.IEnumerator PlayClickFeedback(Vector3 worldPosition)
    {
        // Simple visual feedback - you can expand this with particle effects, sound, etc.
        Debug.Log($"ToiletRaycasting: Playing click feedback for '{gameObject.name}' at position {worldPosition}");
        
        // Example: Scale effect (optional - you can remove this if not needed)
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.1f;
        
        // Scale up
        float halfDuration = feedbackDuration * 0.5f;
        float timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = timer / halfDuration;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
        
        // Scale back down
        timer = 0f;
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;
            float t = timer / halfDuration;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }
        
        // Ensure we're back to original scale
        transform.localScale = originalScale;
        
        Debug.Log($"ToiletRaycasting: Click feedback completed for '{gameObject.name}'.");
    }
    
    private void ResetClickProcessing()
    {
        isProcessingClick = false;
    }
    
    // Public method to manually trigger the toilet video click (useful for testing)
    public void TriggerToiletVideoClick()
    {
        Debug.Log($"ToiletRaycasting: Manually triggering click for '{gameObject.name}'.");
        OnToiletVideoClicked(new RaycastHit()); // Pass empty hit for manual trigger
    }
    
    // Public method to change the target URL at runtime
    public void SetWebsiteURL(string newURL)
    {
        websiteURL = newURL;
        Debug.Log($"ToiletRaycasting: Website URL for '{gameObject.name}' changed to: {websiteURL}");
    }
}
