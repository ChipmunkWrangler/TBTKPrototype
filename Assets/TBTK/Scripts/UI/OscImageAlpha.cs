using UnityEngine;
using UnityEngine.UI;

using System.Collections;

public class OscImageAlpha : MonoBehaviour {

	public float freq=1.5f;
	
	public float max=0.5f;
	public float min=0f;
	
	private Image img;
	private Color color;
	
	void Awake(){
		img=gameObject.GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
		color=img.color;
		
		float alpha=min+(max-min)*Mathf.Abs(Mathf.Sin(freq*Time.time*Mathf.PI));///Mathf.PI));
		
		img.color=new Color(color.r, color.g, color.b, alpha);
	}
	
}
