using UnityEngine;
using UnityEngine.UI;

using System.Collections;

using TBTK;

namespace TBTK {

	public class UIInputOverlay : MonoBehaviour {
		
		public Transform indicator;
		
		private GameObject thisObj;
		private RectTransform rectT;
		private CanvasGroup canvasGroup;
		private static UIInputOverlay instance;
		
		public void Awake(){
			instance=this;
			thisObj=gameObject;
			rectT=thisObj.GetComponent<RectTransform>();
			canvasGroup=thisObj.GetComponent<CanvasGroup>();
			if(canvasGroup==null) canvasGroup=thisObj.AddComponent<CanvasGroup>();
			
			//canvasGroup.alpha=0;
			canvasGroup.interactable=false;
			canvasGroup.blocksRaycasts=false;
			
			tooltipRectT=tooltipObj.GetComponent<RectTransform>();
			
			tooltipObj.SetActive(false);
			
			
			indicator=(Transform)Instantiate(indicator);
			indicator.parent=transform;
			indicator.gameObject.SetActive(false);
			
			
			//thisObj.SetActive(false);
			rectT.anchoredPosition=new Vector3(0, 0, 0);
		}
		
		
		public GameObject tooltipObj;
		private RectTransform tooltipRectT;
		
		public GameObject tooltipPanelBodyObj;
		
		public Text lbAction;
		public Text lbAPCost;
		public Text lbDamage;
		public Text lbChance;
		
		private Tile lastHoveredTile;
		
		// Update is called once per frame
		void Update () {
			if(lastHoveredTile!=null) UpdateTooltipPos();
			
			if(UIMainControl.InTouchMode()) return;
			
			if(UI.IsCursorOnUI(-1)){
				if(tooltipObj.activeInHierarchy) _SetNewHoveredTile(null);
				return;
			}
				
			Tile tile=GridManager.GetHoveredTile();
			if(tile!=lastHoveredTile){
				_SetNewHoveredTile(tile);
			}
		}
		
		
		public static void SetNewHoveredTile(Tile tile){ instance._SetNewHoveredTile(tile); }
		public void _SetNewHoveredTile(Tile tile){
			lastHoveredTile=tile;
			if(lastHoveredTile!=null){
				tooltipObj.SetActive(UpdateDisplay());
			}
			else{
				tooltipObj.SetActive(false);
				indicator.gameObject.SetActive(false);
			}
		}
		
		
		bool UpdateDisplay(){
			if(GridManager.CanMoveToTile(lastHoveredTile)){
				indicator.position=lastHoveredTile.GetPos()+new Vector3(0, 0.05f, 0);
				indicator.gameObject.SetActive(true);
			}
			else indicator.gameObject.SetActive(false);
			
			if(GridManager.CanMoveToTile(lastHoveredTile) && GameControl.UseAPForMove()){
				verticalOffset=-65;
				tooltipPanelBodyObj.SetActive(false);
				
				UpdateTooltipPos();
				
				lbAction.text="Move";
				lbAPCost.text=GameControl.GetSelectedUnit().GetMoveAPCostToTile(lastHoveredTile)+"AP";
				
				return true;
			}
			
			if(GridManager.CanAttackTile(lastHoveredTile)){
				verticalOffset=0;
				tooltipPanelBodyObj.SetActive(true);
				
				UpdateTooltipPos();
				
				Unit selectedUnit=GameControl.GetSelectedUnit();
				
				lbAction.text="Attack";
				
				if(GameControl.UseAPForAttack()) lbAPCost.text="";
				else lbAPCost.text=selectedUnit.GetAttackAPCost().ToString("f0")+"AP";
				
				AttackInstance attInstance=selectedUnit.GetAttackInfo(lastHoveredTile.unit);
				
				if(!attInstance.isMelee)
					lbDamage.text=selectedUnit.GetDamageMin()+"-"+selectedUnit.GetDamageMax();
				else
					lbDamage.text=selectedUnit.GetDamageMinMelee()+"-"+selectedUnit.GetDamageMaxMelee();
				
				lbChance.text=(attInstance.hitChance*100).ToString("f0")+"%\n";
				lbChance.text+=(attInstance.critChance*100).ToString("f0")+"%\n";
				
				return true;
			}
			
			return false;
		}
		
		
		public Vector2 offset;
		
		private float verticalOffset=65;
		void UpdateTooltipPos(){
			Vector3 screenPos=Camera.main.WorldToScreenPoint(lastHoveredTile.GetPos());
			tooltipRectT.localPosition=(screenPos+new Vector3(offset.x, offset.y+verticalOffset, 0))*UIMainControl.GetScaleFactor();
		}
		
	}

}