using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFollow : MonoBehaviour
{
    Camera main;
    void Start()
    {
        main = Camera.main;
    }
    void Update()
    {
        transform.LookAt(transform.position + main.transform.rotation * Vector3.forward);
    }
}
