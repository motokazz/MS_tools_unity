using UnityEngine;

public class EmitSimple : MonoBehaviour
{
    public GameObject enemy;
    public float spawnStartingTime = 0f;
    public float spawnTime = 3f;
	public int spawnQuantity = 1;

    Vector3 pos;
    Quaternion rot;

    void Start ()
    {
        pos = transform.position;
        rot = transform.rotation;
        InvokeRepeating ("Spawn", spawnStartingTime, spawnTime);
    }

    void Update()
    {
        pos = transform.position;
        rot = transform.rotation;
    }

    void Spawn ()
    {
		if(spawnQuantity>0)
		{
			Instantiate (enemy, pos, rot);
			spawnQuantity-=1;
		}
        else
        {
            Destroy(gameObject);
        }
    }
}
