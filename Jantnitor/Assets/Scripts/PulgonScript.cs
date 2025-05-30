using UnityEngine;
using System.Collections.Generic;

public class PulgonScript : MonoBehaviour
{
    // Movimiento pulgón (persigue player)
    private Transform playerTransform;
    private Vector3 target;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float playerPersonalSpace;
    [SerializeField] private float pulgonYOffset;

    private bool grounded;
    public bool playerOn;
    //private AntScript player;

    private bool pulgonActivo;
    private Animator anim;

    // Inventario
    private List<GameObject> inventory = new List<GameObject>();

    void Start()
    {
        anim = transform.GetChild(0).GetComponent<Animator>();
        inventory = new List<GameObject>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        //player = GameObject.FindGameObjectWithTag("Player").GetComponent<AntScript>();
    }

    void Update()
    {
        if (pulgonActivo) {
            float dist2player = Vector3.Distance(transform.position, playerTransform.position);

            // Follow si está bajo tierra
            if (grounded)
            {
                anim.SetBool("isGrounded", true);
                target = new Vector3(playerTransform.position.x, playerTransform.position.y - pulgonYOffset, playerTransform.position.z);
                if (dist2player > playerPersonalSpace)
                {
                    // Rotar hacia el objetivo
                    transform.LookAt(target);

                    // Mover hacia el objetivo
                    transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                }
            }

            // Sacar pulgon con tecla
            if (grounded && playerOn && Input.GetKeyDown(KeyCode.E))
            {
                sacarPulgon();
            }
            // Guardar pulgon con tecla
            else if (!grounded && playerOn && Input.GetKeyDown(KeyCode.E))
            {
                setGrounded();
            }
            // Guardar y follow si player se aleja
            if (!grounded && dist2player > playerPersonalSpace + 2f)
            {
                setGrounded();
            }
        }
    }

    void sacarPulgon()
    {
        grounded = false;
        anim.SetBool("isGrounded", false);
        transform.position = new Vector3(transform.position.x, transform.position.y + pulgonYOffset, transform.position.z);
    }

    public void setGrounded()
    {
        grounded = true;
        //transform.position = new Vector3(transform.position.x, transform.position.y - pulgonYOffset, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            pulgonActivo = true;
            playerOn = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            playerOn = false;
        }
    }
}
