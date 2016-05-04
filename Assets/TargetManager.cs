using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetManager : MonoBehaviour {
	private List<GameObject> targets = new List<GameObject>();
	private int numTargets = 0;
	public delegate void TargetDestroyed();
	public TargetDestroyed OnTargetDestroyed;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void AddTarget(GameObject target){
		targets.Add(target);
		numTargets++;
		OnTargetDestroyed();
	}

	public int GetNumTargets(){
		return numTargets;
	}

	public void DestroyTargetInRadius(Vector3 center, float radius){
		List<GameObject> targetsToDestroy = new List<GameObject>();
		foreach(GameObject target in targets){
			if(target != null && Vector3.Distance(center, target.transform.position) <= radius){
				Destroy(target);
				numTargets--;
				OnTargetDestroyed();
			}
		}
		Voxel3DTerrain terrain = GetComponent<Voxel3DTerrain>();
		//TODO: Multiple chunk updates!!!
		int chunkX = Mathf.FloorToInt(center.x / terrain.chunkSize.x);
		int chunkY = Mathf.FloorToInt(center.z / terrain.chunkSize.y);
		Debug.Log("Destroying blocks in chunk[" +chunkX.ToString()  + "," + chunkY.ToString() + "]");
		byte[,,] chunk = terrain.blockMap[chunkX, chunkY];
		for (int x=0; x<chunk.GetLength(0); x++){
			for (int y=0; y<chunk.GetLength(1); y++){
				for (int lvl=0; lvl<chunk.GetLength(2); lvl++){
					if(Vector3.Distance(new Vector3(x + chunkX * terrain.chunkSize.x, lvl ,y + chunkY * terrain.chunkSize.y), center)<=radius){
						Debug.Log("Destroying block at position[" +x.ToString()  + "," + y.ToString() + "," + lvl.ToString() + "]");
						chunk[x,y,lvl] = 0;
						//Indicate level update on this chunk
						terrain.updateLevel[chunkX, chunkY][lvl] = true;
					}
				}
			}
		}
		//Indicate chunk update necessary!
		terrain.update[chunkX, chunkY] = true;

	}
}
