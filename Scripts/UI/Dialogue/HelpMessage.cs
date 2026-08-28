using System.Collections;
using UnityEngine;
using UnityEngine.UI;
public class HelpMessage:MonoBehaviour
{
    public string message;
    
    BalloonDialog balloonDialog;

    private void Awake()
    {
        balloonDialog = FindAnyObjectByType<BalloonDialog>();
    }

    public void OnPointerEnter()
    {
        balloonDialog.Show(message, this.transform.position);
    }

    public void OnPointerExit()
    {
        balloonDialog.Hide();
    }
}


