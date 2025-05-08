using NUnit.Framework.Interfaces;
using Unity.VisualScripting;
using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    public int amount;
    public Sprite itemIcon;

    public void Interact(AntScript player, int interactionIndex)
    {
        switch (interactionIndex)
        {
            case 0:
                player.AddToPlayerInventory(this);
                Destroy(this.gameObject);
                break;

            case 1:
                player.RemoveFromPlayerInventory(this);
                //Destroy(this.gameObject);
                break;

            default:
                Debug.Log("Índice no reconocido en clase Item.");
                break;
        }
    }
}
