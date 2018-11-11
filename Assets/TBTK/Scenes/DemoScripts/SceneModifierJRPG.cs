using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TBTK;

public class SceneModifierJRPG : MonoBehaviour {

	void OnEnable(){
		TBTK.TBTK.onGameStartE += OnGameStart;
	}
	void OnDisable(){
		TBTK.TBTK.onGameStartE -= OnGameStart;
	}
	
	void OnGameStart(){
		List<Unit> unitList=FactionManager.GetAllUnit();
		for(int i=0; i<unitList.Count; i++){
			unitList[i].RemoveAbility(0);	//remove overwatch
			
			unitList[i].movePerTurn=0;
			unitList[i].moveRemain=0;
			unitList[i].moveRange=0;
			
			unitList[i].attackRange=50;
			
			if(unitList[i].isAIUnit) unitList[i].thisT.rotation=Quaternion.Euler(0, 240, 0);
			else unitList[i].thisT.rotation=Quaternion.Euler(0, 60, 0);
			
			unitList[i].thisT.localScale*=1.25f;
		}
	}
	
}
