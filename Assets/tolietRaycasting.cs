using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Debug Settings")]
    [Tooltip("Enable detailed debug logging")]
    public bool enableDebugLogs = true;

    [Tooltip("Show raycast visualization in Scene view")]
    public bool showRaycastVisualization = true;

    // References
    private Camera playerCamera;
    private bool isProcessingClick = false;
    
    // Input Actions - more reliable for mobile/AR
    private InputAction tapAction;
    private InputAction positionAction;

    void Start()
    {
        // Get reference to the main camera by tag
        InitializeCamera();

        // Initialize Input Actions (better for mobile/AR)
        InitializeInputActions();

        // Validate collider setup
        InitializeCollider();

        // Validate overall setup
        ValidateSetup();
    }

    void OnEnable()
    {
        // Enable input actions when component is enabled
        tapAction?.Enable();
        positionAction?.Enable();
    }

    void OnDisable()
    {
        // Disable input actions when component is disabled
        tapAction?.Disable();
        positionAction?.Disable();
    }

    void OnDestroy()
    {
        // Clean up input actions
        tapAction?.Dispose();
        positionAction?.Dispose();
    }

    void Update()
    {
        // Handle input using Input Actions (more reliable for mobile/AR)
        HandleInputActions();
    }
    
    private void InitializeInputActions()
    {
        // Create Input Actions for tap and position
        // This approach works better on mobile and AR devices
        tapAction = new InputAction("Tap", InputActionType.Button);
        positionAction = new InputAction("Position", InputActionType.Value, "<Pointer>/position");

        // Add bindings for both mouse and touch
        tapAction.AddBinding("<Mouse>/leftButton");
        tapAction.AddBinding("<Touchscreen>/primaryTouch/press");

        // Enable the actions
        tapAction.Enable();
        positionAction.Enable();
        
        if (enableDebugLogs)
        {
            Debug.Log("ToiletRaycasting: Input Actions initialized for both mouse and touch input.");
        }
    }

    private void InitializeCamera()
    {
        // Find camera by MainCamera tag first (as requested)
        GameObject mainCameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCameraObject != null)
        {
            playerCamera = mainCameraObject.GetComponent<Camera>();
            if (enableDebugLogs)
                Debug.Log($"ToiletRaycasting: Found camera '{playerCamera.name}' using MainCamera tag.");
        }

        // Fallback to Camera.main if MainCamera tag didn't work
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera != null && enableDebugLogs)
            {
                Debug.Log($"ToiletRaycasting: Using Camera.main '{playerCamera.name}' as fallback.");
            }
        }

        // Final fallback: find any active camera
        if (playerCamera == null)
        {
            playerCamera = FindFirstObjectByType<Camera>();
            if (playerCamera != null && enableDebugLogs)
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
        // Check for colliders on self or children
        Collider[] colliders = GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            if (enableDebugLogs)
                Debug.Log($"ToiletRaycasting: Found {colliders.Length} collider(s) on '{gameObject.name}' or its children.");
        }
        else
        {
            Debug.LogError($"ToiletRaycasting: No Collider found on '{gameObject.name}' or its children! Please add a collider component.");
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

        if (GetComponentsInChildren<Collider>().Length == 0)
        {
            Debug.LogError($"ToiletRaycasting: No Collider on '{gameObject.name}' or its children! Please add a collider component.");
            setupValid = false;
        }
        else
        {
            // Additional check: warn if all colliders are disabled
            bool anyEnabled = false;
            foreach (var col in GetComponentsInChildren<Collider>())
            {
                if (col.enabled)
                {
                    anyEnabled = true;
                    break;
                }
            }
            if (!anyEnabled)
            {
                Debug.LogWarning($"ToiletRaycasting: All colliders on '{gameObject.name}' and children are disabled. Raycast detection will not work.");
            }
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

    private void HandleInputActions()
    {
        if (playerCamera == null || isProcessingClick) return;

        // Check if tap/click was performed this frame using Input Actions
        if (tapAction.WasPressedThisFrame())
        {
            // Get the current pointer position
            Vector2 inputPosition = positionAction.ReadValue<Vector2>();
            
            // Determine input source for debugging
            string inputSource = "Unknown";
            if (Mouse.current?.leftButton.wasPressedThisFrame == true)
            {
                inputSource = "Mouse";
            }
            else if (Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true)
            {
                inputSource = "Touch";
            }
            
            if (enableDebugLogs)
            {
                Debug.Log($"ToiletRaycasting: {inputSource} input detected at screen position {inputPosition}");
                Debug.Log($"ToiletRaycasting: Input detected via Input Actions. Starting raycast.");
            }
            
            PerformRaycast(inputPosition, inputSource);
        }
    }

    private void PerformRaycast(Vector3 screenPosition, string inputSource)
    {
        // Convert screen position to world ray
        Ray ray = playerCamera.ScreenPointToRay(screenPosition);

        if (enableDebugLogs)
            Debug.Log($"ToiletRaycasting: Performing raycast from {inputSource} at screen position {screenPosition}");

        // Draw the ray in Scene view for debugging
        if (showRaycastVisualization)
        {
            Debug.DrawRay(ray.origin, ray.direction * maxRaycastDistance, Color.red, 2f);
        }

        // Perform the raycast
        if (Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, raycastLayerMask))
        {
            if (enableDebugLogs)
                Debug.Log($"ToiletRaycasting: Raycast hit '{hit.collider.gameObject.name}' at distance {hit.distance} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

            // Check if we hit this object or one of its children
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                if (enableDebugLogs)
                    Debug.Log($"ToiletRaycasting: Hit target object '{gameObject.name}' or child! Triggering click.");
                OnToiletVideoClicked(hit);
            }
            else
            {
                if (enableDebugLogs)
                    Debug.Log($"ToiletRaycasting: Hit object '{hit.collider.gameObject.name}' is not the target '{gameObject.name}' or child.");
            }
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log($"ToiletRaycasting: Raycast from {inputSource} did not hit any objects within range {maxRaycastDistance}.");
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
        if (enableDebugLogs)
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

        if (enableDebugLogs)
            Debug.Log($"ToiletRaycasting: Click feedback completed for '{gameObject.name}'.");
    }

    private void ResetClickProcessing()
    {
        isProcessingClick = false;
    }

    // Alternative input method using OnMouseDown (simpler but requires specific setup)
    void OnMouseDown()
    {
        if (isProcessingClick) return;

        Debug.Log($"ToiletRaycasting: OnMouseDown triggered for '{gameObject.name}'!");
        OnToiletVideoClicked(new RaycastHit()); // Pass empty hit for OnMouseDown trigger
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

    // Public method to test input detection (updated for Input Actions)
    public void TestInputDetection()
    {
        Debug.Log($"=== ToiletRaycasting Input Test for '{gameObject.name}' ===");
        Debug.Log($"Camera found: {playerCamera != null} (Name: {(playerCamera != null ? playerCamera.name : "None")})");
        Debug.Log($"Colliders found: {GetComponentsInChildren<Collider>().Length}");
        Debug.Log($"Tap Action enabled: {tapAction?.enabled}");
        Debug.Log($"Position Action enabled: {positionAction?.enabled}");
        Debug.Log($"Processing click: {isProcessingClick}");

        // Test current input state
        if (tapAction != null)
        {
            Debug.Log($"Tap Action is pressed: {tapAction.IsPressed()}");
        }
        
        if (positionAction != null)
        {
            Vector2 currentPos = positionAction.ReadValue<Vector2>();
            Debug.Log($"Current pointer position: {currentPos}");
        }

        // Check device availability
        Debug.Log($"Mouse available: {Mouse.current != null}");
        Debug.Log($"Touchscreen available: {Touchscreen.current != null}");

        // Force trigger a raycast from screen center for testing
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);
        Debug.Log($"Testing raycast from screen center: {screenCenter}");
        PerformRaycast(screenCenter, "Manual Test");
    }
}