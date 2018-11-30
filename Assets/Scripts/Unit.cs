using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TBTK;

namespace TBTK{
	
	public class Unit : MonoBehaviour {
		
		public delegate void ActionCamHandler(Unit unit, Vector3 targetPos, float chance);
		public static event ActionCamHandler onActionCamE;		//listen by camera only
		
		
		[HideInInspector] public bool isObjectUnit=false;
		
		
		public int prefabID;
		public int instanceID;	
		public int factionID;
		
		private int dataID=-1;//use to identify unit when using data 
		public int GetDataID(){ return dataID; }
		public void SetDataID(int ID){ dataID=ID; }
		
		private int level=1;	//for data only, has no effect on game
		public int GetLevel(){ return level; }
		public void SetLevel(int lvl){ level=lvl; }
		
		public string unitName="Unit";
		public string desp="";
		public Sprite iconSprite;
		
		[HideInInspector] public bool trigger=false;
		[HideInInspector] public bool isAIUnit=false;
		[HideInInspector] public Unit lastAttacker;
		[HideInInspector] public Unit lastTarget;
		
		public int damageType=0;
		public int armorType=0;
		
		public Tile tile;	//occupied tile
		
		public float hitThreshold=0.25f;
		public Transform targetPoint;
		public Transform GetTargetT(){ return targetPoint==null ? thisT : targetPoint; }
		
		public bool requireDirectLOSToAttack=true;
		
		public int value=5;
		
		public float moveSpeed=10;
		[HideInInspector] private float rotateSpeed=3;
		
		public bool IsStunned(){ return activeEffect.stun; }//>0 ? true : false; }
		public bool DisableAbilities(){ return activeEffect.silence; }
		
		public int silenced=0;
		public bool IsSilenced(){ return silenced>0 ? true : false; }
		
		[Header("Basic Stats")]
		public float defaultHP=10;
		public float defaultAP=10;
		public float HP=10;
		public float AP=10;
		
		public float HPPerTurn=0;
		public float APPerTurn=0;
		// <HACKS FOR PROTOTYPE>
		private int numTurnsPlayed = -2; // NextTurn is called twice before a unit can act; not sure why
		//</HACKS>
		public float moveAPCost=0;
		public float attackAPCost=0;
		
		public float turnPriority=1;
		
		public int moveRange=3;
		public int attackRange=3;
		public int attackRangeMin=0;
		public int sight=6;
		
		public int movePerTurn=1;
		public int attackPerTurn=1;
		public int counterPerTurn=1;	//counter attack
		public int abilityPerTurn=1;		//ability
		[HideInInspector] 
		public int moveRemain=1;
		[HideInInspector] 
		public int attackRemain=1;
		[HideInInspector] 
		public int counterRemain=1;
		[HideInInspector]
		public int abilityRemain=1;
		
		
		public float damageMin=3;
		public float damageMax=6;
		
		public float hitChance=.8f;
		public float dodgeChance=.15f;
		
		public float critChance=0.1f;
		public float critAvoidance=0;
		public float critMultiplier=2;
		
		public float stunChance=0;
		public float stunAvoidance=0;
		public int stunDuration=1;
		private Effect stunEffect;
		
		public float silentChance=0;
		public float silentAvoidance=0;
		public int silentDuration=1;
		private Effect silentEffect;
		
		public float flankingBonus=0;
		public float flankedModifier=0;
		
		public bool overwatching=false;
		
		
		[Header("Hybrid Unit")]
		public bool isHybridUnit;
		public int meleeRange;
		
		public float damageMinMelee=3;
		public float damageMaxMelee=6;
		
		public float hitChanceMelee=.8f;
		public float critChanceMelee=0.1f;
		
		public int GetMeleeRange(){ return meleeRange; }
		
		
		//********************************************************************************************************************************
		//these section are functions that get active stats of unit
		
		public float GetFullHP(){ return defaultHP*(1+GetEffHPBuff()+PerkManager.GetUnitHPBuff(prefabID)); }
		public float GetFullAP(){ 
			return numTurnsPlayed + defaultAP*(1+GetEffAPBuff()+PerkManager.GetUnitAPBuff(prefabID)); }
		
		public float GetHPPerTurn(){ return HPPerTurn+GetEffHPPerTurn()+tile.GetHPPerTurn()+PerkManager.GetUnitHPPerTurn(prefabID); }
		public float GetAPPerTurn(){ return APPerTurn+GetEffAPPerTurn()+tile.GetAPPerTurn()+PerkManager.GetUnitAPPerTurn(prefabID); }
		
		public float GetMoveAPCost(){ return (GameControl.UseAPForMove()) ? Mathf.Max(0, moveAPCost+GetEffMoveAPCost()+PerkManager.GetUnitMoveAPCost(prefabID)) : 0 ; }
		public float GetAttackAPCost(){ return (GameControl.UseAPForAttack()) ? Mathf.Max(0, attackAPCost+GetEffAttackAPCost()+PerkManager.GetUnitAttackAPCost(prefabID)) : 0 ; }
		public float GetCounterAPCost(){ return GetAttackAPCost()*GameControl.GetCounterAPMultiplier(); }
		
		public float GetTurnPriority(){ return turnPriority+GetEffTurnPriority()+PerkManager.GetUnitTurnPriority(prefabID); }
		
		public int GetMoveRange(){ return moveRange+GetEffMoveRange()+tile.GetMoveRange()+PerkManager.GetUnitMoveRange(prefabID); }
		public int GetAttackRange(){ return attackRange+GetEffAttackRange()+tile.GetAttackRange()+PerkManager.GetUnitAttackRange(prefabID); }
		public int GetAttackRangeMin(){ return attackRangeMin; }
		public int GetSight(){ return sight+GetEffSight()+tile.GetSight()+PerkManager.GetUnitSight(prefabID); }
		
		public int GetMovePerTurn(){ return movePerTurn+GetEffMovePerTurn()+PerkManager.GetUnitMovePerTurn(prefabID); }
		public int GetAttackPerTurn(){ return attackPerTurn+GetEffAttackPerTurn()+PerkManager.GetUnitAttackPerTurn(prefabID); }
		public int GetCounterPerTurn(){ return counterPerTurn+GetEffCounterPerTurn()+PerkManager.GetUnitCounterPerTurn(prefabID); }
		
		public float GetDamageMin(){ return damageMin*(1+GetEffDamage()+tile.GetDamage()+PerkManager.GetUnitDamage(prefabID)); }
		public float GetDamageMax(){ return damageMax*(1+GetEffDamage()+tile.GetDamage()+PerkManager.GetUnitDamage(prefabID)); }
		public float GetDamageMinMelee(){ return damageMinMelee*(1+GetEffDamage()+tile.GetDamage()+PerkManager.GetUnitDamage(prefabID)); }
		public float GetDamageMaxMelee(){ return damageMaxMelee*(1+GetEffDamage()+tile.GetDamage()+PerkManager.GetUnitDamage(prefabID)); }
		
		public float GetHitChance(){ return hitChance+GetEffHitChance()+tile.GetHitChance()+PerkManager.GetUnitHitChance(prefabID); }
		public float GetDodgeChance(){ return dodgeChance+GetEffDodgeChance()+tile.GetDodgeChance()+PerkManager.GetUnitDodgeChance(prefabID); }
		public float GetHitChanceMelee(){ return hitChanceMelee+GetEffHitChance()+tile.GetHitChance()+PerkManager.GetUnitHitChance(prefabID); }
		public float GetCritChanceMelee(){ return critChanceMelee+GetEffCritChance()+tile.GetCritChance()+PerkManager.GetUnitCritChance(prefabID); }
		
		public float GetCritChance(){ return critChance+GetEffCritChance()+tile.GetCritChance()+PerkManager.GetUnitCritChance(prefabID); }
		public float GetCritAvoidance(){ return critAvoidance+GetEffCritAvoidance()+tile.GetCritAvoidance()+PerkManager.GetUnitCritChance(prefabID); }
		public float GetCritMultiplier(){ return critMultiplier+GetEffCritMultiplier()+tile.GetCritMultiplier()+PerkManager.GetUnitCritChance(prefabID); }
		
		public float GetStunChance(){ return stunChance+GetEffStunChance()+tile.GetStunChance()+PerkManager.GetUnitStunChance(prefabID); }
		public float GetStunAvoidance(){ return stunAvoidance+GetEffStunAvoidance()+tile.GetStunAvoidance()+PerkManager.GetUnitStunAvoidance(prefabID); }
		public int GetStunDuration(){ return stunDuration+GetEffStunDuration()+tile.GetStunDuration()+PerkManager.GetUnitStunDuration(prefabID); }
		
		public float GetSilentChance(){ return silentChance+GetEffSilentChance()+tile.GetSilentChance()+PerkManager.GetUnitSilentChance(prefabID); }
		public float GetSilentAvoidance(){ return silentAvoidance+GetEffSilentAvoidance()+tile.GetSilentAvoidance()+PerkManager.GetUnitSilentAvoidance(prefabID); }
		public int GetSilentDuration(){ return silentDuration+GetEffSilentDuration()+tile.GetSilentDuration()+PerkManager.GetUnitSilentDuration(prefabID); }
		
		public float GetFlankingBonus(){ return flankingBonus+GetEffFlankingBonus()+PerkManager.GetUnitFlankingBonus(prefabID); }
		public float GetFlankedModifier(){ return flankedModifier+GetEffFlankedModifier()+PerkManager.GetUnitFlankedModifier(prefabID); }
		public float GetArmorPenalty(){ return GetEffArmorPenalty(); }
		
		public float GetHPRatio(){
			var full=GetFullHP();
			return full<=0 ? 0 : HP/full;
		}
		public float GetAPRatio(){
			var full=GetFullAP();
			return full<=0 ? 0 : AP/full;
		}
		
		public int GetEffectiveMoveRange(){ 
			if(movePerTurn==0) return 0;
			float apCost=GetMoveAPCost();
			int apAllowance=apCost==0 ? 999999 : (int)Mathf.Abs(AP/apCost);
			return Mathf.Min(GetMoveRange(), apAllowance); 
		}
		
		public bool CanAttack(){ 
			bool apFlag=GameControl.UseAPForAttack() ? AP>=GetAttackAPCost() : true ;
			return attackRemain>0 & GetAttackRange()>0 & !IsStunned() &  apFlag; 
		}
		public bool CanMove(){ return moveRemain>0 & GetEffectiveMoveRange()>0 & !IsStunned(); }
		public bool CanUseAbilities(){ return abilityRemain>0 & !IsStunned() & !IsSilenced(); }
		
		
		
		
		
		
		
		//end get stats section
		//********************************************************************************************************************************
		
		
		//indicate the cover status to selected unit
		public CoverSystem._CoverType coverStatus=CoverSystem._CoverType.None;
		public void UpdateCoverStatus(){
			if(GameControl.GetSelectedUnit()==null) coverStatus=CoverSystem._CoverType.None;
			else{
				if(GameControl.GetSelectedUnit().factionID==factionID) coverStatus=CoverSystem._CoverType.None;
				else coverStatus=CoverSystem.GetCoverType(GameControl.GetSelectedUnit().tile, tile);
			}
		}
		
		
		//********************************************************************************************************************************
		//these section are related to ability effects
		[Header("Misc")]
		public List<Effect> effectList=new List<Effect>();
		public List<Effect> GetEffectList(){ return effectList; }
		
		public Effect activeEffect=new Effect();
		//~ public int stunned=0;
		//~ public UnitStats effectUnitStat;
		
		public void ApplyEffect(Effect eff, Unit srcUnit=null){
			// if(FactionManager.GetSelectedFactionID()!=factionID) eff.duration+=1; // PROTOTYPE HACK not sure what this was supposed to accomplish
			

			float HPVal=Random.Range(eff.HPMin, eff.HPMax);
			HPVal *= (1.0f + GetArmorPenalty());
			if(HPVal<0){
				/*
				if(GameControl.EnableFlanking() && srcUnit!=null){
					//Vector2 dir=new Vector2(srcUnit.tile.pos.x-tgtUnit.tile.pos.x, srcUnit.tile.pos.z-tgtUnit.tile.pos.z);
					float angleTH=180-Mathf.Min(180, GameControl.GetFlankingAngle());
					Quaternion attackRotation=Quaternion.LookRotation(tile.GetPos()-srcUnit.tile.GetPos());
					//Debug.Log(Quaternion.Angle(attackRotation, tgtUnit.thisT.rotation)+"    "+angleTH);
					if(Quaternion.Angle(attackRotation, thisT.rotation)<angleTH){
						float flankingBonus=1+GameControl.GetFlankingBonus()+srcUnit.GetFlankingBonus()-GetFlankedModifier();
						HPVal*=flankingBonus;
					}
				}
				*/
				
				ApplyDamage(-HPVal*DamageTable.GetModifier(armorType, eff.damageType));
			}
			else if(HPVal>0) RestoreHP(HPVal);
			
			AP=Mathf.Clamp(AP+Random.Range(eff.APMin, eff.APMax), 0, GetFullAP());
			
			if(eff.duration<=0) return;
			
			eff.Init();
			effectList.Add(eff);
			UpdateActiveEffect();
			
			EffectTracker.Track(this);
		}
		
		public void IterateEffectDuration(){
			bool changed=false;
			for(int i=0; i<effectList.Count; i++){
				Debug.Log("Iterating effect " + effectList[i].name);
				effectList[i].Iterate();
				if(effectList[i].Due()){
					Debug.Log("Removing effect " + effectList[i].name);
					effectList.RemoveAt(i);	i-=1;
					changed=true;
				}
			}
			if(changed){
				if(effectList.Count>0) UpdateActiveEffect();
				else{
					activeEffect=new Effect();
					EffectTracker.Untrack(this);
				}
			}
		}
		
		public void UpdateActiveEffect(){
			activeEffect=new Effect();
			
			for(int i=0; i<effectList.Count; i++){
				activeEffect.stun|=effectList[i].stun;
				activeEffect.silence|=effectList[i].silence;
				
				activeEffect.HP+=effectList[i].HP;
				activeEffect.AP+=effectList[i].AP;
				
				activeEffect.HPPerTurn+=effectList[i].HPPerTurn;
				activeEffect.APPerTurn+=effectList[i].APPerTurn;
				
				activeEffect.moveAPCost+=effectList[i].moveAPCost;
				activeEffect.attackAPCost+=effectList[i].attackAPCost;
				
				activeEffect.turnPriority+=effectList[i].turnPriority;
				
				activeEffect.moveRange+=effectList[i].moveRange;
				activeEffect.attackRange+=effectList[i].attackRange;
				activeEffect.sight+=effectList[i].sight;
				
				activeEffect.movePerTurn+=effectList[i].movePerTurn;
				activeEffect.attackPerTurn+=effectList[i].attackPerTurn;
				activeEffect.counterPerTurn+=effectList[i].counterPerTurn;
				
				activeEffect.damage+=effectList[i].damage;
				
				activeEffect.hitChance+=effectList[i].hitChance;
				activeEffect.dodgeChance+=effectList[i].dodgeChance;
				
				activeEffect.critChance+=effectList[i].critChance;
				activeEffect.critAvoidance+=effectList[i].critAvoidance;
				activeEffect.critMultiplier+=effectList[i].critMultiplier;
				
				activeEffect.stunChance+=effectList[i].stunChance;
				activeEffect.stunAvoidance+=effectList[i].stunAvoidance;
				activeEffect.stunDuration+=effectList[i].stunDuration;
				
				activeEffect.silentChance+=effectList[i].silentChance;
				activeEffect.silentAvoidance+=effectList[i].silentAvoidance;
				activeEffect.silentDuration+=effectList[i].silentDuration;
				
				activeEffect.flankingBonus+=effectList[i].flankingBonus;
				activeEffect.flankedModifier+=effectList[i].flankedModifier;

				activeEffect.armorPenalty+=effectList[i].armorPenalty;
			}
			
			if(GameControl.GetSelectedUnit()==this && RequireReselect()) GameControl.ReselectUnit();
		}
		
		public bool RequireReselect(){
			if(activeEffect.movePerTurn!=0) return true;
			if(activeEffect.attackPerTurn!=0) return true;
			if(activeEffect.moveRange!=0) return true;
			if(activeEffect.attackRange!=0) return true;
			if(activeEffect.sight!=0) return true;
			return false;
		}
		
		
		
		
		float GetEffHPBuff(){ return activeEffect.HP; }
		float GetEffAPBuff(){ return activeEffect.AP; }
		
		float GetEffHPPerTurn(){ return activeEffect.HPPerTurn; }
		float GetEffAPPerTurn(){ return activeEffect.APPerTurn; }
		
		float GetEffMoveAPCost(){ return activeEffect.moveAPCost; }
		float GetEffAttackAPCost(){ return activeEffect.attackAPCost; }
		
		float GetEffTurnPriority(){ return activeEffect.turnPriority; }
		
		int GetEffMoveRange(){ return activeEffect.moveRange; }
		int GetEffAttackRange(){ return activeEffect.attackRange; }
		int GetEffSight(){ return activeEffect.sight; }
		
		int GetEffAttackPerTurn(){ return activeEffect.attackPerTurn; }
		int GetEffMovePerTurn(){ return activeEffect.movePerTurn; }
		int GetEffCounterPerTurn(){ return activeEffect.counterPerTurn; }
		
		float GetEffDamage(){ return activeEffect.damage; }
		
		float GetEffHitChance(){ return activeEffect.hitChance; }
		float GetEffDodgeChance(){ return activeEffect.dodgeChance; }
		
		float GetEffCritChance(){ return activeEffect.critChance; }
		float GetEffCritAvoidance(){ return activeEffect.critAvoidance; }
		float GetEffCritMultiplier(){ return activeEffect.critMultiplier; }
		
		float GetEffStunChance(){ return activeEffect.stunChance; }
		float GetEffStunAvoidance(){ return activeEffect.stunAvoidance; }
		int GetEffStunDuration(){ return activeEffect.stunDuration; }
		
		float GetEffSilentChance(){ return activeEffect.silentChance; }
		float GetEffSilentAvoidance(){ return activeEffect.silentAvoidance; }
		int GetEffSilentDuration(){ return activeEffect.silentDuration; }
		
		float GetEffFlankingBonus(){ return activeEffect.flankingBonus; }
		float GetEffFlankedModifier(){ return activeEffect.flankedModifier; }
		
		float GetEffArmorPenalty() { return activeEffect.armorPenalty; }
		//end ability effect section
		//********************************************************************************************************************************
		
		
		
		
		
		//********************************************************************************************************************************
		//these section are related to UnitAbilities
		
		public List<int> abilityIDList=new List<int>();
		public List<int> reserveAbilityIDList=new List<int>();
		public List<UnitAbility> abilityList=new List<UnitAbility>();	//only used in runtime
		public List<UnitAbility> GetAbilityList(){ return abilityList; }
		
		public UnitAbility GetUnitAbility(int Index){ return abilityList[Index]; }
		
		public void InitUnitAbility(){ 
			//get bonus abilityID from perk and add it to perkAbilityIDList
			List<int> perkAbilityIDList=PerkManager.GetUnitAbilityIDList(prefabID);
			for(int i=0; i<perkAbilityIDList.Count; i++) abilityIDList.Add(perkAbilityIDList[i]);
			
			List<int> perkAbilityXIDList=PerkManager.GetUnitAbilityXIDList(prefabID);
			for(int i=0; i<perkAbilityXIDList.Count; i++) abilityIDList.Remove(perkAbilityXIDList[i]);

			abilityList=AbilityManagerUnit.GetAbilityListBasedOnIDList(abilityIDList);
			for(int i=0; i<abilityList.Count; i++) abilityList[i].Init(this);
		}
		public void AddAbility(int abilityID, int index=-1){
			UnitAbility ability=AbilityManagerUnit.GetAbilityBasedOnID(abilityID);
			if(ability==null) return;
			Debug.LogWarning("Adding " + ability.name + " to " + this.unitName);

			
			ability.Init(this);
			
			if(index>=0) index=Mathf.Min(index, abilityList.Count);
			else index=abilityList.Count;
			
			abilityIDList.Insert(index, abilityID);
			abilityList.Insert(index, ability);
		}
		public void RemoveAbility(int index){
			if(abilityIDList.Count>index) abilityIDList.RemoveAt(index);
			if(abilityList.Count>index) abilityList.RemoveAt(index);
		}
		
		
		
		public string SelectAbility(int index){	//called from UI to select an ability
			AbilityManager.ExitAbilityTargetMode();
			
			UnitAbility ability=abilityList[index];
			
			string exception=ability.IsAvailable();
			if(exception!="") return exception;
			
			//requireTargetSelection=ability.requireTargetSelection;
			if(!ability.requireTargetSelection) ActivateAbility(tile, index);	
			else AbilityManagerUnit.ActivateTargetMode(tile, abilityList[index], index, this.ActivateAbility, null);
			
			return "";
		}
		
		
		//callback function for when a target tile has been selected for current active ability
		private Tile abilityTargetedTile;
		public void ActivateAbility(Tile targetTile, int index){
			if(targetTile==null) return;
			
			UnitAbility ability=abilityList[index];
			ability.factionID=factionID;
			AP-=ability.GetCost();
			ability.Use();
			
			abilityTargetedTile=targetTile;
			
			if(!ability.enableMoveAfterCast) moveRemain=0;
			if(!ability.enableAttackAfterCast) attackRemain=0;
			if(!ability.enableAbilityAfterCast) abilityRemain=0;
			
			TurnControl.CheckPlayerMoveFlag();
			bool isHit = IsHit(ability);
			bool isCrit = isHit && IsCrit();
			
			if(ability.normalAttack){
				OverlayManager.ClearSelection();
				
				GameObject shootObj=ability.shootObject;
				if(shootObj==null) shootObj=GameControl.GetDefaultShootObject();			
				if (isCrit && onActionCamE!=null) 
				{					
					onActionCamE(this, targetTile.GetPos(), GameControl.GetAbilityActionCamFreq());
				}
				
				StartCoroutine(AttackRoutineAbility(targetTile, targetTile.unit, shootObj, index, isHit, isCrit));
			}
			else{
				float delay=AbilityHit(index, isHit, isCrit);
				StartCoroutine(DelayFinishAction(delay));
			}
		}

		private bool IsHit(Ability ability) {
			float targetDodgeChance = abilityTargetedTile.unit.GetDodgeChance(); // PROTOTYPE HACK Dodge chance affects abilties
			Debug.Log("Dodge chance " + targetDodgeChance);
			Debug.Log("Hit chance " + GetHitChance());
			return Random.value <= ability.chanceToHit * (GetHitChance() - targetDodgeChance); // PROTOYPE HACK: Ability chanceToHit is a multiplier to normal chance to hit. Note that a negative dodge chance makes you easier to hit!
		}

		private bool IsCrit() {
			return Random.Range(0f, 1f)<GetCritChance();
		}
		//callback function when the shootObject of an ability which require shoot hits its target
		public float AbilityHit(int index, bool isHit, bool isCrit){
			if(!isHit) 
			{
				abilityTargetedTile.GetPos();
				new TextOverlay(abilityTargetedTile.GetPos(), "missed", Color.white);
				return 0;
			}
			
			if(abilityList[index].applyToAllUnit) abilityTargetedTile=null;
			
			float critMult = isCrit ? GetCritMultiplier() : 1.0f;
			//~ AbilityManagerUnit.ApplyAbilityEffect(this, abilityTargetedTile, abilityList[index]);
			AbilityManagerUnit.ApplyAbilityEffect(abilityTargetedTile, abilityList[index], (int)abilityList[index].type, this, critMult);
			return abilityList[index].effectDelayDuration;
		}
		
		//this is for ability teleport and spawnNewUnit to settle in new unit
		public void SetNewTile(Tile targetTile){	
			if(tile!=null) tile.unit=null;
			tile=targetTile;
			targetTile.unit=this;
			thisT.position=targetTile.GetPos();
			
			FactionManager.UpdateHostileUnitTriggerStatus(this);
			SetupFogOfWar();
		}
		
		public void Overwatch(Effect effect){
			overwatching=true;
			
			if(effect.duration==1)
			
			effect.Init();
			effectList.Add(effect);
			UpdateActiveEffect();
			
			EffectTracker.Track(this);
		}
		
		//end UnitAbilities section
		//********************************************************************************************************************************
		
		
		
		
		
		
		[HideInInspector] public Transform thisT;
		[HideInInspector] public GameObject thisObj;
		
		void Awake(){
			thisT=transform;
			thisObj=gameObject;
			
			for(int i=0; i<shootPointList.Count; i++){
				if(shootPointList[i]==null){
					shootPointList.RemoveAt(i);
					i-=1;
				}
			}
			
			if(shootPointList.Count==0) shootPointList.Add(thisT);
			
			if(turretObject==null) turretObject=thisT;
			
			SetDefaultRotation();
		}
		
		void Start () {
			HP=GetFullHP();
			if(GameControl.RestoreUnitAPOnTurn()) AP=GetFullAP();
			if (reserveAbilityIDList.Count > 0) canDrawCards = true;
		}
		
		
		
		public void PlaySelectAudio(){
			if(unitAudio!=null) unitAudio.Select();
		}
		
		
		
		private bool rotating=false;
		private Quaternion facingRotation;
		public void Rotate(Quaternion rotation){ 
			facingRotation=rotation;
			if(!rotating) StartCoroutine(_Rotate());
		}
		IEnumerator _Rotate(){
			rotating=true;
			while(Quaternion.Angle(facingRotation, thisT.rotation)>0.5f){
				if(!TurnControl.ClearToProceed()) yield break;
				thisT.rotation=Quaternion.Lerp(thisT.rotation, facingRotation, Time.deltaTime*moveSpeed*2);
				yield return null;
			}
			rotating=false;
		}
		
		public float GetMoveAPCostToTile(Tile targetTile){
			List<Tile> path=AStar.SearchWalkableTile(tile, targetTile);
			while(path.Count>GetEffectiveMoveRange()) path.RemoveAt(path.Count-1);
			return GetMoveAPCost()*path.Count;
		}
		
		public List<Tile> GetPathForAI(Tile targetTile){
			List<Tile> path=AStar.SearchWalkableTile(tile, targetTile);
			while(path.Count>GetEffectiveMoveRange()) path.RemoveAt(path.Count-1);
			return path;
		}
		
		public void Move(Tile targetTile){
			if(moveRemain<=0) return;
			
			moveRemain-=1;
			
			Debug.Log("moving "+name+" to "+targetTile);
			
			OverlayManager.ClearSelection();
			TurnControl.CheckPlayerMoveFlag();
			
			StartCoroutine(MoveRoutine(targetTile));
		}
		public IEnumerator MoveRoutine(Tile targetTile){
			tile.unit=null;
			GridManager.ClearAllTile();
			
			List<Tile> path=AStar.SearchWalkableTile(tile, targetTile);
			
			while(path.Count>GetEffectiveMoveRange()) path.RemoveAt(path.Count-1);
			
			AP-=GetMoveAPCost()*path.Count;
			
			//smooth the path so the unit wont zig-zag along a diagonal direction
			if(GridManager.GetTileType()==_TileType.Square && !GridManager.EnableDiagonalNeighbour()){	
				path.Insert(0, tile);
				path=PathSmoothing.SmoothDiagonal(path);
				path.RemoveAt(0);
			}
			
			while(!TurnControl.ClearToProceed()) yield return null;
			
			if(GameControl.EnableFogOfWar() && isAIUnit && path.Count>0 && !tile.IsVisible()){
				bool pathVisible=false;
				for(int i=0; i<path.Count; i++){
					if(path[i].IsVisible()){
						pathVisible=true;
						break;
					}
				}
				
				if(!pathVisible){
					thisT.position=path[path.Count-1].GetPos();
					tile=path[path.Count-1];
					tile.unit=this;
					yield break;
				}
			}
			
			//path.Insert(0, tile);
			//PathSmoothing.Smooth(path);
			//path.RemoveAt(0);
			
			TurnControl.ActionCommenced();
			
			if(path.Count!=0){
				if(unitAnim!=null) unitAnim.Move();
				if(unitAudio!=null) unitAudio.Move();
			}
			
			while(path.Count>0){
				/*
				//for path smoothing with subpath witin tile
				List<Vector3> tilePath=path[0].GetPath();
				while(tilePath.Count>0){
					while(true){
						Quaternion wantedRot=Quaternion.LookRotation(tilePath[0]-thisT.position);
						thisT.rotation=Quaternion.Lerp(thisT.rotation, wantedRot, Time.deltaTime*moveSpeed*4);
						
						float dist=Vector3.Distance(thisT.position, tilePath[0]);
						if(dist<0.05f) break;
						
						Vector3 dir=(tilePath[0]-thisT.position).normalized;
						thisT.Translate(dir*Mathf.Min(moveSpeed*Time.deltaTime, dist), Space.World);
						yield return null;
					}
					
					tilePath.RemoveAt(0);
				}
				*/
				
				while(true){
					float dist=Vector3.Distance(thisT.position, path[0].GetPos());
					if(dist<0.05f) break;
					
					Quaternion wantedRot=Quaternion.LookRotation(path[0].GetPos()-thisT.position);
					thisT.rotation=Quaternion.Slerp(thisT.rotation, wantedRot, Time.deltaTime*moveSpeed*3);
					
					Vector3 dir=(path[0].GetPos()-thisT.position).normalized;
					thisT.Translate(dir*Mathf.Min(moveSpeed*Time.deltaTime, dist), Space.World);
					yield return null;
				}
				
				tile=path[0];
				UpdateVisibility(path[0]);
				
				FactionManager.UpdateHostileUnitTriggerStatus(this);
				SetupFogOfWar();
				
				int triggerCount=TriggerOverWatch();
				if(triggerCount>0){
					while(!TurnControl.ClearToProceedFromOverWatch()) yield return null;
					
					if(HP<=0){	//if the unit get destroyed there and then by overwatch
						StartCoroutine(Dead());
						if(!isAIUnit) TurnControl.SelectedUnitMoveDepleted();
						TurnControl.ActionCompleted(0.25f);
						yield break;
					}
				}
				
				path.RemoveAt(0);
			}
			
			if(unitAnim!=null) unitAnim.StopMove();
			if(unitAudio!=null) unitAudio.StopMove();
			
			tile.unit=this;
			thisT.position=tile.GetPos();
			tile.TriggerCollectible(this);
			
			TurnControl.ActionCompleted(0.15f);
			
			FinishAction();
		}
		
		
		public int TriggerOverWatch(){
			List<Unit> unitList=FactionManager.GetAllUnit();
			int triggerCount=0;
			for(int i=0; i<unitList.Count; i++){
				if(unitList[i]==this) continue;
				if(unitList[i].CheckOverWatch(this)) triggerCount+=1;
			}
			return triggerCount;
		}
		
		
		public bool CheckOverWatch(Unit unit){
			if(unit==null) return false;
			if(!overwatching) return false;
			
			if(unit.factionID==factionID) return false;
			
			if(GridManager.GetDistance(unit.tile, tile)>GetAttackRange()) return false;
			
			if(GameControl.EnableFogOfWar()){
				if(!FogOfWar.IsTileVisibleToUnit(unit.tile, this)) return false;
			}
			
			//Debug.Log("overwatch attack   "+overwatching);
			overwatching=false;
			
			AttackOverWatch(unit);
			
			return true;
		}
		
		
		
		//for ability
		public IEnumerator AttackRoutineAbility(Tile targetTile, Unit targetUnit, GameObject shootObj, int abilityIndex, bool isHit, bool isCrit){
			Debug.Log("AttackRoutineAbility");
			while(!TurnControl.ClearToProceed()) yield return null;
			TurnControl.ActionCommenced();
			
			aiming=true;
			while(!Aiming(targetTile, targetUnit, shootObj.GetComponent<ShootObject>())) yield return null;
			aiming=false;
			
			//play animation
			float animDelay=PlayAttackAnimationNAudio(false);
			if(animDelay>0) yield return new WaitForSeconds(animDelay);
			
			waitingForAbilityHit=true;
			
			Vector3 targetPos=targetUnit==null ? targetTile.GetPos() : targetUnit.GetTargetT().position;
			float hitThreshold=targetUnit==null ? 0.2f : Mathf.Max(0.2f, targetUnit.hitThreshold);
			
			for(int i=0; i<shootPointList.Count; i++){
				Shoot(shootObj, shootPointList[i], targetPos, hitThreshold, this.AbilityHitCallback);
				if(delayBetweenShootPoint>0) yield return new WaitForSeconds(delayBetweenShootPoint);
			}
			
			while(waitingForAbilityHit) yield return null;
			
			float abilityEffectDelay=AbilityHit(abilityIndex, isHit, isCrit);
			
			TurnControl.ActionCompleted(abilityEffectDelay);
			FinishAction();
			
			if(turretObject!=null || barrelObject!=null){ while(!RotateTurretToOrigin() && !aiming) yield return null; }
		}
		private bool waitingForAbilityHit=false;
		public void AbilityHitCallback(){ waitingForAbilityHit=false; }
		
		
		
		
		public void AttackOverWatch(Unit targetUnit){
			TurnControl.ActionCommenced();
			
			GameControl.DisplayMessage("overwatch");
			
			if(onActionCamE!=null) onActionCamE(this, targetUnit.tile.GetPos(), GameControl.GetAttackActionCamFreq());
			
			StartCoroutine(AttackRoutine(targetUnit, false, true));
		}
		public void Attack(Unit targetUnit, bool isCounter=false){
			if(!isCounter){
				if(attackRemain==0) return;
				if(AP<GetAttackAPCost()) return;
				
				if(!GameControl.EnableActionAfterAttack()){
					moveRemain=0;
					abilityRemain=0;
				}
				
				if(onActionCamE!=null) onActionCamE(this, targetUnit.tile.GetPos(), GameControl.GetAttackActionCamFreq());
			}
			
			attackRemain-=1;
			AP-=GetAttackAPCost();
			
			OverlayManager.ClearSelection();
			TurnControl.CheckPlayerMoveFlag();
			
			StartCoroutine(AttackRoutine(targetUnit, isCounter));
		}
		public IEnumerator AttackRoutine(Unit targetUnit, bool isCounter=false, bool isOverWatch=false){
			if(!isCounter && !isOverWatch){
				while(!TurnControl.ClearToProceed()) yield return null;
				TurnControl.ActionCommenced();
			}
			
			bool isMelee=false;
			if(isHybridUnit && GridManager.GetDistance(tile, targetUnit.tile)<=meleeRange) isMelee=true;
			
			AttackInstance attInstance=GetAttackInfo(targetUnit, isCounter, isOverWatch, isMelee);
			
			ShootObject so=!isMelee ? shootObject : shootObjectMelee ;
			GameObject shootObj=so!=null ? so.gameObject : null ;
			if(shootObj==null) shootObj=GameControl.GetDefaultShootObject();
			
			aiming=true;
			while(!Aiming(targetUnit.tile, targetUnit, shootObj.GetComponent<ShootObject>())) yield return null;
			aiming=false;
			
			//play animation
			float animDelay=PlayAttackAnimationNAudio(attInstance.isMelee);
			if(animDelay>0) yield return new WaitForSeconds(animDelay);
			
			waitingForAttackHit=true;
			
			//shoot
			for(int i=0; i<shootPointList.Count; i++){
				Shoot(shootObj, shootPointList[i], targetUnit.GetTargetT().position, Mathf.Max(0.2f, targetUnit.hitThreshold), this.AttackHitCallback);
				if(delayBetweenShootPoint>0) yield return new WaitForSeconds(delayBetweenShootPoint);
			}
			
			while(waitingForAttackHit) yield return null;
			
			targetUnit.ApplyAttack(attInstance, isOverWatch);	//pass overwatch flag to stop the unit from being destroyed instantly
																					//multiple attack may happen at once, we need to wait until all attack has been completed
																					//after all the attack done, destoy would be call when MoveRoutine is resumed
			
			//if this is a normal attack and the target can counter attack, call the attack function
			//the responsible to call TurnControl.ActionCompleted() is then passed on the the countner attacking unit
			if(!isCounter && !isOverWatch && !attInstance.destroyed && targetUnit.CanCounter(this)) targetUnit.Attack(this, true);
			else{
				//in case we counter attack the selected player unit and destroy it
				if(isCounter && attInstance.destroyed && !targetUnit.isAIUnit){
					//Debug.Log("select any unit");
					TurnControl.SelectedUnitMoveDepleted();
				}
				TurnControl.ActionCompleted(attInstance.destroyed ? targetUnit.GetDestroyDuration() : 0.15f);
			}
			
			//if this is a normal attack, we need to finish the unit action, to reselect and so on
			if(!isCounter && !isOverWatch){
				while(!TurnControl.ClearToProceed()) yield return null;	//wait a bit in case the target unit is still counter attackinging
				FinishAction();
			}
			
			//if(turretObject!=null && turretObject!=thisT){ while(!RotateTurretToOrigin() && !aiming) yield return null; }
			if(turretObject!=null || barrelObject!=null){ while(!RotateTurretToOrigin() && !aiming) yield return null; }
		}
		private bool waitingForAttackHit=false;
		public void AttackHitCallback(){ waitingForAttackHit=false; }
		
		
		public AttackInstance GetAttackInfo(Unit targetUnit, bool isCounter=false, bool isOverWatch=false, bool isMelee=false){
			AttackInstance attInstance=new AttackInstance(this, targetUnit, isCounter, isOverWatch, isMelee);
			attInstance.Process();
			return attInstance;
		}
		
		
		public float PlayAttackAnimationNAudio(bool isMelee=false){
			float delay=0;
			if(unitAnim!=null){
				if(!isMelee) unitAnim.Attack();
				else unitAnim.AttackMelee();
				delay=isMelee ? unitAnim.attackDelayMelee : unitAnim.attackDelay;
			}
			if(unitAudio!=null){
				if(!isMelee) unitAudio.Attack();
				else unitAudio.AttackMelee();
			}
			return delay;
		}
		
		
		public float GetDestroyDuration(){ 
			Debug.Log("fix this");
			return 1.5f;
		}
		
		
		
		
		public bool rotateTurretOnly=false;
		public bool rotateTurretAimInXAxis=true;
		public Transform turretObject;
		public Transform barrelObject;
		public ShootObject shootObject;
		public ShootObject shootObjectMelee;
		public List<Transform> shootPointList=new List<Transform>();
		public float delayBetweenShootPoint=0;
		
		private bool aiming=false;	//to avoid aiming and rotate back to origin at the same time
		bool Aiming(Tile tile, Unit targetUnit=null, ShootObject so=null){
			Quaternion wantedRotY=Quaternion.LookRotation(tile.GetPos()-thisT.position);
			Vector3 targetPos=(targetUnit==null) ? tile.GetPos() : targetUnit.GetTargetT().position;
			
			//rotate body
			if(!rotateTurretOnly && thisT!=turretObject){
				thisT.rotation=Quaternion.Slerp(thisT.rotation, wantedRotY, Time.deltaTime*rotateSpeed);
			}
			
			if(rotateTurretAimInXAxis){
				float offsetX=so!=null ? so.GetProjectileShootAngle(shootPointList[0].position, targetPos) : 0;
				
				if(barrelObject!=null){
					turretObject.rotation=Quaternion.Slerp(turretObject.rotation, wantedRotY, Time.deltaTime*rotateSpeed);
					
					Quaternion wantedRot2=Quaternion.LookRotation(targetPos-barrelObject.position);
					Quaternion barrelRot=barrelDefaultRot*Quaternion.Euler(wantedRot2.eulerAngles.x-offsetX, 0, 0);
					barrelObject.localRotation=Quaternion.Slerp(barrelObject.localRotation, barrelRot, Time.deltaTime*rotateSpeed);
					
					float angle1=Quaternion.Angle(turretObject.rotation, wantedRotY);
					float angle2=Quaternion.Angle(barrelObject.localRotation, barrelRot);
					if(angle1<0.5f && angle2<0.5f) return true;
				}
				else{
					Quaternion wantedRot=Quaternion.LookRotation(targetPos-turretObject.position)*Quaternion.Euler(-offsetX, 0, 0);
					turretObject.rotation=Quaternion.Slerp(turretObject.rotation, wantedRot, Time.deltaTime*rotateSpeed);
					float angle=Quaternion.Angle(turretObject.rotation, wantedRot);
					if(angle<.5f) return true;
				}
			}
			else{
				turretObject.rotation=Quaternion.Slerp(turretObject.rotation, wantedRotY, Time.deltaTime*rotateSpeed);
				if(Mathf.Abs(turretObject.rotation.eulerAngles.y-wantedRotY.eulerAngles.y)<1f) return true;
			}
			
			return false;
		}
		bool RotateTurretToOrigin(){
			float angle=0;
			if(turretObject!=null && turretObject!=thisT){
				turretObject.localRotation=Quaternion.Lerp(turretObject.localRotation, turretDefaultRot, Time.deltaTime*rotateSpeed);
				angle=Quaternion.Angle(turretObject.localRotation, turretDefaultRot);
			}
			
			if(barrelObject!=null){
				barrelObject.localRotation=Quaternion.Slerp(barrelObject.localRotation, barrelDefaultRot, Time.deltaTime*rotateSpeed);
				float angleAlt=Quaternion.Angle(barrelObject.localRotation, barrelDefaultRot);
				return (angle>1 || angleAlt>1) ? false : true ;
			}
			else return angle>1 ? false : true ;
		}
		
		private Quaternion turretDefaultRot;
		private Quaternion barrelDefaultRot;
		void SetDefaultRotation(){
			turretDefaultRot=turretObject.localRotation;
			if(barrelObject!=null) barrelDefaultRot=barrelObject.localRotation;
		}
		
		
		private void Shoot(GameObject shootObj, Transform sp, Vector3 targetPos, float hitTH, ShootObject.HitCallback callback){
			GameObject sObjInstance=(GameObject)Instantiate(shootObj, sp.position, sp.rotation);
			ShootObject soInstance=sObjInstance.GetComponent<ShootObject>();
			soInstance.Shoot(targetPos, hitTH, callback);
		}
		
		
		
		IEnumerator DelayFinishAction(float delay){
			yield return new WaitForSeconds(delay);
			FinishAction();
		}
		void FinishAction(){
			if(isAIUnit) return;
			
			GameControl.ReselectUnit();
			/*
			if(IsAllActionCompleted()){
				FactionManager.UnitMoveDepleted(this);
				TurnControl.SelectedUnitMoveDepleted();
			}
			*/
			
			//check hp in case unit trigger a collectible and killed itself
			if(IsAllActionCompleted() || HP<=0){
				FactionManager.UnitMoveDepleted(this);
				TurnControl.SelectedUnitMoveDepleted();
			}
			else GameControl.ReselectUnit();
		}
		
		
		public Effect GetStunEffect(){
			stunEffect=new Effect();
			stunEffect.name="Stun";
			stunEffect.desp="Stunned by "+unitName+"'s attack";
			stunEffect.duration=stunDuration;
			return stunEffect;
		}
		
		public Effect GetSilentEffect(){
			silentEffect=new Effect();
			silentEffect.name="Silent";
			silentEffect.desp="Silenced by "+unitName+"'s attack";
			silentEffect.duration=silentDuration;
			return silentEffect;
		}
		
		
		public void ApplyAttack(AttackInstance attInstance, bool dontDestroyUnit=false){
			attInstance.srcUnit.lastTarget=this;
			lastAttacker=attInstance.srcUnit;
			if(isAIUnit && !trigger) trigger=true;
			
			if(attInstance.missed){
				new TextOverlay(GetTargetT().position, "missed", Color.white);
				return;
			}
			
			if(unitAudio!=null) unitAudio.Hit();
			if(unitAnim!=null) unitAnim.Hit();
			
			ApplyDamage(attInstance.damage * (1.0f + GetArmorPenalty()), attInstance.critical, dontDestroyUnit);
			
			if(attInstance.stunned) ApplyEffect(attInstance.srcUnit.GetStunEffect());
			
			if(attInstance.silenced) ApplyEffect(attInstance.srcUnit.GetSilentEffect());
			
		}
		public void ApplyDamage(float dmg, bool critical=false, bool dontDestroyUnit=false){
			if(!critical) new TextOverlay(GetTargetT().position, dmg.ToString("f0"), Color.white);
			else new TextOverlay(GetTargetT().position, dmg.ToString("f0")+" Critical!", new Color(1f, .6f, 0, 1f));
			
			HP-=dmg;
			if(HP<=0){
				HP=0;
				if(!dontDestroyUnit) StartCoroutine(Dead());
			}
		}
		
		public GameObject destroyEffectObj;
		public bool autoDestroyEffect=false;
		public float destroyEffectDuration=1.5f;
		IEnumerator Dead(){
			if(destroyEffectObj!=null){
				if(!autoDestroyEffect) ObjectPoolManager.Spawn(destroyEffectObj, GetTargetT().position, Quaternion.identity);
				else ObjectPoolManager.Spawn(destroyEffectObj, GetTargetT().position, Quaternion.identity, destroyEffectDuration);
			}
			
			float delay=0;
			if(unitAudio!=null) delay=unitAudio.Destroy();
			if(unitAnim!=null) delay=Mathf.Max(delay, unitAnim.Destroy());
			
			if(isAIUnit) TBTK.OnAIUnitDestroyed(this);
			else TBTK.OnPlayerUnitDestroyed(this);
			
			FactionManager.OnUnitDestroyed(this);
			GridManager.OnUnitDestroyed(this);
			TurnControl.OnUnitDestroyed();	//to track cd on ability and effect
			
			ClearVisibleTile();
			tile.unit=null;
			
			yield return new WaitForSeconds(delay);
			Destroy(thisObj);
		}
		
		public void RestoreHP(float val){
			HP=Mathf.Min(HP+val, GetFullHP());
			new TextOverlay(GetTargetT().position, val.ToString("f0"), Color.green);
		}
		
		//called when a unit just reach it's turn
		public bool NewTurn(){
			//Debug.Log("ResetUnitTurnData");
			numTurnsPlayed++;
			moveRemain=GetMovePerTurn();
			attackRemain=GetAttackPerTurn();
			counterRemain=GetCounterPerTurn();
			abilityRemain=abilityPerTurn;
			//disableAbilities=0;
			
			if(moveRange==0) moveRemain=0;
			if(attackRange==0) attackRemain=0;
			
			
			if(GameControl.RestoreUnitAPOnTurn()) AP=GetFullAP();
			else AP=Mathf.Min(AP+GetAPPerTurn(), GetFullAP());
			
			HP=Mathf.Min(HP+GetHPPerTurn(), GetFullHP());
			if(HP<=0){
				StartCoroutine(Dead());
				return false;
			}
			
			return true;
		}


		public bool canDrawCards  {get; private set; } = false;

		public class AbilitySelection {
			public AbilitySelection(Unit cardOwner, int abilityId) {
				this.cardOwner = cardOwner;
				this.abilityId = abilityId;
			}
			public Unit cardOwner {get; private set;}
			public int abilityId {get; private set;}
		};

		public static AbilitySelection DrawCard(List<Unit> unitsInFaction)
		{
			int numAbilitiesInDeck = unitsInFaction.Select(unit => unit.reserveAbilityIDList.Count).Sum();
			 Debug.LogWarning("num abilities in deck " + numAbilitiesInDeck);
			if (numAbilitiesInDeck > 0)
			{	
				int cardIdx = Random.Range(0, numAbilitiesInDeck);
				 Debug.LogWarning(cardIdx);
				int cardOwnerIdx = 0;
				while( cardIdx >= unitsInFaction[cardOwnerIdx].reserveAbilityIDList.Count) {
					cardIdx -= unitsInFaction[cardOwnerIdx].reserveAbilityIDList.Count;
					++cardOwnerIdx;
				};
				Unit cardOwner = unitsInFaction[cardOwnerIdx];
				Debug.LogWarning("card owner = " + cardOwner.unitName);
				Debug.LogWarning("card idx = " + cardIdx + "/" + cardOwner.reserveAbilityIDList.Count);
				
				int newAbilityId = cardOwner.reserveAbilityIDList[cardIdx];
				cardOwner.reserveAbilityIDList.RemoveAt(cardIdx);
				return new AbilitySelection(cardOwner, newAbilityId);
			}
			return null;
		}		
		public bool CanCounter(Unit unit){
			if(!GameControl.EnableCounter()) return false;
			if(IsStunned()) return false;
			if(counterRemain<=0) return false;
			if(GetCounterAPCost()>AP) return false;
			
			float dist=GridManager.GetDistance(unit.tile, tile);
			if(dist>GetAttackRange()) return false;
			
			if(GameControl.EnableFogOfWar()){
				if(requireDirectLOSToAttack && !FogOfWar.InLOS(unit.tile, tile)) return false; 
				if(!FogOfWar.IsTileVisibleToFaction(unit.tile, factionID)) return false;
			}
			
			return true;
		}
		
		public bool IsAllActionCompleted(){
			if(IsStunned()) return true;
			if(attackRemain>0 && AP>=GetAttackAPCost()) return false;
			if(moveRemain>0 && AP>=GetMoveAPCost()) return false;
			//if(CanUseAbilities()) return false;
			return true;
		}
		
		
		
		//********************************************************************************************************************************
		//these section are related to FogOfWar
		
		[HideInInspector] private List<Tile> visibleTileList=new List<Tile>();	//a list of tile visible to the unit
		
		//called whenever the unit moved into a new tile
		//when ignoreFaction is set to true,  even AI faction will run the function, this is for optimization since the function will be called in MoveRoutine very often
		public void SetupFogOfWar(bool ignoreFaction=false){
			if(!GameControl.EnableFogOfWar()) return;
			if(!ignoreFaction && isAIUnit) return;
			StartCoroutine(FogOfWarNewTile(ignoreFaction));
		}
		
		//add new tiles within sight to visibleTileList and remove those that are out of sight
		public IEnumerator FogOfWarNewTile(bool ignoreFaction){
			List<Tile> newList=GridManager.GetTilesWithinDistance(tile, sight, false);
			newList.Add(tile);
			
			ClearVisibleTile(newList);
			
			for(int i=0; i<newList.Count; i++){
				if(!visibleTileList.Contains(newList[i])){
					newList[i].SetVisible(FogOfWar.CheckTileVisibility(newList[i]));
				}
			}
			
			visibleTileList=newList;
			
			yield return null;
		}
		//set visible tile that becomes invisible to invisible
		public void ClearVisibleTile(List<Tile> newList=null){
			//Debug.DrawLine(tile.GetPos()+new Vector3(0.5f, 0, 0.5f), tile.GetPos()+new Vector3(-0.5f, 0, -0.5f), Color.blue, 2);
			//Debug.DrawLine(tile.GetPos()+new Vector3(-0.5f, 0, 0.5f), tile.GetPos()+new Vector3(0.5f, 0, -0.5f), Color.blue, 2);
			
			for(int i=0; i<visibleTileList.Count; i++){
				if(visibleTileList[i]==tile) visibleTileList[i].SetVisible(true);
				
				bool flag=FogOfWar.CheckTileVisibility(visibleTileList[i]);
				visibleTileList[i].SetVisible(flag);
				
				//Color color=flag ? Color.green : Color.red;
				//Debug.DrawLine(visibleTileList[i].GetPos()+new Vector3(0.5f, 0, 0.5f), visibleTileList[i].GetPos()+new Vector3(-0.5f, 0, -0.5f), color, 2);
			}
		}
		
		//called just before a unit start moving into a new tile, for AI unit to show/hide itself as it move in/out of fog-of-war
		//also called when a unit is just been placed on a grid in mid-game
		public void UpdateVisibility(Tile newTile=null){
			if(!GameControl.EnableFogOfWar()) return;
			if(!isAIUnit) return;
			
			if(newTile==null) newTile=tile;
			
			if(newTile.IsVisible()){
				thisObj.layer=TBTK.GetLayerUnit();
				Utilities.SetLayerRecursively(thisT, TBTK.GetLayerUnit());
			}
			else{
				thisObj.layer=TBTK.GetLayerUnitInvisible();
				Utilities.SetLayerRecursively(thisT, TBTK.GetLayerUnitInvisible());
			}
		}
		
		//end FogOfWar section
		//********************************************************************************************************************************
		
		
		
		
		
		
		
		
		[HideInInspector] private UnitAudio unitAudio;
		public void SetAudio(UnitAudio unitAudioInstance){ unitAudio=unitAudioInstance; }
		
		[HideInInspector] private UnitAnimation unitAnim;
		public void SetAnimation(UnitAnimation unitAnimInstance){ unitAnim=unitAnimInstance; }
		public void DisableAnimation(){ unitAnim=null; }
	}
	
}