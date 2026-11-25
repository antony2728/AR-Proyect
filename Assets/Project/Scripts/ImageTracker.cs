using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageTracker : MonoBehaviour
{
    public ARTrackedImageManager imageManager;
    public List<BestData> bests = new List<BestData>();

    private Dictionary<TrackableId, TrackedBest> spawnedObjects = new Dictionary<TrackableId, TrackedBest>();
    public IEnumerable<TrackedBest> SpawnedObject => spawnedObjects.Values;

    private void OnEnable()
    {
        imageManager.trackedImagesChanged += OnImageChange;
    }

    private void OnDisable()
    {
        imageManager.trackedImagesChanged -= OnImageChange;
    }

    private void OnImageChange(ARTrackedImagesChangedEventArgs args) 
    {
        foreach (var img in args.added) 
        {
            RegisterImage(img);
        }

        foreach (ARTrackedImage img in args.updated) 
        {
            RegisterImage(img);
        }

        foreach (var img in args.removed)
        {
            RemoveImage(img.trackableId);
        }
    }

    private void RegisterImage(ARTrackedImage img) 
    {
        TrackableId id = img.trackableId;
        if (spawnedObjects.TryGetValue(id, out TrackedBest exist)) 
        {
            exist.trackedImage = img;
            UpdateVisibility(exist);
            return;
        }

        string name = img.referenceImage.name;
        BestData data = FindBest(name);

        if (data == null || data.prefab == null)
            return;

        GameObject model = Instantiate(data.prefab, img.transform);
        TrackedBest best = new TrackedBest(img, data, model);
        spawnedObjects.Add(id, best);
        model.GetComponent<ModelInfo>().txName.text = data.bestName;
        UpdateVisibility(best);
        Debug.Log($"Best Detected: {data.bestName}");
    }

    BestData FindBest(string name) 
    {
        foreach (BestData data in bests)
        {
            if (data != null && data.name == name) 
            {
                return data;
            }
        }

        return null;
    }

    private void RemoveImage(TrackableId id) 
    {
        if (spawnedObjects.TryGetValue(id, out TrackedBest best)) 
        {
            best.isRemoved = true;
            if (best.instance != null) 
            {
                Destroy(best.instance);
            }

            spawnedObjects.Remove(id);
        }
    }

    private void UpdateVisibility(TrackedBest best)
    {
        bool shouldVis = best.IsTracking();
        best.SetVisible(shouldVis);
    }

    public void SetBestRract(TrackedBest best, bool reacting) 
    {
        best.SetReacting(reacting);
        UpdateVisibility(best);
    }
}
