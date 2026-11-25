using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

public class SelectingInfo : MonoBehaviour
{
    [Header("AR Camera")]
    public Camera arCamera;

    [Header("UI")]
    public GameObject panelInfo;
    public TMP_Text txName;
    public TMP_Text txDescription;
    public TMP_Text txLevel;
    public Image imgModel;

    private ModelInfo selectedModel;

    private void Start()
    {
        panelInfo.SetActive(false);
    }

    private void Update()
    {
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                Vector2 position = touch.position.ReadValue();

                if (IsPointerOverUI(position))
                    return;

                DetectModel(position);
            }
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 position = Mouse.current.position.ReadValue();

            if (IsPointerOverUI(position))
                return;

            DetectModel(position);
        }
    }

    void DetectModel(Vector2 screenPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000000000f))
        {
            Debug.Log("HIT: " + hit.collider.name);
            ModelInfo model = hit.collider.GetComponentInParent<ModelInfo>();

            if (model != null)
            {
                selectedModel = model;
                model.animator.SetTrigger("Interact");
                ShowInfo(model);
                return;
            }
            else
            {
                Debug.LogWarning("Detectó un objeto, pero no tiene ModelInfo en sus padres");
            }
        }
        else 
        {
            Debug.Log("No se detectó ningún Collider");
        }

        selectedModel = null;
        panelInfo.SetActive(false);
    }

    void ShowInfo(ModelInfo model)
    {
        panelInfo.SetActive(true);

        BestData b = model.BestData;
        txName.text = b.name;
        txDescription.text = b.description;
        txLevel.text = b.level.ToString();
        imgModel.sprite = b.icon;
        Debug.Log("Modelo seleccionado: " + b.name);
    }

    bool IsPointerOverUI(Vector2 position)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventData =
            new PointerEventData(EventSystem.current);

        eventData.position = position;

        List<RaycastResult> results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }
}
