using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TBTK;

public class SceneModifierFull : MonoBehaviour {

	void OnEnable(){
		TBTK.TBTK.onGameStartE += OnGameStart;
	}
	void OnDisable(){
		TBTK.TBTK.onGameStartE -= OnGameStart;
	}
	
	void OnGameStart(){
		List<Unit> unitList=FactionManager.GetAllUnit();
		for(int i=0; i<unitList.Count; i++){
			//unitList[i].AddAbility(13);	//insert ability with abilityID-13 (overwatch) the unit ability list
			
			unitList[i].movePerTurn=2;
			unitList[i].moveRemain=2;
			unitList[i].moveRange=5;
			
			unitList[i].attackRange=10;
			unitList[i].sight=10;
			
			unitList[i].SetupFogOfWar();
		}
	}
	
}
