using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 目玉の自動アニメーションスクリプト
/// </summary>
public class MS_eye_motion : MonoBehaviour
{
    [SerializeField] Transform rightEyeball;
    [SerializeField] Transform leftEyeball;

    //eyeball

    [SerializeField] Vector2 eyeballMoveTimingRange = new Vector2(0.1f, 10f);
    [SerializeField] float eyeballSpeed = 5.0f;

    [SerializeField] float RotYMin = -5.0f;
    [SerializeField] float RotYMax = 5.0f;

    [SerializeField] float RotXMin = -5.0f;
    [SerializeField] float RotXMax = 5.0f;

    [SerializeField] float RotZMin = 0.0f;
    [SerializeField] float RotZMax = 0.0f;

    float timeOut = 1.0f;

    Vector3 target;
    Quaternion orgRightEyeball;
    Quaternion orgLeftEyeball;



    // Start is called before the first frame update
    void Start()
    {
        target = new Vector3(0f, 0f, 0f);

        if (rightEyeball)
        {
            orgRightEyeball = rightEyeball.transform.localRotation;
        }

        if (leftEyeball)
        {
            orgLeftEyeball = leftEyeball.transform.localRotation;
        }

        StartCoroutine("eyeballMoveTiming");
        


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        eyeballMover(target);
    }


    IEnumerator eyeballMoveTiming()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeOut);
            retarget();
            timeOut = Random.Range(eyeballMoveTimingRange.x, eyeballMoveTimingRange.y);

        }

    }


    void retarget()
    {
        target = new Vector3(Random.Range(RotXMin, RotXMax), Random.Range(RotYMin, RotYMax), Random.Range(RotZMin, RotZMax));
    }

    void eyeballMover(Vector3 target)
    {
        if (rightEyeball)
        {
            Quaternion fromRRot = rightEyeball.transform.localRotation;
            Quaternion toRRot = Quaternion.Euler(target.x, target.y, target.z);
            rightEyeball.transform.localRotation = Quaternion.Slerp(fromRRot, orgRightEyeball * toRRot, Time.deltaTime * eyeballSpeed);
        }

        if (leftEyeball)
        {
            Quaternion fromLRot = leftEyeball.transform.localRotation;
            Quaternion toLRot = Quaternion.Euler(target.x, target.y, target.z);
            leftEyeball.transform.localRotation = Quaternion.Slerp(fromLRot, orgLeftEyeball * toLRot, Time.deltaTime * eyeballSpeed);
        }

    }


}
