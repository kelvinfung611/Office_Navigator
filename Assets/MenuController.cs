using UnityEngine;
using UnityEngine.UIElements;
using System.Collections; // Required for IEnumerator

public class MenuController : MonoBehaviour
{
    public VisualElement ui;
    public Button saveButton; // Button on BodyFirst to initiate scanning

    [Header("External Controllers")]
    public GameObject navigationUIControllerObject; 

    private VisualElement bodyFirst;
    private VisualElement bodySecond;
    private VisualElement bodyThird;
    private VisualElement bodyFourth;
    private VisualElement popUpMsg; // Reference to the PopUpMsg element
    private Label popUpMsgLabel; // Reference to the Label inside PopUpMsg

    // Buttons for navigation
    private Button myCarButton; 
    private Button backToMapButton; 
    private Button goToCarButton; 
    private Button myLocationButton; 

    private void Awake()
    {
        ui = GetComponent<UIDocument>().rootVisualElement;

        bodyFirst = ui.Q<VisualElement>("BodyFirst");
        bodySecond = ui.Q<VisualElement>("BodySecond");
        bodyThird = ui.Q<VisualElement>("BodyThird");
        bodyFourth = ui.Q<VisualElement>("BodyFourth");
        popUpMsg = ui.Q<VisualElement>("PopUpMsg");

        // Null checks for all bodies and popup
        if (bodyFirst == null) Debug.LogError("BodyFirst element not found. Check UXML name.");
        if (bodySecond == null) Debug.LogError("BodySecond element not found. Check UXML name.");
        if (bodyThird == null) Debug.LogError("BodyThird element not found. Check UXML name.");
        if (bodyFourth == null) Debug.LogError("BodyFourth element not found. Check UXML name.");
        if (popUpMsg == null) Debug.LogError("PopUpMsg element not found. Check UXML name.");
        else
        {
            // Assuming the Label inside PopUpMsg is the first Label child or has a specific name/class
            // For robustness, it's best to give the Label a unique name in UXML, e.g., "pop-up-text-label"
            // popUpMsgLabel = popUpMsg.Q<Label>("pop-up-text-label"); 
            popUpMsgLabel = popUpMsg.Q<Label>(className: "pop-up-label-style"); // Query by class if no name
            if (popUpMsgLabel == null) Debug.LogError("Label inside PopUpMsg not found. Check UXML structure or class name.");
        }

        // Query navigation buttons
        saveButton = ui.Q<Button>("saveButton");
        myCarButton = ui.Q<Button>("my-car-button");
        backToMapButton = ui.Q<Button>("back-to-map-button");
        goToCarButton = ui.Q<Button>("go-to-car-button");
        myLocationButton = ui.Q<Button>("my-location-button");

        if (saveButton == null) Debug.LogError("SaveButton not found. Check UXML name.");
        if (myCarButton == null) Debug.LogError("my-car-button not found. Check UXML name.");
        if (backToMapButton == null) Debug.LogError("back-to-map-button not found. Check UXML name.");
        if (goToCarButton == null) Debug.LogError("go-to-car-button not found. Check UXML name.");
        if (myLocationButton == null) Debug.LogError("my-location-button not found. Check UXML name.");

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
        ShowPage(bodyFirst);
        if(popUpMsg != null) popUpMsg.RemoveFromClassList("visible"); // Ensure popup is hidden at start
    }

    private void OnEnable()
    {
        if (saveButton != null) saveButton.RegisterCallback<ClickEvent>(OnSaveButtonClicked);
        if (myCarButton != null) myCarButton.RegisterCallback<ClickEvent>(OnMyCarButtonClicked);
        if (backToMapButton != null) backToMapButton.RegisterCallback<ClickEvent>(OnBackToMapButtonClicked);
        if (goToCarButton != null) goToCarButton.RegisterCallback<ClickEvent>(OnGoToCarButtonClicked);
        if (myLocationButton != null) myLocationButton.RegisterCallback<ClickEvent>(OnMyLocationButtonClicked);
    }

    private void OnDisable()
    {
        if (saveButton != null) saveButton.UnregisterCallback<ClickEvent>(OnSaveButtonClicked);
        if (myCarButton != null) myCarButton.UnregisterCallback<ClickEvent>(OnMyCarButtonClicked);
        if (backToMapButton != null) backToMapButton.UnregisterCallback<ClickEvent>(OnBackToMapButtonClicked);
        if (goToCarButton != null) goToCarButton.UnregisterCallback<ClickEvent>(OnGoToCarButtonClicked);
        if (myLocationButton != null) myLocationButton.UnregisterCallback<ClickEvent>(OnMyLocationButtonClicked);
        
        StopAllCoroutines();
    }

    private void ShowPage(VisualElement pageToShow)
    {
        if (bodyFirst != null) bodyFirst.AddToClassList("hidden");
        if (bodySecond != null) bodySecond.AddToClassList("hidden");
        if (bodyThird != null) bodyThird.AddToClassList("hidden");
        if (bodyFourth != null) bodyFourth.AddToClassList("hidden");

        if (pageToShow != null)
        {
            pageToShow.RemoveFromClassList("hidden");
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

    private void OnSaveButtonClicked(ClickEvent evt)
    {
        Debug.Log("Save button clicked! Transitioning to QR Scan page.");
        ShowPage(bodySecond);
        StartCoroutine(SimulateQRScanAndProceedToSaveSpot());
    }

    private IEnumerator SimulateQRScanAndProceedToSaveSpot()
    {
        Debug.Log("Simulating QR code scan for saving spot...");
        yield return new WaitForSeconds(3f); 
        Debug.Log("Scan complete. Transitioning to Mall Explore page.");
        ShowPage(bodyThird);
        UpdateParkingInfo("Level P2, Zone B", "Saved just now"); // Example parking info
        StartCoroutine(ShowPopUpMessageCoroutine("Parking spot saved", 2.5f));
    }

    private void OnMyLocationButtonClicked(ClickEvent evt)
    {
        Debug.Log("'My Location' button clicked. Transitioning to QR Scan page to update location.");
        ShowPage(bodySecond);
        StartCoroutine(SimulateLocationUpdateAndProceed());
    }

    private IEnumerator SimulateLocationUpdateAndProceed()
    {
        Debug.Log("Simulating QR code scan for location update...");
        yield return new WaitForSeconds(3f); // Simulate scan time
        Debug.Log("Location update scan complete. Transitioning back to Mall Explore page.");
        ShowPage(bodyThird);
        // Here you might update some internal state about the user's current location if needed
        StartCoroutine(ShowPopUpMessageCoroutine("location updated", 2.5f));
    }

    private void OnMyCarButtonClicked(ClickEvent evt)
    {
        Debug.Log("'My Car' button clicked. Transitioning to Navigate to Car page.");
        ShowPage(bodyFourth);
    }

    private void OnBackToMapButtonClicked(ClickEvent evt)
    {
        Debug.Log("'Back to Mall Map' button clicked. Transitioning to Mall Explore page.");
        ShowPage(bodyThird);
    }

    private void OnGoToCarButtonClicked(ClickEvent evt)
    {
        Debug.Log("'Go to My Car' button clicked. Hiding MenuController UI and activating NavigationUIController.");
        ShowPage(null); 

        if (navigationUIControllerObject != null)
        {
            navigationUIControllerObject.SetActive(true); 
        }
        else
        {
            Debug.LogError("NavigationUIController GameObject is not assigned in MenuController. Cannot activate it.");
        }
    }
    
    public void UpdateParkingInfo(string location, string time)
    {
        if (bodyFourth != null)
        {
            var locationLabel = bodyFourth.Q<Label>("parking-location-text");
            var timeLabel = bodyFourth.Q<Label>("parking-time-text");

            if (locationLabel != null) locationLabel.text = location;
            else Debug.LogWarning("parking-location-text Label not found in BodyFourth.");

            if (timeLabel != null) timeLabel.text = time;
            else Debug.LogWarning("parking-time-text Label not found in BodyFourth.");
        }
    }
}
