using UnityEngine;

public class Enemy : MonoBehaviour//, IInteractable
{
    public int _life;
    public int _damage;

    private Rigidbody enemy_rb;
    [SerializeField] private float throwForce = 1;
    private Animator anim;

    //public void Interact(AntScript player, int interactionIndex)
    //{
    //    switch (interactionIndex)
    //    {
    //        case 0: // Hit from player
    //            //player.AddToPlayerInventory(this);
    //            //Destroy(this.gameObject);
    //            break;

    //        case 1: // Item thrown from player
    //            //player.RemoveFromPlayerInventory(this);
    //            //Destroy(this.gameObject);
    //            break;

    //        default:
    //            Debug.Log("Índice no reconocido en clase Item.");
    //            break;
    //    }
    //}

    private void Start()
    {
        anim = GetComponent<Animator>();
        enemy_rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            anim.SetBool("isAttacking", true);
        }

        if (Input.GetKey(KeyCode.O))
        {
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }
    }

    private void EndAttack()
    {
        anim.SetBool("isAttacking", false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hit")
        {
            // Mirar en la dirección del click
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            //{
            //    Vector3 targetPosition = hit.point;
            //    // Calcular la dirección desde el personaje hacia el punto
            //    Vector3 direction = targetPosition - transform.position;
            //    direction.y = 0f; // Evitar que gire hacia arriba o abajo
            //}

            enemy_rb.AddForce(transform.TransformDirection(Vector3.forward) * throwForce, ForceMode.Impulse);
            _life--;

            if (_life == 0)
            {
                anim.SetBool("isDead", true);
                Destroy(this.gameObject,3);
            }
            print("enemigo hiteado");
        }
    }
}
