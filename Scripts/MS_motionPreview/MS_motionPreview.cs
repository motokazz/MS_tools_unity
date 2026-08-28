using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using TMPro;

public class MS_motionPreview : MonoBehaviour
{
    [System.Serializable] public class MS_motionPreview_floatParameters
    {
        public string paramName;
        public string bindAxis;
        public Slider UISlider;
    }

    [System.Serializable]
    public class MS_motionPreview_floatParameters_vp
    {
        public string paramNameV;
        public string paramNameH;
        public OnScreenStick OSStick;

    }

    [System.Serializable] public class MS_motionPreview_triggerParams
    {
        public string paramName;
        public Button UIButton;
    }

    [SerializeField] Animator animator;
    [SerializeField] List<MS_motionPreview_floatParameters> floatParameters = new List<MS_motionPreview_floatParameters>();
    [SerializeField] List<MS_motionPreview_floatParameters_vp> vpParameters = new List<MS_motionPreview_floatParameters_vp>();
    [SerializeField] List<MS_motionPreview_triggerParams> triggerParameters = new List<MS_motionPreview_triggerParams>();


    //TurnTable
    [SerializeField] Transform turnTable;
    [SerializeField] Slider sliderTurnTable;



    // Start is called before the first frame update
    void Start()
    {
        foreach(MS_motionPreview_floatParameters p in floatParameters)
        {
            if(p.UISlider != null)
            {
                p.UISlider.onValueChanged.AddListener(delegate { ValueChangedCheck(p.paramName, p.UISlider); });
            }
        }

        foreach (MS_motionPreview_triggerParams p in triggerParameters)
        {

            if (p.UIButton != null)
            {
                TextMeshProUGUI tmp = p.UIButton.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = p.paramName;
                }
                p.UIButton.onClick.AddListener (delegate { OnClickedButtonTrigger(p.paramName); });
            }
        }

        sliderTurnTable.onValueChanged.AddListener(delegate { TurnTable(turnTable, sliderTurnTable); });

    }

    // Update is called once per frame
    void Update()
    {

    }

    void ValueChangedCheck(string paramNane,Slider slider)
    {
        animator.SetFloat(paramNane, slider.value);
    }

    void OnClickedButtonTrigger(string paramNane)
    {
        if (animator.GetBool(paramNane))
        {
            animator.SetBool(paramNane, false);
        }
        else
        {
            animator.SetBool(paramNane,true);
        }
    }

    void TurnTable(Transform trn,Slider slider)
    {
        if(trn != null)
        {
            trn.transform.eulerAngles = new Vector3(trn.eulerAngles.x, slider.value, trn.eulerAngles.z);
        }
    }
}
