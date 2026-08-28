using UnityEngine;

using Unity.Cinemachine;
public class CinemachineInputAxisControllerSwitch : MonoBehaviour
{
    CinemachineInputAxisController[] controllers;
    [SerializeField] KeyCode key = KeyCode.P;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controllers = FindObjectsByType<CinemachineInputAxisController>(FindObjectsSortMode.None);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(key)) {
            Switch();
        }

    }

    void Switch()
    {
        bool controllerStatus = true;
        if (controllers[0].enabled)
        {
            controllerStatus = false;
        }

        foreach (var controller in controllers)
        {
            controller.enabled = controllerStatus;
        }
    }
}
