using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;

public class AndroidCodeReaderToggleableSample : MonoBehaviour {

    [SerializeField]
    private ARCameraManager cameraManager;
    [SerializeField]
    private string lastResult = "";

    [SerializeField]
    private TextMeshProUGUI debugResultText;

    private Texture2D cameraImageTexture;
    private bool scanningEnabled = false;

    private IBarcodeReader barcodeReader = new BarcodeReader {
        AutoRotate = false,
        Options = new ZXing.Common.DecodingOptions {
            TryHarder = false
        }
    };

    private Result result;

    private void OnEnable() {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    private void OnDisable() {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs) {

        if (!scanningEnabled) {
            return;
        }

        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image)) {
            return;
        }

        var conversionParams = new XRCpuImage.ConversionParams {
            // Get the entire image.
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 2.
            outputDimensions = new Vector2Int(image.width / 2, image.height / 2),

            // Choose RGBA format.
            outputFormat = TextureFormat.RGBA32,

            // Flip across the vertical axis (mirror image).
            transformation = XRCpuImage.Transformation.MirrorY
        };

        // See how many bytes you need to store the final image.
        int size = image.GetConvertedDataSize(conversionParams);

        // Allocate a buffer to store the image.
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Extract the image data
        image.Convert(conversionParams, buffer);

        // The image was converted to RGBA32 format and written into the provided buffer
        // so you can dispose of the XRCpuImage. You must do this or it will leak resources.
        image.Dispose();

        // At this point, you can process the image, pass it to a computer vision algorithm, etc.
        // In this example, you apply it to a texture to visualize it.

        // You've got the data; let's put it into a texture so you can visualize it.
        cameraImageTexture = new Texture2D(
            conversionParams.outputDimensions.x,
            conversionParams.outputDimensions.y,
            conversionParams.outputFormat,
            false);

        cameraImageTexture.LoadRawTextureData(buffer);
        cameraImageTexture.Apply();

        // Done with your temporary data, so you can dispose it.
        buffer.Dispose();

        // Detect and decode the barcode inside the bitmap
        result = barcodeReader.Decode(cameraImageTexture.GetPixels32(), cameraImageTexture.width, cameraImageTexture.height);

        // Do something with the result
        if (result != null) {
            lastResult = result.Text + " " + result.BarcodeFormat;
            debugResultText.text = lastResult;
            Debug.Log("DECODED TEXT FROM ANDROID CAMERA: " + lastResult);
        }
    }

    public void ToggleScanning() {
        scanningEnabled = !scanningEnabled;
    }

    public string GetCurrentState()
    {
        return "Is Scanner running? - " + scanningEnabled;
    }

    public string GetLastResult()
    {
        return lastResult;
    }
}
