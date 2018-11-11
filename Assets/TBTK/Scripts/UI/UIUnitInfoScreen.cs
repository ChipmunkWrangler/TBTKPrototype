using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class UIUnitInfoScreen : MonoBehaviour {

		private GameObject thisObj;
		private RectTransform rectT;
		private CanvasGroup canvasGroup;
		private static UIUnitInfoScreen instance;
		
		public Text lbName;
		public Text lbDesp;
		
		public Transform statsListParent;
		public List<UIButton> statsItemList=new List<UIButton>();
		
		public List<UIButton> abilityItemList=new List<UIButton>();
		public List<UIButton> effectItemList=new List<UIButton>();
		
		public GameObject effectTooltipObj;
		public Text lbEffectName;
		public Text lbEffectDesp;
		public Text lbEffectDuration;
		
		public GameObject abilityTooltipObj;
		public Text lbAbilityName;
		public Text lbAbilityDesp;
		public Text lbAbilityCost;
		public Text lbAbilityCooldown;
		
		public UIButton buttonClose;
		
		public void Awake(){
			instance=this;
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			canvasGroup.alpha=0;
			canvasGroup.interactable=false;
			canvasGroup.blocksRaycasts=false;
			
			InitiateElement();
			
			abilityTooltipObj.SetActive(false);
			effectTooltipObj.SetActive(false);
			
			thisObj.SetActive(false);
			rectT.anchoredPosition=new Vector3(0, 0, 0);
		}
		
		
		void InitiateElement(){
			for(int i=0; i<10; i++){
				GameObject obj=statsListParent.GetChild(i).gameObject;
				statsItemList.Add(new UIButton(obj));
				
				if(i==0) statsItemList[i].label.text="Hit-Point:";
				else if(i==1) statsItemList[i].label.text="Action-Point:";
				else if(i==2) statsItemList[i].label.text="Damage:";
				else if(i==3) statsItemList[i].label.text="Hit Chance:";
				else if(i==4) statsItemList[i].label.text="Critical Chance:";
				
				else if(i==5) statsItemList[i].label.text="Move Range:";
				else if(i==6) statsItemList[i].label.text="Attack Range:";
				else if(i==7) statsItemList[i].label.text="Move Priority:";
				else if(i==8) statsItemList[i].label.text="Dodge Chance:";
				else if(i==9) statsItemList[i].label.text="Critical Avoidance:";
			}
			
			for(int i=0; i<6; i++){
				if(i==0) abilityItemList[0].Init();
				else if(i>0) abilityItemList.Add(UIButton.Clone(abilityItemList[0].rootObj, "AbilityItem"+(i+1)));
				abilityItemList[i].SetCallback(this.OnHoverAbilityItem, this.OnExitAbilityItem, null, null);
				abilityItemList[i].SetActive(false);
			}
			
			for(int i=0; i<6; i++){
				if(i==0) effectItemList[0].Init();
				else if(i>0) effectItemList.Add(UIButton.Clone(effectItemList[0].rootObj, "EffectItem"+(i+1)));
				effectItemList[i].SetCallback(this.OnHoverEffectItem, this.OnExitEffectItem, null, null);
				effectItemList[i].SetActive(false);
			}
			
			buttonClose.Init();
		}
		
		
		public void OnHoverAbilityItem(GameObject butObj){
			int ID=GetAbilityItemID(butObj);
			
			Ability ability=selectedUnit.GetUnitAbility(ID);
			
			lbAbilityName.text=ability.name;
			lbAbilityDesp.text=ability.desp;
			lbAbilityCost.text="Cost: "+ability.cost+"AP";
			lbAbilityCooldown.text="Cooldown: "+ability.cooldown;
			
			abilityTooltipObj.SetActive(true);
		}
		public void OnExitAbilityItem(GameObject butObj){
			abilityTooltipObj.SetActive(false);
		}
		private int GetAbilityItemID(GameObject butObj){
			for(int i=0; i<abilityItemList.Count; i++){
				if(abilityItemList[i].rootObj==butObj) return i;
			}
			return 0;
		}
		
		public void OnHoverEffectItem(GameObject butObj){
			int ID=GetEffectItemID(butObj);
			
			lbEffectName.text=selectedUnit.effectList[ID].name;
			lbEffectDesp.text=selectedUnit.effectList[ID].desp;
			lbEffectDuration.text=selectedUnit.effectList[ID].duration+" turn remains";
			
			effectTooltipObj.SetActive(true);
		}
		public void OnExitEffectItem(GameObject butObj){
			effectTooltipObj.SetActive(false);
		}
		private int GetEffectItemID(GameObject butObj){
			for(int i=0; i<effectItemList.Count; i++){
				if(effectItemList[i].rootObj==butObj) return i;
			}
			return 0;
		}
		
		
		public void UpdateDisplay(Unit unit){
			selectedUnit=unit;
			
			lbName.text=unit.unitName;
			lbDesp.text=unit.desp;
			
			statsItemList[0].labelAlt.text=unit.HP+"/"+unit.GetFullHP();//"Hit-Point:";
			statsItemList[1].labelAlt.text=unit.AP+"/"+unit.GetFullAP();;//"Action-Point:";
			statsItemList[2].labelAlt.text=unit.GetDamageMin()+"-"+unit.GetDamageMax();//"Damage:";
			statsItemList[3].labelAlt.text=(unit.GetHitChance()*100).ToString("f0")+"%";//"Hit Chance:";
			statsItemList[4].labelAlt.text=(unit.GetCritChance()*100).ToString("f0")+"%";//"Critical Chance:";
			
			statsItemList[5].labelAlt.text=unit.GetMoveRange().ToString("f0");//"Move Range:";
			statsItemList[6].labelAlt.text=unit.GetAttackRange().ToString("f0");//"Attack Range:";
			statsItemList[7].labelAlt.text=unit.GetTurnPriority().ToString("f0");//"Move Priority:";
			statsItemList[8].labelAlt.text=(unit.GetDodgeChance()*100).ToString("f0")+"%";//"Dodge Chance:";
			statsItemList[9].labelAlt.text=(unit.GetCritAvoidance()*100).ToString("f0")+"%";//"Critical Avoidance:";
			
			
			for(int i=0; i<abilityItemList.Count; i++){
				if(i<unit.abilityList.Count){
					int useRemain=unit.abilityList[i].GetUseRemain();
					
					abilityItemList[i].imgIcon.sprite=unit.abilityList[i].icon;
					abilityItemList[i].label.text=useRemain<0 ? "∞" : useRemain.ToString();
					abilityItemList[i].SetActive(true);
				}
				else abilityItemList[i].SetActive(false);
			}
			
			for(int i=0; i<effectItemList.Count; i++){
				if(i<unit.effectList.Count){
					effectItemList[i].imgIcon.sprite=unit.effectList[i].icon;
					effectItemList[i].label.text=unit.effectList[i].GetRemainingDuration().ToString();
					effectItemList[i].SetActive(true);
				}
				else effectItemList[i].SetActive(false);
			}
		}
		
		
		public void OnCloseButton(){
			UIMainControl.HideUnitInfoScreen();
		}
		
		
		private Unit selectedUnit;
		public static void Show(Unit unit){ instance._Show(unit); }
		public void _Show(Unit unit){
			canvasGroup.interactable=true;
			canvasGroup.blocksRaycasts=true;
			
			UpdateDisplay(unit);
			buttonClose.SetActive(true);
			
			rectT.localPosition=new Vector3(0, 0, 0);
			UIMainControl.FadeIn(canvasGroup, 0.25f, thisObj);
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			UIMainControl.FadeOut(canvasGroup, 0.25f);
			StartCoroutine(DelayHide());
		}
		IEnumerator DelayHide(){
			canvasGroup.interactable=false;
			canvasGroup.blocksRaycasts=false;
			
			
			yield return new WaitForSeconds(0.35f);
			//rectT.localPosition=new Vector3(-5000, -5000, 0);
			
			thisObj.SetActive(false);
		}
		
	}

}