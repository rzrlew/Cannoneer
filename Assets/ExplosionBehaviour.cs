using UnityEngine;
using System.Collections;

public class ExplosionBehaviour : MonoBehaviour {
	public delegate void ExplosionDamage(Vector3 position, float radius);
	public float radius = 15f;
	public bool doDamage = false;
	public ExplosionDamage OnExplosion;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(doDamage){
			OnExplosion(transform.position, radius);
			doDamage = false;
		}
	}
}
