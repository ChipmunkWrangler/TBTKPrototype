using UnityEngine;
using System.Collections;

public class DemoUILoadOut : MonoBehaviour {
	
	private GameObject thisObj;
	private RectTransform rectT;
	private CanvasGroup canvasGroup;
	private static DemoUILoadOut instance;
	
	public void Awake(){
		instance=this;
		thisObj=gameObject;
		rectT=thisObj.GetComponent<RectTransform>();
		canvasGroup=thisObj.GetComponent<CanvasGroup>();
		if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
		
	}
	
	IEnumerator Start(){
		yield return null;
		yield return null;
		rectT.anchoredPosition=new Vector3(0, 0, 0);
		
	}
	
	
	
	public static void Show(){ instance._Show(); }
	public void _Show(){
		DemoUIMainControl.FadeIn(canvasGroup, 0.25f, thisObj);
	}
	public static void Hide(){ instance._Hide(); }
	public void _Hide(){
		DemoUIMainControl.FadeOut(canvasGroup, 0.25f, thisObj);
	}
	
}
