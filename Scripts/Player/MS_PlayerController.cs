using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MS_PlayerController : MonoBehaviour
{
    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference speedUp;

    [Tooltip("移動スピード")]
    [SerializeField] float speed = 5.0f;
    [Tooltip("ダッシュした時のスピード倍率")]
    [SerializeField] float speedUpMuliplyer = 2f;
    [Tooltip("歩きモーションの出るスピード")]
    [SerializeField] float walkSpeed = 0.5f;

    [Tooltip("加速度")]
    [SerializeField] float acceleration = 15f;
    [Tooltip("回転速度")]
    [SerializeField] float rotationSpeed = 10.0f;

    // 子クラスで重力計算に使うため protected に変更
    [SerializeField] protected float gravity = -9.81f;

    [SerializeField] public Animator animator;
    [Tooltip("ブレンドツリーに渡す値を滑らかに補完する係数。数字が少ないとゆっくり切り替わる")]
    [SerializeField] float smoothMotion = 10f;

    private float motionSpeed;

    // 子クラスからアクセスできるように protected に変更
    protected CharacterController characterController;
    private Camera mainCamera;

    private Vector3 currentHorizontalVelocity = Vector3.zero;

    // 子クラスで書き換えるため protected に変更
    protected float verticalVelocity;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        Mover();
        MotionControl();
    }

    private void Mover()
    {
        Vector2 moveInput = move.action.ReadValue<Vector2>();

        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 desiredDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        if (desiredDirection.magnitude > 0.1f)
        {
            Quaternion toRotation = Quaternion.LookRotation(desiredDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * rotationSpeed);
        }

        float currentTargetSpeed = speed;
        if (speedUp.action.IsPressed())
        {
            currentTargetSpeed *= speedUpMuliplyer;
        }

        Vector3 targetVelocity = desiredDirection * currentTargetSpeed;

        currentHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetVelocity, Time.deltaTime * acceleration);

        // 重力（上下の動き）の計算を別のメソッドに分離
        UpdateVerticalVelocity();

        Vector3 finalMovement = currentHorizontalVelocity;
        finalMovement.y = verticalVelocity;

        characterController.Move(finalMovement * Time.deltaTime);
    }

    // 子クラスで上書き(override)できるように protected virtual にする
    protected virtual void UpdateVerticalVelocity()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void MotionControl()
    {
        if (animator == null) return;

        float currentSpeed = currentHorizontalVelocity.magnitude;
        motionSpeed = Mathf.Lerp(motionSpeed, currentSpeed, Time.deltaTime * smoothMotion);
        animator.SetFloat("Speed", motionSpeed / speed * walkSpeed);
    }

    public void SetTransform(Vector3 pos)
    {
        characterController.enabled = false;
        transform.position = pos;
        transform.rotation = Quaternion.identity;
        characterController.enabled = true;
    }
}