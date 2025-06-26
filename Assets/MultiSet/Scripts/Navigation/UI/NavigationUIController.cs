using UnityEngine;
using TMPro;
using MultiSet;

/**
 * Handles the navigation UI state and input.
 */
public class NavigationUIController : MonoBehaviour
{
    public static NavigationUIController instance;

    private MapLocalizationManager mapLocalizationManager;

    [SerializeField]
    public GameObject navigationUI;

    [SerializeField]
    public GameObject CaptureButton;

    [Tooltip("Label to show remaining distance")]
    public TextMeshProUGUI remainingDistance;

    [Tooltip("Button to stop navigation")]
    public GameObject stopButton;

    [Tooltip("SelectList where POIs are shown")]
    public SelectList poiList;

    [Tooltip("Parent GameObject of POIs selection UI")]
    public GameObject DestinationSelectUI;

    [Tooltip("Label to show name of current destination")]
    public TextMeshProUGUI destinationName;

    [Tooltip("Parent GameObject of navigation progress slider")]
    public GameObject navigationProgressSlider;

    [Tooltip("Navigation Path Material")]
    public Material material;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        CaptureButton.SetActive(true);
        navigationUI.SetActive(true);
        ShowNavigationUIElements(false);
        DestinationSelectUI.SetActive(false);

        destinationName.text = "";
        mapLocalizationManager = FindFirstObjectByType<MapLocalizationManager>();
        if (mapLocalizationManager == null)
        {
            Debug.LogError("MapLocalizationManager not found in the scene. Ensure it is present. Cannot start localization.");
            return; // Exit if critical component is missing
        }
                Debug.Log("NavigationUIController active. Requesting AR Localization to start.");
        if (NavigationController.instance != null) // Check if NavigationController is ready
        {
            if(!string.IsNullOrEmpty(mapLocalizationManager?.mapOrMapsetCode))
            {
                mapLocalizationManager.LocalizeFrame();
                Debug.Log("AR localization process initiated by NavigationUIController.");
                // You might want to show some "Localizing..." UI here.
                // The DestinationSelectUI should typically only be shown after successful localization
                // and if the user needs to select a destination. For "Go to My Car",
                // the destination is already known, so you might directly proceed to navigation setup
                // once localization is confirmed.
            }
            else
            {
                Debug.LogError("Map or Mapset code is not set in MapLocalizationManager. Cannot start AR localization.");
            }
        }
        else
        {
            Debug.LogError("NavigationController instance is not available. Cannot explicitly start AR localization from UI.");
        }
        // --- END OF AR LOCALIZATION TRIGGER ---
    }

    void Update()
    {
        HandleNavigationState();
        UpdateRemainingDistance();
    }

    // handles the 
    void HandleNavigationState()
    {
        if (NavigationController.instance.IsCurrentlyNavigating())
        {
            destinationName.text = NavigationController.instance.currentDestination.poiName;
            return;
        }
        destinationName.text = "";
    }

    /**
     * Toggles visibility of destination select UI.
     */
    public void ToggleDestinationSelectUI()
    {
        DestinationSelectUI.SetActive(!DestinationSelectUI.activeSelf);

        if (!DestinationSelectUI.activeSelf)
        {
            poiList.ResetPOISearch();
            return;
        }

        poiList.RenderPOIs();
    }

    public void ResetPoiSearch()
    {
        poiList.ResetPOISearch();
    }

    public void RenderPoiCall()
    {
        poiList.RenderPOIs();
    }

    // User clicked to start navigation. Is called from ListItemUI.cs
    public void ClickedStartNavigation(POI poi)
    {
        NavigationController.instance.SetPOIForNavigation(poi);
        ToggleDestinationSelectUI();

        ShowNavigationUIElements(true);
    }

    // User clicked to stop navigation
    public void ClickedStopButton()
    {
        ShowNavigationUIElements(false);
        NavigationController.instance.StopNavigation();
    }

    // toggle visibility of navigation UI elements
    void ShowNavigationUIElements(bool isVisible)
    {
        // for navigation
        navigationProgressSlider.SetActive(isVisible);
        stopButton.SetActive(isVisible);
    }

    // Update info about remaining distance.
    void UpdateRemainingDistance()
    {
        if (!NavigationController.instance.IsCurrentlyNavigating())
        {
            remainingDistance.SetText("");
            return;
        }

        int distance = PathEstimationUtils.instance.getRemainingDistanceMeters();
        string distanceText = distance + "";

        if (distance > 1)
        {
            if (material != null)
                material.SetFloat("_PathLength", distance);
        }
        if (distance <= 1)
        {
            distanceText += " m remaining";
        }
        else
        {
            distanceText += " m remaining";
        }
        remainingDistance.text = distanceText;
    }

    // Show arrival state, is called from NavigationController.cs
    public void ShowArrivedState()
    {
        ShowNavigationUIElements(false);
        ToastManager.Instance.ShowAlert("You arrived at the destination!");
    }
}
