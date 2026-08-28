using UnityEngine;

public class PortalCamera : MonoBehaviour
{
    public Transform playerCamera;   // メインカメラ
    public Transform portalEntrance; // 入口の枠（Transform）
    public Transform portalExit;     // 出口の枠（Transform）

    void LateUpdate()
    {
        // 1. 入口ポータルから見たプレイヤーの相対的な位置を計算
        Vector3 relativePos = portalEntrance.InverseTransformPoint(playerCamera.position);

        // 【修正点1】入口ポータルの回転の「逆（Inverse）」をプレイヤーの回転に掛けることで、相対的な回転を計算します
        Quaternion relativeRot = Quaternion.Inverse(portalEntrance.rotation) * playerCamera.rotation;

        // 2. 出口ポータルにおいて、入口での相対位置を再現する
        // 【修正点2】ポータルを通り抜ける動き（180度反転）を表現するため、ローカル座標のX軸とZ軸を反転させます
        Vector3 relativePosReflected = new Vector3(-relativePos.x, relativePos.y, -relativePos.z);
        Vector3 newPos = portalExit.TransformPoint(relativePosReflected);

        // 入口から出口への180度回転（Y軸）を考慮したクォータニオン計算
        Quaternion halfTurn = Quaternion.Euler(0f, 180f, 0f);
        Quaternion newRot = portalExit.rotation * halfTurn * relativeRot;

        // 3. 出口カメラの座標と回転を更新
        transform.position = newPos;
        transform.rotation = newRot;
    }
}