using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportOggetto : MonoBehaviour
{
    public GameObject oggettoToTeleport;
    public bool useCustomVaulesInstead = false; //use custom position and rotation
    public Vector3 coordinate;

    public bool applicaRotazione;

    public Vector3 rotazione;

    public void muoviOggetto()
    {
        if (useCustomVaulesInstead) oggettoToTeleport.transform.position = new Vector3(coordinate.x, coordinate.y, coordinate.z);
        else oggettoToTeleport.transform.position = gameObject.transform.position;

        if (applicaRotazione)
        {
            if (useCustomVaulesInstead) oggettoToTeleport.transform.eulerAngles = new Vector3(rotazione.x, rotazione.y, rotazione.z);
            else oggettoToTeleport.transform.eulerAngles = gameObject.transform.rotation.eulerAngles;
        }
    }


}