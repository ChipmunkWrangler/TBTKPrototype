using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TBTK;

public class SceneModifierSimple : MonoBehaviour {
	
	public bool adjustGridColor=false;
	public Color gridColor;
	
	IEnumerator Start(){
		OnGameStart();
		
		yield return null;
		yield return null;
		yield return null;
		
		if(adjustGridColor){
			GameObject gridObject=GridManager.GetGridObject();
			gridObject=gridObject.transform.GetChild(0).gameObject;
			gridObject.GetComponent<Renderer>().material.SetColor("_Color", gridColor);
		}
	}
	
	void OnEnable(){
		TBTK.TBTK.onGameStartE += OnGameStart;
	}
	void OnDisable(){
		TBTK.TBTK.onGameStartE -= OnGameStart;
	}
	
	void OnGameStart(){
		List<Unit> unitList=FactionManager.GetAllUnit();
		for(int i=0; i<unitList.Count; i++){
			if(unitList[i].isAIUnit) unitList[i].thisT.rotation=Quaternion.Euler(0, -90, 0);
			else unitList[i].thisT.rotation=Quaternion.Euler(0, 90, 0);
		}
	}
	
}
