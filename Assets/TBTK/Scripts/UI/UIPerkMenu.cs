using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class UIPerkMenu : MonoBehaviour {
		
		public bool demoMenu=false;
		
		public bool manuallySetupItem=false;

		public List<UIPerkItem> perkItemList=new List<UIPerkItem>();
		private int selectID=0;
		
		public Text lbPerkPoint;
		public Text lbPerkRsc;
		
		public Text lbPerkName;
		public Text lbPerkDesp;
		public Text lbPerkReq;
		public Text lbPerkCost;
		
		public UIButton butPurchase;
		public UIButton butClose;
		
		private GameObject thisObj;
		private RectTransform rectT;
		private CanvasGroup canvasGroup;
		private static UIPerkMenu instance;
		
		public void Awake(){
			instance=this;
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			if(demoMenu) canvasGroup.alpha=0;
		}
		
		void Start(){
			if(!UIMainControl.EnablePerkButton()) thisObj.SetActive(false);
			
			if(!manuallySetupItem){
				List<Perk> perkList=PerkManager.GetPerkList();
				for(int i=0; i<perkList.Count; i++){
					if(i==0) perkItemList[0].Init();
					else if(i>0) perkItemList.Add(UIPerkItem.Clone(perkItemList[0].rootObj, "PerkButton"+(i+1)));
					
					perkItemList[i].imgIcon.sprite=perkList[i].icon;
					perkItemList[i].perkID=perkList[i].prefabID;
					perkItemList[i].selectHighlight.SetActive(i==0);
					
					perkItemList[i].SetCallback(null, null, this.OnPerkItem, null);
				}
				
				UpdateContentRectSize();
			}
			else{
				for(int i=0; i<perkItemList.Count; i++){
					perkItemList[i].Init();
					perkItemList[i].selectHighlight.SetActive(i==0);
					perkItemList[i].SetCallback(null, null, this.OnPerkItem, null);
				}
			}
			
			butPurchase.Init();
			if(butClose.rootObj!=null) butClose.Init();
			
			UpdatePerkItemList();
			UpdateDisplay();
			
			if(demoMenu) thisObj.SetActive(false);
			//rectT.localPosition=new Vector3(0, 0, 0);
		}
		
		
		public GridLayoutGroup layoutGroup;
		private void UpdateContentRectSize(){
			int rowCount=(int)Mathf.Ceil(perkItemList.Count/(float)layoutGroup.constraintCount);
			float size=rowCount*layoutGroup.cellSize.y+rowCount*layoutGroup.spacing.y+layoutGroup.padding.top;
			
			RectTransform contentRect=layoutGroup.gameObject.GetComponent<RectTransform>();
			contentRect.sizeDelta=new Vector2(contentRect.sizeDelta.x, size);
		}
		
		
		void Update(){
			if(IsOn() && Input.GetKeyDown(KeyCode.Escape)) OnCloseButton();
		}
		
		
		public void OnPerkItem(GameObject butObj, int pointerID=-1){
			int ID=GetButtonID(butObj);
			
			perkItemList[selectID].selectHighlight.SetActive(false);
			
			selectID=ID;
			
			perkItemList[selectID].selectHighlight.SetActive(true);
			UpdateDisplay();
		}
		
		int GetButtonID(GameObject butObj){
			for(int i=0; i<perkItemList.Count; i++){
				if(perkItemList[i].rootObj==butObj) return i;
			}
			return 0;
		}
		
		
		
		void UpdateDisplay(){
			Perk perk=PerkManager.GetPerk(perkItemList[selectID].perkID);
			
			lbPerkName.text=perk.name;
			lbPerkDesp.text=perk.desp+"    ";
			
			if(perk.purchased){
				lbPerkReq.text="";
				
				lbPerkCost.text="";
				butPurchase.label.text="Purchased";
				butPurchase.button.interactable=false;
				return;
			}
			
			butPurchase.label.text="Purchase";
			
			string text=perk.IsAvailable();
			if(text==""){
				lbPerkCost.text="Cost: $"+perk.cost;
				lbPerkReq.text="";
				butPurchase.button.interactable=true;
			}
			else{
				lbPerkCost.text="";
				lbPerkReq.text=text;
				butPurchase.button.interactable=false;
			}
		}
		
		void UpdatePerkItemList(){
			if(lbPerkPoint!=null) lbPerkPoint.text=PerkManager.GetPerkPoint().ToString();
			if(lbPerkRsc!=null) lbPerkRsc.text=PerkManager.GetPerkCurrency().ToString();
			
			for(int i=0; i<perkItemList.Count; i++){
				bool purchased=PerkManager.IsPerkPurchased(perkItemList[i].perkID);
				bool available=PerkManager.IsPerkAvailable(perkItemList[i].perkID)=="";
				perkItemList[i].purchasedHighlight.SetActive(purchased);
				perkItemList[i].unavailableHighlight.SetActive(!(purchased || available));
				if(perkItemList[i].connector!=null) perkItemList[i].connector.SetActive(purchased);
			}
		}
		
		
		
		public void OnPurchaseButton(){
			string text=PerkManager.PurchasePerk(perkItemList[selectID].perkID);
			
			if(text!=""){
				UIMessage.DisplayMessage(text);
				return;
			}
			
			UpdatePerkItemList();
			
			UpdateDisplay();
		}
		
		
		public void OnCloseButton(){
			UIMainControl.OnClosePerkMenu();
		}
		public static void DisableCloseButton1(){	//not in used?
			//instance.butClose.SetActive(false);
		}
		
		
		private bool isOn=false;
		public static bool IsOn(){ return instance==null ? false : instance.isOn; }
		
		public static void Show(){ instance._Show(); }
		public void _Show(){
			UpdatePerkItemList();
			UpdateDisplay();
			
			isOn=true;
			butClose.SetActive(true);
			
			rectT.localPosition=Vector3.zero;
			UIMainControl.FadeIn(canvasGroup, 0.25f, thisObj);
			
			
		}
		public static void Hide(){ instance._Hide(); }
		public void _Hide(){
			isOn=false;
			UIMainControl.FadeOut(canvasGroup, 0.25f, thisObj);
			butClose.SetActive(false);
			//StartCoroutine(DelayHide());
		}
		IEnumerator DelayHide(){
			yield return new WaitForSeconds(0.25f);
			isOn=false;
			//rectT.localPosition=new Vector3(-5000, -5000, 0);
		}
		
	}

}