using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOff : MonoBehaviour
{
    public GameObject objectToDeactivate;
    public GameObject objectToActivate;
    void Deactivate()
    {
        objectToDeactivate.SetActive(false);
        objectToActivate.SetActive(true);
    }
}
