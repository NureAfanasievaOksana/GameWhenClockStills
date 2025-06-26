using UnityEngine;

public class Gear : MonoBehaviour
{
    public string gearId;
    [HideInInspector] public GearPuzzleController.GearSlot currentSlot;

    void Start()
    {
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<CircleCollider2D>();
        }

        gameObject.tag = "Gear";
    }
}