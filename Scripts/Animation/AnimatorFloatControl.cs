using UnityEngine;
using UnityEngine.Playables;

public class AnimatorFloatControl : MonoBehaviour
{
    [SerializeField, Range(-1.0f, 1.0f)] float var;
    [SerializeField] Animator[] animators;
    [SerializeField] string paramName;

    // Update is called once per frame
    void Update()
    {
        foreach (var animator in animators)
        {
            animator.SetFloat(paramName, var);
        }
    }
}
