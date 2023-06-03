using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Emgu.CV.Aruco;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Microsoft.MixedReality.Toolkit;

public class EmguCVAruco : MonoBehaviour {
    public RawImage displayImg, headsUpImg;
    public Texture2D blankTex;
    public bool useWebcam = false, debug = false;
    public int refreshRate = 24; // FPS

    byte[] pixelData;
    WebCamTexture webCam;
    RenderTexture currentRT;
    Dictionary markerLookup;
    DetectorParameters markerParams;
    Camera Cam;
    Texture2D image, tex, tex2, tex3;
    Mat img, dispImg;
    VectorOfInt ids; // name/id of the detected markers
    VectorOfVectorOfPointF corners; // corners of the detected marker
    Color[] drawImage;

    void Start() {
        markerParams = new();
        img = new();
        dispImg = new();
        ids = new();
        corners = new();

        markerLookup = new(Dictionary.PredefinedDictionaryName.DictArucoOriginal);
        markerParams.MarkerBorderBits = 1;
        markerParams.AdaptiveThreshWinSizeMin = 3;
        markerParams.AdaptiveThreshWinSizeMax = 23;
        markerParams.AdaptiveThreshWinSizeStep = 10;
        markerParams.MinMarkerPerimeterRate = 0.03;
        markerParams.MaxMarkerPerimeterRate = 4.0;
        markerParams.PolygonalApproxAccuracyRate = 0.03;
        markerParams.MinCornerDistanceRate = 0.05;
        markerParams.MinDistanceToBorder = 3;
        markerParams.PerspectiveRemovePixelPerCell = 8;
        markerParams.PerspectiveRemoveIgnoredMarginPerCell = 0.13;

        //print(markerParams.AdaptiveThreshConstant); // 7
        //print(markerParams.MinMarkerDistanceRate); // 0.05
        //print(markerParams.CornerRefinementWinSize); // 5
        //print(markerParams.CornerRefinementMaxIterations); // 30
        //print(markerParams.CornerRefinementMinAccuracy); // 0.1
        //print(markerParams.MaxErroneousBitsInBorderRate); // 0.35
        //print(markerParams.MinOtsuStdDev); // 5.0
        //print(markerParams.ErrorCorrectionRate); // 0.6
        //print(markerParams.AprilTagQuadDecimate); // 
        //print(markerParams.AprilTagQuadSigma); // 0.8
        //print(markerParams.AprilTagMinClusterPixels); // 
        //print(markerParams.AprilTagMaxNmaxima); // 
        //print(markerParams.AprilTagCriticalRad); // 
        //print(markerParams.AprilTagMaxLineFitMse); // 
        //print(markerParams.AprilTagMinWhiteBlackDiff); // 
        //print(markerParams.AprilTagDeglitch); // 

        Cam = GetComponent<Camera>();
        tex = new(Cam.targetTexture.width, Cam.targetTexture.height);
        tex2 = new(Cam.targetTexture.width, Cam.targetTexture.height);
        tex3 = new(Cam.targetTexture.width, Cam.targetTexture.height, TextureFormat.RGBA32, false);
        //tex2.alphaIsTransparency = true; tex3.alphaIsTransparency = true;

        webCam = new() { requestedWidth = Cam.targetTexture.width };

        blankTex.Reinitialize(Cam.targetTexture.width, Cam.targetTexture.height);
        Color[] blankColors = blankTex.GetPixels(0, 0, blankTex.width, blankTex.height);
        for (int i = 0; i < blankTex.width * blankTex.height; i++) { blankColors[i] = Color.clear; }
        blankTex.SetPixels(0, 0, blankTex.width, blankTex.height, blankColors);
        blankTex.Apply();

        displayImg.gameObject.SetActive(debug);
        CoreServices.DiagnosticsSystem.ShowDiagnostics = debug;
        StartCoroutine(LookTimer());
    }

    IEnumerator LookTimer() {
        while (true) {
            yield return new WaitForSeconds(1/(float) refreshRate);
            LookForMarker();
        }
    }

    private void Update() { // Debugging only
        if (Input.GetKeyDown(KeyCode.Q)) {
            debug = !debug;
            displayImg.gameObject.SetActive(debug);
            CoreServices.DiagnosticsSystem.ShowDiagnostics = debug;
        }
    }

    void LookForMarker() {
        //Debug.Log(webCam.width);
        //Debug.Log(webCam.height);
        if (useWebcam) {
            if (tex.width != webCam.width) {
                tex.Reinitialize(webCam.width, webCam.height);
                tex2.Reinitialize(webCam.width, webCam.height);
                tex3.Reinitialize(webCam.width, webCam.height);
                webCam.Play();
            }

            tex.SetPixels(webCam.GetPixels(0, 0, webCam.width, webCam.height));
            tex.Apply();
            pixelData = tex.EncodeToPNG();
        } else {
            if (tex.width != Cam.targetTexture.width) {
                tex.Reinitialize(Cam.targetTexture.width, Cam.targetTexture.height);
                tex2.Reinitialize(Cam.targetTexture.width, Cam.targetTexture.height);
                tex3.Reinitialize(Cam.targetTexture.width, Cam.targetTexture.height);
                webCam.Stop();
            }

            currentRT = RenderTexture.active;
            RenderTexture.active = Cam.targetTexture;

            Cam.Render();

            image = new(Cam.targetTexture.width, Cam.targetTexture.height);
            image.ReadPixels(new Rect(0, 0, Cam.targetTexture.width, Cam.targetTexture.height), 0, 0);
            RenderTexture.active = currentRT;

            pixelData = image.EncodeToPNG();
            Destroy(image);    
        }

        if (!debug) {
            CvInvoke.Imdecode(pixelData, ImreadModes.Grayscale, img);
        } else {
            CvInvoke.Imdecode(pixelData, ImreadModes.AnyColor, img);
        }
        ArucoInvoke.DetectMarkers(img, markerLookup, corners, ids, markerParams);

        if (debug) {
            ArucoInvoke.DrawDetectedMarkers(img, corners, ids, new MCvScalar(255, 0, 255));
            ImageConversion.LoadImage(tex, img.ToImage<Bgr, byte>().ToJpegData());
            displayImg.texture = tex;

            tex2.SetPixels(blankTex.GetPixels(0, 0, blankTex.width, blankTex.height));
            tex2.Apply();
            CvInvoke.Imdecode(tex2.EncodeToPNG(), ImreadModes.AnyColor, dispImg);
            ArucoInvoke.DrawDetectedMarkers(dispImg, corners, ids, new MCvScalar(255, 0, 255));
            ImageConversion.LoadImage(tex2, dispImg.ToImage<Bgra, byte>().ToJpegData());

            drawImage = tex2.GetPixels(0, 0, tex2.width, tex2.height);
            for (int i = 0; i < tex2.width * tex2.height; i++) {
                if (drawImage[i].maxColorComponent < 0.2) {
                    drawImage[i] = Color.clear;
                }
            }
            tex3.SetPixels(0, 0, tex2.width, tex2.height, drawImage);
            tex3.Apply();
            headsUpImg.texture = tex3;
        }
    }
}