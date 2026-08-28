
using UnityEngine;

public class MS_InvisibleAwake : MonoBehaviour
{
    void Awake()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }
}
