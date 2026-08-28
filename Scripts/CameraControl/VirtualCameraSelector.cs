using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.UI;
using TMPro;
public class VirtualCameraSelector : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCameraBase[] cinemachineVirtualCameraBases;
    [SerializeField] TMP_Dropdown dropdown;

    void Awake()
    {
        // カメラオブジェクトごと全部非アクティブ
        List<string> optionList = new List<string>();
        foreach (var virtualCamera in cinemachineVirtualCameraBases) {
            virtualCamera.gameObject.SetActive(true);
            virtualCamera.Priority = 0; 
            optionList.Add(virtualCamera.name);
        }
        SelectionByOption(0);

        //dropdownlist作成
        dropdown.ClearOptions();
        dropdown.AddOptions(optionList);
        dropdown.onValueChanged.AddListener((value)=>SelectionByOption(value));
        
    }

    private void Start()
    {
        
    }

    void SelectionByOption(int num)
    {
        foreach (var virtualCamera in cinemachineVirtualCameraBases) {
            //virtualCamera.gameObject.SetActive(true);
            virtualCamera.Priority = 0; 
        }
        //cinemachineVirtualCameraBases[num].gameObject.SetActive(true);
        cinemachineVirtualCameraBases[num].Priority = 1;
    }



}
