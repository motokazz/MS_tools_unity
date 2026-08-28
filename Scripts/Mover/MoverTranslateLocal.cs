using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoverTranslateLocal : MonoBehaviour
{
    public Vector3 translate;
    // Update is called once per frame
    void Update()
    {
        transform.Translate(translate*Time.deltaTime);
    }
}
