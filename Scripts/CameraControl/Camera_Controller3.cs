using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 新しいInputSystem対応カメラコントローラー
/// ※InputSystem必須
/// </summary>

//[RequireComponent(typeof(PlayerInput))]
public class Camera_Controller3 : MonoBehaviour
{
    [Header("PlayerInputアサイン")]
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference upAction;
    [SerializeField] private InputActionReference downAction;
    [SerializeField] private InputActionReference controlActivation;
    [SerializeField] private InputActionReference cameraResetAction;
    [SerializeField] private InputActionReference speedUpAction;
    [SerializeField] private InputActionReference dragAction;
    [SerializeField] private InputActionReference yLockAction;


    [Header("セッティング")]
    [SerializeField] bool enableControllActivation = true; //カメラ操作可能不能切り替え

    [SerializeField] float Normal_Speed = 5.0f; //Normal movement speed

    [SerializeField] float Shift_Speed = 10.0f; //multiplies movement speed by how long shift is held down.

    [SerializeField] float Speed_Cap = 10.0f; //Max cap for speed when shift is held down

    [SerializeField] float rotateSensivity = 0.5f;//回転のセンシティブ
    [SerializeField] float rotateAcceraration = 1.0f;//回転加速度
    
    [SerializeField] float dragSensitivity = 30.0f;//ドラッグのセンシティブ

    [SerializeField] bool invertY = true;

    [SerializeField] Transform target;

    private float Total_Speed = 1.0f; //Total speed variable for shift

    private Vector3 InitPosition;
    private Vector3 InitRotation;

    private Transform tempTramsform;


    void Awake()
    {

    }

    private void Start()
    {
        //　カメラ初期状態取得
        InitPosition = transform.position;
        InitRotation = transform.eulerAngles;

        // 一時的にトランスフォーム保存
        tempTramsform = transform;

        controlActivation.action.canceled += OnControlActivationReleased;

        //Get Target (if none target is self)
        if (!target)
        {
            target = transform;
        }
    }


    private void Update()
    {
        if (enableControllActivation)
        {
            
            if (controlActivation.action.IsPressed()) //カメラ回転と移動
            {

                transform.position = tempTramsform.position;
                transform.rotation = tempTramsform.rotation;

                RotateCameraByQuaternion(target);
                moveCamera();
            }

            if (dragAction.action.IsPressed()) //カメラドラッグ
            {
                moveCamera();
            }
        }
        else //コントールアクティベーションが無ければ常にカメラを動かす
        {
            RotateCameraByQuaternion(target);
            moveCamera();
        }

        //Set Camera initiral position
        if (cameraResetAction.action.WasPressedThisFrame())
        {
            transform.position = InitPosition;
            transform.eulerAngles = InitRotation;
        }
    }

    private void OnControlActivationReleased(InputAction.CallbackContext context)
    {
        tempTramsform = transform;
    }


    //カメラ回転・クオータニオン
    void RotateCameraByQuaternion(Transform transform)
    {
        Vector2 mouseDelta = rotateSensivity * lookAction.action.ReadValue<Vector2>();

        Quaternion rotation = transform.rotation;

        Quaternion horiz = Quaternion.AngleAxis(mouseDelta.x, Vector3.up);

        int invertYVar = invertY?-1:1;

        Quaternion vert = Quaternion.AngleAxis(mouseDelta.y * invertYVar , Vector3.right);
        Quaternion toRotation = horiz * rotation * vert;
        transform.rotation = toRotation;

    }

    //カメラ移動

    void moveCamera()
    {
        Vector3 Cam;
        //Keyboard controls
        if (dragAction.action.IsPressed())
        {
            Cam = (-(Vector3)lookAction.action.ReadValue<Vector2>() * dragSensitivity) + GetBaseInput();
        }
        else
        {
            Cam = GetBaseInput();
        }
        //
        Cam = new Vector3(Cam.x, Cam.y, Cam.z + (Input.GetAxis("Mouse ScrollWheel")*100));

        //加速
        if (speedUpAction.action.IsPressed())
        {
            Total_Speed += Time.deltaTime;  
            Cam = Cam * Total_Speed * Shift_Speed;
            Cam.x = Mathf.Clamp(Cam.x, -Speed_Cap, Speed_Cap);
            Cam.y = Mathf.Clamp(Cam.y, -Speed_Cap, Speed_Cap);
            Cam.z = Mathf.Clamp(Cam.z, -Speed_Cap, Speed_Cap);
        }
        else
        {
            Total_Speed = Mathf.Clamp(Total_Speed * 0.5f, 1f, 1000f);  
            Cam = Cam * Normal_Speed;
        }

        Cam = Cam * Time.deltaTime;
        
        Vector3 newPosition = transform.position;
       
        if (yLockAction.action.IsPressed())
        {
            //If the player wants to move on X and Z axis only by pressing space (good for re-adjusting angle shots)
            transform.Translate(Cam);
            newPosition.x = transform.position.x;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
        else
        {
            transform.Translate(Cam);
        }

    }


    //Joystick・キーボードによるカメラ移動方向ベクトル
    private Vector3 GetBaseInput()
    {   
        Vector3 Camera_Velocity = new Vector3();
        Vector2 move = moveAction.action.ReadValue<Vector2>();

        float HorizontalInput = move.x; //Input for horizontal movement      
        float VerticalInput = move.y; //Input for Vertical movement


        float UpDownInput = 0;
        if (upAction.action.IsPressed())
        {
            UpDownInput = 0.5f;
        }
        if (downAction.action.IsPressed())
        {
            UpDownInput = -0.5f;
        }

        Camera_Velocity += new Vector3(HorizontalInput, 0, 0);

        Camera_Velocity += new Vector3(0, 0, VerticalInput);

        Camera_Velocity += new Vector3(0, UpDownInput , 0);

        return Camera_Velocity;


    }

}