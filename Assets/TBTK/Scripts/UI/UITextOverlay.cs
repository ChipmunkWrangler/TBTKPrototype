using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class UITextOverlay : MonoBehaviour {
		
		public List<Text> textOverlayList=new List<Text>();
		
		private Color defaultColor=default(Color);
		private int defaultFontSize=16;
		
		public static UITextOverlay instance;
		
		void Start(){
			instance=this;
			
			for(int i=0; i<20; i++){
				if(i>0){
					GameObject obj=UI.Clone(textOverlayList[0].gameObject, "Text "+i);
					textOverlayList.Add(obj.GetComponent<Text>());
				}
				textOverlayList[i].text="";
				textOverlayList[i].gameObject.SetActive(false);
			}
			
			durationMultiplier=1/textDuration;
			
			if(textOverlayList.Count>0){
				defaultColor=textOverlayList[0].color;
				defaultFontSize=textOverlayList[0].fontSize;
			}
		}
		
		
		void OnEnable(){
			TextOverlay.onTextOverlayE += NewTextOverlay;
		}
		void OnDisable(){
			TextOverlay.onTextOverlayE -= NewTextOverlay;
		}
		
		
		public float textDuration=0.5f;
		private float durationMultiplier;
		
		void NewTextOverlay(TextOverlay overlayInstance){
			if(!UIMainControl.EnableTextOverlay()) return;
			
			Text txt=GetUnusedTextOverlay();
			
			txt.text=overlayInstance.msg;
			if(overlayInstance.useColor) txt.color=overlayInstance.color;
			else txt.color=defaultColor;
			
			txt.fontSize=(int)Mathf.Round(defaultFontSize*overlayInstance.scale);
			
			txt.transform.localPosition=GetScreenPos(overlayInstance.pos);
			txt.gameObject.SetActive(true);
			
			StartCoroutine(TextOverlayRoutine(overlayInstance.pos, txt));
		}
		IEnumerator TextOverlayRoutine(Vector3 pos, Text txt){
			StartCoroutine(ZoomEffect(txt.transform));
			
			Transform txtT=txt.transform;
			Vector3 movedPos=Vector3.zero;
			float duration=0;
			
			while(duration<1){
				Vector3 screenPos=GetScreenPos(pos);
				movedPos+=new Vector3(0, 7.5f*Time.deltaTime, 0);
				txtT.localPosition=screenPos+movedPos;
				
				Color color=txt.color;
				color.a=duration>0.5f ? (1-duration)*2 : 1;
				txt.color=color;
				
				duration+=Time.deltaTime*durationMultiplier;
				yield return null;
			}
			
			txt.text="";
			txt.fontSize=defaultFontSize;
			txt.gameObject.SetActive(false);
		}
		IEnumerator ZoomEffect(Transform textT){
			float duration=0;
			float defaultScale=textT.localScale.x;
			while(duration<1){
				float scale=Mathf.Lerp(1f, 1.5f, duration);
				textT.localScale=new Vector3(scale, scale, scale)*defaultScale;
				duration+=Time.deltaTime*durationMultiplier*6;
				yield return null;
			}
			duration=0;
			while(duration<1){
				float scale=Mathf.Lerp(1.5f, 1f, duration);
				textT.localScale=new Vector3(scale, scale, scale)*defaultScale;
				duration+=Time.deltaTime*durationMultiplier*4;
				yield return null;
			}
			
			textT.localScale=new Vector3(1, 1, 1)*defaultScale;
		}
		
		
		
		public Vector3 GetScreenPos(Vector3 worldPos){
			Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos)*UIMainControl.GetScaleFactor(); 
			screenPos.z=0;
			return screenPos;
		}
		
		
		private Text GetUnusedTextOverlay(){
			for(int i=0; i<textOverlayList.Count; i++){
				if(textOverlayList[i].text=="") return textOverlayList[i];
			}
			
			int count=textOverlayList.Count;
			GameObject obj=UI.Clone(textOverlayList[0].gameObject, "Text "+textOverlayList.Count);
			textOverlayList.Add(obj.GetComponent<Text>());
			
			textOverlayList[count].text="";
			textOverlayList[count].gameObject.SetActive(false);
			
			return textOverlayList[count];
		}
		
		
	}
	


	public class TextOverlay{
		public delegate void TextOverlayHandler(TextOverlay textO); 
		public static event TextOverlayHandler onTextOverlayE;

		public Vector3 pos;
		public string msg;
		public float scale;
		public Color color;
		public bool useColor=false;
		
		public TextOverlay(Vector3 p, string m, float s=1){
			pos=p+GetScatterPos();
			msg=m;
			scale=s;
			if(onTextOverlayE!=null) onTextOverlayE(this);
		}
		public TextOverlay(Vector3 p, string m, Color col, float s=1){
			pos=p+GetScatterPos();
			msg=m;
			color=col;
			scale=s;
			useColor=true;
			
			if(onTextOverlayE!=null) onTextOverlayE(this);
		}
		
		public Vector3 GetScatterPos(){
			float rand=.75f;
			return new Vector3(Random.Range(-rand, rand), Random.Range(-rand, rand), Random.Range(-rand, rand));
		}
	}

}