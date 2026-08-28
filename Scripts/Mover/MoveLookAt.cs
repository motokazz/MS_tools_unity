using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveLookAt : MonoBehaviour
{
	
	
	public float turretRotationSmooth = 0.8f;
	
	private Transform player;
	
	private void Start()
	{
		// 始めにプレイヤーの位置を取得できるようにする
		if (GameObject.FindWithTag("Player"))
		{
			player = GameObject.FindWithTag("Player").transform;
		}
	}

	private void Update()
	{
		if (player != null){
			// 砲台をプレイヤーの方向に向ける
			Quaternion targetRotation = Quaternion.LookRotation(player.position - transform.position,Vector3.back);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turretRotationSmooth);
		}
	}
}
