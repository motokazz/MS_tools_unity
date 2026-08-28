using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover_world_position : MonoBehaviour
{
    [SerializeField] Vector3 translate;
    [SerializeField] Vector3 rotate;
    [SerializeField] float lifeTime = 5.0f;

    // Transformではなく、値そのものであるVector3とQuaternionで保存する
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        // ゲーム開始時の「位置」と「回転」の数値を記録
        startPosition = transform.position;
        startRotation = transform.rotation;

        StartCoroutine(Life());
    }

    void Update()
    {
        transform.position += translate * Time.deltaTime;
        transform.Rotate(rotate * Time.deltaTime);
    }

    IEnumerator Life()
    {
        // while(true)で囲むことで、ゲーム終了まで無限に繰り返す
        while (true)
        {
            // 指定時間（lifeTime）待機
            yield return new WaitForSeconds(lifeTime);

            // 記録しておいた初期位置・初期回転に戻す
            transform.position = startPosition;
            transform.rotation = startRotation;
        }
    }
}