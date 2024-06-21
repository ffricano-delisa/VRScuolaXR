using UnityEngine;

public class ToggleActivation : MonoBehaviour
{
    public GameObject targetObject;

   public void ToggleGameObject()
    {
        if (targetObject != null && !targetObject.activeSelf)       
            targetObject.SetActive(true);
            
        else if (targetObject != null && targetObject.activeSelf)    
            targetObject.SetActive(false);
        
    }
}
