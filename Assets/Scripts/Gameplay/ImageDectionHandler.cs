using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro;

public class ImageDectionHandler : MonoBehaviour
{
    [SerializeField] private GameObject HUD_Canvas;

    private ARTrackedImageManager trackedImageManager;

    private void Start()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    private void Update()
    {
        if (HUD_Canvas.activeSelf)
        {
            trackedImageManager.enabled = true;
        }
        else
        {
            trackedImageManager.enabled = false;
        }
    }
}
