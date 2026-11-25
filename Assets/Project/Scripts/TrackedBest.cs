using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TrackedBest
{
    public ARTrackedImage trackedImage;
    public BestData bestData;
    public GameObject instance;

    public bool isReacting = false;
    public bool isRemoved = false;

    public TrackedBest(ARTrackedImage trackedImage, BestData bestData, GameObject instance)
    {
        this.trackedImage = trackedImage;
        this.bestData = bestData;
        this.instance = instance;
    }

    public bool IsTracking() 
    {
        return !isRemoved && trackedImage != null && trackedImage.trackingState == TrackingState.Tracking;
    }

    public Vector3 GetPos() 
    {
        return trackedImage.transform.position;
    }

    public void SetVisible(bool visible) 
    {
        if(instance != null)
            instance.SetActive(visible);
    }

    public void SetReacting(bool reacting) 
    {
        isReacting = reacting;
    }
}
