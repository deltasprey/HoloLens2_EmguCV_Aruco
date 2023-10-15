# HoloLens2_EmguCV_Aruco
Tracking of a single ArUco marker using the webcam on the Microsoft HoloLens 2.

**This project could not be built to the HoloLens 2 and has not been tested as a result.**

The position of the marker is tracked decently well but the rotation isn't, haven't figured out how to fix it. Camera calibration is required if any sort of accuracy is desired (which I haven't done).

## 2D Tracking in Editor
![2D Tracking of ArUco marker in Unity Editor](https://github.com/deltasprey/HoloLens2_EmguCV_Aruco/blob/main/2D%20tracking.png)

## 3D Tracking in Editor
![3D Tracking of ArUco marker in Unity Editor](https://github.com/deltasprey/HoloLens2_EmguCV_Aruco/blob/main/3D%20tracking.png)

# Here are the steps to create a similar project of your own:
1. Create a new 3D Core Unity project. Follow the steps in the tutorial to set it up for HoloLens development. https://learn.microsoft.com/en-us/training/paths/beginner-hololens-2-tutorials/  
2. Add NuGet package manager to your project. Follow the steps below to download and import. 
   - Download NuGet for Unity .unitypackage file from https://github.com/GlitchEnzo/NuGetForUnity/releases  
   - In your Assests folder, right click --> Import Package --> Custom Package. Select the downloaded file and import it. 
     - If a NuGet tab doesn’t appear on your menu bar, click the NuGet file in your Assests folder, enable Load on startup and Apply. 
     - If it still doesn’t appear then restart (close and reopen) your Unity project. 
3. Click NuGet on your menu bar and Manage NuGet Packages. 
4. Search for “Emgu.CV” and install Emgu.CV by Emgu Corporation. 

Upon trying to use the EmguCV functions, you’ll likely notice a lot of DllNotFoundException error messages in the console. To fix this, go through the following steps: 
1. Download EmguCV for your local machine from https://sourceforge.net/projects/emgucv/files/emgucv/4.6.0/  
2. Go through the installation process. 
3. In your Assests folder on Unity, create a Plugins folder. 
4. Navigate to your Plugins folder through Windows Explorer. 
5. Open a new Windows Explorer window and navigate to OS (C:) --> Emgu --> emgucv-windesktop --> libs --> runtimes.  
6. Copy and paste the win-x64 folder to your Plugins folder (This just gets it working in the Unity editor).
