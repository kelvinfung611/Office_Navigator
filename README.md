# Office Navigator

An augmented reality (AR) navigation application built with Unity that helps users navigate through office spaces using XR technology. The app provides turn-by-turn navigation to various points of interest (POIs) such as offices, meeting rooms, facilities, and amenities within a building.

## 🎯 High-Level Overview

**Office Navigator** is an immersive AR navigation system designed for indoor office environments. Built on Unity's XR Interaction Toolkit and AR Foundation, this application transforms your mobile device into a personal office guide. Users can select destinations from an intuitive UI menu, and the app provides real-time AR navigation overlays to guide them through the physical space.

### Key Features
- **AR-Based Navigation**: Leverages device cameras and sensors for spatial awareness
- **Interactive UI**: Modern UI Toolkit-based interface with visual feedback and animations
- **Multiple Destinations**: Navigate to 12+ predefined locations including executive offices, meeting rooms, and facilities
- **Interactive Elements**: Tap-to-interact functionality for embedded video content and external links
- **Billboard Components**: UI elements and media that dynamically face the user's camera
- **Spatial Localization**: Real-time scanning and positioning system for accurate navigation

### Technology Stack
- **Unity Engine**: Real-time 3D development platform
- **XR Interaction Toolkit**: High-level component-based interaction system for VR/AR
- **AR Foundation**: Cross-platform AR framework
- **UI Toolkit**: Runtime UI system using UXML/USS (Unity's web-inspired UI framework)
- **Input System**: Modern input handling supporting touch, mouse, and XR controllers

---

## 📋 Architecture Overview

The application follows a modular architecture with clear separation of concerns:

```
User Interaction → Menu System → Navigation Controller → AR Visualization → Spatial Tracking
```

1. **User Interface Layer**: Handles user input and displays navigation options
2. **Navigation Logic Layer**: Manages POI selection and route initialization
3. **AR Rendering Layer**: Overlays navigation cues in the physical environment
4. **Interaction Layer**: Processes raycasting and touch/click interactions
5. **Camera Management Layer**: Handles billboard effects and look-at behaviors

---

## 🧩 Major Components

### 1. **MenuController** (`MenuController.cs`)
**Purpose**: Central UI controller managing the navigation menu system and user flow.

**Key Responsibilities**:
- Manages UI Toolkit VisualElements for multi-page navigation interface
- Handles POI selection through a dictionary mapping UI elements to destination names
- Controls UI page transitions (welcome screen → destination selection → scanning → navigation)
- Implements scanner animation effects for spatial localization feedback
- Coordinates with NavigationUIController to initiate turn-by-turn navigation

**Supported Destinations**:
- Executive offices: CEO, CIO, CFO, COO
- Meeting spaces: Meeting rooms 2, 3, 4; Training room
- Facilities: Toilet 6/F, Pantry, Printing room, IT department

**UI Flow**:
```
Get Started → Select Destination → Localization Scan → Begin Navigation
```

**Integration Points**:
- Uses Unity's **UI Toolkit** (UIDocument, VisualElement API) for runtime UI
- Communicates with NavigationUIController via static state variables
- Provides visual feedback through USS-styled elements (scanner effects, pop-up messages)

---

### 2. **ToiletRaycasting** (`tolietRaycasting.cs`)
**Purpose**: Handles interactive raycasting for clickable video/media elements in AR space.

**Key Responsibilities**:
- Performs raycasting from camera to detect interactive objects in the scene
- Supports both mouse clicks (editor testing) and touch input (mobile AR)
- Opens URLs in external browsers when interactive media is clicked
- Provides visual click feedback effects with configurable duration
- Includes comprehensive debugging and visualization tools

**Technical Implementation**:
- Uses **Input System Actions** (more reliable for mobile/AR than legacy Input Manager)
- Implements `InputAction` for tap detection: supports both `<Mouse>/leftButton` and `<Touchscreen>/primaryTouch/press`
- Configurable raycast distance and layer masks for precise interaction control
- Coroutine-based click feedback system

**Use Cases**:
- Interactive video displays embedded in the office environment
- Clickable informational posters or screens
- AR content that links to external resources

**Input Handling**:
```csharp
tapAction = new InputAction("Tap", InputActionType.Button);
positionAction = new InputAction("Position", InputActionType.Value, "<Pointer>/position");
```

---

### 3. **LookAtScript** (`LookAtScript.cs`)
**Purpose**: Billboard component that makes objects always face the player camera.

**Key Responsibilities**:
- Continuously rotates objects to face the player's camera
- Supports Y-axis constraint for horizontal-only rotation (prevents tilting)
- Automatically finds and tracks the main camera
- Provides smooth "billboard" effect for UI panels and media

**Configuration Options**:
- `constrainYAxis`: When enabled, only rotates horizontally (typical for AR UI panels)
- `enableDebugLogs`: Detailed logging for troubleshooting

**Technical Details**:
- Updates transform rotation every frame in `Update()`
- Maintains Y position to create natural billboard effect
- Fallback camera detection: tries `Camera.main` → `MainCamera` tag → any active camera

**Best Practices**:
Attach to any VisualElement or 3D object that should remain readable regardless of user position (navigation arrows, information panels, video screens).

---

### 4. **DogPicture** (`DogPicture.cs`)
**Purpose**: Specialized billboard script for decorative or informational images.

**Key Responsibilities**:
- Simplified version of LookAtScript for static image elements
- Always constrains Y-axis rotation for a flat billboard effect
- Minimal overhead for non-critical visual elements

**Use Case**:
Originally designed for decorative office elements (like pet photos), but can be used for any static image or poster that should face the user.

**Camera Finding Logic**:
```csharp
1. Camera.main
2. GameObject.FindGameObjectWithTag("MainCamera")
3. FindObjectOfType<Camera>()
```

---

## 🎨 UI System (Unity UI Toolkit)

The application uses **Unity's UI Toolkit** (the modern UXML/USS-based system) instead of legacy Canvas-based UI:

### Key Concepts:
- **UXML**: XML-like markup for UI structure (similar to HTML)
- **USS**: Styling sheets for visual appearance (similar to CSS)
- **VisualElement**: Base class for all UI elements (similar to DOM elements)
- **UIDocument**: Component that loads and displays UXML

### UI Elements Used:
- `VisualElement`: Container elements for layout
- `Button`: Interactive navigation controls
- `Label`: Text displays for messages and prompts
- Custom styled elements: Scanner animations, pop-up messages

### Styling Features:
- Transitions and animations for scanner effects
- Custom classes for visual states
- Color, opacity, and transform animations

---

## 🔧 XR Integration

### XR Interaction Toolkit Integration:
While the provided scripts don't directly show XROrigin or AR Raycast components, the application is designed to work with:

- **AR Session**: Manages AR system lifecycle
- **AR Session Origin**: Defines the coordinate system for AR tracking
- **XR Ray Interactor**: Enables XR controller/hand-based UI interactions
- **XRUIInputModule**: Replaces standard EventSystem input for XR compatibility

### Input System Architecture:
The app uses Unity's **Input System** package for cross-platform input:
- **Touch**: Primary input for mobile AR
- **Mouse**: Fallback for editor testing
- **XR Controllers**: Future support for AR headsets

---

## 📱 Deployment

### Build Configuration:
- **Platform**: Android (APK available as `office.apk`)
- **XR Settings**: Configured in `Assets/XR/XRGeneralSettingsPerBuildTarget.asset`
- **Scene**: Main scene located at `Assets/Scenes/SampleScene.unity`

### Requirements:
- Unity 2021.3 or later (LTS recommended)
- AR Foundation package
- XR Interaction Toolkit package
- Input System package (enable in Project Settings)
- Mobile device with ARCore (Android) or ARKit (iOS) support

---

## 🚀 Getting Started

### For Developers:
1. Open project in Unity Editor
2. Ensure required packages are installed (AR Foundation, XR Interaction Toolkit)
3. Open `Assets/Scenes/SampleScene.unity`
4. Configure XR settings for your target platform
5. Build and deploy to device or test in Unity Editor

### For Users:
1. Install the APK on an ARCore-compatible Android device
2. Grant camera and location permissions
3. Launch the app and follow on-screen instructions
4. Select your destination and wait for localization scan
5. Follow AR navigation cues to your destination

---

## 📝 Notes

- **Git Ignore**: Configured to exclude large media files (`SHINE@ISS*.mp4`) and build archives (`office.zip`)
- **Performance**: Optimized for mobile AR with efficient raycasting and UI updates
- **Extensibility**: Modular architecture allows easy addition of new POIs and interactive elements

---

## 🔮 Future Enhancements

Potential areas for expansion:
- Multi-floor navigation support
- Voice-guided turn-by-turn instructions
- Real-time occupancy detection for meeting rooms
- Integration with calendar systems for meeting reminders
- Accessibility features (high contrast modes, audio cues)

---

## 📄 License

*Add your license information here*

---

**Built with ❤️ using Unity and XR Interaction Toolkit**
