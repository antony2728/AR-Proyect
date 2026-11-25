using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameLook : MonoBehaviour
{
    public Camera targetCamera;

    private void Start()
    {
        targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        transform.rotation = Quaternion.LookRotation(
            transform.position - targetCamera.transform.position
        );
    }
}
