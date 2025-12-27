using UnityEngine;

public class AddCollidersToChildren : MonoBehaviour
{
    void Start()
    {
        foreach (Transform t in transform.GetComponentsInChildren<Transform>())
        {
            if (t.GetComponent<MeshFilter>() != null && 
                t.GetComponent<Collider>() == null)
            {
                t.gameObject.AddComponent<MeshCollider>();
            }
        }

        Debug.Log("Коллайдеры добавлены!");
    }
}
