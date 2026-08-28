using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover_world_position_wave : MonoBehaviour
{
    public Vector3 translate;
    public Vector3 wave_size;
    public Vector3 rotate;
    public Vector3 shake_size;
    public float wave_rate=0.1f;
    public float lifeTime = 5.0f;
    public bool destroy_by_lifetime = true;

    Vector3 wave_func;
    Vector3 shake_func;

    int count;
    float timer;
    float seed;
    Vector3 tempPosition;
    Quaternion tempRotation;

    void Awake()
    {

        if (destroy_by_lifetime)
        {
            Destroy (gameObject, lifeTime);
        }
        tempPosition = transform.position;
        tempRotation = transform.rotation;
        count = 0;
        timer = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (timer>lifeTime){
            transform.position = tempPosition;
            transform.rotation = tempRotation;
            //transform.rotation = tempTransform.rotation;
            timer = 0;
        }

        seed = Mathf.Cos(count*wave_rate);
  
        wave_func.x = seed*wave_size.x;
        wave_func.y = seed*wave_size.y;
        wave_func.z = seed*wave_size.z;

        transform.position += (translate*(1/lifeTime)) * Time.deltaTime + wave_func;

        shake_func.x = seed*shake_size.x;
        shake_func.y = seed*shake_size.y;
        shake_func.z = seed*shake_size.z;

        transform.Rotate((rotate*(1/lifeTime))*Time.deltaTime+shake_func);

        count+=1;
        timer += Time.deltaTime;
    }
}
