using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeTimer : MonoBehaviour {

	[SerializeField]
	float lifeTime;

	void Awake () {
		StartCoroutine( KillAfterSeconds() );
	}

	IEnumerator KillAfterSeconds()	{
		yield return new WaitForSeconds(lifeTime+0.01f);

		Destroy(gameObject);
	}

}
