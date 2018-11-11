using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System.Collections;
using System.Collections.Generic;

namespace TBTK {
	
	public delegate void Callback(GameObject uiObj);
	public delegate void CallbackInputDependent(GameObject uiObj, int pointerID);
	
	
	
	public class UI{
		//inputID=-1 - mouse cursor, 	inputID>=0 - touch finger index
		public static bool IsCursorOnUI(int inputID=-1){
			EventSystem eventSystem = EventSystem.current;
			return ( eventSystem.IsPointerOverGameObject( inputID ) );
		}
		
		
		public static GameObject Clone(GameObject srcObj, string name="", Vector3 posOffset=default(Vector3)) {
			GameObject newObj=(GameObject)MonoBehaviour.Instantiate(srcObj);
			newObj.name=name=="" ? srcObj.name : name ;
			
			newObj.transform.SetParent(srcObj.transform.parent);
			newObj.transform.localPosition=srcObj.transform.localPosition+posOffset;
			newObj.transform.localScale=new Vector3(1, 1, 1);
			
			return newObj;
		}
	}
	
	
	
	[System.Serializable]
	public class UIObject{
		public GameObject rootObj;
		[HideInInspector] public Transform rootT;
		[HideInInspector] public RectTransform rectT;
		
		[HideInInspector] public Image imgRoot;
		[HideInInspector] public Image imgIcon;
		[HideInInspector] public Text label;
		
		[HideInInspector] public UIItemCallback itemCallback;
		
		public UIObject(){}
		public UIObject(GameObject obj){
			rootObj=obj;
			Init();
		}
		public virtual void Init(){
			rootT=rootObj.transform;
			rectT=rootObj.GetComponent<RectTransform>();
			
			imgRoot=rootObj.GetComponent<Image>();
			
			foreach(Transform child in rectT){
				if(child.name=="Image"){
					imgIcon=child.GetComponent<Image>();
				}
				else if(child.name=="Text"){
					label=child.GetComponent<Text>();
				}
			}
		}
		
		public static UIObject Clone(GameObject srcObj, string name="", Vector3 posOffset=default(Vector3)){
			GameObject newObj=UI.Clone(srcObj, name, posOffset);
			return new UIObject(newObj);
		}
		
		public void SetCallback(Callback enter=null, Callback exit=null, CallbackInputDependent down=null, CallbackInputDependent up=null){
			itemCallback=rootObj.AddComponent<UIItemCallback>();
			itemCallback.SetEnterCallback(enter);
			itemCallback.SetExitCallback(exit);
			itemCallback.SetDownCallback(down);
			itemCallback.SetUpCallback(up);
		}
		
	}
	
	
	
	
	[System.Serializable]
	public class UIButton : UIObject{
		
		[HideInInspector] public CanvasGroup canvasG;
		
		[HideInInspector] public Text labelAlt;
		[HideInInspector] public Text labelAlt2;
		
		[HideInInspector] public Image imgHoverHighlight;
		[HideInInspector] public Image imgDisHighlight;
		[HideInInspector] public Image imgHighlight;
		
		[HideInInspector] public Button button;
		
		public UIButton(){}
		public UIButton(GameObject obj){
			rootObj=obj;
			Init();
		}
		public override void Init(){
			base.Init();
			
			button=rootObj.GetComponent<Button>();
			canvasG=rootObj.GetComponent<CanvasGroup>();
			
			foreach(Transform child in rectT){
				if(child.name=="TextAlt"){
					labelAlt=child.GetComponent<Text>();
				}
				if(child.name=="TextAlt2"){
					labelAlt2=child.GetComponent<Text>();
				}
				if(child.name=="Highlight"){
					imgHighlight=child.GetComponent<Image>();
				}
				if(child.name=="HoverHighlight"){
					imgHoverHighlight=child.GetComponent<Image>();
				}
				if(child.name=="DisableHighlight"){
					imgDisHighlight=child.GetComponent<Image>();
				}
			}
		}
		
		public static new UIButton Clone(GameObject srcObj, string name="", Vector3 posOffset=default(Vector3)){
			GameObject newObj=UI.Clone(srcObj, name, posOffset);
			return new UIButton(newObj);
		}
		
		
		public void SetActive(bool flag){
			if(flag && imgHoverHighlight!=null) imgHoverHighlight.enabled=false;
			if(flag && imgDisHighlight!=null) imgDisHighlight.enabled=false;
			rootObj.SetActive(flag);
		}
		
	}
	
	
	
	//used in perk menu only
	[System.Serializable]
	public class UIPerkItem : UIButton{
		public int perkID=-1;
		
		[HideInInspector] public GameObject selectHighlight;
		[HideInInspector] public GameObject purchasedHighlight;
		[HideInInspector] public GameObject unavailableHighlight;
		
		[HideInInspector] public GameObject connector;
		[HideInInspector] public GameObject connectorBG;
		
		public UIPerkItem(){}
		public UIPerkItem(GameObject obj){
			rootObj=obj;
			Init();
		}
		public override void Init(){
			base.Init();
			
			button=rootObj.GetComponent<Button>();
			
			foreach(Transform child in rectT){
				if(child.name=="SelectHighlight"){
					selectHighlight=child.gameObject;
					selectHighlight.SetActive(false);
				}
				else if(child.name=="PurchasedHighlight"){
					purchasedHighlight=child.gameObject;
				}
				else if(child.name=="UnavailableHighlight"){
					unavailableHighlight=child.gameObject;
				}
				else if(child.name=="Connector"){
					connector=child.gameObject;
				}
				else if(child.name=="ConnectorBG"){
					connectorBG=child.gameObject;
				}
				
				if(connectorBG!=null && connector!=null){
					connector.transform.SetParent(connectorBG.transform);
					connectorBG.transform.SetParent(rootT.parent);
					connector.SetActive(false);
				}
			}
		}
		
		public static new UIPerkItem Clone(GameObject srcObj, string name="", Vector3 posOffset=default(Vector3)){
			GameObject newObj=UI.Clone(srcObj, name, posOffset);
			return new UIPerkItem(newObj);
		}
	}
	
	
	
	public class UIItemCallback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler{
		private Callback enterCB;
		private Callback exitCB;
		private CallbackInputDependent downCB;
		private CallbackInputDependent upCB;
		
		public void SetEnterCallback(Callback callback){ enterCB=callback; }
		public void SetExitCallback(Callback callback){ exitCB=callback; }
		public void SetDownCallback(CallbackInputDependent callback){ downCB=callback; }
		public void SetUpCallback(CallbackInputDependent callback){ upCB=callback; }
		
		public void OnPointerEnter(PointerEventData eventData){ if(enterCB!=null) enterCB(thisObj); }
		public void OnPointerExit(PointerEventData eventData){ if(exitCB!=null) exitCB(thisObj); }
		public void OnPointerDown(PointerEventData eventData){ if(downCB!=null) downCB(thisObj, eventData.pointerId); }
		public void OnPointerUp(PointerEventData eventData){ if(upCB!=null) upCB(thisObj, eventData.pointerId); }
		
		private GameObject thisObj;
		void Awake(){ thisObj=gameObject; }
	}

}