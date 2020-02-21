using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnequipBowAction : StateMachineBehaviour
{
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Unequipping", false);
        Transform weapon = animator.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).Find("Weapon");
        if (weapon != null)
        {
            GameObject hip = animator.transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
            weapon.parent = hip.transform;
            weapon.localPosition = new Vector3(0.58f, 0.185f, -0.58f);
            weapon.localRotation = Quaternion.Euler(0, 0, 75);
        }
    }
}
