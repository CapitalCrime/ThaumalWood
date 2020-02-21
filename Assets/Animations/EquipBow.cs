using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipBow : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Equipping", false);
        Transform weapon = animator.transform.GetChild(0).GetChild(0).GetChild(0).Find("Weapon");
        if (weapon != null)
        {
            GameObject rightHand = animator.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).gameObject;
            weapon.parent = rightHand.transform;
            weapon.localPosition = new Vector3(0, 0, -0.3f);
            weapon.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
