using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class AimCameraController : MonoBehaviour
{
    [SerializeField] private Transform yawTarget;
    [SerializeField] private Transform pitchTarget;

    [SerializeField] private InputActionReference lookInput;
    [SerializeField] private InputActionReference switchShouldInput;

    [SerializeField] private float mouseSensitivity = 0.05f;
    [SerializeField] private float gamepadSensitivity = 0.5f;
    [SerializeField] private float sensitivity = 1.5f;

    [SerializeField] private float pitchMin = -40f;
    [SerializeField] private float pitchMax = 80f;

    [SerializeField] private CinemachineThirdPersonFollow aimCam;

    [SerializeField] private float shoulderSwitchSpeed = 5f;

    [SerializeField] private bool invertY = true;

    private float yaw;
    private float pitch;
    private float targetCameraSide;

    private void Awake()
    {
        aimCam = GetComponent<CinemachineThirdPersonFollow>();
        targetCameraSide = aimCam.CameraSide;
    }

    private void Start()
    {
        Vector3 angles = yawTarget.rotation.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        lookInput.asset.Enable();
    }

    private void OnEnable()
    {
        switchShouldInput.action.Enable();
        switchShouldInput.action.performed += OnSwitchShoulder;
    }

    private void OnDisable()
    {

    }

    private void OnSwitchShoulder(InputAction.CallbackContext context)
    {
        targetCameraSide = aimCam.CameraSide < 0.5f ? 1f : 0f;
    }

    private void Update()
    {
        Vector2 look = lookInput.action.ReadValue<Vector2>();

        if (Mouse.current != null && Mouse.current.delta.IsActuated())
        {
            look *= mouseSensitivity;
        }
        else if (Gamepad.current != null && Gamepad.current.rightStick.IsActuated())
        {
            look *= gamepadSensitivity;

        }

        yaw += look.x * sensitivity;

        if (invertY)
        {
            pitch += -look.y * sensitivity;
        }
        else
        {
            pitch += look.y * sensitivity;
        }

        yawTarget.rotation = Quaternion.Euler(0f, yaw, 0f);
        pitchTarget.localRotation = Quaternion.Euler(pitch,0f,0f);

        aimCam.CameraSide = Mathf.Lerp(aimCam.CameraSide,targetCameraSide,Time.deltaTime * shoulderSwitchSpeed);

    }
}