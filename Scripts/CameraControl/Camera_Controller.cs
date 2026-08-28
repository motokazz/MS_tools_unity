using UnityEngine;

/// <summary>
/// 旧Input対応カメラコントローラー
/// </summary>
 
public class Camera_Controller : MonoBehaviour
{ 

    [SerializeField] float Normal_Speed = 1.0f; //Normal movement speed
   
    [SerializeField] float Shift_Speed = 5.0f; //multiplies movement speed by how long shift is held down.
   
    [SerializeField] float Speed_Cap = 5.0f; //Max cap for speed when shift is held down
  
    [SerializeField] float moveSensitivity = 0.2f; //How sensitive it with mouse

    [SerializeField] float rotateSensivity = 2.0f;//回転のセンシティブ

    [SerializeField] bool invertY = false;

    [SerializeField] Transform target;

    public enum mouse_buttons {
        left,
        right,
        middle
    }

    [Header("◆　キー設定")]
    [SerializeField] mouse_buttons Camera_Rotate = mouse_buttons.right;
    [SerializeField] mouse_buttons Camera_Drag = mouse_buttons.middle;
    [SerializeField] KeyCode cameraResetKey = KeyCode.R;

    private Vector3 Mouse_Location = new Vector3(255, 255, 255); //Mouse location on screen during play (Set to near the middle of the screen)
    
    private float Total_Speed = 1.0f; //Total speed variable for shift

    Vector3 InitPosition;
    Vector3 InitRotation;


    private void Start()
    {
        //Get Camera initiral position
        InitPosition = transform.position;
        InitRotation = transform.eulerAngles;

        //Get Target (if none target is self)
        if (!target)
        {
            target = transform;
        }
    }

    private void Update()
    {
        // Camera Rotation
        if (Input.GetMouseButtonDown((int)Camera_Rotate) || Input.GetMouseButtonDown((int)Camera_Drag))
        {
            Mouse_Location = Input.mousePosition;
        }

        if (Input.GetMouseButton((int)Camera_Rotate)){
            RotateCameraByQuaternion(target,invertY);
            moveCamera();
        }


        //Set Camera initiral position
        if (Input.GetKey(cameraResetKey))
        {
            transform.position = InitPosition;
            transform.eulerAngles = InitRotation;
        }
    }


    //カメラ回転・クオータニオン
    void RotateCameraByQuaternion(Transform transform,bool inverty)
    {
        Vector2 mouseDelta = rotateSensivity * new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));

        Quaternion rotation = transform.rotation;

        Quaternion horiz = Quaternion.AngleAxis(mouseDelta.x, Vector3.up);

        int invertYVar = 1;
        if (inverty)
        {
            invertYVar = -1;
        }

        Quaternion vert = Quaternion.AngleAxis(mouseDelta.y * invertYVar , Vector3.right);
        transform.rotation = horiz * rotation * vert;
    }

    //カメラ移動

    void moveCamera()
    {
        Vector3 Cam;
        //Keyboard controls
        if (Input.GetMouseButton((int)Camera_Drag))
        {
            Cam = DragCamera();
        }
        else
        {
            Cam = GetBaseInput();
        }
        //
        Cam = new Vector3(Cam.x, Cam.y, Cam.z + (Input.GetAxis("Mouse ScrollWheel")*100));

        //加速
        if (Input.GetKey(KeyCode.LeftShift))
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
       
        if (Input.GetKey(KeyCode.Space))
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
        
        float HorizontalInput = Input.GetAxis("Horizontal"); //Input for horizontal movement      
        float VerticalInput = Input.GetAxis("Vertical"); //Input for Vertical movement

        float UpDownInput = 0;
        if (Input.GetKey(KeyCode.E))
        {
            UpDownInput = 0.5f;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            UpDownInput = -0.5f;
        }

        Camera_Velocity += new Vector3(HorizontalInput, 0, 0);

        Camera_Velocity += new Vector3(0, 0, VerticalInput);

        Camera_Velocity += new Vector3(0, UpDownInput , 0);

        return Camera_Velocity;


    }

    //マウスによるカメラ移動
    private Vector3 DragCamera()
    {
        Vector3 ret;
        //Camera drag based on mouse position
        Mouse_Location = Input.mousePosition - Mouse_Location;
        Mouse_Location = new Vector3(-Mouse_Location.x * moveSensitivity, -Mouse_Location.y * moveSensitivity, 0);
        ret = Mouse_Location;
        Mouse_Location = Input.mousePosition;
        return ret;
    }

}