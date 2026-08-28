using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// トリガーでPlayableDirectorをPlay
/// </summary>
public class MS_PlayableDirectorByCollideTrigger : MonoBehaviour
{
    [Header("ターゲットアニメーター")]
    [SerializeField] PlayableDirector playableDirector;

    [Header("検知するTag名")]
    [SerializeField] string _tagName = "Enemy";

    [Header("On/Off可能")]
    [Tooltip("スイッチを押せる回数 -1 で無限")]
    [SerializeField] int switchableCount = -1;

    [Header("正面判定")]
    [SerializeField] bool frontDitect = false;
    [SerializeField] float frontGap = 10.0f;

    [Header("発生頻度（ランダム）")]
    [SerializeField] float frequency = 1.0f;

    [Header("TriggerEnter/Exit発動用タイムライン")]
    [SerializeField] TimelineAsset timelineEnter;
    [SerializeField] TimelineAsset timelineExit;

    [Header("強制終了")]
    [SerializeField] bool _forceExit = false;

    Collider tempCollider;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag(_tagName)) { return; }


        if (switchableCount != 0)
        {
            //正面判定
            var angle = 180 - Quaternion.Angle(this.transform.rotation, other.transform.rotation);
            if (frontDitect && Mathf.Abs(angle) > frontGap)
            {
                return;
            }
            //ランダム
            var rand = Random.Range(0f, 1f);
            //Debug.Log(rand);
            if (rand > frequency)
            {
                return;
            }

            if (timelineEnter != null)
            {
                playableDirector.Play(timelineEnter);
            }
            else
            {
                playableDirector.Play();
            }

            //
            if (switchableCount > 0)
            {
                switchableCount -= 1;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == tempCollider)
        {
            if (_forceExit)
            {
                playableDirector.time = playableDirector.duration;
            }
            if (timelineExit != null)
            {
                playableDirector.Play(timelineExit);
            }
            tempCollider = null;
        }
    }

    public bool IsDone()
    {
        return playableDirector.time >= playableDirector.duration;
    }
}
