using UnityEngine;
using UnityEditor;

public class RaycastMoveWindow : EditorWindow
{
    private bool alignToNormal = false;

    [MenuItem("MS_Tools/Level/Raycast Move")]
    public static void ShowWindow()
    {
        GetWindow<RaycastMoveWindow>("Raycast Move");
    }

    private void OnGUI()
    {
        GUILayout.Label("オプション", EditorStyles.boldLabel);
        alignToNormal = GUILayout.Toggle(alignToNormal, "当たった面の角度に合わせる (Align to Normal)");

        GUILayout.Space(10);
        GUILayout.Label("レイキャスト方向 (ローカル軸)", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+X (Right)")) MoveSelected(Vector3.right);
        if (GUILayout.Button("-X (Left)")) MoveSelected(Vector3.left);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+Y (Up)")) MoveSelected(Vector3.up);
        if (GUILayout.Button("-Y (Down)")) MoveSelected(Vector3.down);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("+Z (Forward)")) MoveSelected(Vector3.forward);
        if (GUILayout.Button("-Z (Back)")) MoveSelected(Vector3.back);
        GUILayout.EndHorizontal();
    }

    // 引数名を localDirection に変更し、役割を明確化
    private void MoveSelected(Vector3 localDirection)
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("オブジェクトが選択されていません。移動させたいオブジェクトを選択してください。");
            return;
        }

        // エディタ上で移動した直後でも物理判定が正しく行われるように、コライダー情報を最新に更新する
        Physics.SyncTransforms();

        Undo.SetCurrentGroupName("Raycast Move");
        int undoGroup = Undo.GetCurrentGroup();

        foreach (GameObject obj in Selection.gameObjects)
        {
            // 【変更点】オブジェクトのローカル軸（自身の傾き）をベースに、レイを飛ばすワールド方向を計算する
            Vector3 worldDirection = obj.transform.TransformDirection(localDirection);

            // 自身を無視して一番近い衝突判定を取得（ローカル軸から計算した worldDirection を使用）
            RaycastHit? closestHit = GetClosestHit(obj.transform.position, worldDirection, obj.transform);

            if (closestHit.HasValue)
            {
                RaycastHit hit = closestHit.Value;

                Undo.RecordObject(obj.transform, "Raycast Move");

                // 位置を衝突位置に移動
                obj.transform.position = hit.point;

                // オプション：当たった面の法線に角度を合わせる
                if (alignToNormal)
                {
                    // オブジェクトの現在のローカルのUp方向を面の法線に向けるよう回転を補正
                    obj.transform.rotation = Quaternion.FromToRotation(obj.transform.up, hit.normal) * obj.transform.rotation;
                }
            }
            else
            {
                Debug.Log($"[{obj.name}] の方向にはコライダーが見つかりませんでした。");
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
    }

    private RaycastHit? GetClosestHit(Vector3 origin, Vector3 direction, Transform sourceTransform)
    {
        RaycastHit[] hits = Physics.RaycastAll(origin, direction);
        RaycastHit? closestHit = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.collider.transform.IsChildOf(sourceTransform)) continue;

            if (hit.distance < minDistance)
            {
                minDistance = hit.distance;
                closestHit = hit;
            }
        }

        return closestHit;
    }
}