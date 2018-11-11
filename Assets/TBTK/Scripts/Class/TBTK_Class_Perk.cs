using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {
	
	public enum _PerkType{
		Unit, 						//modify a particular set of unit
		Unit_All, 				
		UnitAbility, 				//modify a particular set of unit ability
		UnitAbility_All, 		
		FactionAbility, 		//modify a particular set of faction ability
		FactionAbility_All, 	
		
		NewUnitAbility, 		//unlock new unit ability
		NewFactionAbility,	//unlock new faction ability
	}

	[System.Serializable]
	public class Perk {
		
		public int prefabID;
		public int instanceID;
		
		public Sprite icon;
		public string name;
		public string desp;
		
		public _PerkType type;
		
		public bool purchased=false;
		
		public int cost=1;
		//public int minLevel=1;								//min level to reach before becoming available (check GameControl.levelID)
		//public int minWave=0;								//min wave to reach before becoming available
		public int minPerkPoint=0;						//min perk point 
		public List<int> prereq=new List<int>();	//prerequisite perk before becoming available, element is removed as the perk is unlocked in runtime
		
		//remove this, this is no longer in use
		public List<int> itemIDList=new List<int>();
		
		public List<int> unitIDList=new List<int>();			//associated unit for _PerkType.Unit
		public List<int> unitAbilityIDList=new List<int>();	//associated unit for _PerkType.UnitAbility
		public List<int> facAbilityIDList=new List<int>();	//associated unit for _PerkType.FactionAbility
		
		
		//for ability modifier
		
		public float abCostMod=0;
		public int abCooldownMod=0;
		public int abUseLimitMod=0;
		public float abHitChanceMod=0;
		
		public int abRangeMod=0;
		public int abAOERangeMod=0;
		
		public float abHPMod=0;	//direct dmg/gain for the HP
		public float abAPMod=0;	//direct dmg/gain for the AP
		//public int abStunMod=0;
		
		public int abDurationMod=0;
		
		
		
		//for unit and ability modifier
		public UnitStat stats=new UnitStat();
		
		
		public bool addAbilityToAllUnit=false;
		public List<int> newABUnitIDList=new List<int>();	//unit to received new ability
		
		public int newUnitAbilityID=-1;	//to add new unit ability
		public int subUnitAbilityID=-1;	//to replace exisitng unit ability?
		
		public int newFacAbilityID=-1;	//to add new faction ability
		public int subFacAbilityID=-1;	//to replace existing faction ability
		
		//public int abilityID=0;	//for add new ability
		//public int abilityXID=-1; //for remove ability
		
		
		public string IsAvailable(){
			//Debug.Log("  "+SpawnManager.GetCurrentWaveID());
			if(purchased) return "Purchased";
			//if(GameControl.GetLevelID()<minLevel) return "Unlocked at level "+minLevel;
			//if(Mathf.Max(SpawnManager.GetCurrentWaveID()+1, 1)<minWave) return "Unlocked at Wave "+minWave;
			if(PerkManager.GetPerkCurrency()<cost) return "Insufficient perk currency";
			if(PerkManager.GetPerkPoint()<minPerkPoint) return "Insufficient perk point";
			if(prereq.Count>0){
				string text="Require: ";
				bool first=true;
				List<Perk> perkList=PerkManager.GetPerkList();
				for(int i=0; i<prereq.Count; i++){
					for(int n=0; n<perkList.Count; n++){
						if(perkList[n].prefabID==prereq[i]){
							text+=((!first) ? ", " : "")+perkList[n].name;
							first=false;
							break;
						}
					}
				}
				return text;
				//return "Not all prerequisite perk has been unlocked";
			}
			return "";
		}
		
		public string Purchase(bool useCurrency=true){
			purchased=true;
			
			if(useCurrency){
				if(PerkManager.GetPerkCurrency()<cost) return "Insufficient perk currency";
				PerkManager.SpendCurrency(cost);
			}
			
			return "";
		}
	
		public Perk Clone(){
			Perk perk=new Perk();
			
			perk.prefabID=prefabID;
			
			perk.icon=icon;
			perk.name=name;
			perk.desp=desp;
			
			perk.type=type;
			
			perk.purchased=purchased;
			
			perk.cost=cost;
			perk.minPerkPoint=minPerkPoint;
			perk.prereq=new List<int>( prereq );
			//for(int i=0; i<prereq.Count; i++) perk.prereq.Add(prereq[i]);
			
			
			//for(int i=0; i<itemIDList.Count; i++) perk.itemIDList.Add(itemIDList[i]);
			perk.unitIDList=new List<int>( unitIDList );
			perk.unitAbilityIDList=new List<int>( unitAbilityIDList );
			perk.facAbilityIDList=new List<int>( facAbilityIDList );
			
			perk.abCostMod=abCostMod;
			perk.abCooldownMod=abCooldownMod;
			perk.abUseLimitMod=abUseLimitMod;
			perk.abHitChanceMod=abHitChanceMod;
			
			perk.abRangeMod=abRangeMod;
			perk.abAOERangeMod=abAOERangeMod;			perk.abHPMod=abHPMod;
			perk.abAPMod=abAPMod;
			
			perk.abDurationMod=abDurationMod;
			
			perk.stats=stats.Clone();
			
			perk.addAbilityToAllUnit=addAbilityToAllUnit;
			perk.newABUnitIDList=newABUnitIDList;
			
			perk.newUnitAbilityID=newUnitAbilityID;	
			perk.subUnitAbilityID=subUnitAbilityID;	
			
			perk.newFacAbilityID=newFacAbilityID;	
			perk.subFacAbilityID=subFacAbilityID;	
			
			//perk.abilityID=abilityID;
			//perk.abilityXID=abilityXID;
			
			return perk;
		}

	}
	
	
	
	
	[System.Serializable]
	public class PerkAbilityModifier{
		public int prefabID;
		
		public float cost=0;
		public int cooldown=0;
		
		public int useLimit=0;
		public float hitChance=0;
		
		public int range=0;
		public int aoeRange=0;
		
		public float HP=0;	//direct dmg/gain for the HP
		public float AP=0;
		
		public int  effectDuration=0;
		
		public UnitStat stats=new UnitStat();
	}
	
	[System.Serializable]
	public class PerkUnitModifier{
		public int prefabID;
		
		public UnitStat stats=new UnitStat();
		
		public List<int> abilityIDList=new List<int>();
		public List<int> abilityXIDList=new List<int>();
	}
	
	
	
	
	

}