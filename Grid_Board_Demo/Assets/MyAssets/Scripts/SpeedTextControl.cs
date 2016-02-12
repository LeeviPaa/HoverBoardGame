using UnityEngine;
using System.Collections;

public class SpeedTextControl : MonoBehaviour {
    GameObject player;
    float speed;
    UnityEngine.UI.Text speedT;

	// Use this for initialization
	void Start () 
    {
        player = GameObject.Find("Player");
        speedT = GetComponent<UnityEngine.UI.Text>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        speed = player.GetComponent<ThirdPersonController>().getspeed();
        speedT.text = speed.ToString() + " km/h";
	}
}
