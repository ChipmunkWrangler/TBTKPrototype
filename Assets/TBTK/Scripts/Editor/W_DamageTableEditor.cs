using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class DamageTableEditorWindow : TBEditorWindow {

		private static DamageTableEditorWindow window;
		
	   public static void Init () {
			// Get existing open window or if none, make a new one:
			window = (DamageTableEditorWindow)EditorWindow.GetWindow(typeof (DamageTableEditorWindow));
			window.minSize=new Vector2(470, 300);
			//~ window.maxSize=new Vector2(471, 301);
			
			LoadDB();
		}
		
		
		
		private enum _Tab{Armor, Damage}
		private _Tab tab=_Tab.Armor;
		bool delete=false;

		public override bool OnGUI () {
			if(!base.OnGUI()) return true;
			
			if(window==null) Init();
			
			int startX=0;
			int startY=0;
			
			if(GUI.Button(new Rect(10, 10, 100, 30), "New Armor")) {
				ArmorType armorType=new ArmorType();
				armorType.name="New Armor";
				damageTableDB.armorTypeList.Add(armorType);
				UpdateLabel_DamageTable();
			}
			if(GUI.Button(new Rect(120, 10, 100, 30), "New Damage")) {
				DamageType damageType=new DamageType();
				damageType.name="New Damage";
				damageTableDB.damageTypeList.Add(damageType);
				UpdateLabel_DamageTable();
			}
			
			
			List<DamageType> damageTypeList=damageTableDB.damageTypeList;
			List<ArmorType> armorTypeList=damageTableDB.armorTypeList;
			
			
			Rect visibleRect=new Rect(10, 50, window.position.width-20, 185);
			Rect contentRect=new Rect(10, 50, 118+damageTypeList.Count*105, 5+(armorTypeList.Count+1)*25);
			
			GUI.Box(visibleRect, "");
			scrollPos = GUI.BeginScrollView(visibleRect, scrollPos, contentRect);
			
				startY=60;
				startX=20;
				for(int i=0; i<damageTypeList.Count; i++){
					DamageType dmgType=damageTypeList[i];
					if(selectID==i && tab==_Tab.Damage) GUI.color=new Color(0, 1, 1, 1);
					if(GUI.Button(new Rect(startX+=105, startY, 100, 20), dmgType.name)){
						selectID=i; 	tab=_Tab.Damage;
						delete=false;
						GUI.FocusControl ("");
					}
					GUI.color=Color.white;
				}
				
				
				
				startY=60;
				for(int i=0; i<armorTypeList.Count; i++){
					startX=20;
					
					ArmorType armorType=armorTypeList[i];
					if(selectID==i && tab==_Tab.Armor) GUI.color=new Color(0, 1, 1, 1);
					if(GUI.Button(new Rect(startX, startY+=25, 100, 20), armorType.name)){
						selectID=i; 	tab=_Tab.Armor;
						delete=false;
						GUI.FocusControl ("");
					}
					GUI.color=Color.white;
					
					if(armorType.modifiers.Count!=damageTypeList.Count){
						while(armorType.modifiers.Count<damageTypeList.Count) armorType.modifiers.Add(1);
						while(armorType.modifiers.Count>damageTypeList.Count) armorType.modifiers.RemoveAt(armorType.modifiers.Count-1);
						SetDirtyTB();
					}
					
					startX+=110;
					for(int j=0; j<damageTypeList.Count; j++){
						armorType.modifiers[j]=EditorGUI.FloatField(new Rect(startX, startY, 90, 20), armorType.modifiers[j]);
						startX+=105;
					}
				}
				
			
			
			GUI.EndScrollView();
			
			
			
			
			
			startX=10;
			startY=250;
			
			
			
			if(selectID>=0){
				DAType daInstance=null;
				if(tab==_Tab.Damage){
					selectID=Mathf.Min(selectID, damageTypeList.Count-1);
					if(selectID<=-1) return true;
					daInstance=damageTypeList[selectID];
				}
				if(tab==_Tab.Armor){
					selectID=Mathf.Min(selectID, armorTypeList.Count-1);
					if(selectID<=-1) return true;
					daInstance=armorTypeList[selectID];
				}
			
				GUI.Label(new Rect(startX, startY, 200, 17), "Name:");
				daInstance.name=EditorGUI.TextField(new Rect(startX+80, startY, 150, 17), daInstance.name);
				
				
				GUIStyle styleL=new GUIStyle(GUI.skin.textArea);
						styleL.wordWrap=true;
						styleL.clipping=TextClipping.Clip;
						styleL.alignment=TextAnchor.UpperLeft;
				EditorGUI.LabelField(new Rect(startX, startY+=25, 150, 17), "Description: ");
				daInstance.desp=EditorGUI.TextArea(new Rect(startX, startY+=17, window.position.width-20, 50), daInstance.desp, styleL);
				
				
				string label="";
				if(tab==_Tab.Damage) {
					for(int i=0; i<armorTypeList.Count; i++){
						label+=" - cause "+(armorTypeList[i].modifiers[selectID]*100).ToString("f0")+"% damage to "+armorTypeList[i].name+"\n";
					}
				}
				if(tab==_Tab.Armor){
					for(int i=0; i<damageTypeList.Count; i++){
						label+=" - take "+(armorTypeList[selectID].modifiers[i]*100).ToString("f0")+"% damage from "+damageTypeList[i].name+"\n";
					}
				}
				GUI.Label(new Rect(startX, startY+=60, window.position.width-20, 100), label);
				
				
				startX=300;
				startY=250;
				if(!delete){
					if(GUI.Button(new Rect(startX, startY, 80, 20), "delete")) delete=true;
				}
				else if(delete){
					if(GUI.Button(new Rect(startX, startY, 80, 20), "cancel")) delete=false;
					GUI.color=Color.red;
					if(GUI.Button(new Rect(startX+=90, startY, 80, 20), "confirm")){
						if(tab==_Tab.Damage){
							//damageTableDB.RemoveDamageType(selectID);
							damageTypeList.RemoveAt(selectID);
						}
						if(tab==_Tab.Armor){
							//damageTableDB.RemoveArmorType(selectID);
							armorTypeList.RemoveAt(selectID);
						}
						UpdateLabel_DamageTable();
						selectID=-1;
					}
					GUI.color=Color.white;
				}
			}
			
			
			
			if(GUI.changed) SetDirtyTB();
			
			
			return true;
		}

	}


}
