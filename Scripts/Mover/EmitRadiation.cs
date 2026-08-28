using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmitRadiation : MonoBehaviour
{
    public GameObject enemy;

    public int Count = 10;
    public float radiationAngle = 180f;

    Vector3 pos;
    Quaternion rot;
    Quaternion q;
    float addAngle;

    void Awake()
    {
        pos = transform.position;
        
        addAngle = radiationAngle/(Count-1);
        q=Quaternion.Euler(0,-radiationAngle*0.5f,0);
        rot = transform.rotation*q;

        bang();

        Destroy(gameObject);
    }

    void bang ()
    {
        int i;
                
        for (i=0;i<Count;i++){
            q=Quaternion.Euler(0,addAngle*i,0);
		    Instantiate (enemy, pos, rot*q);
        }
    }
}
