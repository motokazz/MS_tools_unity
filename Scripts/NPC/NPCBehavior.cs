using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
public class NPCBehavior: MonoBehaviour
{
    [Header("NavMeshAgent")]
    [SerializeField] NavMeshAgent agent;

    [Header("NPCAnimation")]
    [SerializeField] NPCAnimation npcAnmation;

    [Header("StartBehaviors")]
    [SerializeField] BehaviorMethods startBehavior;

    [Header("Behaviors")]
    [SerializeField] NPCWander wander;
    [SerializeField] NPCPointMovement pointMovement;
    [SerializeField] MS_PlayerController playerController;

    [Header("ボタン割り付け")]
    [SerializeField] Button buttonAllDisable;
    [SerializeField] Button buttonWander;
    [SerializeField] Button buttonPointMovement;
    [SerializeField] Button buttonPlayerControl;

    private enum BehaviorMethods {disable,wander,player,pointMovement};

    private void Awake()
    {
        buttonWander.onClick.AddListener(() => ActivateWonder());
        buttonPointMovement.onClick.AddListener(() => ActivatePointMovement());
        buttonAllDisable.onClick.AddListener(() => AllDisable());
        buttonPlayerControl.onClick.AddListener(() => ActivatePlayerControl());

    }



    private void Start()
    {
        switch (startBehavior)
        {
            case BehaviorMethods.disable:
                AllDisable();
                break;
            case BehaviorMethods.wander:
                ActivateWonder();
                break;
            case BehaviorMethods.player:
                ActivatePlayerControl();
                break;
            case BehaviorMethods.pointMovement:
                ActivatePointMovement();
                break;
        }
    }




    void ActivatePlayerControl()
    {
        if (agent != null) { agent.isStopped = true; }
        AllDisable();
        playerController.enabled = true;
        buttonPlayerControl.GetComponent<Image>().color = buttonPlayerControl.colors.pressedColor;
    }

    void ActivateWonder()
    {
        if (agent != null) { agent.isStopped = false; }
        AllDisable();
        npcAnmation.enabled = true;
        wander.enabled = true;
        buttonWander.GetComponent<Image>().color = buttonWander.colors.pressedColor;
    }
    void ActivatePointMovement()
    {
        if (agent != null) { agent.isStopped = false; }
        AllDisable();
        npcAnmation.enabled = true;
        pointMovement.enabled = true;
        buttonPointMovement.GetComponent<Image>().color = buttonPointMovement.colors.pressedColor;
    }
    void AllDisable()
    {

        playerController.enabled = false;
        npcAnmation.enabled = false;
        wander.enabled = false;
        pointMovement.enabled = false;

        buttonPlayerControl.GetComponent<Image>().color = buttonPlayerControl.colors.normalColor;
        buttonWander.GetComponent<Image>().color = buttonWander.colors.normalColor;
        buttonPointMovement.GetComponent<Image>().color = buttonPointMovement.colors.normalColor;

    }

}
