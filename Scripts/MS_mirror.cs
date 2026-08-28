using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 鏡用カメラの挙動スクリプト
/// reflectionCameraを利用して鏡用のRenderTextureを作成する用途
/// 参考：https://qiita.com/nkjzm/items/ccba41a6e7e5211aae95
/// ※必須：メインカメラのタグを「MainCamera」にする
/// </summary>
public class MS_mirror : MonoBehaviour
{
    [Tooltip("メインカメラ")]
    Camera cam;

    [Tooltip("反射用カメラ。レンダーテクスチャをレンダリングする")]
    [SerializeField] Camera reflectionCamera;
    
    [Tooltip("鏡面反射の中心点となるオブジェクト。")]
    [SerializeField] Transform mirrorPosition;
    
    [Tooltip("鏡のサイズ")]
    [SerializeField] float size=1;

    float distance;

    private void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (cam!=null)
        {
            // カメラから鏡面へのベクトル
            var diff = mirrorPosition.transform.position - cam.transform.position;
            // 鏡面の垂直ベクトル
            var normal = mirrorPosition.transform.forward;
            // 鏡面からの反射ベクトル
            var reflection = diff + 2 * (Vector3.Dot(-diff, normal)) * normal;
            // 鏡面座標に反転させた反射ベクトルを加算する
            reflectionCamera.transform.position = mirrorPosition.transform.position - reflection;

            // 鏡面の方向に向ける
            reflectionCamera.transform.LookAt(mirrorPosition.position);

            // カメラ設定の更新
            distance = Vector3.Distance(mirrorPosition.transform.position, reflectionCamera.transform.position);
            reflectionCamera.nearClipPlane = distance * 0.99f;

            // 焦点距離と表示したい鏡面サイズから画角(FOV)を計算する
            reflectionCamera.fieldOfView = 2 * Mathf.Atan(size / (2 * distance)) * Mathf.Rad2Deg;
        }
    }
}
