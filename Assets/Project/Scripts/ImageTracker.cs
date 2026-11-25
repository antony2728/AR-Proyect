
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

    private Dictionary<TrackableId, TrackedBest> spawnedObjects =
        new Dictionary<TrackableId, TrackedBest>();

    public IEnumerable<TrackedBest> SpawnedObject => spawnedObjects.Values;

    private void OnEnable()
    {
        Debug.Log("[ImageTracker] OnEnable ejecutado.");

        if (imageManager == null)
        {
            Debug.LogError("[ImageTracker] imageManager NO está asignado.");
            return;
        }

        imageManager.trackedImagesChanged += OnImageChange;

        Debug.Log("[ImageTracker] Evento trackedImagesChanged registrado.");
        Debug.Log($"[ImageTracker] Referencias configuradas: {imageManager.referenceLibrary.count}");
        Debug.Log($"[ImageTracker] BestData configurados: {bests.Count}");
    }

    private void OnDisable()
    {
        Debug.Log("[ImageTracker] OnDisable ejecutado.");

        if (imageManager != null)
        {
            imageManager.trackedImagesChanged -= OnImageChange;
        }
    }

    private void OnImageChange(ARTrackedImagesChangedEventArgs args)
    {
        Debug.Log(
            $"[ImageTracker] Cambio de imágenes | " +
            $"Added: {args.added.Count} | " +
            $"Updated: {args.updated.Count} | " +
            $"Removed: {args.removed.Count}"
        );

        foreach (var img in args.added)
        {
            Debug.Log($"[ImageTracker] Imagen AÑADIDA: {img.referenceImage.name}");
            RegisterImage(img);
        }

        foreach (ARTrackedImage img in args.updated)
        {
            Debug.Log(
                $"[ImageTracker] Imagen ACTUALIZADA: {img.referenceImage.name} | " +
                $"Tracking: {img.trackingState}"
            );

            RegisterImage(img);
        }

        foreach (var img in args.removed)
        {
            Debug.LogWarning(
                $"[ImageTracker] Imagen REMOVIDA: {img.referenceImage.name}"
            );

            RemoveImage(img.trackableId);
        }
    }

    private void RegisterImage(ARTrackedImage img)
    {
        if (img == null)
        {
            Debug.LogError("[ImageTracker] RegisterImage recibió una imagen NULL.");
            return;
        }

        TrackableId id = img.trackableId;

        Debug.Log(
            $"[ImageTracker] Registrando imagen: {img.referenceImage.name} | " +
            $"ID: {id} | Tracking: {img.trackingState}"
        );

        if (spawnedObjects.TryGetValue(id, out TrackedBest exist))
        {
            exist.trackedImage = img;
            UpdateVisibility(exist);
            return;
        }

        string name = img.referenceImage.name;

        Debug.Log($"[ImageTracker] Buscando BestData para: '{name}'");

        BestData data = FindBest(name);

        if (data == null)
        {
            Debug.LogError(
                $"[ImageTracker] NO SE ENCONTRÓ BestData para '{name}'. " +
                $"Revisa que BestData.name coincida exactamente con el nombre " +
                $"de la Reference Image."
            );

            return;
        }

        Debug.Log($"[ImageTracker] BestData encontrado: {data.bestName}");

        if (data.prefab == null)
        {
            Debug.LogError(
                $"[ImageTracker] El BestData '{data.name}' tiene el prefab NULL."
            );

            return;
        }

        try
        {
            Debug.Log($"[ImageTracker] Instanciando prefab: {data.prefab.name}");

            GameObject model = Instantiate(data.prefab, img.transform);

            if (model == null)
            {
                Debug.LogError("[ImageTracker] Instantiate devolvió NULL.");
                return;
            }

            TrackedBest best = new TrackedBest(img, data, model);

            spawnedObjects.Add(id, best);

            Debug.Log($"[ImageTracker] Modelo instanciado: {model.name}");

            ModelInfo modelInfo = model.GetComponent<ModelInfo>();

            if (modelInfo == null)
            {
                Debug.LogError(
                    $"[ImageTracker] El prefab '{model.name}' no tiene ModelInfo."
                );
            }
            else if (modelInfo.txName == null)
            {
                Debug.LogError(
                    $"[ImageTracker] ModelInfo de '{model.name}' tiene txName NULL."
                );
            }
            else
            {
                modelInfo.txName.text = data.bestName;
                Debug.Log($"[ImageTracker] Nombre asignado: {data.bestName}");
            }

            UpdateVisibility(best);

            Debug.Log($"[ImageTracker] CRIATURA CREADA CORRECTAMENTE: {data.bestName}");
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"[ImageTracker] EXCEPCIÓN AL CREAR LA CRIATURA '{name}':\n{e}"
            );
        }
    }

    BestData FindBest(string name)
    {
        if (bests == null)
        {
            Debug.LogError("[ImageTracker] La lista bests es NULL.");
            return null;
        }

        foreach (BestData data in bests)
        {
            if (data == null)
            {
                Debug.LogWarning("[ImageTracker] Hay un elemento NULL en bests.");
                continue;
            }

            Debug.Log(
                $"[ImageTracker] Comparando Reference '{name}' " +
                $"con BestData '{data.name}'"
            );

            if (data.name == name)
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
            Debug.Log($"[ImageTracker] Eliminando criatura: {best.bestData.bestName}");

            best.isRemoved = true;

            if (best.instance != null)
            {
                Destroy(best.instance);
            }

            spawnedObjects.Remove(id);
        }
        else
        {
            Debug.LogWarning(
                $"[ImageTracker] Se intentó eliminar una imagen sin criatura asociada. ID: {id}"
            );
        }
    }

    private void UpdateVisibility(TrackedBest best)
    {
        if (best == null)
        {
            Debug.LogError("[ImageTracker] UpdateVisibility recibió NULL.");
            return;
        }

        bool shouldVis = best.IsTracking();

        Debug.Log(
            $"[ImageTracker] Visibilidad de {best.bestData.bestName}: {shouldVis}"
        );

        best.SetVisible(shouldVis);
    }

    public void SetBestRract(TrackedBest best, bool reacting)
    {
        if (best == null)
        {
            Debug.LogError("[ImageTracker] SetBestRract recibió NULL.");
            return;
        }

        Debug.Log(
            $"[ImageTracker] Reacción de {best.bestData.bestName}: {reacting}"
        );

        best.SetReacting(reacting);
        UpdateVisibility(best);
    }
}
