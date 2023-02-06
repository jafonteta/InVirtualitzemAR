using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class ARPlaceTrackedImages : MonoBehaviour
{
    // Cache AR tracked images manager from ARCoreSession
    private ARTrackedImageManager _trackedImagesManager;

    // List of prefabs - these have to have the same names as the 2D images in the reference image library
    public GameObject[] ArPrefabs;

    // Internal storage of created prefabs for easier updating
    private readonly Dictionary<string, GameObject> _instantiatedPrefabs = new();

    // Reference to logging UI element in the canvas
    public UnityEngine.UI.Text Log;

    // Objectos opcionales para testear el estado del seguimiento de una imagen, lo cual permite imprimir información supuerpuesta a la visualización del objeto
    public GameObject[] testTrackState;

    public bool debugear = false;
    
    void Awake()
    {
        foreach(GameObject boton in testTrackState)
        {
            boton.SetActive(false);
        }
        _trackedImagesManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {

        _trackedImagesManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        _trackedImagesManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Good reference: https://forum.unity.com/threads/arfoundation-2-image-tracking-with-many-ref-images-and-many-objects.680518/#post-4668326
        // https://github.com/Unity-Technologies/arfoundation-samples/issues/261#issuecomment-555618182
        
        // Go through all tracked images that have been added
        // (-> new markers detected)
        foreach (var trackedImage in eventArgs.added)
        {
            // Get the name of the reference image to search for the corresponding prefab
            var imageName = trackedImage.referenceImage.name;
            foreach (var curPrefab in ArPrefabs)
            {
                if (string.Compare(curPrefab.name, imageName, StringComparison.Ordinal) == 0 
                    && !_instantiatedPrefabs.ContainsKey(imageName))
                {
                    // Found a corresponding prefab for the reference image, and it has not been instantiated yet
                    // -> new instance, with the ARTrackedImage as parent (so it will automatically get updated
                    // when the marker changes in real-life)
                    var newPrefab = Instantiate(curPrefab, trackedImage.transform);
                    // Store a reference to the created prefab
                    _instantiatedPrefabs[imageName] = newPrefab;
                    if(debugear == true && Log != null)
                    {
                        Log.text = $"{Time.time} -> Instantiated prefab for tracked image (name: {imageName}).\n" +
                                   $"newPrefab.transform.parent.name: {newPrefab.transform.parent.name}.\n" +
                                   $"guid: {trackedImage.referenceImage.guid}";
                        ShowAndroidToastMessage("Instantiated!");
                    }

                }
            }
        }

        // Disable instantiated prefabs that are no longer being actively tracked
        foreach (var trackedImage in eventArgs.updated)
        {
            if(trackedImage.trackingState == TrackingState.None)
            {
                if (debugear == true && Log != null)
                {
                    Log.text = $"{Time.time} \n" +
                            $" \n" +
                            $" \n" +
                            $"-> trackingState NONE (name: {trackedImage.referenceImage.name}). \n" +
                            $"guid: {trackedImage.referenceImage.guid}";
                    //ShowAndroidToastMessage("None!");
                }

            }
            else if(trackedImage.trackingState == TrackingState.Limited)
            {
                if (debugear == true && Log != null)
                {
                    Log.text = $"{Time.time} \n" +
                     $" \n" +
                     $" \n" +
                     $"-> trackingState LIMITED (name: {trackedImage.referenceImage.name}). \n" +
                     $"guid: {trackedImage.referenceImage.guid}";
                    //ShowAndroidToastMessage("None!");
                }
            }
            else
            {
                if (debugear == true && Log != null)
                {
                    Log.text = $"{Time.time} \n" +
                            $" \n" +
                            $" \n" +
                            $"-> trackingState TRACKING (name: {trackedImage.referenceImage.name}). \n" +
                            $"guid: {trackedImage.referenceImage.guid}";
                    //ShowAndroidToastMessage("Tracking!");
                }
            }

            _instantiatedPrefabs[trackedImage.referenceImage.name]
                .SetActive(trackedImage.trackingState == TrackingState.Tracking);

            // este SWITCH ES EL QUE HAY QUE EDITAR
            switch (trackedImage.referenceImage.name)
            {
                case "PIIE_AR_MO004_LONJA":
                    testTrackState[0].SetActive(trackedImage.trackingState == TrackingState.Tracking);
                    break;
                case "PIIE_AR_MO018_RELOJ":
                    testTrackState[1].SetActive(trackedImage.trackingState == TrackingState.Tracking);
                    break;
                default:
                    print("No hay objeto trackeado en estos momentos");
                    break;
            }


            /*
            Log.text = $"{Time.time} -> Updated (name: {trackedImage.referenceImage.name}).\n" +
                               $"guid: {trackedImage.referenceImage.guid}";
            ShowAndroidToastMessage("Updated!");
            */
        }

        // Remove is called if the subsystem has given up looking for the trackable again.
        // (If it's invisible, its tracking state would just go to limited initially).
        // Note: ARCore doesn't seem to remove these at all; if it does, it would delete our child GameObject
        // as well.
        foreach (var trackedImage in eventArgs.removed)
        {
            // Destroy the instance in the scene.
            // Note: this code does not delete the ARTrackedImage parent, which was created
            // by AR Foundation, is managed by it and should therefore also be deleted
            // by AR Foundation.

            Destroy(_instantiatedPrefabs[trackedImage.referenceImage.name]);

            // Also remove the instance from our array
            _instantiatedPrefabs.Remove(trackedImage.referenceImage.name);
            

            // Alternative: do not destroy the instance, just set it inactive
            //_instantiatedPrefabs[trackedImage.referenceImage.name].SetActive(false);

            Log.text = $"REMOVED (guid: {trackedImage.referenceImage.guid}).";
        }
    }

    /// <summary>
    /// Show an Android toast message.
    /// </summary>
    /// <param name="message">Message string to show in the toast.</param>
    private static void ShowAndroidToastMessage(string message)
    {
#if UNITY_ANDROID
        using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        if (unityActivity == null) return;
        var toastClass = new AndroidJavaClass("android.widget.Toast");
        unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            // Last parameter = length. Toast.LENGTH_LONG = 1
            using var toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText",
                unityActivity, message, 1);
            toastObject.Call("show");
        }));
#endif
    }
}
