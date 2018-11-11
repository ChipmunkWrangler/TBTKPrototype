using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class UIUnitAbilityButton : MonoBehaviour {
		
		public UIButton buttonCancel;
		
		public List<UIButton> buttonList=new List<UIButton>();
		
		private GameObject thisObj;
		private RectTransform rectT;
		private CanvasGroup canvasGroup;
		private static UIUnitAbilityButton instance;
		public static UIUnitAbilityButton GetInstance(){ return instance; }
		
		void Awake(){
			instance=this;
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			tooltipRectT=tooltipObj.GetComponent<RectTransform>();
			tooltipObj.SetActive(false);
			
			rectT.localPosition=new Vector3(0, 0, 0);
		}
		
		void Start(){
			for(int i=0; i<6; i++){
				if(i==0) buttonList[0].Init();
				else if(i>0) buttonList.Add(UIButton.Clone(buttonList[0].rootObj, "AbilityButton"+(i+1)));
				
				if(UIMainControl.InTouchMode()) buttonList[i].SetCallback(null, null, this.OnAbilityButton, null);
				else buttonList[i].SetCallback(this.OnHoverButton, this.OnExitButton, this.OnAbilityButton, null);
				
				buttonList[i].rootObj.SetActive(false);
			}
			
			buttonCancel.Init();
			buttonCancel.SetCallback(null, null, this.OnCancelButton, null);
			buttonCancel.SetActive(false);
			//buttonCancel.rectT.SetAsLastSibling();
		}
		
		
		
		void OnEnable(){
			TBTK.onUnitSelectedE += OnUnitSelected;
			TBTK.onUnitABTargetModeE += OnTargetMode;
		}
		void OnDisable(){
			TBTK.onUnitSelectedE -= OnUnitSelected;
			TBTK.onUnitABTargetModeE -= OnTargetMode;
		}
		
		
		void OnTargetMode(int index=-1){
			tooltipObj.SetActive(false);
			for(int i=0; i<buttonList.Count; i++) buttonList[i].imgHighlight.enabled=(i==index);
			
			if(index>=0){
				buttonCancel.rectT.localPosition=buttonList[index].rectT.localPosition;
				buttonCancel.SetActive(true);
			}
			else buttonCancel.SetActive(false);
		}
		
		
		
		void OnUnitSelected(Unit unit){
			if(unit==null){
				for(int i=0; i<buttonList.Count; i++) buttonList[i].SetActive(false);
				tooltipObj.SetActive(false);
			}
			else{
				List<UnitAbility> abilityList=unit.GetAbilityList();
				
				for(int i=0; i<buttonList.Count; i++){
					if(i>=abilityList.Count){
						buttonList[i].SetActive(false);
					}
					else{
						buttonList[i].imgIcon.sprite=abilityList[i].icon;
						buttonList[i].button.interactable=(abilityList[i].IsAvailable()=="");
						buttonList[i].SetActive(true);
					}
				}
			}
		}
		
		
		
		public static void OnGameInAction(bool flag){ instance._OnGameInAction(flag); }
		public void _OnGameInAction(bool flag){
			if(!flag) return;	//dont need to set the button state when action complete since OnUnitSelected will be called
			for(int i=0; i<buttonList.Count; i++) buttonList[i].button.interactable=!flag;
		}
		
		public static void OnNewTurn(bool flag){ instance._OnNewTurn(flag); }
		public void _OnNewTurn(bool flag){
			_Show();
		}
		
		
		
		public GameObject tooltipObj;
		private RectTransform tooltipRectT;
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
			
			string exception=GameControl.GetSelectedUnit().SelectAbility(ID);
			if(exception!="") UIMessage.DisplayMessage(exception);
		}
		
		public void ClearTouchModeButton(){
			if(currentButtonID>=0) buttonList[currentButtonID].imgHighlight.enabled=false;
			currentButtonID=-1;
			OnExitButton(null);
		}
		
		public void OnHoverButton(GameObject butObj){
			int ID=GetButtonID(butObj);
			Ability ability=GameControl.GetSelectedUnit().GetUnitAbility(ID);
				
			lbTooltipName.text=ability.name;
			lbTooltipDesp.text=ability.desp;
			lbTooltipCost.text="cost: "+ability.GetCost()+"AP";
			lbTooltipCooldown.text="cooldown: "+ability.GetCooldown();
			
			tooltipRectT.localPosition=new Vector3(tooltipRectT.localPosition.x, buttonList[ID].rectT.localPosition.y+40, 0);
			
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
		
		
		
		
		public static void Show(){ instance._Show(); }
		public void _Show(){
			canvasGroup.interactable=true;
			canvasGroup.blocksRaycasts=true;
			
			UIMainControl.FadeIn(canvasGroup, 0.25f);
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			OnCancelButton(null);
			
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