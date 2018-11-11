using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class UIMessage : MonoBehaviour {
		
		[System.Serializable]
		public class UIMsgItem{
			public GameObject rootObj;
			public RectTransform rectT;
			
			public Text label;
			public CanvasGroup canvasG;
			
			public UIMsgItem(){}
			public UIMsgItem(GameObject obj){
				rootObj=obj;
				Init();
			}
			public virtual void Init(){
				canvasG=rootObj.GetComponent<CanvasGroup>();
				rectT=rootObj.GetComponent<RectTransform>();
				label=rootObj.GetComponent<Text>();
			}
			
			public static UIMsgItem Clone(GameObject srcObj, string name=""){
				return new UIMsgItem(UI.Clone(srcObj, name));
			}
		}
		
		
		
		public GameObject messageObj;
		public List<UIMsgItem> msgList=new List<UIMsgItem>();
		
		private static UIMessage instance;
		
		void Awake () {
			instance=this;
			gameObject.GetComponent<RectTransform>().localPosition=new Vector3(0, 0, 0);
		}
		
		// Use this for initialization
		void Start () {
			for(int i=0; i<15; i++){
				if(i==0) msgList.Add(new UIMsgItem(messageObj));
				else msgList.Add(UIMsgItem.Clone(messageObj, "TextMessage"+(i+1)));
				
				if(msgList[i].canvasG==null)
					msgList[i].canvasG=msgList[i].rootObj.AddComponent<CanvasGroup>();
				
				msgList[i].canvasG.interactable=false;
				msgList[i].canvasG.blocksRaycasts=false;
				
				msgList[i].rootObj.SetActive(false);
			}
		}
		
		
		void OnEnable(){
			TBTK.onGameMessageE += _DisplayMessage;
		}
		void OnDisable(){
			TBTK.onGameMessageE -= _DisplayMessage;
		}
		
		
		//void Update(){
		//	if(Input.GetKeyDown(KeyCode.Space)){
		//		_DisplayMessage("text text text text - "+Random.Range(99, 99999999));
		//	}
		//}
		
		
		public static void DisplayMessage(string msg){ instance._DisplayMessage(msg); }
		void _DisplayMessage(string msg){
			Debug.Log(msg+"  "+transform);
			
			//int index=GetUnusedTextIndex();
			int index=0;
			
			msgList[index].label.text=msg;
			
			StartCoroutine(DisplayItemRoutine(msgList[index]));
		}
		
		
		private Vector3 scale=new Vector3(1, 1, 1);
		private Vector3 scaleZoomed=new Vector3(1.25f, 1.25f, 1.25f);
		
		IEnumerator DisplayItemRoutine(UIMsgItem item){
			item.rectT.SetAsFirstSibling();
			
			UIMainControl.FadeIn(item.canvasG, 0.1f, item.rootObj);
			
			StartCoroutine(ScaleRectTRoutine(item.rectT, .1f, scale, scaleZoomed));	
			yield return StartCoroutine(UIMainControl.WaitForRealSeconds(.1f));
			//yield return new WaitForSeconds(0.1f);
			StartCoroutine(ScaleRectTRoutine(item.rectT, .25f, scaleZoomed, scale));
			
			yield return StartCoroutine(UIMainControl.WaitForRealSeconds(.8f));
			//yield return new WaitForSeconds(0.8f);
			
			UIMainControl.FadeOut(item.canvasG, 1.0f, item.rootObj);
		}
		
		
		IEnumerator ScaleRectTRoutine(RectTransform rectt, float dur, Vector3 startS, Vector3 endS){
			float timeMul=1f/dur;
			float duration=0;
			while(duration<1){
				rectt.localScale=Vector3.Lerp(startS, endS, duration);
				duration+=Time.unscaledDeltaTime*timeMul;
				yield return null;
			}
		}
		
		
		private int GetUnusedTextIndex(){
			for(int i=0; i<msgList.Count; i++){
				if(msgList[i].rootObj.activeInHierarchy) continue;
				return i;
			}
			
			msgList.Add(UIMsgItem.Clone(messageObj, "TextMessage"+(msgList.Count+1)));
			return msgList.Count-1;
		}
	}

}