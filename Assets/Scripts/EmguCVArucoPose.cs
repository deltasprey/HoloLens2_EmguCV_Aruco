using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Emgu.CV;
using Emgu.CV.Aruco;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Microsoft.MixedReality.Toolkit;

public class EmguCVArucoPose : MonoBehaviour {
    public GameObject headsUpImg;
    public Transform poseCube;
    public int refreshRate = 24; // FPS
    public float markerLength = 0.08f; // meters

    Dictionary markerLookup;
    DetectorParameters markerParams;
    WebCamTexture webCam;
    Texture2D tex;
    Matrix<double> cameraMatrix, distCoeffs, rotationMatrix;

    byte[] pixelData;
    Mat img, rvecs, tvecs;
    Vector3 cubePosition, row1, row2;
    VectorOfInt ids; // name/id of the detected markers
    VectorOfVectorOfPointF corners, markerCorners; // corners of the detected marker

    void Start() {
        markerParams = new();
        img = new();
        ids = new();
        corners = new();
        rvecs = new();
        tvecs = new();

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

        webCam = new() { requestedWidth = GetComponent<Camera>().targetTexture.width };
        webCam.Play();

        tex = new(webCam.width, webCam.height);

        cameraMatrix = new Matrix<double>(3, 3);
        cameraMatrix[0, 0] = webCam.width;
        cameraMatrix[1, 1] = webCam.height;
        cameraMatrix[0, 2] = webCam.width / 2;
        cameraMatrix[1, 2] = webCam.height / 2;
        cameraMatrix[2, 2] = 1f;

        distCoeffs = new Matrix<double>(1, 5);
        rotationMatrix = new Matrix<double>(3, 3);

        CoreServices.DiagnosticsSystem.ShowDiagnostics = false;
        //headsUpImg.SetActive(false);

        StartCoroutine(LookTimer());
    }

    private void Update() { // Debugging only
        if (Input.GetKeyDown(KeyCode.Q)) {
            headsUpImg.SetActive(!headsUpImg.activeSelf);
        }
    }

    IEnumerator LookTimer() {
        while (true) {
            yield return new WaitForSeconds(1/(float) refreshRate);
            LookForMarker();
        }
    }

    void LookForMarker() {
        tex.SetPixels(webCam.GetPixels(0, 0, webCam.width, webCam.height));
        tex.Apply();
        headsUpImg.GetComponent<RawImage>().texture = tex;
        pixelData = tex.EncodeToPNG();
        CvInvoke.Imdecode(pixelData, ImreadModes.Grayscale, img);
        ArucoInvoke.DetectMarkers(img, markerLookup, corners, ids, markerParams);

        if (ids.Size > 0) {
            markerCorners = new();
            markerCorners.Push(corners[0]);
            ArucoInvoke.EstimatePoseSingleMarkers(markerCorners, markerLength, cameraMatrix, distCoeffs, rvecs, tvecs);

            cubePosition = new Vector3((float)(double)tvecs.GetData().GetValue(0, 0, 0), 
                                       (float)(double)tvecs.GetData().GetValue(0, 0, 1), 
                                       (float)(double)tvecs.GetData().GetValue(0, 0, 2));
            cubePosition.y *= -1;
            cubePosition = transform.parent.transform.TransformPoint(cubePosition);
            poseCube.position = cubePosition;

            CvInvoke.Rodrigues(rvecs, rotationMatrix);
            row1 = new Vector3((float)rotationMatrix.GetRow(1)[0, 0], (float)rotationMatrix.GetRow(1)[0, 1], (float)rotationMatrix.GetRow(1)[0, 2]);
            row2 = new Vector3((float)rotationMatrix.GetRow(2)[0, 0], (float)rotationMatrix.GetRow(2)[0, 1], (float)rotationMatrix.GetRow(2)[0, 2]);
            //Quaternion cubeRotation = Quaternion.LookRotation(row2, row1);
            Quaternion cubeRotation = Quaternion.LookRotation(new Vector3((float)(double)rvecs.GetData().GetValue(0, 0, 0),
                                                                          (float)(double)rvecs.GetData().GetValue(0, 0, 1),
                                                                          (float)(double)rvecs.GetData().GetValue(0, 0, 2)));

            //poseCube.rotation = transform.parent.transform.rotation * cubeRotation;
            poseCube.rotation = cubeRotation;
            //poseCube.rotation = Quaternion.Euler(cubeRotation.eulerAngles.x, cubeRotation.eulerAngles.y, cubeRotation.eulerAngles.z);
        }
    }
}