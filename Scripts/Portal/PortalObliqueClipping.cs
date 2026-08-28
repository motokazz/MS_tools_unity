using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PortalObliqueClipping : MonoBehaviour
{
    public Transform portalPlane; // 出口側のポータルのTransform（板ポリゴンなど）

    private Camera portalCamera;

    void Awake()
    {
        portalCamera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        SetObliqueProjection();
    }

    void SetObliqueProjection()
    {
        // 1. ポータル平面の法線（面が向いている方向）と位置を取得
        // ※ポータルの板ポリゴンの向きによって forward, up, -forward など調整してください
        Vector3 normal = portalPlane.forward;
        Vector3 position = portalPlane.position;

        // 2. カメラのビュー行列（ワールド座標をカメラ相対座標に変換する行列）を取得
        Matrix4x4 viewMatrix = portalCamera.worldToCameraMatrix;

        // 3. ポータルの位置と法線を「カメラから見たローカル空間」に変換
        Vector3 camSpacePos = viewMatrix.MultiplyPoint(position);
        Vector3 camSpaceNormal = viewMatrix.MultiplyVector(normal).normalized;

        // 4. 平面の方程式に基づく距離（D）を計算
        // 平面の方程式: Ax + By + Cz + D = 0 より、D = -(Ax + By + Cz)
        float d = -Vector3.Dot(camSpaceNormal, camSpacePos);

        // 5. カメラ空間でのクリップ平面を Vector4 で定義 (x, y, z が法線、w が距離)
        Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, d);

        // 6. 斜めクリッピング行列を計算して、カメラのProjection Matrixを上書き
        portalCamera.projectionMatrix = portalCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
    }
}