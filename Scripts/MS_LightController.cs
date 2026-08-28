using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// スポットライトの値を実行中に弄る
/// TextMeshProUGUIを指定しておくと各パラメーターが表示される
/// </summary>
/// 
public class MS_LightController : MonoBehaviour
{
    [SerializeField] Light[] targetLights;

    [Header("LightControl")]

    [SerializeField] KeyCode lightOn = KeyCode.G;

    [SerializeField] KeyCode innerSpotAngleUpKey = KeyCode.Y;
    [SerializeField] KeyCode innerSpotAngleDownKey = KeyCode.H;

    [SerializeField] KeyCode outerSpotAngleUpKey = KeyCode.U;
    [SerializeField] KeyCode outerSpotAngleDownKey = KeyCode.J;

    [SerializeField] KeyCode intensityUpKey = KeyCode.I;
    [SerializeField] KeyCode intensityDownKey = KeyCode.K;

    [SerializeField] KeyCode rangeUpKey = KeyCode.O;
    [SerializeField] KeyCode rangeDownKey = KeyCode.L;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI intensityText;
    [SerializeField] TextMeshProUGUI rangeText;
    [SerializeField] TextMeshProUGUI innerSpotAngleText;
    [SerializeField] TextMeshProUGUI outerSpotAngleText;

    // Start is called before the first frame update
    void Start()
    {
        refleshUI();
    }

    // Update is called once per frame
    void Update()
    {
        //On/Off
        if (Input.GetKeyDown(lightOn))
        {
            LightOn();
        }

        //range
        if (Input.GetKey(rangeUpKey))
        {
            RangeUP();
        }
        if (Input.GetKey(rangeDownKey))
        {
            RangeDown();
        }
        //inner/outer width
        if (Input.GetKey(innerSpotAngleUpKey))
        {
            InnerSpotAngleUp();
        }
        if (Input.GetKey(innerSpotAngleDownKey))
        {
            InnerSpotAngleDown();
        }
        if (Input.GetKey(outerSpotAngleUpKey))
        {
            OuterSpotAngleUp();
        }
        if (Input.GetKey(outerSpotAngleDownKey))
        {
            OuterSpotAngleDown();
        }
        //intensity
        if (Input.GetKey(intensityUpKey))
        {
            IntensityUp();
        }
        if (Input.GetKey(intensityDownKey))
        {
            IntensityDown();
        }
    }


    void LightOn()
    {
        foreach (Light light in targetLights) {
            if (light.enabled)
            {
                light.enabled = false;
            }
            else { light.enabled = true; } 
        }
        refleshUI();
    }

    void InnerSpotAngleUp()
    {
        foreach(Light light in targetLights){ light.innerSpotAngle += 1;}
        refleshUI();
    }

    void InnerSpotAngleDown()
    {
        foreach (Light light in targetLights) { light.innerSpotAngle -= 1; }
        refleshUI();
    }
    void OuterSpotAngleUp()
    {
        foreach (Light light in targetLights) { light.spotAngle += 1; }
        refleshUI();
    }

    void OuterSpotAngleDown()
    {
        foreach (Light light in targetLights) { light.spotAngle -= 1; }
        refleshUI();
    }

    void IntensityUp()
    {
        foreach (Light light in targetLights) { light.intensity += 0.1f; }
        refleshUI();
    }
    void IntensityDown()
    {
        foreach (Light light in targetLights) { light.intensity -= 0.1f; }
        refleshUI();
    }


    void RangeUP()
    {
        foreach (Light light in targetLights) { light.range += 1; }
        refleshUI();
    }

    void RangeDown()
    {
        foreach (Light light in targetLights) { light.range -= 1; }
        refleshUI();
    }

    void refleshUI()
    {
        foreach (Light light in targetLights)
        {
            if (intensityText)
            {
                intensityText.text = "intensity (" + intensityUpKey.ToString() + "/" + intensityDownKey.ToString() + ") = " + light.intensity.ToString();
            }

            if (rangeText)
            {
                rangeText.text = "range (" + rangeUpKey.ToString() + "/" + rangeDownKey.ToString() + ") = " + light.range.ToString();
            }

            if (innerSpotAngleText)
            {
                innerSpotAngleText.text = "innerSpotAngle (" + innerSpotAngleUpKey.ToString() + "/" + innerSpotAngleDownKey.ToString() + ") = " + light.innerSpotAngle.ToString();
            }

            if (outerSpotAngleText)
            {
                outerSpotAngleText.text = "outerSpotAngle (" + outerSpotAngleUpKey.ToString() + "/" + outerSpotAngleDownKey.ToString() + ") = " + light.spotAngle.ToString();
            }
        }
    }

}
