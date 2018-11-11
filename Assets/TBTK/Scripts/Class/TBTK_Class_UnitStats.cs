using UnityEngine;
using System.Collections;

using TBTK;

namespace TBTK {

	[System.Serializable]
	public class UnitStat{
		public float HP=0;					//used for Data
		public float AP=0;					
		
		public float HPPerTurn=0;
		public float APPerTurn=0;
		
		public float moveAPCost=0;
		public float attackAPCost=0;
		
		public float turnPriority=0;
		
		public int moveRange=0;
		public int attackRange=0;
		public int sight=0;
		
		public int movePerTurn=0;
		public int attackPerTurn=0;
		public int counterPerTurn=0;	//counter attack
		
		public float damage=0;			//used for perk/ability modifier
		public float damageMin=0;		//used for Data
		public float damageMax=0;
		
		public float hitChance=0f;
		public float dodgeChance=0;
		
		public float critChance=0;
		public float critAvoidance=0;
		public float critMultiplier=0;
		
		public float stunChance=0;
		public float stunAvoidance=0;
		public int stunDuration=0;
		
		public float silentChance=0;
		public float silentAvoidance=0;
		public int silentDuration=0;
		
		public float flankingBonus=0;
		public float flankedModifier=0;
		
		
		
		
		
		public UnitStat Clone(){
			UnitStat stat=new UnitStat();
			
			stat.HP=HP;
			stat.AP=AP;
			
			stat.HPPerTurn=HPPerTurn;
			stat.APPerTurn=APPerTurn;
			
			stat.moveAPCost=moveAPCost;
			stat.attackAPCost=attackAPCost;
			
			stat.turnPriority=turnPriority;
			
			stat.moveRange=moveRange;
			stat.attackRange=attackRange;
			stat.sight=sight;
			
			stat.movePerTurn=movePerTurn;
			stat.attackPerTurn=attackPerTurn;
			stat.counterPerTurn=counterPerTurn;
			
			stat.damage=damage;
			stat.damageMin=damageMin;
			stat.damageMax=damageMax;
			
			stat.hitChance=hitChance;
			stat.dodgeChance=dodgeChance;
			
			stat.critChance=critChance;
			stat.critAvoidance=critAvoidance;
			stat.critMultiplier=critMultiplier;
			
			stat.stunChance=stunChance;
			stat.stunAvoidance=stunAvoidance;
			stat.stunDuration=stunDuration;
			
			stat.silentChance=silentChance;
			stat.silentAvoidance=silentAvoidance;
			stat.silentDuration=silentDuration;
			
			stat.flankingBonus=flankingBonus;
			stat.flankedModifier=flankedModifier;
			
			return stat;
		}
		
		
		public void CopyFromUnit(Unit unit){	//for UnitData
			HP=unit.HP;
			AP=unit.AP;
			
			HPPerTurn=unit.HPPerTurn;
			APPerTurn=unit.APPerTurn;
			
			moveAPCost=unit.moveAPCost;
			attackAPCost=unit.attackAPCost;
			
			turnPriority=unit.turnPriority;
			
			moveRange=unit.moveRange;
			attackRange=unit.attackRange;
			sight=unit.sight;
			
			movePerTurn=unit.movePerTurn;
			attackPerTurn=unit.attackPerTurn;
			counterPerTurn=unit.counterPerTurn;
			
			damageMin=unit.damageMin;
			damageMax=unit.damageMax;
			
			hitChance=unit.hitChance;
			dodgeChance=unit.dodgeChance;
			
			critChance=unit.critChance;
			critAvoidance=unit.critAvoidance;
			critMultiplier=unit.critMultiplier;
			
			stunChance=unit.stunChance;
			stunAvoidance=unit.stunAvoidance;
			stunDuration=unit.stunDuration;
			
			silentChance=unit.silentChance;
			silentAvoidance=unit.silentAvoidance;
			silentDuration=unit.silentDuration;
			
			flankingBonus=unit.flankingBonus;
			flankedModifier=unit.flankedModifier;
		}
		
		public void CopyToUnit(Unit unit){	//for UnitData
			unit.HP=HP;
			unit.AP=AP;
			
			unit.HPPerTurn=HPPerTurn;
			unit.APPerTurn=APPerTurn;
			
			unit.moveAPCost=moveAPCost;
			unit.attackAPCost=attackAPCost;
			
			unit.turnPriority=turnPriority;
			
			unit.moveRange=moveRange;
			unit.attackRange=attackRange;
			unit.sight=sight;
			
			unit.movePerTurn=movePerTurn;
			unit.attackPerTurn=attackPerTurn;
			unit.counterPerTurn=counterPerTurn;
			
			unit.damageMin=damageMin;
			unit.damageMax=damageMax;
			
			unit.hitChance=hitChance;
			unit.dodgeChance=dodgeChance;
			
			unit.critChance=critChance;
			unit.critAvoidance=critAvoidance;
			unit.critMultiplier=critMultiplier;
			
			unit.stunChance=stunChance;
			unit.stunAvoidance=stunAvoidance;
			unit.stunDuration=stunDuration;
			
			unit.silentChance=silentChance;
			unit.silentAvoidance=silentAvoidance;
			unit.silentDuration=silentDuration;
			
			unit.flankingBonus=flankingBonus;
			unit.flankedModifier=flankedModifier;
		}
	}
	
}