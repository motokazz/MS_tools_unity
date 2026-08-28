using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Animatorチェッカー
/// パラメーターコントロールできるUIを自動的に作成する。
/// 
/// ～使いかた～
/// Animatorに対象アニメーターを設定
/// ParentにGUIの親を設定（Panelとか）
/// 各種ＵＩコンポーネントにそれぞれprefab化したUIを設定
/// 
/// ※　Canvas_AnimationCheckerがセットアップ済みアセット
/// 
/// </summary>
/// 

public class AnimatorParameterLister : MonoBehaviour
{
    [Header("対象オブジェクトの親")]
    [SerializeField] GameObject animatorParent;

    [Header("UIコントローラーの親")]
    [SerializeField] RectTransform GUIparent;
    
    [Header("各種コントローラー")]
    [SerializeField] Toggle toggle;
    [SerializeField] Slider slider;
    [SerializeField] Button button;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] TMP_Text tmpText;
    [SerializeField] ScrollRect scrollRect;

    [Header("セレクト")]
    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] Button nextButton;
    [SerializeField] Button prevButton;

    public class AnimatorParameter
    {
        public string name = string.Empty;
        public AnimatorControllerParameterType controllerType;
    }
    List<AnimatorParameter> parameters = new List<AnimatorParameter>();


    Animator animator;
    int count = 0;

    void Start()
    {
        scrollRect.verticalNormalizedPosition = 1;
        //Dropdown構築
        dropdown.ClearOptions();
        List<string> optionList = new List<string>();
        var gos = GetChildlenGameObjects(animatorParent.transform);
        foreach (var g in gos)
        {
            optionList.Add(g.name);
        }
        dropdown.AddOptions(optionList);
        dropdown.onValueChanged.AddListener((value) => CharSelect(value));

        //PrefabSelectorボタン
        nextButton.onClick.AddListener(()=> NextChar());
        prevButton.onClick.AddListener(()=> PrevChar());

        //一体目呼び出し
        PrefabSelector();
    }

    //Animator探して準備
    void Setups()
    {

        GameObject targetObject;

        //Parent特定
        if (animatorParent != null)
        {
            //animatorParent設定があるときはそのオブジェクトが親
            targetObject = animatorParent.gameObject;
        }
        else
        {
            //無ければ自分が親
            targetObject = gameObject;
        }

        //Animator探し
        if (targetObject != null)
        {
            //アニメーターを探す
            animator = targetObject.GetComponent<Animator>();

            //アニメーターが無ければ子からアニメーターを探す
            if (animator == null)
            {
                animator = targetObject.GetComponentInChildren<Animator>();
            }
        }
        else
        {
            //ターゲット無ければ処理しない
            return;
        }

        //AnimatorParameterを設定
        parameters = new List<AnimatorParameter>();
        foreach (var param in animator.parameters)
        {
            AnimatorParameter animatorParameter = new AnimatorParameter();
            animatorParameter.name = param.name;
            animatorParameter.controllerType = param.type;
            parameters.Add(animatorParameter);
        }
    }

    //子のゲームオブジェクトリスト
    GameObject[] GetChildlenGameObjects(Transform tr)
    {
        GameObject[] gos = new GameObject[tr.childCount];

        // 0～個数-1までの子を順番に配列に格納
        for (var i = 0; i < gos.Length; ++i)
        {
            gos[i] = tr.GetChild(i).gameObject;
        }
        return gos;
    }

    // 対象オブジェクトセレクト
    void CharSelect(int id)
    {
        count = id;
        PrefabSelector();
    }

    void NextChar()
    {
        count++;
        if(count > animatorParent.transform.childCount-1) {count = 0;}
        dropdown.value = count;
        PrefabSelector();
    }

    void PrevChar()
    {
        count--;
        if(count < 0) {count = animatorParent.transform.childCount-1;}
        dropdown.value = count;
        PrefabSelector();
    }

    void PrefabSelector()
    {
        //子オブジェクトを全部非アクティブ
        var gos = GetChildlenGameObjects(animatorParent.transform);
        foreach (var obj in gos) { 
            obj .SetActive(false);
        }
        
        //カウントチェック
        if (count > gos.Length)
        {
            count= 0;
        }
        if (count<0)
        {
            count = gos.Length;
        }

        //GameObject設定
        GameObject go = gos[count];
        go.SetActive(true);

        //Animator設定。無かったら何もしない
        if (go.GetComponent<Animator>() != null) { 
            animator = go.GetComponent<Animator>();

            // MS_PlayerController用
            if (gameObject.GetComponent<MS_PlayerController>() != null)
            {
                Debug.Log("*");
                var ms_playerController = gameObject.GetComponent<MS_PlayerController>();
                ms_playerController.animator = go.GetComponent<Animator>();
            }

        }
        else {return; }




        //全部初期化
        Setups();

        //GUI再構築
        DestroyGUI();
        CreateGUI();

    }

 
    // GUI
    void CreateGUI()
    {
        foreach (var param in parameters)
        {
            Rect rect = new Rect(10, 10, 300, 50);

            switch (param.controllerType)
            {
                // Bool
                case AnimatorControllerParameterType.Bool:

                    var tgl = Instantiate(toggle, GUIparent);
                    tgl.name = param.name;
                    tgl.GetComponentInChildren<Text>().text = param.name;
                    tgl.onValueChanged.AddListener((value) => { ChangeToggle(param.name, value); });

                    break;

                //Trigger
                case AnimatorControllerParameterType.Trigger:

                    var but = Instantiate(button, GUIparent);
                    but.name = param.name;
                    but.GetComponentInChildren<TMP_Text>().text = param.name;
                    but.onClick.AddListener(() => { ChangeTrigger(param.name); });
                    break;

                //Float
                case AnimatorControllerParameterType.Float:

                    var tx = Instantiate(tmpText, GUIparent);
                    tx.text = param.name;

                    var sld = Instantiate(slider, GUIparent);

                    sld.name = param.name;
                    //sld.GetComponentInChildren<Text>().text = param.name;
                    sld.onValueChanged.AddListener((value) => { ChangeFloat(param.name, value); });
                    break;

                //Int
                case AnimatorControllerParameterType.Int:
                    
                    var txi = Instantiate(tmpText, GUIparent);
                    txi.text = param.name;

                    var fld = Instantiate(inputField, GUIparent);
                    fld.name = param.name;

                    fld.text = animator.GetInteger(param.name).ToString();

                    fld.onValueChanged.AddListener((value) => { ChangeInt(param.name, int.Parse(value)); });
                    break;
            }
        }
    }

    void DestroyGUI()
    {
        var children = GetChildlenGameObjects(GUIparent);
        Debug.Log(children.ToString());
        foreach (var go in children)
        {
            Destroy(go.gameObject);
        }
    }

    // Bool処理
    void ChangeToggle(string paramName, bool isOn)
    {
        if (isOn)
        {
            animator.SetBool(paramName, true);
        }
        else
        {
            animator.SetBool(paramName, false);
        }
    }

    //Trigger処理
    private void ChangeTrigger(string paramName)
    {
        animator.SetTrigger(paramName);
    }

    //Float処理
    private void ChangeFloat(string paramName,float value)
    {
        Debug.Log(paramName+":"+value.ToString());
        animator.SetFloat(paramName, value);
    }

    //Int処理
    private void ChangeInt(string paramName,int value)
    {
        animator.SetInteger(paramName, value);
    }

}
