using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeFOV : MonoBehaviour
{
    Camera cam;
    Camera secondCam;
    void Start()
    {
        cam = Camera.main;
        secondCam = cam.transform.GetChild(0).GetComponent<Camera>();
    }

    public void SliderValue(UnityEngine.UI.Slider slider)
    {
        cam.fieldOfView = slider.value;
        secondCam.fieldOfView = slider.value;
    }
}
