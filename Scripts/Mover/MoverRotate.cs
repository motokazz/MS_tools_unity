using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoverRotate : MonoBehaviour
{
    public Vector3 rotate;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotate*Time.deltaTime);
    }
}
