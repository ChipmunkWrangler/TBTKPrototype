using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class UIUnitOverlayManager : MonoBehaviour {
		
		public UIUnitOverlay overlayObj;
		
		public Color friendlyHPColor=Color.green;
		public Color friendlyAPColor=Color.red;
		public Color hostileHPColor=Color.white;
		public static Color GetFriendlyHPColor(){ return instance.friendlyHPColor; }
		public static Color GetFriendlyAPColor(){ return instance.friendlyAPColor; }
		public static Color GetHostileHPColor(){ return instance.hostileHPColor; }
		
		private GameObject thisObj;
		//private RectTransform rectT;
		private CanvasGroup canvasGroup;
		private static UIUnitOverlayManager instance;
		public static UIUnitOverlayManager GetInstance(){ return instance; }
		
		void Awake(){
			instance=this;
			thisObj=gameObject;
			//rectT=thisObj.GetComponent<RectTransform>();
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			//rectT.localPosition=new Vector3(0, 0, 0);
		}
		
		
		
		
		void OnEnable(){
			TBTK.onGameStartE += OnGameStart;
			TBTK.onNewUnitE += AddNewUnit;
		}
		void OnDisable(){
			TBTK.onGameStartE -= OnGameStart;
			TBTK.onNewUnitE -= AddNewUnit;
		}
		
		
		void OnGameStart(){
			List<Unit> unitList=FactionManager.GetAllUnit();
			for(int i=0; i<unitList.Count; i++) AddNewUnit(unitList[i]);
		}
		
		void AddNewUnit(Unit unit){
			GameObject obj=UI.Clone(overlayObj.gameObject, "UnitOverlay", Vector3.zero);
			obj.GetComponent<UIUnitOverlay>().SetUnit(unit);
		}
		
	}

}