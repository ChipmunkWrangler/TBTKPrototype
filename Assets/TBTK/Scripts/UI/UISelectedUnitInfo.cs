using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class UISelectedUnitInfo : MonoBehaviour {
		
		public Image imgIcon;
		
		public Slider sliderHP;
		public Slider sliderAP;
		
		public Text lbHP;
		public Text lbAP;
		
		public List<UIButton> itemList=new List<UIButton>();
		
		public GameObject effectTooltipObj;
		public Text lbEffectName;
		public Text lbEffectDesp;
		public Text lbEffectDuration;
		
		public UIButton buttonInfo;
		
		private Unit selectedUnit;
		
		private GameObject thisObj;
		private RectTransform rectT;
		private CanvasGroup canvasGroup;
		private static UISelectedUnitInfo instance;
		public static UISelectedUnitInfo GetInstance(){ return instance; }
		
		
		void Awake(){
			instance=this;
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
		}
		
		
		void Start(){
			for(int i=0; i<6; i++){
				if(i==0) itemList[0].Init();
				else if(i>0) itemList.Add(UIButton.Clone(itemList[0].rootObj, "Item"+(i+1)));
				
				itemList[i].SetCallback(this.OnHoverItem, this.OnExitItem, null, null);
				
				itemList[i].rootObj.SetActive(false);
			}
			
			rectT.localPosition=new Vector3(0, 0, 0);
			
			effectTooltipObj.SetActive(false);
			
			buttonInfo.Init();
			buttonInfo.SetActive(false);
			
			//canvasGroup.alpha=0;
			//thisObj.SetActive(false);
		}
		
		
		void OnEnable(){
			TBTK.onUnitSelectedE += OnUnitSelected;
		}
		void OnDisable(){
			TBTK.onUnitSelectedE -= OnUnitSelected;
		}
		
		
		public void OnUnitSelected(Unit unit){
			_UpdateDisplay(unit);
		}
		
		
		void OnHoverItem(GameObject itemObj){
			int ID=GetItemID(itemObj);
			
			lbEffectName.text=selectedUnit.effectList[ID].name;
			lbEffectDesp.text=selectedUnit.effectList[ID].desp;
			lbEffectDuration.text=selectedUnit.effectList[ID].duration+" turn remains";
			
			effectTooltipObj.SetActive(true);
		}
		public void OnExitItem(GameObject butObj){
			effectTooltipObj.SetActive(false);
		}
		
		
		private int GetItemID(GameObject butObj){
			for(int i=0; i<itemList.Count; i++){
				if(itemList[i].rootObj==butObj) return i;
			}
			return 0;
		}
		
		
		public static void UpdateDisplay(Unit unit){ instance._UpdateDisplay(unit); }
		public void _UpdateDisplay(Unit unit){
			selectedUnit=unit;
			
			if(selectedUnit!=null){
				imgIcon.sprite=selectedUnit.iconSprite;
				imgIcon.enabled=true;
				
				sliderHP.value=selectedUnit.GetHPRatio();
				sliderAP.value=selectedUnit.GetAPRatio();
				
				lbHP.text=selectedUnit.HP.ToString("f0")+"/"+selectedUnit.GetFullHP().ToString("f0");
				lbAP.text=selectedUnit.AP.ToString("f0")+"/"+selectedUnit.GetFullAP().ToString("f0");
				
				for(int i=0; i<itemList.Count; i++){
					if(i<selectedUnit.effectList.Count){
						itemList[i].imgIcon.sprite=selectedUnit.effectList[i].icon;
						itemList[i].label.text=selectedUnit.effectList[i].GetRemainingDuration().ToString();
						itemList[i].rootObj.SetActive(true);
					}
					else itemList[i].rootObj.SetActive(false);
				}
				
				buttonInfo.SetActive(true);
			}
			else{
				imgIcon.sprite=null;
				imgIcon.enabled=false;
				
				sliderHP.value=0;
				sliderAP.value=0;
				
				lbHP.text="";
				lbAP.text="";
				
				for(int i=0; i<itemList.Count; i++) itemList[i].rootObj.SetActive(false);
				
				buttonInfo.SetActive(false);
			}
		}
		
		
		public void OnUnitInfoScreenButton(){
			UIMainControl.OnShowUnitInfoScreen(selectedUnit);
		}
		
		
		public static void Show(){ instance._Show(); }
		public void _Show(){
			canvasGroup.interactable=true;
			canvasGroup.blocksRaycasts=true;
			
			//UpdateDisplay(tile);
			buttonInfo.SetActive(true);
			
			if(thisObj.activeInHierarchy) return;
			UIMainControl.FadeIn(canvasGroup, 0.2f, thisObj);
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			UIMainControl.FadeOut(canvasGroup, 0.2f, thisObj);
			//StartCoroutine(DelayHide());
		}
		IEnumerator DelayHide(){
			buttonInfo.SetActive(false);
			
			yield return new WaitForSeconds(0.2f);
			
			canvasGroup.interactable=false;
			canvasGroup.blocksRaycasts=false;
		}
		
		public static void HideInstant(){
			instance.canvasGroup.alpha=0;
			instance.thisObj.SetActive(false);
		}
		
	}

}