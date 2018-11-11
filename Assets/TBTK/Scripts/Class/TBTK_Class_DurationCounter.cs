using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{

	//a counter class used specifically to track the cooldown/effect-duration 
	[System.Serializable]
	public class TBDuration{
		public int duration=0;		//the actual duration remained in term of turn
		public int turnCounter=0;	//number of end turn remained for next duration tick (duration-=1)
		
		//public TBDuration(int dur=0){ Set(dur); }
		
		public void Set(int dur){
			Debug.Log("Duration set to " + dur);
			duration=dur;
			ResetTurnCounter();
		}
		
		public void Iterate(){	//called at each end turn event, also called when a faction is destroyed
			turnCounter-=1;
			if(turnCounter==0){
				duration-=1;
				Debug.Log("Duration decremented to " + duration);
				ResetTurnCounter();
			}
		}
		
		private void ResetTurnCounter(){
			if(TurnControl.GetTurnMode()==_TurnMode.UnitPerTurn) turnCounter=FactionManager.GetTotalUnitCount();
			else turnCounter=FactionManager.GetTotalFactionCount();
		}
		
		public bool Due(){ return duration<=0 ? true : false ; }
	}
	
	
}
