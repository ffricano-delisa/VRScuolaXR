using UnityEngine;

public static class AlignToObjectCenter
{

    public static void Align(GameObject currentObject, GameObject referenceObject)
    {
        if (referenceObject != null)
        {
            // Ottieni il centro dell'oggetto di riferimento
            Vector3 referenceCenter = GetObjectCenter(referenceObject);

            // Ottieni il centro dell'oggetto corrente
            Vector3 currentCenter = GetObjectCenter(currentObject);

            // Calcola la differenza tra il centro dell'oggetto corrente e quello di riferimento
            Vector3 offset = referenceCenter - currentCenter;

            // Trasla l'oggetto corrente in modo che il suo centro coincida con il centro dell'oggetto di riferimento
            currentObject.transform.position += offset;
        }
        else
        {
            Debug.LogWarning("Reference object is not assigned.");
        }
    }

    // Metodo per ottenere il centro di un oggetto
    private static Vector3 GetObjectCenter(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            return obj.transform.position;
        }

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        return bounds.center;
    }
}
