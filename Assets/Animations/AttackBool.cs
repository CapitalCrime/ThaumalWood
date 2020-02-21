using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBool : StateMachineBehaviour
{
    Transform leftHand;
    Transform bowString;
    Transform bow;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Attacking", false);
        if (animator.GetBool("Archer"))
        {
            leftHand = animator.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetChild(0);
            bow = animator.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).Find("Weapon").GetChild(0).GetChild(0);
            if(bow.childCount > 1)
            {
                bowString = bow.GetChild(1);
            } else
            {
                bowString = leftHand.Find("String");
            }
            bowString.GetChild(0).gameObject.SetActive(true);
            bowString.parent = leftHand;
            bowString.localEulerAngles = new Vector3(0, 0, 150);
            bowString.localPosition = Vector3.zero;
        }
    }
    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (animator.GetBool("Archer"))
        {
            bowString.parent = bow;
            bowString.GetChild(0).gameObject.SetActive(false);
            bowString.localPosition = new Vector3(0, 0, -1f);
            bowString.localEulerAngles = new Vector3(90, 0, 90);
        }
    }
}
