using UnityEngine;

public class ColCheck:MonoBehaviour
{

    private void Start()
    {
        Debug.Log("start");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"OnTriggerEnter called! Other: {other.name} | Tag: {other.tag} | Layer: {LayerMask.LayerToName(other.gameObject.layer)}");
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log($"OnTriggerStay - Still inside: {other.name}");
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"OnTriggerExit: {other.name}");
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Enter");
    }


}
