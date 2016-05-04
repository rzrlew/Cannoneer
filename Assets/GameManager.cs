using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour {
	public GameObject terrain;
	public GameObject uiPanel;
	private GameObject terrainInstance;
	private bool oneTime = false;
	// Use this for initialization
	void Start () {
		terrainInstance = Instantiate(terrain);
		terrainInstance.GetComponent<TargetManager>().OnTargetDestroyed = UpdateTargetsText;

	}
	
	// Update is called once per frame
	void Update () {
		if(!oneTime){
			terrainInstance.GetComponent<Voxel3DTerrain>().cannonInstance.GetComponentInChildren<CannonBehaviour>().OnElevationChange = UpdateElevationText;
			oneTime = true;
		}
	}

	public void DoExplosionDamage(Vector3 position, float radius){
		terrainInstance.GetComponent<TargetManager>().DestroyTargetInRadius(position, radius);
	}

	void UpdateTargetsText(){
		Text[] UITexts = uiPanel.GetComponentsInChildren<Text>();
		foreach(Text textInstance in UITexts){
			if(textInstance.name == "NumTargetsLeftText"){
				textInstance.text = "Targets Left:" + terrainInstance.GetComponent<TargetManager>().GetNumTargets().ToString();
			}
		}
	}

	void UpdateElevationText(){
		Text[] UITexts = uiPanel.GetComponentsInChildren<Text>();
		foreach(Text textInstance in UITexts){
			if(textInstance.name == "ElevationText"){
				textInstance.text = "Elevation(º):" + terrainInstance.GetComponent<Voxel3DTerrain>().cannonInstance.GetComponentInChildren<CannonBehaviour>().transform.localRotation.eulerAngles.x.ToString();
			}
		}
	}
}
