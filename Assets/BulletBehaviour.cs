using UnityEngine;
using System.Collections;

public class BulletBehaviour : MonoBehaviour {
	public GameObject terrain;
	public GameObject cannon;
	public GameObject explosionFx;
	private Rigidbody bulletRigidBody;
	// Use this for initialization
	void Start () {
		bulletRigidBody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if(Input.touchCount>0){

//			bulletRigidBody.AddRelativeForce(new Vector3(Input.touches[0].deltaPosition.x * 10,0,0));//Relative force left
//			bulletRigidBody.AddRelativeForce(new Vector3(Input.touches[0].deltaPosition.x * 10,0,0));//Relative force right
//			bulletRigidBody.AddRelativeForce(new Vector3(0,Input.touches[0].deltaPosition.y * 5,0));//Relative force up
//			bulletRigidBody.AddRelativeForce(new Vector3(0,Input.touches[0].deltaPosition.y * 5,0));//Relative force down
			if(Input.touches[0].position.x < Screen.width/2){
				bulletRigidBody.AddRelativeForce(new Vector3(-10,0,0));//Relative force left
			}
			if(Input.touches[0].position.x > Screen.width/2){
				bulletRigidBody.AddRelativeForce(new Vector3(10,0,0));//Relative force right
			}
		}
		bulletRigidBody.AddRelativeForce(new Vector3(Input.GetAxis("Horizontal") * 10,0,0));//Relative force left
		bulletRigidBody.AddRelativeForce(new Vector3(Input.GetAxis("Horizontal") * 10,0,0));//Relative force right
		bulletRigidBody.AddRelativeForce(new Vector3(0,Input.GetAxis("Vertical") * 5,0));//Relative force up
		bulletRigidBody.AddRelativeForce(new Vector3(0,Input.GetAxis("Vertical") * 5,0));//Relative force down
		transform.rotation = Quaternion.LookRotation(bulletRigidBody.velocity ,Vector3.up);//Rotate in velocity direction.
	}

	void OnCollisionEnter(Collision col){
		//Instantiate Explosion
		GameObject explosionInstance = Instantiate(explosionFx);
		explosionInstance.transform.position = col.contacts[0].point;
		explosionInstance.GetComponent<ExplosionBehaviour>().OnExplosion = terrain.GetComponent<TargetManager>().DestroyTargetInRadius;
		//Camera shot view
		Camera.main.transform.SetParent(cannon.transform);
		Camera.main.transform.position = col.contacts[0].point + new Vector3(0,30,0);
		Camera.main.transform.LookAt(col.contacts[0].point);
		Camera.main.GetComponent<CameraBehaviour>().shotView = true;

		
		Destroy(transform.gameObject);
	}
}
