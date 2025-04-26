using JetBrains.Annotations;
using UnityEngine;

public class Item : MonoBehaviour, IInteractable
{
    public int amount;
    public Sprite itemIcon;

    public void Interact(AntScript player)
    {
        player.AddToPlayerInventory(this);

        //player.AddWater(amount);
        Destroy(this.gameObject);
    }
    

}
