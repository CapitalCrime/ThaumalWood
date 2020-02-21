using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseScript : MonoBehaviour
{
    public Texture2D texture;
    public GameObject panel;
    private void OnMouseOver()
    {
        Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
    }

    private void OnMouseExit()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnMouseDown()
    {
        if(panel != null)
        {
            panel.GetComponent<Animator>().Play("ListenText", 0, 0);
        }
    }
}
