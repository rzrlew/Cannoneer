using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour {

	public bool shotView = false;
	public GameObject cannon;
	public float shotViewCounter = 3f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(shotView){
			shotViewCounter -= Time.deltaTime;
			if(shotViewCounter <= 0f){
				//Return Camera to cannon
				transform.SetParent(cannon.transform);
				transform.localRotation = new Quaternion(0,0,0,0);
				transform.Rotate(new Vector3(-45f,0,0));
				transform.localPosition = new Vector3(0,0,-5f);
				//Reset counter
				shotViewCounter = 3f;
				shotView = false;
				//Activate cannon
				cannon.GetComponent<CannonBehaviour>().active = true;
			}
		}
	}
}
