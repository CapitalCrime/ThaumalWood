using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnequipAction : StateMachineBehaviour
{
    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Unequipping", false);
        Transform weapon = animator.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).Find("Weapon");
        if(weapon != null)
        {
            GameObject hip = animator.transform.GetChild(0).GetChild(0).gameObject;
            weapon.parent = hip.transform;
            weapon.localPosition = new Vector3(-1f, -0.86f, 0.22f);
            weapon.localRotation = Quaternion.Euler(90, 85, -90);
        }
    }
}
