using System.Collections;
using UnityEngine;



/// <summary>
/// ドルアーガ風スライム
/// 
/// スライムPrefabにこのスクリプトを追加
/// 壁オブジェクトに Wall Layer を設定
/// wallLayer に Wall を指定
/// moveDistance = マスサイズ
/// 再生 > ニョロニョロ動く
/// </summary>


public class DruagaSlime : MonoBehaviour
{
    public float moveSpeed = 2.0f;
    public float moveDistance = 1.0f;

    [Header("Think Time")]
    public float thinkTimeMin = 0.2f;
    public float thinkTimeMax = 0.6f;

    [Header("Telegraph")]
    public float telegraphTime = 0.25f;
    public float telegraphStretch = 0.15f;

    public LayerMask wallLayer;

    Vector3 currentDir;
    bool isMoving;
    Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
        StartCoroutine(SlimeRoutine());
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("PlayerDead");
        if (other.tag == "Player")
        {
            Debug.Log("PlayerDead");
        }
    }

    IEnumerator SlimeRoutine()
    {
        while (true)
        {
            if (!isMoving)
            {
                ChooseDirection();
                float thinkTime = Random.Range(thinkTimeMin, thinkTimeMax);
                yield return new WaitForSeconds(thinkTime);

                if (currentDir != Vector3.zero)
                {
                    yield return Telegraph();
                    yield return MoveOneCell();
                }
            }
            yield return null;
        }
    }

    void ChooseDirection()
    {
        Vector3[] dirs =
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };

        // シャッフル
        for (int i = 0; i < dirs.Length; i++)
        {
            int r = Random.Range(i, dirs.Length);
            (dirs[i], dirs[r]) = (dirs[r], dirs[i]);
        }

        foreach (var dir in dirs)
        {
            if (!IsWallAhead(dir))
            {
                currentDir = dir;
                return;
            }
        }

        currentDir = Vector3.zero;
    }

    bool IsWallAhead(Vector3 dir)
    {
        return Physics.Raycast(
            transform.position,
            dir,
            moveDistance,
            wallLayer
        );
    }

    IEnumerator Telegraph()
    {
        float t = 0f;

        Vector3 stretch =
            baseScale +
            new Vector3(
                Mathf.Abs(currentDir.x),
                0,
                Mathf.Abs(currentDir.z)
            ) * telegraphStretch;

        while (t < 1f)
        {
            t += Time.deltaTime / telegraphTime;
            transform.localScale = Vector3.Lerp(baseScale, stretch, t);
            yield return null;
        }

        // 一瞬止める（超重要）
        yield return new WaitForSeconds(0.05f);
    }

    IEnumerator MoveOneCell()
    {
        isMoving = true;

        Vector3 start = transform.position;
        Vector3 end = start + currentDir * moveDistance;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;
        transform.localScale = baseScale;
        isMoving = false;
    }



}
