using UnityEngine;
using System.Collections;

public class BulletRotation : MonoBehaviour {
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.A)){
			transform.Rotate(Vector3.left, 360 * Time.deltaTime);
		}
		if(Input.GetKey(KeyCode.D)){
			transform.Rotate(Vector3.left, -360 * Time.deltaTime);
		}
		if(Input.touchCount>0){
			
			//			bulletRigidBody.AddRelativeForce(new Vector3(Input.touches[0].deltaPosition.x * 10,0,0));//Relative force left
			//			bulletRigidBody.AddRelativeForce(new Vector3(Input.touches[0].deltaPosition.x * 10,0,0));//Relative force right
			//			bulletRigidBody.AddRelativeForce(new Vector3(0,Input.touches[0].deltaPosition.y * 5,0));//Relative force up
			//			bulletRigidBody.AddRelativeForce(new Vector3(0,Input.touches[0].deltaPosition.y * 5,0));//Relative force down
			if(Input.touches[0].position.x < Screen.width/2){
				transform.Rotate(Vector3.left, 360 * Time.deltaTime);
			}
			if(Input.touches[0].position.x > Screen.width/2){
				transform.Rotate(Vector3.left, -360 * Time.deltaTime);
			}
		}
	}
}
