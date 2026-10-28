using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class MS_Jump : MonoBehaviour
{
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private Animator animator;

    [SerializeField] private float jumpHeight = 2.0f;
    [SerializeField] private float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;

    private bool isJumping = false;   // アニメーション用にジャンプ中かどうかを明確に管理

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        jumpAction.action.Enable();
        jumpAction.action.performed += OnJump;
    }

    void OnDestroy()
    {
        jumpAction.action.performed -= OnJump;
    }

    void Update()
    {
        JumpCore();
    }

    private void JumpCore()
    {
        if (controller.isGrounded)
        {
            animator.SetBool("Air", false);
            if (isJumping)
            {
                isJumping = false;
            }

            if (velocity.y < 0f)
                velocity.y = -2f; // ← 修正ポイント
        }
        else
        {
            animator.SetBool("Air", true);
        }
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }


    private void OnJump(InputAction.CallbackContext context)
    {
        // 地面にいる時だけジャンプ可能
        if (controller.isGrounded && !isJumping)
        {
            Debug.Log("JUMP");

            // 物理的に正しいジャンプ初速の計算
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
            animator.SetBool("Air", true);
            isJumping = true;
        }
    }
}