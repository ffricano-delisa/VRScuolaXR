using UnityEngine;

public class DeactivateSelfAfterDelay : MonoBehaviour
{
    // Public function to be called externally to deactivate the object after a delay
    public void DeactivateAfterDelay()
    {
        float delay = 0.5f;
        Invoke("DeactivateObject", delay);
    }

    // Private function to deactivate the object
    private void DeactivateObject()
    {
        gameObject.SetActive(false);
    }
}
