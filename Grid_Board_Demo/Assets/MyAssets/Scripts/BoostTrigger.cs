using UnityEngine;
using System.Collections;

public class BoostTrigger : MonoBehaviour 
{
    public float boostSpeed = 200.0f;
    GameObject player;
    void Start()
    {
        player = GameObject.Find("Player");
        
    }
    void OnTriggerStay(Collider other)
    {
        other.GetComponent<ThirdPersonController>().setspeed(boostSpeed);

    }


}
