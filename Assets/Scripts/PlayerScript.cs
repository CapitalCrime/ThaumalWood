using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    public int currentCharacter = 0;
    Rigidbody rb;
    GameObject currentObj;
    GameObject player;
    GameObject head;
    GameObject skull;
    GameObject body;
    Animator animator;
    MazeScript mazeScript;

    public Camera mainCam;

    bool walking = false;
    bool equipped = false;

    float time = 0.0f;
    float walkspeed = 450.0f;
    float speed = 0.0f;
    float targetspeed = 0.0f;

    public LayerMask clickMask;

    void Start()
    {
        mazeScript = gameObject.GetComponent<MazeScript>();
    }

    public void initChar(GameObject plyr)
    {
        if (plyr != null)
        {
            player = plyr;
            rb = player.GetComponent<Rigidbody>();
            head = player.transform.Find("RPGCharacter").GetChild(0).Find("LowerBody").GetChild(0).GetChild(0).gameObject;
            skull = head.transform.GetChild(0).gameObject;
            animator = player.transform.Find("RPGCharacter").GetComponent<Animator>();

        }
    }


    void FixedUpdate()
    {
        if (rb != null && !mazeScript.inBattle)
        {
            rb.velocity = dir;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null && !mazeScript.inBattle)
        {
            move();
            if(player.transform.position.y < -5)
            {
                player.transform.position = new Vector3(player.transform.position.x, 2, player.transform.position.z);
            }
        }
    }

    Coroutine equipAnim;
    float mouseMoveX;
    float mouseMoveZ;
    float headAngle;
    float xRot = 0.0f;
    float yRot = 0.0f;
    float playerRot = 0.0f;
    bool turning = false;
    bool strafing = false;
    bool running = false;
    float turnSpeed = 0.0f;

    Vector3 dir;

    void move()
    {
        turnSpeed = 180.0f * Time.fixedDeltaTime;
        dir = Vector3.zero;

        if (Input.GetMouseButton(1))
        {
            mouseMoveX = ((Input.mousePosition.x / Screen.width) - 0.5f) * 2.0f;
            mouseMoveZ = ((Input.mousePosition.y / Screen.height) - 0.6f) * 2.0f;
            xRot -= mouseMoveZ * turnSpeed;
            yRot += mouseMoveX * turnSpeed;
            headAngle = (xRot > 180) ? yRot - 360 : yRot;
            if (Mathf.Abs(headAngle) > 30)
            {
                if (headAngle > 30)
                {
                    playerRot += (yRot - 30) * (turnSpeed / 4);
                }
                else if (headAngle < -30)
                {
                    playerRot += (yRot + 30) *(turnSpeed / 4);
                }
                turning = true;
            }
            xRot = Mathf.Clamp(xRot, -30, 30);
            yRot = Mathf.Clamp(yRot, -30, 30);
        }
        if (Input.GetKey(KeyCode.A))
        {
            walking = true;
            turning = true;
            dir.x -= head.transform.right.x;
            dir.z -= head.transform.right.z;
            if (yRot < 30)
            {
                playerRot = -turnSpeed;
                yRot += turnSpeed;
            }
            strafing = true;
        }else if (Input.GetKey(KeyCode.D))
        {
            walking = true;
            turning = true;
            dir.x += head.transform.right.x;
            dir.z += head.transform.right.z;
            if (yRot > -30)
            {
                playerRot = turnSpeed;
                yRot += -turnSpeed;
            }
            strafing = true;
        }
        if (Input.GetKey(KeyCode.W))
        {
            walking = true;
            dir.x += head.transform.forward.x;
            dir.z += head.transform.forward.z;
            if (turning == false)
            {
                if (Mathf.Abs(yRot) > 5.0f)
                {
                    if (yRot > 1.5f)
                    {
                        playerRot += turnSpeed;
                        yRot += -turnSpeed;
                    }
                    else if (yRot < -1.5f)
                    {
                        playerRot += -turnSpeed;
                        yRot += turnSpeed;
                    }
                }
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            walking = true;
            dir.x -= head.transform.forward.x;
            dir.z -= head.transform.forward.z;
        }
        if (strafing)
        {
            yRot = Mathf.Clamp(yRot, -30, 30);
        }

        dir = dir.normalized * Time.fixedDeltaTime * speed;
        dir.y += rb.velocity.y;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            running = true;
            targetspeed = walkspeed * 2.0f;
            time = 0.0f;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            running = false;
            targetspeed = walkspeed;
            time = 0.0f;
        }
        
        if(speed != targetspeed)
        {
            speed = Mathf.SmoothStep(speed, targetspeed, time);
            time += Time.deltaTime;
        } else
        {
            time = 0.0f;
        }

        animator.SetFloat("Speed", speed / (walkspeed * 2.0f));
        player.transform.Rotate(0, playerRot, 0);
        head.transform.localEulerAngles = new Vector3(xRot, yRot, head.transform.localEulerAngles.z);
        playerRot = 0.0f;
        turning = false;
        strafing = false;

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (equipped)
            {
                animator.SetBool("Unequipping", true);
            } else
            {
                animator.SetBool("Equipping", true);
            }
            equipped = !equipped;
        }

        if (!walking)
        {
            targetspeed = 0.0f;
        } else if (targetspeed == 0.0f)
        {
            if (running)
            {
                targetspeed = walkspeed * 2.0f;
            } else
            {
                targetspeed = walkspeed;
            }
        }

        xRot = (head.transform.localEulerAngles.x > 180) ? head.transform.localEulerAngles.x - 360 : head.transform.localEulerAngles.x;
    }

    void LateUpdate()
    {
        if (!mazeScript.inBattle)
        {
            if(player != null)
            {
                mainCam.transform.position = skull.transform.position + skull.transform.forward * 6 + skull.transform.right * 0.5f;
                mainCam.transform.rotation = skull.transform.rotation * Quaternion.Euler(0, 180, 0);
            }
            walking = false;
        }
    }
}
