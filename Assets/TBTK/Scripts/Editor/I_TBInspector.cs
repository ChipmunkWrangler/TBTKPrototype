using UnityEngine;
using UnityEditor;

using System.Collections;
using System.Collections.Generic;

using TBTK;

namespace TBTK{
	
	public class TBEditorInspector : Editor {

		protected static bool styleDefined=false;
		protected static GUIStyle headerStyle;
		protected static GUIStyle foldoutStyle;
		protected static GUIStyle conflictStyle;
		protected static GUIStyle toggleHeaderStyle;
		
		protected GUIContent cont;
		protected GUIContent contN=GUIContent.none;
		protected GUIContent[] contL;
		
		
		public override void OnInspectorGUI(){
			DefineStyle();
		}
		
		
		protected static bool showDefaultEditor=false;
		protected void DefaultInspector(){
			EditorGUILayout.Space();
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("", GUILayout.MaxWidth(10));
			showDefaultEditor=EditorGUILayout.Foldout(showDefaultEditor, "Show default editor", foldoutStyle);
			EditorGUILayout.EndHorizontal();
			if(showDefaultEditor) DrawDefaultInspector();
			
			EditorGUILayout.Space();
		}
		
		
		protected static void DefineStyle(){
			if(styleDefined) return;
			styleDefined=true;
			
			headerStyle=new GUIStyle("Label");
			headerStyle.fontStyle=FontStyle.Bold;
			headerStyle.normal.textColor = Color.black;
			
			toggleHeaderStyle=new GUIStyle("Toggle");
			toggleHeaderStyle.fontStyle=FontStyle.Bold;
			//toggleHeaderStyle.normal.textColor = Color.black;
			
			foldoutStyle=new GUIStyle("foldout");
			foldoutStyle.fontStyle=FontStyle.Bold;
			foldoutStyle.normal.textColor = Color.black;
			
			conflictStyle=new GUIStyle("Label");
			conflictStyle.normal.textColor = Color.red;
		}
		
		
		private static bool loaded=false;
		protected static void LoadDB(){
			if(loaded) return;
			loaded=true;
			
			LoadDamageTable();
			
			LoadUnit();
			LoadCollectible();
			
			LoadUnitAbility();
			LoadFactionAbility();
			
			LoadPerk();
		}
		
		
		
		protected static DamageTableDB damageTableDB;
		protected static string[] damageTypeLabel;
		protected static string[] armorTypeLabel;
		protected static void LoadDamageTable(){ TBEditor.LoadDamageTable(); }
		protected static void UpdateLabel_DamageTable(){ TBEditor.UpdateLabel_DamageTable(); }
		public static void SetDamageDB(DamageTableDB db, string[] dmgLabel, string[] armLabel){
			damageTableDB=db;
			damageTypeLabel=dmgLabel;
			armorTypeLabel=armLabel;
		}
		
		protected static UnitDB unitDB;
		protected static List<int> unitIDList=new List<int>();
		protected static string[] unitLabel;
		protected static void LoadUnit(){ TBEditor.LoadUnit(); }
		protected static void UpdateLabel_Unit(){ TBEditor.UpdateLabel_Unit(); }
		public static void SetUnitDB(UnitDB db, List<int> IDList, string[] label){
			unitDB=db;
			unitIDList=IDList;
			unitLabel=label;
		}
		
		protected static UnitAbilityDB uAbilityDB;
		protected static List<int> uAbilityIDList=new List<int>();
		protected static string[] uAbilityLabel;
		protected static void LoadUnitAbility(){ TBEditor.LoadUnitAbility(); }
		protected static void UpdateLabel_UnitAbility(){ TBEditor.UpdateLabel_UnitAbility(); }
		public static void SetAbilityDB(UnitAbilityDB db, List<int> IDList, string[] label){
			uAbilityDB=db;
			uAbilityIDList=IDList;
			uAbilityLabel=label;
		}
		
		protected static FactionAbilityDB fAbilityDB;
		protected static List<int> fAbilityIDList=new List<int>();
		protected static string[] fAbilityLabel;
		protected static void LoadFactionAbility(){ TBEditor.LoadFactionAbility(); }
		protected static void UpdateLabel_FactionAbility(){ TBEditor.UpdateLabel_FactionAbility(); }
		public static void SetAbilityDB(FactionAbilityDB db, List<int> IDList, string[] label){
			fAbilityDB=db;
			fAbilityIDList=IDList;
			fAbilityLabel=label;
		}
		
		protected static PerkDB perkDB;
		protected static List<int> perkIDList=new List<int>();
		protected static string[] perkLabel;
		protected static void LoadPerk(){ TBEditor.LoadPerk(); }
		protected static void UpdateLabel_Perk(){ TBEditor.UpdateLabel_Perk(); }
		public static void SetPerkDB(PerkDB db, List<int> IDList, string[] label){
			perkDB=db;
			perkIDList=IDList;
			perkLabel=label;
		}
		
		protected static CollectibleDB collectibleDB;
		protected static List<int> collectibleIDList=new List<int>();
		protected static string[] collectibleLabel;
		protected static void LoadCollectible(){ TBEditor.LoadCollectible(); }
		protected static void UpdateLabel_Collectible(){ TBEditor.UpdateLabel_Collectible(); }
		public static void SetCollectibleDB(CollectibleDB db, List<int> IDList, string[] label){
			collectibleDB=db;
			collectibleIDList=IDList;
			collectibleLabel=label;
		}
		
		
		
		
		
		
		protected SerializedProperty srlPpt;
	
		protected const float labelWidth=125;
		protected const float fieldWidth=50;
		protected const float fieldWidthL=140;
		protected const float fieldWidthS=10;
		
		protected void PropertyFieldL(SerializedProperty property, GUIContent gcon){ PropertyField(property, gcon, fieldWidthL); }
		protected void PropertyFieldS(SerializedProperty property, GUIContent gcon){ PropertyField(property, gcon, fieldWidthS); }
		protected void PropertyField(SerializedProperty property, GUIContent gcon, float width=fieldWidth){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(gcon, GUILayout.MaxWidth(labelWidth));
			EditorGUILayout.PropertyField(property, contN, GUILayout.MaxWidth(width));
			EditorGUILayout.EndHorizontal();
		}
		protected void InvalidField(GUIContent gcon){
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(gcon, GUILayout.MaxWidth(labelWidth));
			EditorGUILayout.LabelField("-", GUILayout.MaxWidth(fieldWidthS));
			EditorGUILayout.EndHorizontal();
		}
		
		
	}

}