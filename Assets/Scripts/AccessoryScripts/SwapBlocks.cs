using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapBlocks : MonoBehaviour
{
    SwapScript script;

    private void Start()
    {
        script = GameObject.Find("EventSystem").GetComponent<SwapScript>();
    }
    private void OnMouseDown()
    {
        gameObject.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0, 0.5f);
        if (script.swapWith == -1)
        {
            script.swapWith = int.Parse(gameObject.name);
        }
    }
}
