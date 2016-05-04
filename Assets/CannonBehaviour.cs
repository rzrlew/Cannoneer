using UnityEngine;
using System.Collections;

public class CannonBehaviour : MonoBehaviour {
	public GameObject terrain;
	public GameObject bullet;
	public bool active;
	public float shotForce = 2000;
	public delegate void ElevationChange();
	public ElevationChange OnElevationChange;
	// Use this for initialization
	void Start () {
		active = true;
		transform.localRotation = Quaternion.Euler(new Vector3(45f,0,0));
		Camera.main.GetComponent<CameraBehaviour>().cannon = gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		if(active){
			//Fire Bullet
			if(Input.GetAxis("Fire1")>0.1){
				//Instantiate bullet
				GameObject bulletInstance = Instantiate(bullet, transform.position, transform.rotation) as GameObject;
				bulletInstance.GetComponent<Rigidbody>().AddForce(transform.up * shotForce);
				bulletInstance.GetComponent<BulletBehaviour>().cannon = gameObject;
				bulletInstance.GetComponent<BulletBehaviour>().terrain = terrain;

				//Set camera to follow bullet
				Camera.main.transform.SetParent(bulletInstance.transform);
				Camera.main.transform.localRotation = new Quaternion(0,0,0,0);
				Camera.main.transform.localPosition = new Vector3(0,0,-10);

				//Disable cannon control
				active = false;
			}
			//Turn cannon left
			transform.parent.Rotate(new Vector3(0, Input.GetAxis("Horizontal") * 90 * Time.deltaTime, 0));
			//Turn cannon right
			transform.parent.Rotate(new Vector3(0, Input.GetAxis("Horizontal") * 90 * Time.deltaTime, 0));
			//Turn cannon up
			if(Input.GetAxis("Vertical") < 0.1f || Input.GetAxis("Vertical") > 0.1f){
				transform.RotateAround(transform.parent.position, transform.parent.right, Input.GetAxis("Vertical") * 45 * Time.deltaTime);
				transform.RotateAround(transform.parent.position, transform.parent.right, Input.GetAxis("Vertical") * 45 * Time.deltaTime);
				OnElevationChange();
				//transform.Rotate(new Vector3(45 * Time.deltaTime, 0, 0));
			}
		}
	}
}
