using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class MenuController : MonoBehaviour
{
    public VisualElement ui;

    [Header("External Controllers")]
    public GameObject navigationUIControllerObject;

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

        // Register clicks for all destination list items
        for (int i = 1; i <= 13; i++)
        {
            var listItem = ui.Q<VisualElement>($"list-item-{i}");
            if (listItem != null)
            {
                listItem.RegisterCallback<ClickEvent>(OnDestinationItemSelected);
            }
        }
    }

    private void OnDisable()
    {
        if (getStartedButton != null) getStartedButton.UnregisterCallback<ClickEvent>(OnGetStartedClicked);
        if (goBackButtonScan != null) goBackButtonScan.UnregisterCallback<ClickEvent>(OnGoBackClicked);

        for (int i = 1; i <= 13; i++)
        {
            var listItem = ui.Q<VisualElement>($"list-item-{i}");
            if (listItem != null)
            {
                listItem.UnregisterCallback<ClickEvent>(OnDestinationItemSelected);
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
        Debug.Log("Destination item clicked! Transitioning to Get Ready page.");
        ShowPage(getReadyPage);
    }    private void OnGetStartedClicked(ClickEvent evt)
    {
        Debug.Log("Get Started button clicked! Transitioning to QR Scan page.");
        ShowPage(bodySecond);
        if (scannerLine != null) 
        {
            StartCoroutine(AnimateScanner());
        }
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
        ShowPage(bodyThird);
    }

    private IEnumerator SimulateQRScanAndNavigate()
    {
        Debug.Log("Simulating QR code scan for navigation...");
        yield return new WaitForSeconds(3f); 
        Debug.Log("Scan complete. Starting navigation.");
        // Here you would typically transition to the actual AR navigation view
        // For now, we can just log it or go back to the main menu as an example
        if (navigationUIControllerObject != null)
        {
            ui.style.display = DisplayStyle.None; // Hide the main menu
            navigationUIControllerObject.SetActive(true); // Show the navigation UI
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
}
