using System;
using Windows.Kinect;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ZXing;
using ZXing.Common;
using ZXing.Multi.QrCode;

public class CalibrateScript : MonoBehaviour
{
    private bool isUsingKinect = true;
    private WebCamTexture camTexture;
        
    private KinectSensor sensor;
    private Texture2D kinectTexture;
        
    private byte[] colorData;
    private MultiSourceFrameReader reader;
    private GameObject[] boxes;

    public Camera cam;
    public RawImage rawImage;

    private Vector3 topLeftPos = new Vector3(227,  13);
    private Vector3 botRightPos = new Vector3(1082, 495);
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
    }

    private void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            SceneManager.LoadScene(0);
        }
        
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

        this.rawImage.texture = isUsingKinect ? kinectTexture : camTexture;
        
        print("TOPLEFT: " + topLeftPos.x + ", " + topLeftPos.y);
        print("BOTRIGHT: " + botRightPos.x + ", " + botRightPos.y);

        try
        {

            var barcodeReader = new QRCodeMultiReader();

            Result[] results;

            Color32LuminanceSource source;
            if (isUsingKinect)
            {
                source = new Color32LuminanceSource(kinectTexture.GetPixels32(),
                    kinectTexture.width, kinectTexture.height);
            }
            else
            {
                source = new Color32LuminanceSource(camTexture.GetPixels32(),
                    camTexture.width, camTexture.height);
            }

            BinaryBitmap bitmap = new BinaryBitmap(new HybridBinarizer(source));
            results = barcodeReader.decodeMultiple(bitmap);


            foreach (var result in results)
            {
                var resultPoints = result.ResultPoints;

                if (result.Text == "1")
                {
                    Vector3 vector3;
                    if (isUsingKinect)
                    {
                        vector3 = new Vector3(resultPoints[0].X / kinectTexture.width * Screen.width,
                            (resultPoints[0].Y / kinectTexture.height * Screen.height) );
                    }
                    else
                    {
                        vector3 = new Vector3(resultPoints[0].X / camTexture.width * Screen.width,
                            (resultPoints[0].Y / camTexture.height * Screen.height) );
                    }

                    this.topLeftPos = vector3;
                    QRObjectHandler.TopLeftPos = vector3;
                }
                else if (result.Text == "2")
                {
                    Vector3 vector3;
                    if (isUsingKinect)
                    {
                        vector3 = new Vector3(resultPoints[0].X / kinectTexture.width * Screen.width,
                            (resultPoints[0].Y / kinectTexture.height * Screen.height) );
                    }
                    else
                    {
                        vector3 = new Vector3(resultPoints[0].X / camTexture.width * Screen.width,
                            (resultPoints[0].Y / camTexture.height * Screen.height) );
                    }

                    this.botRightPos = vector3;
                    QRObjectHandler.BotRightPos = vector3;
                }
            }
        }
        catch (Exception ex)
        {
            
        }
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(topLeftPos.x,  topLeftPos.y, botRightPos.x - topLeftPos.x, botRightPos.y - topLeftPos.y), "Press space to confirm selection");
    }

    private void SetupCameraTexture()
    {
        camTexture = new WebCamTexture();

        if (camTexture != null)
        {
            camTexture.Play();
        }
    }

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
}