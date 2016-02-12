using UnityEngine;
using System.Collections;

public class TransformTrigger1 : MonoBehaviour 
{
    public Transform toPos;

    void OnTriggerEnter(Collider other)
    {
        other.GetComponent<ThirdPersonController>().setpos(toPos);
    }
}
