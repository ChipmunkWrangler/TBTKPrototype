using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class UIUnitInfo : MonoBehaviour {
		
		public Transform indicator;
		
		public Image imgIcon;
		
		public Slider sliderHP;
		public Slider sliderAP;
		
		public Text lbHP;
		public Text lbAP;
		
		public List<UIButton> itemList=new List<UIButton>();
		
		private RectTransform effectTooltipRectT;
		public GameObject effectTooltipObj;
		public Text lbEffectName;
		public Text lbEffectDesp;
		public Text lbEffectDuration;
		
		private Unit selectedUnit;
		
		private GameObject thisObj;
		private RectTransform rectT;
		private CanvasGroup canvasGroup;
		private static UIUnitInfo instance;
		public static UIUnitInfo GetInstance(){ return instance; }
		
		
		void Awake(){
			instance=this;
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			
			effectTooltipRectT=effectTooltipObj.GetComponent<RectTransform>();
			
		}
		
		
		void Start(){
			for(int i=0; i<10; i++){
				if(i==0) itemList[0].Init();
				else if(i>0) itemList.Add(UIButton.Clone(itemList[0].rootObj, "Item"+(i+1)));
				
				itemList[i].SetCallback(this.OnHoverItem, this.OnExitItem, null, null);
				
				itemList[i].rootObj.SetActive(false);
			}
			
			indicator=(Transform)Instantiate(indicator);
			indicator.parent=transform;
			indicator.gameObject.SetActive(false);
			
			rectT.localPosition=new Vector3(0, 0, 0);
			
			effectTooltipObj.SetActive(false);
			
			canvasGroup.alpha=0;
			thisObj.SetActive(false);
		}
		
		
		void OnHoverItem(GameObject itemObj){
			int ID=GetItemID(itemObj);
			
			lbEffectName.text=selectedUnit.effectList[ID].name;
			lbEffectDesp.text=selectedUnit.effectList[ID].desp;
			lbEffectDuration.text=selectedUnit.effectList[ID].duration+" turn remains";
			
			Debug.Log(itemList[ID].rectT.localPosition.x);
			effectTooltipRectT.localPosition=new Vector3(itemList[ID].rectT.localPosition.x, effectTooltipRectT.localPosition.y, 0);
			
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
		
		
		public static void UpdateDisplay(Tile tile){ instance._UpdateDisplay(tile); }
		public void _UpdateDisplay(Tile tile){
			selectedUnit=tile.unit;
			
			if(selectedUnit!=null){
				imgIcon.sprite=selectedUnit.iconSprite;
				
				sliderHP.value=selectedUnit.GetHPRatio();
				sliderAP.value=selectedUnit.GetAPRatio();
				
				lbHP.text=selectedUnit.HP.ToString("f0")+"/"+selectedUnit.GetFullHP().ToString("f0");
				lbAP.text=selectedUnit.AP.ToString("f0")+"/"+selectedUnit.GetFullAP().ToString("f0");
				
				for(int i=0; i<itemList.Count; i++){
					if(i<selectedUnit.effectList.Count){
						itemList[i].imgIcon.sprite=selectedUnit.effectList[i].icon;
						itemList[i].rootObj.SetActive(true);
					}
					else itemList[i].rootObj.SetActive(false);
				}
				
				indicator.position=tile.GetPos()+new Vector3(0, 0.05f, 0);
				indicator.gameObject.SetActive(true);
			}
			else _Hide();
		}
		
		
		public void OnUnitInfoScreenButton(){
			UIMainControl.OnShowUnitInfoScreen(selectedUnit);
		}
		
		
		
		public static void Show(Tile tile){ instance._Show(tile); }
		public void _Show(Tile tile){
			if(tile.unit==null || !tile.IsVisible()){
				if(thisObj.activeInHierarchy) _Hide();
				else return;
			}
			
			canvasGroup.interactable=true;
			canvasGroup.blocksRaycasts=true;
			
			UpdateDisplay(tile);
			
			if(thisObj.activeInHierarchy) return;
			UIMainControl.FadeIn(canvasGroup, 0.2f, thisObj);
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			indicator.gameObject.SetActive(false);
			
			UIMainControl.FadeOut(canvasGroup, 0.2f, thisObj);
			//StartCoroutine(DelayHide());
		}
		IEnumerator DelayHide(){
			yield return new WaitForSeconds(0.2f);
			
			canvasGroup.interactable=false;
			canvasGroup.blocksRaycasts=false;
		}
	
	}

}