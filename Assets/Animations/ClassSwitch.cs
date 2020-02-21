using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassSwitch : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        string classtype = animator.transform.parent.name;
        bool inBattle = GameObject.Find("EventSystem").GetComponent<MazeScript>().inBattle;
        switch (classtype)
        {
            case "Knight":
                animator.SetBool("Warrior", true);
                break;
            case "Bartender":
                animator.SetBool("Warrior", true);
                break;
            case "Priest":
                animator.SetBool("Warrior", true);
                break;
            case "Archer":
                animator.SetBool("Archer", true);
                break;
            case "Thief":
                animator.SetBool("Warrior", true);
                break;
        }
        if (inBattle)
        {
            animator.SetBool("InBattle", true);
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
