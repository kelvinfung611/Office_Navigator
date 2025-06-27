using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

public class MenuController : MonoBehaviour
{
    public VisualElement ui;

    [Header("External Controllers")]
    public GameObject navigationUIControllerObject;

    // Shared navigation state - NavigationUIController will check this in its Start()
    public static bool shouldStartNavigationOnInit = false;
    public static POI navigationPOI = null;

    private VisualElement bodySecond;
    private VisualElement bodyThird;
    private VisualElement getReadyPage;    private VisualElement popUpMsg; // Reference to the PopUpMsg element
    private Label popUpMsgLabel; // Reference to the Label inside PopUpMsg
    private VisualElement scannerLine;
    private VisualElement scannerGlow;
    private VisualElement scannerTrail1;
    private VisualElement scannerTrail2;
    private VisualElement scannerTrail3;

    // Buttons for navigation
    private Button getStartedButton;
    private Button goBackButtonScan;

    // Store the selected POI when GO button is clicked
    private POI selectedPOI;

    // Mapping between destination list items and POI names
    private Dictionary<int, string> destinationToPOIMapping = new Dictionary<int, string>()
    {
        {1, "CEO"}, // list-item-1 -> CEO POI
        {2, "CIO"}, // list-item-2 -> CTO POI
        {3, "CFO"}, // list-item-3 -> CFO POI
        {4, "COO"}, // list-item-4 -> COO POI
        {5, "Toliet 6/F"}, // list-item-5 -> Toilet POI
        {6, "Meeting room 3"}, // list-item-6 -> Meeting room 3 POI
        {7, "Meeting room 4"}, // list-item-7 -> Meeting room 4 POI
        {8, "Meeting room 2"}, // list-item-8 -> Meeting room 2 POI
        {9, "Training room"}, // list-item-9 -> Training room POI
        {10, "PnC room"}, // list-item-10 -> PnC room POI
        {11, "Printing room"}, // list-item-11 -> Printing room POI
        {12, "IT"} // list-item-12 -> Michael IT POI
    };

    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;

        bodySecond = ui.Q<VisualElement>("BodySecond");
        bodyThird = ui.Q<VisualElement>("BodyThird");        getReadyPage = ui.Q<VisualElement>("GetReadyPage");
        popUpMsg = ui.Q<VisualElement>("PopUpMsg");
        scannerLine = ui.Q<VisualElement>("scanner-line");
        scannerGlow = ui.Q<VisualElement>("scanner-glow");
        scannerTrail1 = ui.Q<VisualElement>("scanner-trail-1");
        scannerTrail2 = ui.Q<VisualElement>("scanner-trail-2");
        scannerTrail3 = ui.Q<VisualElement>("scanner-trail-3");

        // Null checks for all bodies and popup
        if (bodySecond == null) Debug.LogError("BodySecond element not found. Check UXML name.");
        if (bodyThird == null) Debug.LogError("BodyThird element not found. Check UXML name.");
        if (getReadyPage == null) Debug.LogError("GetReadyPage element not found. Check UXML name.");        if (popUpMsg == null) Debug.LogError("PopUpMsg element not found. Check UXML name.");
        if (scannerLine == null) Debug.LogError("scanner-line element not found. Check UXML name.");
        if (scannerGlow == null) Debug.LogError("scanner-glow element not found. Check UXML name.");
        if (scannerTrail1 == null) Debug.LogError("scanner-trail-1 element not found. Check UXML name.");
        if (scannerTrail2 == null) Debug.LogError("scanner-trail-2 element not found. Check UXML name.");
        if (scannerTrail3 == null) Debug.LogError("scanner-trail-3 element not found. Check UXML name.");
        else
        {
            popUpMsgLabel = popUpMsg.Q<Label>(className: "pop-up-label-style");
            if (popUpMsgLabel == null) Debug.LogError("Label inside PopUpMsg not found. Check UXML structure or class name.");
        }

        // Query navigation buttons
        getStartedButton = ui.Q<Button>("get-started-button");
        goBackButtonScan = ui.Q<Button>("go-back-button-scan");

        if (getStartedButton == null) Debug.LogError("get-started-button not found. Check UXML name.");
        if (goBackButtonScan == null) Debug.LogError("go-back-button-scan not found. Check UXML name.");

        if (navigationUIControllerObject != null)
        {
            navigationUIControllerObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("NavigationUIController GameObject is not assigned in the MenuController Inspector.");
        }
    }

    void Start()
    {
        ShowPage(bodyThird); // Start with the destination list
        if(popUpMsg != null) popUpMsg.RemoveFromClassList("visible");
    }

    private void OnEnable()
    {
        if (getStartedButton != null) getStartedButton.RegisterCallback<ClickEvent>(OnGetStartedClicked);
        if (goBackButtonScan != null) goBackButtonScan.RegisterCallback<ClickEvent>(OnGoBackClicked);

        // Register clicks for all destination list items (dynamically detect available items)
        for (int i = 1; i <= 20; i++) // Check up to 20 items to be safe
        {
            var listItem = ui.Q<VisualElement>($"list-item-{i}");
            if (listItem != null)
            {
                // Register click for the entire card
                listItem.RegisterCallback<ClickEvent>(OnDestinationItemSelected);
                
                // Also register click specifically for the GO button
                var goButton = listItem.Q<Button>();
                if (goButton != null)
                {
                    // Capture the current value of i in a local variable to avoid closure issues
                    int currentIndex = i;
                    goButton.RegisterCallback<ClickEvent>(evt => OnDestinationGoButtonClicked(evt, currentIndex));
                }
                Debug.Log($"Registered click events for list-item-{i}");
            }
            else
            {
                // Stop when we can't find more items
                Debug.Log($"Found {i-1} destination items in UI");
                break;
            }
        }
    }

    private void OnDisable()
    {
        if (getStartedButton != null) getStartedButton.UnregisterCallback<ClickEvent>(OnGetStartedClicked);
        if (goBackButtonScan != null) goBackButtonScan.UnregisterCallback<ClickEvent>(OnGoBackClicked);

        // Unregister clicks for all destination list items (dynamically detect available items)
        for (int i = 1; i <= 20; i++) // Check up to 20 items to be safe
        {
            var listItem = ui.Q<VisualElement>($"list-item-{i}");
            if (listItem != null)
            {
                listItem.UnregisterCallback<ClickEvent>(OnDestinationItemSelected);
            }
            else
            {
                // Stop when we can't find more items
                break;
            }
        }
        
        StopAllCoroutines();
    }

    private void ShowPage(VisualElement pageToShow)
    {
        if (bodySecond != null) bodySecond.AddToClassList("hidden");
        if (bodyThird != null) bodyThird.AddToClassList("hidden");
        if (getReadyPage != null) getReadyPage.AddToClassList("hidden");

        if (scannerLine != null) 
        {
            scannerLine.RemoveFromClassList("scanning");
        }

        if (pageToShow != null)
        {
            pageToShow.RemoveFromClassList("hidden");
        }
    }

    private void OnDestinationItemSelected(ClickEvent evt)
    {
        Debug.Log("Destination item clicked! (Card selection - no action taken)");
        // Remove automatic transition - only GO button should trigger navigation flow
    }

    private void OnDestinationGoButtonClicked(ClickEvent evt, int destinationIndex)
    {
        evt.StopPropagation(); // Prevent the card click event from firing
        
        Debug.Log($"GO button clicked for destination {destinationIndex}!");
        
        // Get the POI name from mapping
        if (destinationToPOIMapping.TryGetValue(destinationIndex, out string poiName))
        {
            // Find the POI in the augmented space
            POI targetPOI = FindPOIByName(poiName);
            Debug.Log($"Looking for POI: {poiName}");
            if (targetPOI != null)
            {
                Debug.Log($"POI '{poiName}' found. Transitioning to Get Ready page.");
                // Store the selected POI for later use when "Get Started" is clicked
                selectedPOI = targetPOI;
                // Transition to Get Ready page instead of starting navigation immediately
                ShowPage(getReadyPage);
            }
            else
            {
                Debug.LogError($"POI '{poiName}' not found in the scene!");
                // Show user-friendly error message
                StartCoroutine(ShowPopUpMessageCoroutine($"Destination '{poiName}' is currently unavailable", 3f));
            }
        }
        else
        {
            Debug.LogError($"No POI mapping found for destination index {destinationIndex}");
            // Show user feedback for invalid destination
            StartCoroutine(ShowPopUpMessageCoroutine("This destination is not available", 3f));
        }
    }

    private POI FindPOIByName(string poiName)
    {
        // First try to find through NavigationController's augmented space
        if (NavigationController.instance?.augmentedSpace != null)
        {
            var pois = NavigationController.instance.augmentedSpace.GetComponentsInChildren<POI>();
            foreach (var poi in pois)
            {
                if (poi.poiName.Equals(poiName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return poi;
                }
            }
        }
        
        // Fallback: search all POIs in the scene
        var allPOIs = FindObjectsByType<POI>(FindObjectsSortMode.None);
        foreach (var poi in allPOIs)
        {
            if (poi.poiName.Equals(poiName, System.StringComparison.OrdinalIgnoreCase))
            {
                return poi;
            }
        }
        
        return null;
    }    private void OnGetStartedClicked(ClickEvent evt)
    {
        Debug.Log("Get Started button clicked! Transitioning to QR Scan page.");
        ShowPage(bodySecond);
        if (scannerLine != null) 
        {
            StartCoroutine(AnimateScanner());
        }
        // Start the scan simulation and navigation with the selected POI
        StartCoroutine(SimulateQRScanAndNavigate());
    }    private IEnumerator AnimateScanner()
    {
        while (bodySecond != null && !bodySecond.ClassListContains("hidden"))
        {
            // Scanning down with afterimage effect
            for (float t = 0; t <= 1; t += Time.deltaTime * 2f) // Faster animation
            {
                if (scannerLine != null)
                {
                    float mainPos = Mathf.Lerp(-10f, 610f, t);
                    float trail1Pos = Mathf.Lerp(-10f, 610f, Mathf.Max(0, t - 0.05f));
                    float trail2Pos = Mathf.Lerp(-10f, 610f, Mathf.Max(0, t - 0.1f));
                    float trail3Pos = Mathf.Lerp(-10f, 610f, Mathf.Max(0, t - 0.15f));
                    float glowPos = Mathf.Lerp(-15f, 615f, Mathf.Max(0, t - 0.02f));

                    // Apply positions with sticking effect at edges
                    scannerLine.style.top = AddStickingEffect(mainPos, t);
                    if (scannerTrail1 != null) scannerTrail1.style.top = trail1Pos;
                    if (scannerTrail2 != null) scannerTrail2.style.top = trail2Pos;
                    if (scannerTrail3 != null) scannerTrail3.style.top = trail3Pos;
                    if (scannerGlow != null) scannerGlow.style.top = glowPos;
                }
                yield return null;
            }
            
            // Brief pause at bottom with enhanced glow
            yield return new WaitForSeconds(0.1f);
            
            // Scanning up with afterimage effect
            for (float t = 0; t <= 1; t += Time.deltaTime * 2f) // Faster animation
            {
                if (scannerLine != null)
                {
                    float mainPos = Mathf.Lerp(610f, -10f, t);
                    float trail1Pos = Mathf.Lerp(610f, -10f, Mathf.Max(0, t - 0.05f));
                    float trail2Pos = Mathf.Lerp(610f, -10f, Mathf.Max(0, t - 0.1f));
                    float trail3Pos = Mathf.Lerp(610f, -10f, Mathf.Max(0, t - 0.15f));
                    float glowPos = Mathf.Lerp(615f, -15f, Mathf.Max(0, t - 0.02f));

                    // Apply positions with sticking effect at edges
                    scannerLine.style.top = AddStickingEffect(mainPos, t);
                    if (scannerTrail1 != null) scannerTrail1.style.top = trail1Pos;
                    if (scannerTrail2 != null) scannerTrail2.style.top = trail2Pos;
                    if (scannerTrail3 != null) scannerTrail3.style.top = trail3Pos;
                    if (scannerGlow != null) scannerGlow.style.top = glowPos;
                }
                yield return null;
            }
            
            // Brief pause at top
            yield return new WaitForSeconds(0.1f);
        }
    }

    private float AddStickingEffect(float position, float t)
    {
        // Add slight sticking effect at the edges
        if (t < 0.05f) // Stick at top
        {
            return Mathf.Lerp(-10f, position, t * 20f);
        }
        else if (t > 0.95f) // Stick at bottom
        {
            return Mathf.Lerp(position, position + 5f, (t - 0.95f) * 20f);
        }
        return position;
    }

    private void OnGoBackClicked(ClickEvent evt)
    {
        Debug.Log("Go Back button clicked! Returning to destination list.");
        // Stop any running animations
        StopAllCoroutines();
        ShowPage(bodyThird);
    }

    public void ReturnToMainMenu()
    {
        // Public method that can be called by the navigation controller
        // when navigation is complete or cancelled
        Debug.Log("Returning to main menu from navigation...");
        if (navigationUIControllerObject != null)
        {
            navigationUIControllerObject.SetActive(false);
        }
        ui.style.display = DisplayStyle.Flex;
        ShowPage(bodyThird); // Return to destination list
    }

    private IEnumerator SimulateQRScanAndNavigate()
    {
        Debug.Log("Simulating QR code scan for navigation...");
        yield return new WaitForSeconds(5f); 
        Debug.Log("Scan complete. Starting navigation.");
        
        // Stop the scanner animation first
        StopAllCoroutines();
        
        // Check if we have a selected POI and navigation controller
        if (selectedPOI != null && NavigationController.instance != null)
        {
            Debug.Log($"Starting navigation to {selectedPOI.poiName}");
            
            // Check if navigationUIControllerObject is properly assigned
            if (navigationUIControllerObject != null)
            {
                Debug.Log("Activating navigation UI controller...");
                try
                {
                    // Get the NavigationUIController component
                    NavigationUIController navUIController = navigationUIControllerObject.GetComponent<NavigationUIController>();
                    if (navUIController != null)
                    {
                        Debug.Log("NavigationUIController component found, setting up shared navigation flags...");
                        
                        // Set shared static flags that NavigationUIController will check in its Start()
                        shouldStartNavigationOnInit = true;
                        navigationPOI = selectedPOI;
                        
                        // Activate the navigation UI object - this will trigger Start() and check our flags
                        navigationUIControllerObject.SetActive(true);
                        
                        Debug.Log($"Navigation flags set for {selectedPOI.poiName} - NavigationUIController will handle it in Start()");
                    }
                    else
                    {
                        Debug.LogError("NavigationUIController component not found on the assigned GameObject!");
                        // Fallback: set POI directly on NavigationController
                        NavigationController.instance.SetPOIForNavigation(selectedPOI);
                        navigationUIControllerObject.SetActive(true);
                        Debug.Log($"Navigation initiated directly through NavigationController for {selectedPOI.poiName}");
                    }
                    
                    ui.style.display = DisplayStyle.None; // Hide the main menu
                    Debug.Log("Navigation UI activated successfully.");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error activating navigation UI: {e.Message}");
                    Debug.LogError($"Stack trace: {e.StackTrace}");
                    // Fallback: stay on scanning page or show error
                    ShowPage(bodySecond);
                }
            }
            else
            {
                Debug.LogError("NavigationUIController GameObject is not assigned! Cannot start navigation.");
                // Fallback: show a message and stay on scanning page
                if (popUpMsg != null && popUpMsgLabel != null)
                {
                    StartCoroutine(ShowPopUpMessageCoroutine("Navigation system not available", 2f));
                }
                // Keep the scanning page active instead of going back to destination list
                ShowPage(bodySecond);
            }
        }
        else
        {
            Debug.LogError("No POI selected or NavigationController not found!");
            // Fallback: show error message and return to destination list
            if (popUpMsg != null && popUpMsgLabel != null)
            {
                StartCoroutine(ShowPopUpMessageCoroutine("Navigation not available", 2f));
            }
            ShowPage(bodyThird);
        }
    }

    private IEnumerator ShowPopUpMessageCoroutine(string message, float duration)
    {
        if (popUpMsg != null && popUpMsgLabel != null)
        {
            popUpMsgLabel.text = message;
            popUpMsg.AddToClassList("visible");
            yield return new WaitForSeconds(duration);
            popUpMsg.RemoveFromClassList("visible");
        }
        else
        {
            Debug.LogError("PopUpMsg or its label is not assigned. Cannot show message.");
        }
    }

    // Method to clear navigation flags (can be called by NavigationUIController)
    public static void ClearNavigationFlags()
    {
        shouldStartNavigationOnInit = false;
        navigationPOI = null;
        Debug.Log("Navigation flags cleared");
    }
}
