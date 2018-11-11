using UnityEngine;
using System.Collections;

using TBTK;

namespace TBTK{
	
	//contains all the information of an attack (normal attack, counter attack, ability-shoot)
	//an instance of the class is generate each time an attack took place
	public class AttackInstance {
		
		public bool processed=false;	//set to true when the instance has been process (all the stats calculated)
		
		
		public Unit srcUnit;
		public Unit tgtUnit;
		
		public bool isAbility=false;
		public int unitAbilityID=-1;
		
		public bool isMelee=false;
		
		public bool isCounter=false;
		public bool isOverwatch=false;
		
		public CoverSystem._CoverType coverType=CoverSystem._CoverType.None;
		
		public float hitChance=1;
		public float critChance=0;
		public float stunChance=0;
		public float silentChance=0;
		
		public bool missed=false;
		public bool critical=false;
		public bool stunned=false;
		public bool silenced=false;
		public bool flanked=false;
		public bool destroyed=false;
		
		public float damage=0;
		public int stun=0;
		public int silent=0;
		
		public float damageTableModifier=1;
		public float flankingBonus=1;
		
		//constructor for normal and counter attack
		public AttackInstance(Unit sUnit, Unit tUnit, bool counter=false, bool overwatch=false, bool melee=false){
			srcUnit=sUnit;
			tgtUnit=tUnit;
			isCounter=counter;
			isOverwatch=overwatch;
			isMelee=melee;
			
			CalculateChance();
		}
		
		//constructor for shooting ability
		public AttackInstance(Unit sUnit, int abilityID, float hit=Mathf.Infinity){
			isAbility=true;
			srcUnit=sUnit;
			unitAbilityID=abilityID;
			hitChance=hit;
			if(Random.Range(0f, 1f)>hitChance) missed=true;
		}
		
		
		public void SetTarget(Unit unit){
			tgtUnit=unit;
			CalculateChance();
		}
		
		//calculate the chance and determine various condition
		private bool calculated=false;
		private void CalculateChance(){
			if(calculated) return;
			calculated=true;
			
			float coverDodgeBonus=0;
			float exposedCritBonus=0;
			
			//if cover system is enabled, get the dodge and crit bonus
			if(GameControl.EnableCover()){
				coverType=CoverSystem.GetCoverType(srcUnit.tile, tgtUnit.tile);
				if(coverType==CoverSystem._CoverType.Half) coverDodgeBonus=CoverSystem.GetHalfCoverDodgeBonus();
				else if(coverType==CoverSystem._CoverType.Full) coverDodgeBonus=CoverSystem.GetFullCoverDodgeBonus();
				else exposedCritBonus=CoverSystem.GetExposedCritChanceBonus();
			}
			
			//calculate the hit chance
			float hit=!isMelee ? srcUnit.GetHitChance() : srcUnit.GetHitChanceMelee();
			float dodge=tgtUnit.GetDodgeChance()+coverDodgeBonus;
			hitChance=Mathf.Clamp(hit-dodge, 0f, 1f);
			
			//calculate the critical chance
			float critHit=(!isMelee ? srcUnit.GetCritChance() : srcUnit.GetCritChanceMelee())+exposedCritBonus;
			float critAvoid=tgtUnit.GetCritAvoidance();
			critChance=Mathf.Clamp(critHit-critAvoid, 0f, 1f);
			
			//calculate stun chance
			float stunHit=srcUnit.GetStunChance();
			float stunAvoid=tgtUnit.GetStunAvoidance();
			stunChance=Mathf.Clamp(stunHit-stunAvoid, 0f, 1f);
			
			//calculate silent chance
			float silentHit=srcUnit.GetSilentChance();
			float silentAvoid=tgtUnit.GetSilentAvoidance();
			silentChance=Mathf.Clamp(silentHit-silentAvoid, 0f, 1f);
			
			if(isOverwatch){
				hitChance-=GameControl.GetOverwatchHitPenalty();
				critHit-=GameControl.GetOverwatchCritPenalty();
			}
			
			//check if flanking is enabled an applicable in this instance
			if(GameControl.EnableFlanking()){
				//Vector2 dir=new Vector2(srcUnit.tile.pos.x-tgtUnit.tile.pos.x, srcUnit.tile.pos.z-tgtUnit.tile.pos.z);
				float angleTH=180-Mathf.Min(180, GameControl.GetFlankingAngle());
				Quaternion attackRotation=Quaternion.LookRotation(tgtUnit.tile.GetPos()-srcUnit.tile.GetPos());
				//Debug.Log(Quaternion.Angle(attackRotation, tgtUnit.thisT.rotation)+"    "+angleTH);
				if(Quaternion.Angle(attackRotation, tgtUnit.thisT.rotation)<angleTH) flanked=true;
			}
		}
		
		//do the stats processing
		public void Process(){
			if(processed) return;
			
			if(isAbility){	//if this instance is for ability, then there's no need to calculate the rest of the stats
				if(Random.Range(0f, 1f)>hitChance){
					missed=true;
					return;
				}
			}
			
			processed=true;
			
			//first determine all the chances and condition
			CalculateChance();
			
			//if the attack missed, skip the rest of the calculation
			if(Random.Range(0f, 1f)>hitChance){
				missed=true;
				return;
			}
			
			//get the base damage
			if(!isMelee) damage=Random.Range(srcUnit.GetDamageMin(), srcUnit.GetDamageMax());
			else damage=Random.Range(srcUnit.GetDamageMinMelee(), srcUnit.GetDamageMaxMelee());
			
			//modify the damage with damage to armor modifier
			int armorType=tgtUnit.armorType;
			int damageType=srcUnit.damageType;
			damageTableModifier=DamageTable.GetModifier(armorType, damageType);
			damage*=damageTableModifier;
			
			//if this is a counter attack, modify the damage with counter modifier
			if(isCounter) damage*=GameControl.GetCounterDamageMultiplier();
			
			//this the target is flanked, apply the flanking bonus
			if(!isCounter && flanked){
				flankingBonus=1+GameControl.GetFlankingBonus()+srcUnit.GetFlankingBonus()-tgtUnit.GetFlankedModifier();
				damage*=flankingBonus;
			}
			
			//if the attack crits, add the critical multiplier
			if(Random.Range(0f, 1f)<critChance){
				critical=true;
				damage*=srcUnit.GetCritMultiplier();
				//damage*=!isMelee ? srcUnit.GetCritMultiplier() : srcUnit.GetCritMultiplierMelee();
			}
			
			//if the attack stuns the target, get the stun duration
			if(Random.Range(0f, 1f)<stunChance){
				stunned=true;
				stun=srcUnit.GetStunDuration();
			}
			
			//if the attack stuns the target, get the stun duration
			if(Random.Range(0f, 1f)<silentChance){
				silenced=true;
				silent=srcUnit.GetSilentDuration();
			}
			
			//check if the unit is destroyed in this instance and make the destroyed flag according
			if(damage>tgtUnit.HP) destroyed=true;
			
			//Debug.Log("Damage: "+damage);
			//new TextOverlay(tgtUnit.GetTargetT().position, damage.ToString("f0"), Color.white);
		}
		
		
		
		
	}

}