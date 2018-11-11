using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class UIFactionAbilityButton : MonoBehaviour {

		public Slider sliderEnergy;
		public Text lbEnergy;
		
		public UIButton buttonCancel;
		
		public List<UIButton> buttonList=new List<UIButton>();
		
		public RectTransform lineRectT;
		
		private GameObject thisObj;
		private RectTransform rectT;
		private CanvasGroup canvasGroup;
		private static UIFactionAbilityButton instance;
		public static UIFactionAbilityButton GetInstance(){ return instance; }
		
		void Awake(){
			instance=this;
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			canvasGroup.alpha=0;
			canvasGroup.interactable=false;
			canvasGroup.blocksRaycasts=false;
			
			tooltipObj.SetActive(false);
			
			rectT.localPosition=new Vector3(0, 0, 0);
		}
		
		void Start(){
			for(int i=0; i<8; i++){
				if(i==0) buttonList[0].Init();
				else if(i>0) buttonList.Add(UIButton.Clone(buttonList[0].rootObj, "AbilityButton"+(i+1)));
				
				if(UIMainControl.InTouchMode()) buttonList[i].SetCallback(null, null, this.OnAbilityButton, null);
				else buttonList[i].SetCallback(this.OnHoverButton, this.OnExitButton, this.OnAbilityButton, null);
				//buttonList[i].SetCallback(this.OnHoverButton, this.OnExitButton, this.OnAbilityButton, null);
				
				buttonList[i].rootObj.SetActive(false);
			}
			
			buttonCancel.Init();
			buttonCancel.SetCallback(null, null, this.OnCancelButton, null);
			buttonCancel.SetActive(false);
			buttonCancel.rectT.SetAsLastSibling();
		}
		
		
		
		void OnEnable(){
			TBTK.onFactionABTargetModeE += OnTargetMode;
			TBTK.onNewFactionAbilityE += OnNewAbility;
		}
		void OnDisable(){
			TBTK.onFactionABTargetModeE -= OnTargetMode;
			TBTK.onNewFactionAbilityE -= OnNewAbility;
		}
		
		
		void OnTargetMode(int index=-1){
			tooltipObj.SetActive(false);
			
			for(int i=0; i<buttonList.Count; i++) buttonList[i].imgHighlight.enabled=(i==index);
			_OnNewTurn(true);
			
			if(index>=0){
				buttonCancel.rectT.localPosition=buttonList[index].rectT.localPosition;
				buttonCancel.SetActive(true);
			}
			else buttonCancel.SetActive(false);
		}
		
		
		void OnNewAbility(Ability ability){
			if(showing) _OnNewTurn(true);
		}
		
		
		
		private List<FactionAbility> abilityList=new List<FactionAbility>();
		public static void OnGameInAction(bool flag){ instance._OnGameInAction(flag); }
		public void _OnGameInAction(bool flag){
			if(!UIMainControl.IsPlayerTurn()) return;
			for(int i=0; i<abilityList.Count; i++) buttonList[i].button.interactable=(abilityList[i].IsAvailable()=="" & !flag);
		}
		
		public static void OnNewTurn(bool flag){ instance._OnNewTurn(flag); }
		public void _OnNewTurn(bool flag){
			if(!flag) return;
			
			abilityList=FactionManager.GetCurrentFaction().abilityInfo.abilityList;
			
			if(abilityList.Count==0) return;
			
			for(int i=0; i<buttonList.Count; i++){
				if(i>=abilityList.Count) buttonList[i].SetActive(false);
				else{
					buttonList[i].imgIcon.sprite=abilityList[i].icon;
					buttonList[i].label.text=abilityList[i].GetUseRemain()>0 ? abilityList[i].GetUseRemain().ToString() : "∞";
					buttonList[i].button.interactable=(abilityList[i].IsAvailable()=="");
					buttonList[i].SetActive(true);
				}
			}
			
			if(abilityList.Count==0) lineRectT.sizeDelta=new Vector2(0, lineRectT.sizeDelta.y);
			else lineRectT.sizeDelta=new Vector2(20+abilityList.Count*60, lineRectT.sizeDelta.y);
			
			UpdateEnergyDisplay();
			
			_Show();
		}
		
		
		
		void UpdateEnergyDisplay(){
			float energy=AbilityManagerFaction.GetFactionEnergy(FactionManager.GetSelectedFactionID());
			float energyFull=AbilityManagerFaction.GetFactionEnergyFull(FactionManager.GetSelectedFactionID());
			lbEnergy.text=energy+"/"+energyFull;
			sliderEnergy.value=energy/energyFull;
		}
		
		
		
		public GameObject tooltipObj;
		public Text lbTooltipName;
		public Text lbTooltipDesp;
		public Text lbTooltipCost;
		public Text lbTooltipCooldown;
		
		
		private int currentButtonID=-1; //last touched button, for touch mode only
		public void OnAbilityButton(GameObject butObj, int pointerID=-1){
			int ID=GetButtonID(butObj);
			
			if(UIMainControl.InTouchMode()){
				if(currentButtonID>=0) buttonList[currentButtonID].imgHighlight.enabled=false;
				if(currentButtonID!=ID){
					currentButtonID=ID;
					buttonList[ID].imgHighlight.enabled=true;
					OnHoverButton(butObj);
					return;
				}
				ClearTouchModeButton();
			}
			
			string exception=AbilityManagerFaction.SelectAbility(ID);
			if(exception!="") UIMessage.DisplayMessage(exception);
			
			buttonList[ID].imgHighlight.enabled=true;
		}
		
		public void ClearTouchModeButton(){
			if(currentButtonID>=0) buttonList[currentButtonID].imgHighlight.enabled=false;
			currentButtonID=-1;
			OnExitButton(null);
		}
		
		public void OnHoverButton(GameObject butObj){
			int ID=GetButtonID(butObj);
			
			lbTooltipName.text=abilityList[ID].name;
			lbTooltipDesp.text=abilityList[ID].desp;
			lbTooltipCost.text="cost:"+abilityList[ID].GetCost()+"";
			lbTooltipCooldown.text="cooldown:"+abilityList[ID].GetCooldown();
			
			tooltipObj.SetActive(true);
		}
		
		public void OnExitButton(GameObject butObj){
			tooltipObj.SetActive(false);
		}
		
		
		
		private int GetButtonID(GameObject butObj){
			for(int i=0; i<buttonList.Count; i++){
				if(buttonList[i].rootObj==butObj) return i;
			}
			return 0;
		}
		
		
		public void OnCancelButton(GameObject butObj, int pointerID=-1){
			AbilityManager.ExitAbilityTargetMode();
			buttonCancel.SetActive(false);
		}
		
		
		
		private bool showing=false;
		
		public static void Show(){ instance._Show(); }
		public void _Show(){
			if(showing) return;
			showing=true;
			
			canvasGroup.interactable=true;
			canvasGroup.blocksRaycasts=true;
			
			UIMainControl.FadeIn(canvasGroup, 0.25f);
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			if(!showing) return;
			showing=false;
			
			UIMainControl.FadeOut(canvasGroup, 0.25f);
			StartCoroutine(DelayHide());
		}
		IEnumerator DelayHide(){
			yield return new WaitForSeconds(0.3f);
			
			canvasGroup.interactable=false;
			canvasGroup.blocksRaycasts=false;
		}
		
	}

}