using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
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
}
