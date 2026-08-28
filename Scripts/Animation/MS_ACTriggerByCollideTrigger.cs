using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MS_ACTriggerByCollideTrigger : MonoBehaviour
{
    [Header("ターゲットアニメーター")]
    [SerializeField] Animator animator;
    [SerializeField] string triggerNameOn;
    [SerializeField] string triggerNameOff;

    [Header("検知するコリジョン名")]
    [Tooltip("※タグを増やさないようにコリジョン名を使用")]
    [SerializeField] string colisionName = "Activator";

    [Header("On/Off可能")]
    [Tooltip("スイッチを押せる回数 -1 で無限")]
    [SerializeField] int switchableCount = -1;
    [Tooltip("スイッチ起動後の作動不能時間")]
    [SerializeField] float switchInterval = 0.5f;

    [Header("スタート時の設定")]
    [SerializeField] bool switchFlag = false;
    // Start is called before the first frame update

    private void Start()
    {
        if (switchFlag)
        {
            animator.SetTrigger(triggerNameOn);
        }
        else
        {
            animator.SetTrigger(triggerNameOff);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.name == colisionName)
        {
            if (switchableCount != 0)
            {
                //
                if (switchFlag)
                {
                    StartCoroutine(DelayCoroutine(triggerNameOff,false,switchInterval));
                }
                else
                {
                    StartCoroutine(DelayCoroutine(triggerNameOn, true, switchInterval));
                }

                //
                if(switchableCount > 0)
                {
                    switchableCount -= 1;
                }
            }
        }
    }

    //アニメーション再生後のインターバル設定
    private IEnumerator DelayCoroutine(string triggerName,bool flag,float interval)
    {
        yield return null;

        animator.SetTrigger(triggerName);

        yield return new WaitForSeconds(interval);

        switchFlag = flag;
    }
}
