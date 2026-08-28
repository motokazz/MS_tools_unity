using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MS_ACTriggerByButton : MonoBehaviour
{
    [SerializeField] Animator[] animators;
    [SerializeField] KeyCode keyCode = KeyCode.None;
    [SerializeField] string triggerName;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(keyCode))
        {
            foreach (var animator in animators)
            {
                animator.SetTrigger(triggerName);
            }
        }
    }
}
