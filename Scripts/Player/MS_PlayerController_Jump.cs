using UnityEngine;
using UnityEngine.InputSystem;

// MS_PlayerController を継承する
public class MS_PlayerController_Jump : MS_PlayerController
{
    [Header("Jump Settings")]
    [SerializeField] InputActionReference jump;
    [Tooltip("ジャンプの高さ")]
    [SerializeField] float jumpHeight = 1.2f;

    // 前のフレームの接地状態を保持する変数 (着地検知用)
    private bool wasGrounded;

    // 親クラスの重力計算メソッドを上書き(override)する
    protected override void UpdateVerticalVelocity()
    {
        // 現在の接地状態を取得
        bool isGrounded = characterController.isGrounded;

        if (isGrounded)
        {
            if (verticalVelocity < 0.0f)
            {
                verticalVelocity = -2f;
            }

            // --- ジャンプ開始のアニメーション ---
            // 接地していて、ジャンプ入力があった場合
            if (jump != null && jump.action.triggered)
            {
                // 物理的なジャンプ処理
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                // アニメーショントリガーを発動 (トリガー名はAnimatorに合わせて変更)
                if (animator != null)
                {
                    // "JumpStart" という名前のトリガーを作成してください
                    animator.SetTrigger("Jump");
                }
            }

            // --- 着地のアニメーション ---
            // 前のフレームで空中、今のフレームで接地した場合
            if (!wasGrounded)
            {
                // アニメーショントリガーを発動 (トリガー名はAnimatorに合わせて変更)
                if (animator != null)
                {
                    // "Land" という名前のトリガーを作成してください
                    animator.SetTrigger("Land");
                }
            }
        }

        // 重力を常に適用
        verticalVelocity += gravity * Time.deltaTime;

        // 次のフレームのために接地状態を保存
        wasGrounded = isGrounded;
    }
}