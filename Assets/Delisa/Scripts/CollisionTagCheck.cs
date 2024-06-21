using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionTagCheck : MonoBehaviour
{
    public string TagString;
    public UnityEvent customEnterEvent;
    public UnityEvent customExitEvent;

    private void OnTriggerEnter(Collider other) //OnTriggerEnter(Collider other) funziona per i collider segnati come "trigger". Per i collider con il checkbox "collider" vuoto, usiamo OnCollisionEnter(Collision other)
    {
        Debug.Log(this.gameObject.name + "Collided with: " + other.gameObject.name);
        if (other.gameObject.tag == TagString) customEnterEvent.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log(this.gameObject.name + "Collided with: " + other.gameObject.name);
        if (other.gameObject.tag == TagString) customExitEvent.Invoke();
    }
}
