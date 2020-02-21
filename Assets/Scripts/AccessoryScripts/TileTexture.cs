using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileTexture : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(5/transform.lossyScale.x, 10/transform.lossyScale.y));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
