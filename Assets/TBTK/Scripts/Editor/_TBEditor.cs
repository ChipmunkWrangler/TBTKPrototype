using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK {

	public class TBEditor {
		
		public static bool dirty=false; 	//a cache which the value is changed whenever something in the editor changed (name, icon, etc)
													//to let other custom editor not in focus to repaint
		
		
		public static int GetUnitIndex(int ID){
			for(int i=0; i<unitDB.unitList.Count; i++){
				if(unitDB.unitList[i].prefabID==ID) return (i+1);
			}
			return -1;
		}
		public static int GetUnitAbilityIndex(int ID){
			for(int i=0; i<uAbilityDB.abilityList.Count; i++){
				if(uAbilityDB.abilityList[i].prefabID==ID) return (i+1);
			}
			return -1;
		}
		public static int GetFactionAbilityIndex(int ID){
			for(int i=0; i<fAbilityDB.abilityList.Count; i++){
				if(fAbilityDB.abilityList[i].prefabID==ID) return (i+1);
			}
			return -1;
		}
		public static int GetPerkIndex(int ID){
			for(int i=0; i<perkDB.perkList.Count; i++){
				if(perkDB.perkList[i].prefabID==ID) return (i+1);
			}
			return -1;
		}
		public static int GetCollectibleIndex(int ID){
			for(int i=0; i<collectibleDB.collectibleList.Count; i++){
				if(collectibleDB.collectibleList[i].prefabID==ID) return (i+1);
			}
			return -1;
		}
		
		
		public static bool ExistInDB(Unit unit){ return unitDB.unitList.Contains(unit); }
		public static bool ExistInDB(Collectible collectible){ return collectibleDB.collectibleList.Contains(collectible); }
		

		protected static DamageTableDB damageTableDB;
		protected static string[] damageTypeLabel;
		protected static string[] armorTypeLabel;
		public static void LoadDamageTable(){ 
			damageTableDB=DamageTableDB.LoadDB();
			UpdateLabel_DamageTable();
			
			TBEditorWindow.SetDamageDB(damageTableDB, damageTypeLabel, armorTypeLabel);
			TBEditorInspector.SetDamageDB(damageTableDB, damageTypeLabel, armorTypeLabel);
		}
		public static void UpdateLabel_DamageTable(){
			damageTypeLabel=new string[damageTableDB.damageTypeList.Count];
			for(int i=0; i<damageTableDB.damageTypeList.Count; i++){ 
				damageTypeLabel[i]=damageTableDB.damageTypeList[i].name;
				if(damageTypeLabel[i]=="") damageTypeLabel[i]="unnamed";
			}
			
			armorTypeLabel=new string[damageTableDB.armorTypeList.Count];
			for(int i=0; i<damageTableDB.armorTypeList.Count; i++){
				armorTypeLabel[i]=damageTableDB.armorTypeList[i].name;
				if(armorTypeLabel[i]=="") armorTypeLabel[i]="unnamed";
			}
			
			TBEditorWindow.SetDamageDB(damageTableDB, damageTypeLabel, armorTypeLabel);
			TBEditorInspector.SetDamageDB(damageTableDB, damageTypeLabel, armorTypeLabel);
			
			dirty=!dirty;
		}
		
		
		
		protected static UnitDB unitDB;
		protected static List<int> unitIDList=new List<int>();
		protected static string[] unitLabel;
		public static void LoadUnit(){
			unitDB=UnitDB.LoadDB();
			
			for(int i=0; i<unitDB.unitList.Count; i++){
				if(unitDB.unitList[i]!=null) unitIDList.Add(unitDB.unitList[i].prefabID);
				else{ unitDB.unitList.RemoveAt(i);	i-=1; }
			}
			
			UpdateLabel_Unit();
			
			TBEditorWindow.SetUnitDB(unitDB, unitIDList, unitLabel);
			TBEditorInspector.SetUnitDB(unitDB, unitIDList, unitLabel);
		}
		public static void UpdateLabel_Unit(){
			unitLabel=new string[unitDB.unitList.Count+1];
			unitLabel[0]="Unassigned";
			for(int i=0; i<unitDB.unitList.Count; i++){
				string name=unitDB.unitList[i].unitName;
				if(name=="") name="unnamed";
				while(Array.IndexOf(unitLabel, name)>=0) name+="_";
				unitLabel[i+1]=name;
			}
			
			TBEditorWindow.SetUnitDB(unitDB, unitIDList, unitLabel);
			TBEditorInspector.SetUnitDB(unitDB, unitIDList, unitLabel);
			
			dirty=!dirty;
		}
		
		
		
		protected static UnitAbilityDB uAbilityDB;
		protected static List<int> uAbilityIDList=new List<int>();
		protected static string[] uAbilityLabel;
		public static void LoadUnitAbility(){
			uAbilityDB=UnitAbilityDB.LoadDB();
			
			for(int i=0; i<uAbilityDB.abilityList.Count; i++){
				if(uAbilityDB.abilityList[i]!=null) uAbilityIDList.Add(uAbilityDB.abilityList[i].prefabID);
				else{ uAbilityDB.abilityList.RemoveAt(i);	i-=1; }
			}
			
			UpdateLabel_UnitAbility();
			
			TBEditorWindow.SetAbilityDB(uAbilityDB, uAbilityIDList, uAbilityLabel);
			TBEditorInspector.SetAbilityDB(uAbilityDB, uAbilityIDList, uAbilityLabel);
		}
		public static void UpdateLabel_UnitAbility(){
			uAbilityLabel=new string[uAbilityDB.abilityList.Count+1];
			uAbilityLabel[0]="Unassigned";
			for(int i=0; i<uAbilityDB.abilityList.Count; i++){
				string name=uAbilityDB.abilityList[i].name;
				if(name=="") name="unnamed";
				while(Array.IndexOf(uAbilityLabel, name)>=0) name+="_";
				uAbilityLabel[i+1]=name;
			}
			
			TBEditorWindow.SetAbilityDB(uAbilityDB, uAbilityIDList, uAbilityLabel);
			TBEditorInspector.SetAbilityDB(uAbilityDB, uAbilityIDList, uAbilityLabel);
			
			dirty=!dirty;
		}
		
		
		
		protected static FactionAbilityDB fAbilityDB;
		protected static List<int> fAbilityIDList=new List<int>();
		protected static string[] fAbilityLabel;
		public static void LoadFactionAbility(){
			fAbilityDB=FactionAbilityDB.LoadDB();
			
			for(int i=0; i<fAbilityDB.abilityList.Count; i++){
				if(fAbilityDB.abilityList[i]!=null) fAbilityIDList.Add(fAbilityDB.abilityList[i].prefabID);
				else{ fAbilityDB.abilityList.RemoveAt(i);	i-=1; }
			}
			
			UpdateLabel_FactionAbility();
			
			TBEditorWindow.SetAbilityDB(fAbilityDB, fAbilityIDList, fAbilityLabel);
			TBEditorInspector.SetAbilityDB(fAbilityDB, fAbilityIDList, fAbilityLabel);
		}
		public static void UpdateLabel_FactionAbility(){
			fAbilityLabel=new string[fAbilityDB.abilityList.Count+1];
			fAbilityLabel[0]="Unassigned";
			for(int i=0; i<fAbilityDB.abilityList.Count; i++){
				string name=fAbilityDB.abilityList[i].name;
				if(name=="") name="unnamed";
				while(Array.IndexOf(fAbilityLabel, name)>=0) name+="_";
				fAbilityLabel[i+1]=name;
			}
			
			TBEditorWindow.SetAbilityDB(fAbilityDB, fAbilityIDList, fAbilityLabel);
			TBEditorInspector.SetAbilityDB(fAbilityDB, fAbilityIDList, fAbilityLabel);
			
			dirty=!dirty;
		}
		
		
		
		protected static PerkDB perkDB;
		protected static List<int> perkIDList=new List<int>();
		protected static string[] perkLabel;
		public static void LoadPerk(){
			perkDB=PerkDB.LoadDB();
			
			for(int i=0; i<perkDB.perkList.Count; i++){
				if(perkDB.perkList[i]!=null) perkIDList.Add(perkDB.perkList[i].prefabID);
				else{ perkDB.perkList.RemoveAt(i);	i-=1; }
			}
			
			UpdateLabel_Perk();
			
			TBEditorWindow.SetPerkDB(perkDB, perkIDList, perkLabel);
			TBEditorInspector.SetPerkDB(perkDB, perkIDList, perkLabel);
		}
		public static void UpdateLabel_Perk(){
			perkLabel=new string[perkDB.perkList.Count+1];
			perkLabel[0]="Unassigned";
			for(int i=0; i<perkDB.perkList.Count; i++){
				string name=perkDB.perkList[i].name;
				if(name=="") name="unnamed";
				while(Array.IndexOf(perkLabel, name)>=0) name+="_";
				perkLabel[i+1]=name;
			}
			
			TBEditorWindow.SetPerkDB(perkDB, perkIDList, perkLabel);
			TBEditorInspector.SetPerkDB(perkDB, perkIDList, perkLabel);
			
			dirty=!dirty;
		}
		
		
		
		protected static CollectibleDB collectibleDB;
		protected static List<int> collectibleIDList=new List<int>();
		protected static string[] collectibleLabel;
		public static void LoadCollectible(){
			collectibleDB=CollectibleDB.LoadDB();
			
			for(int i=0; i<collectibleDB.collectibleList.Count; i++){
				if(collectibleDB.collectibleList[i]!=null) collectibleIDList.Add(collectibleDB.collectibleList[i].prefabID);
				else{ collectibleDB.collectibleList.RemoveAt(i);	i-=1; }
			}
			
			UpdateLabel_Collectible();
			
			TBEditorWindow.SetCollectibleDB(collectibleDB, collectibleIDList, collectibleLabel);
			TBEditorInspector.SetCollectibleDB(collectibleDB, collectibleIDList, collectibleLabel);
		}
		public static void UpdateLabel_Collectible(){
			collectibleLabel=new string[collectibleDB.collectibleList.Count+1];
			collectibleLabel[0]="Unassigned";
			for(int i=0; i<collectibleDB.collectibleList.Count; i++){
				string name=collectibleDB.collectibleList[i].itemName;
				if(name=="") name="unnamed";
				while(Array.IndexOf(collectibleLabel, name)>=0) name+="_";
				collectibleLabel[i+1]=name;
			}
			
			TBEditorWindow.SetCollectibleDB(collectibleDB, collectibleIDList, collectibleLabel);
			TBEditorInspector.SetCollectibleDB(collectibleDB, collectibleIDList, collectibleLabel);
			
			dirty=!dirty;
		}
		
		
		
		
		
		
		
		public static bool DrawSprite(Rect rect, Sprite sprite, string tooltip="", bool drawBox=true){
			if(drawBox) GUI.Box(rect, new GUIContent("", tooltip));
			
			if(sprite!=null){
				Texture t = sprite.texture;
				Rect tr = sprite.textureRect;
				Rect r = new Rect(tr.x / t.width, tr.y / t.height, tr.width / t.width, tr.height / t.height );
				
				rect.x+=2;
				rect.y+=2;
				rect.width-=4;
				rect.height-=4;
				GUI.DrawTextureWithTexCoords(rect, t, r);
			}
			
			//if(addXButton){
			//	rect.width=12;	rect.height=12;
			//	bool flag=GUI.Button(rect, "X", GetXButtonStyle());
			//	return flag;
			//}
			
			return false;
		}
		
		
		
		
		public delegate void SetObjListCallback(List<GameObject> objHList, string[] objHLabelList);
	
		public static void GetObjectHierarchyList(GameObject obj, SetObjListCallback callback){
			List<GameObject> objHList=new List<GameObject>();
			List<string> tempLabelList=new List<string>();
			
			HierarchyList hList=GetTransformInHierarchy(obj.transform, 0);
			
			objHList.Add(null);
			tempLabelList.Add(" - ");
			
			for(int i=0; i<hList.ListT.Count; i++){
				objHList.Add(hList.ListT[i].gameObject);
			}
			for(int i=0; i<hList.ListName.Count; i++){
				while(tempLabelList.Contains(hList.ListName[i])) hList.ListName[i]+=".";
				tempLabelList.Add(hList.ListName[i]);
			}
			
			string[] objHLabelList=new string[tempLabelList.Count];
			for(int i=0; i<tempLabelList.Count; i++) objHLabelList[i]=tempLabelList[i];
			
			callback(objHList, objHLabelList);
		}
		
		
		private static HierarchyList GetTransformInHierarchy(Transform transform, int depth){
			HierarchyList hl=new HierarchyList();
			
			hl=GetTransformInHierarchyRecursively(transform, depth);
			
			hl.ListT.Insert(0, transform);
			hl.ListName.Insert(0, "-"+transform.name);
			
			return hl;
		}
		private static HierarchyList GetTransformInHierarchyRecursively(Transform transform, int depth){
			HierarchyList hList=new HierarchyList();
			depth+=1;
			foreach(Transform t in transform){
				string label="";
				for(int i=0; i<depth; i++) label+="   ";
				
				hList.ListT.Add(t);
				hList.ListName.Add(label+"-"+t.name);
				
				HierarchyList tempHL=GetTransformInHierarchyRecursively(t, depth);
				foreach(Transform tt in tempHL.ListT){
					hList.ListT.Add(tt);
				}
				foreach(string ll in tempHL.ListName){
					hList.ListName.Add(ll);
				}
			}
			return hList;
		}
		
		private class HierarchyList{
			public List<Transform> ListT=new List<Transform>();
			public List<string> ListName=new List<string>();
		}
	}

}