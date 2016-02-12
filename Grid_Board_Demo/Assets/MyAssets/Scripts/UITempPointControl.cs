using UnityEngine;
using System.Collections;

public class UITempPointControl : MonoBehaviour {

    GameObject player;
    float points;
    UnityEngine.UI.Text speedT;

    // Use this for initialization
    void Start()
    {
        player = GameObject.Find("Player");
        speedT = GetComponent<UnityEngine.UI.Text>();
    }

    // Update is called once per frame
    void Update()
    {
        points = player.GetComponent<ThirdPersonController>().getTempPoints();
        speedT.text = "Trick: " + (points * 10).ToString();
    }
}
