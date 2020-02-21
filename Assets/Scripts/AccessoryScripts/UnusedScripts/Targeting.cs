using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targeting : MonoBehaviour
{

    // Use this for initialization
    GameObject[] targets;
    GameObject head;
    GameObject player;
    Vector3 lookTarget;
    int currentTarget;
    bool targeting;
    void Start()
    {
        targeting = false;
        currentTarget = 0;
        head = transform.parent.transform.Find("Head").gameObject;
        player = GameObject.Find("Character0");
        lookTarget = transform.parent.transform.Find("Head").transform.right;
        targets = new GameObject[10];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            targeting = !targeting;
            currentTarget = 0;
        }

        if (targeting && targets[currentTarget] != null)
        {
            lookTarget = new Vector3(targets[currentTarget].transform.position.x, transform.position.y, targets[currentTarget].transform.position.z) - transform.position;
            head.transform.right = lookTarget;
            float headAngle = (head.transform.localEulerAngles.y > 180) ? head.transform.localEulerAngles.y - 360 : head.transform.localEulerAngles.y;
            if(Mathf.Abs(headAngle) > 70)
            {
                float turnSpeed = Time.deltaTime * 180.0f;
                if (headAngle > 70)
                {
                    player.transform.Rotate(0, turnSpeed, 0);
                    head.transform.localEulerAngles = new Vector3(head.transform.localEulerAngles.x, head.transform.localEulerAngles.y - turnSpeed, head.transform.localEulerAngles.z);
                } else if(headAngle < -70)
                {
                    player.transform.Rotate(0, -turnSpeed, 0);
                    head.transform.localEulerAngles = new Vector3(head.transform.localEulerAngles.x, head.transform.localEulerAngles.y + turnSpeed, head.transform.localEulerAngles.z);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            currentTarget += 1;
            if(currentTarget == targets.Length)
            {
                currentTarget -= 1;
            }
        }

        if (targeting && targets[currentTarget] == null)
        {
            int i = 0;
            while (i < targets.Length)
            {
                if (targets[i] != null)
                {
                    currentTarget = i;
                    break;
                }
                i++;
            }
            if(i == targets.Length)
            {
                targeting = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            Debug.Log("Enemy spotted");
            int i = 0;
            while(i < targets.Length)
            {
                if(targets[i] == null)
                {
                    targets[i] = other.gameObject;
                    break;
                }
                i++;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
        {
            Debug.Log("Enemy lost");
            int i = 0;
            while (i < targets.Length)
            {
                if (targets[i] != null && targets[i] == other.gameObject)
                {
                    targets[i] = null;
                    break;
                }
                i++;
            }
        }
    }
}
