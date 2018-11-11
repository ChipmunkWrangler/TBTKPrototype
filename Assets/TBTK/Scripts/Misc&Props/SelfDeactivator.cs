using UnityEngine;
using System.Collections;

using TBTK;

namespace TBTK{

	public class SelfDeactivator : MonoBehaviour {
		
		public enum _Type{RealTime, TurnBased}
		public _Type timerTrackType=_Type.RealTime;

		public float duration=1;
		public TBDuration durationCounter=new TBDuration();
		
		void OnEnable(){
			if(timerTrackType==_Type.RealTime) ObjectPoolManager.Unspawn(gameObject, duration);
			else if(timerTrackType==_Type.TurnBased){
				durationCounter.Set((int)duration);
				
				TBTK.onNewTurnE += IterateDuration;
				TBTK.onUnitDestroyedE += OnUnitDestroyed;
				TBTK.onFactionDestroyedE += OnFactionDestroyed;
			}
		}
		
		void OnDisable(){
			if(timerTrackType!=_Type.TurnBased) return;
			
			TBTK.onNewTurnE -= IterateDuration;
			TBTK.onUnitDestroyedE -= OnUnitDestroyed;
			TBTK.onFactionDestroyedE -= OnFactionDestroyed;
		}
		
		void OnUnitDestroyed(Unit unit){
			if(TurnControl.GetTurnMode()!=_TurnMode.UnitPerTurn) return;
			IterateDuration();
		}
		void OnFactionDestroyed(int facID){
			if(TurnControl.GetTurnMode()==_TurnMode.UnitPerTurn) return;
			IterateDuration();
		}
		
		void IterateDuration(bool flag=false){
			durationCounter.Iterate();
			if(durationCounter.Due()){
				ObjectPoolManager.Unspawn(gameObject);
			}
		}
		
		
	}

}