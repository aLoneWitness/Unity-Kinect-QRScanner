using System;
using Windows.Kinect;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;
using ZXing.Multi.QrCode;
/// <summary>
/// This module provides support for mapping QR code 2D Vector locations
/// from ScreenSpace view to the game view and map it to specified objects.
/// </summary>
public class QRObjectHandler : MonoBehaviour
{
    // Start is called before the first frame update
    private bool isUsingKinect = false;
    private WebCamTexture camTexture;

    private KinectSensor sensor;
    private Texture2D kinectTexture;

    private Texture2D croppedTexture;
        
    private byte[] colorData;
    private MultiSourceFrameReader reader;
    private GameObject[] boxes;

    public Camera cam;
    public RawImage rawImage;

    public static Vector3 TopLeftPos = new Vector3(300, 0);
    public static Vector3 BotRightPos = new Vector3(1650, 800);
    private void Start()
    {
        if(isUsingKinect)
        {
            SetupKinectTexture();
        }
        else
        {
            SetupCameraTexture();
        }
        boxes = GameObject.FindGameObjectsWithTag("IronBlock");
    }

    /// <summary>
    /// Sets up the Camera Texture when isUsingKinect is set to false
    /// This results in the source of the texture being the users webcam
    /// </summary>
    private void SetupCameraTexture()
    {
        camTexture = new WebCamTexture();

        if (camTexture != null)
        {
            camTexture.Play();
        }
    }
    
    /// <summary>
    /// Sets up the Kinect Texture when isUsingKinect is set to true
    /// This results in the source of the texture being the Kinect's RGB sensor.
    /// </summary>
    private void SetupKinectTexture()
    {
        sensor = KinectSensor.GetDefault();

        if(sensor != null)
        {
            reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color);
            
            var colorFrameDescription = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            kinectTexture = new Texture2D(colorFrameDescription.Width, colorFrameDescription.Height, TextureFormat.RGBA32, false);
            colorData = new byte[colorFrameDescription.BytesPerPixel * colorFrameDescription.LengthInPixels];
                
            if(!sensor.IsOpen)
            {
                sensor.Open();
            }

        }
    }

    private void Update()
    {
        // Retrieves Texture from kinect every frame
        if (isUsingKinect)
        {
            if (reader != null)
            {
                var frame = reader.AcquireLatestFrame();
                if (frame != null)
                {
                    var colorFrame = frame.ColorFrameReference.AcquireFrame();
                    if (colorFrame != null)
                    {
                        colorFrame.CopyConvertedFrameDataToArray(colorData, ColorImageFormat.Rgba);
                        kinectTexture.LoadRawTextureData(colorData);
                        kinectTexture.Apply();
                        colorFrame.Dispose();
                        colorFrame = null;
                    }
                    frame = null;
                }
            }
        }
            
        CropTexture();
            
        rawImage.texture = croppedTexture;

        try
        {

            var barcodeReader = new QRCodeMultiReader();

            Result[] results;

            Color32LuminanceSource source = new Color32LuminanceSource(croppedTexture.GetPixels32(),
                croppedTexture.width, croppedTexture.height);
            BinaryBitmap bitmap = new BinaryBitmap(new HybridBinarizer(source));
            results = barcodeReader.decodeMultiple(bitmap);


            foreach (var result in results)
            {
                var resultPoints = result.ResultPoints;
                var angle = GetAngle(resultPoints);
                var box = boxes[int.Parse(result.Text) - 1];
                if (box == null) return;

                var vector3 = new Vector3(resultPoints[0].X / croppedTexture.width * Screen.width,
                    -(resultPoints[0].Y / croppedTexture.height * Screen.height) + Screen.height);
                var worldVector = cam.ScreenToWorldPoint(vector3);
                worldVector.z = 0;
                box.transform.position = worldVector;
                box.transform.rotation = Quaternion.Euler(0, 0, (float) angle);
            }
        }
        catch (Exception ex)
        {
            print(ex.Message);
        }
    }

    /// <summary>
    /// Gets the currently selected texture and crops it to the CropRect set in config.
    /// </summary>  
    private void CropTexture()
    {
        Color[] colors;
        if (isUsingKinect)
        {
            colors = kinectTexture.GetPixels((int)TopLeftPos.x, (int)TopLeftPos.y, (int)(BotRightPos.x - TopLeftPos.x), (int)(BotRightPos.y - TopLeftPos.y));
        }
        else
        {
            colors = camTexture.GetPixels((int)TopLeftPos.x, (int)TopLeftPos.y, (int)(BotRightPos.x - TopLeftPos.x), (int)(BotRightPos.y - TopLeftPos.y)); 
        }


        croppedTexture = new Texture2D((int)(BotRightPos.x - TopLeftPos.x), (int)(BotRightPos.y - TopLeftPos.y), TextureFormat.ARGB32, false);
        croppedTexture.SetPixels(0, 0, croppedTexture.width, croppedTexture.height, colors, 0);
        croppedTexture.Apply();
    }
    
    /// <summary>
    /// Consumes resultPoint data from the QR Library, gets the 2 opposing 2D Vectors and calculates angle of object.
    /// </summary>
    /// <param name="points">Two opposing 2d vectors of a rectangle</param>
    /// <returns>Degrees of the angle</returns>
    private double GetAngle(ResultPoint[] points)
    {
        ResultPoint a = points[1];
        ResultPoint b = points[2];
        ResultPoint c = points[0];

        double z = Math.Abs(a.X - b.X);
        double x = Math.Abs(a.Y - b.Y);
        double theta = Math.Atan(x / z) * (180.0 / Math.PI);

        // Quadrants 0 and 1
        if(a.Y > b.Y) {
            if(a.X > b.X) {
                theta = 90 + (90 - theta);
            }
        }
        // Quadrants 2 or 3
        else {
            if (a.X > b.X) {
                theta = 180 + theta;
            }
            else {
                theta = 360 - theta;
            }
        }

        return theta;
    }
}