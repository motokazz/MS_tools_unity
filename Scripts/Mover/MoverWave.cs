using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoverWave : MonoBehaviour
{
    public Vector3 wave_size;
    public float speed=0.1f;


    Vector3 wave_func;
    int count;
    float seed;

    // Start is called before the first frame update
    void Awake()
    {
        count=0;
    }

    // Update is called once per frame
    void Update()
    {
        seed = Mathf.Cos(count*speed);
        
        wave_func.x = seed*wave_size.x;
        wave_func.y = seed*wave_size.y;
        wave_func.z = seed*wave_size.z;

        transform.Translate(wave_func * Time.deltaTime);

        count+=1;

    }
}
