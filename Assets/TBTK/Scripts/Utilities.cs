using UnityEngine;
using System.Collections;

using TBTK;

namespace TBTK{

	public class Utilities {//: MonoBehaviour {
		
		public static float VectorToAngle60(Vector2 dir){
			float angle=Vector2ToAngle(dir);
			return RoundAngleTo60(angle);
		}
		public static float VectorToAngle90(Vector2 dir){
			float angle=Vector2ToAngle(dir);
			return RoundAngleTo90(angle);
		}
		
		
		public static float RoundAngleTo60(float angle){
			//~ if(angle>=30 && angle<90) angle=60;
			//~ else if(angle>=90 && angle<150) angle=120;
			//~ else if(angle>=150 && angle<210) angle=180;
			//~ else if(angle>=210 && angle<270) angle=240;
			//~ else if(angle>=270 && angle<330) angle=300;
			//~ else angle=0;
			
			if(angle>=0 && angle<60) angle=30;
			else if(angle>=60 && angle<120) angle=90;
			else if(angle>=120 && angle<180) angle=150;
			else if(angle>=180 && angle<240) angle=210;
			else if(angle>=240 && angle<300) angle=270;
			else angle=330;
			
			return angle;
		}
		public static float RoundAngleTo90(float angle){
			if(angle>=45 && angle<135) angle=90;
			else if(angle>=135 && angle<225) angle=180;
			else if(angle>=225 && angle<315) angle=270;
			else angle=0;
			
			return angle;
		}
		
		
		//converting vector to angle
		//public static float VectorToAngle(Vector3 dir){ return VectorToAngle(new Vector3(dir.x, dir.z)); }
		public static float Vector2ToAngle(Vector2 dir){
			/*if(dir.x==0){
				if(dir.y>0) return 90;
				else if(dir.y<0) return 270;
				else return 0;
			}
			else if(dir.y==0){
				if(dir.x>0) return 0;
				else if(dir.x<0) return 180;
				else return 0;
			}*/
			
			float h=Mathf.Sqrt(dir.x*dir.x+dir.y*dir.y);
			float angle=Mathf.Asin(dir.y/h)*Mathf.Rad2Deg;
			
			if(dir.y>0){
				if(dir.x<0)  angle=180-angle;
			}
			else{
				if(dir.x>0)  angle=360+angle;
				if(dir.x<0)  angle=180-angle;
			}
			
			if(angle==360) angle=0;
			
			while(angle>360) angle-=360;
			while(angle<0) angle+=360;
			
			return angle;
		}
		
		
		
		//from unit utility
		public static Vector3 GetWorldScale(Transform transform){
			Vector3 worldScale = transform.localScale;
			Transform parent = transform.parent;
			
			while (parent != null){
				worldScale = Vector3.Scale(worldScale,parent.localScale);
				parent = parent.parent;
			}
			
			return worldScale;
		}
		
		public static void SetLayerRecursively(Transform root, int layer){
			foreach(Transform child in root) {
				child.gameObject.layer=layer;
				SetLayerRecursively(child, layer);
			}
		}
		
	}

}
