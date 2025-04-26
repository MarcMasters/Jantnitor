using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AntScript : MonoBehaviour
{
    // Movement
    [SerializeField] private float moveSpeed;
    private Rigidbody rb;
    private Vector3 direction;
    [SerializeField] ParticleSystem moveParticles;

    // Pickup
    [SerializeField] private GameObject pickUpArms;
    [SerializeField] private GameObject defaultArms;
    public bool pickingUp = false;
    private bool onPickupRange = false;

    // Resources raycast
    RaycastHit resourceHitInfo;
    [SerializeField] LayerMask layerMask;
    private Vector3 rayPosition;
    [SerializeField] private Transform antHands;
    [SerializeField] private Transform resourceContainer; // objeto padre en drop
    private Transform resourceTrans;
    private Rigidbody resourceRb;
    private Collider resourceCol;

    // Throwing
    [SerializeField] private float throwForce;
    private bool hasThrown;

    // Enemies raycast
    [SerializeField] LayerMask enemyLayerMask;
    RaycastHit enemyHitInfo;
    private bool onHitRange;

    // Almacenamiento de items en pulgon
    private PulgonScript pulgon;

    // Inventario
    private List<Item> inventory = new List<Item>();
    [SerializeField] private int maxInventory = 4;
    private int _gotas = 0;

    [SerializeField] private GameObject itemIconPrefab;
    [SerializeField] private Transform inventoryContent;
    private List<GameObject> uiInventory;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pulgon = GameObject.FindGameObjectWithTag("Pulgon").GetComponent<PulgonScript>();
        
        inventory = new List<Item>();
        uiInventory = new List<GameObject>();
    }

    private void FixedUpdate()
    {
        moveAnt();
    }

    public void AddToPlayerInventory(Item item)
    {
        // Se añade item a la lista de inventario
        inventory.Add(item);

        // Se añade icono a la lista de uiInventario
        GameObject uiItemIcon = Instantiate(itemIconPrefab, inventoryContent); // genera una image hija de inventoryContent
        Image im = uiItemIcon.GetComponent<Image>();    // image del gameobject
        im.sprite = item.itemIcon;                      // Cambio del sprite de image por el del item (preasignado)
        uiInventory.Add(uiItemIcon);                    // Se añade el icono correspondiente a la lista

    }

    public void AddWater(int gotas)
    {
        _gotas += gotas;
    }

    void Update()
    {
        //print(resourceTrans);
        //print(enemyTrans);

        //////////// RAYCAST ////////////
        // rayPosition Se actualiza igual que el movimiento del pj
        rayPosition = new Vector3(transform.position.x, transform.position.y - 0.75f, transform.position.z);
        raycast();

        //////////// RECOGIDA ////////////
        managePickup();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            dropResource(true);
        }

        //////////// PULGÓN ////////////
        if (Input.GetKey(KeyCode.E) && pulgon.playerOn)
        {

        }
    }

    private void moveAnt()
    {
        direction = Vector3.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            direction.x = Input.GetAxis("Horizontal");
            direction.z = Input.GetAxis("Vertical");

            rb.linearDamping = 0f;

            // Movimiento en 8 direcciones (sin suavizado entre ellas)
            //if (Input.GetKey(KeyCode.W)) { direction.z = 1; }
            //if (Input.GetKey(KeyCode.S)) { direction.z = -1; }
            //if (Input.GetKey(KeyCode.A)) { direction.x = -1; }
            //if (Input.GetKey(KeyCode.D)) { direction.x = 1; }

            //rb.MovePosition(rb.position + direction.normalized * moveSpeed * Time.fixedDeltaTime);  // tp de personaje
            //rb.AddForce(direction.normalized * moveSpeed * Time.fixedDeltaTime, ForceMode.Impulse); // demasiada inercia
            rb.linearVelocity = direction.normalized * moveSpeed * Time.fixedDeltaTime;  // fisica no realista (movimiento instantaneo)

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + 180;
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

            if (!moveParticles.isPlaying)
            {
                moveParticles.Play();
            }
        }
        else
        {
            //parar
            rb.linearDamping = 3f;
            if (moveParticles.isPlaying)
            {
                moveParticles.Stop();
            }
        }
        //print(direction);
    }

    private void raycast()
    {
        /// Determina si el player está en rango o no (de recoger item, golpear enemigo, etc)
        
        Ray ray = new Ray(rayPosition, transform.TransformDirection(Vector3.back));
        Debug.DrawRay(rayPosition, transform.TransformDirection(Vector3.back) * 1.5f, Color.green);
        if (Physics.Raycast(ray, out resourceHitInfo, 1.5f, layerMask))
        {
            onPickupRange = true;
            //print("Hit something");
        }
        else
        {
            onPickupRange = false;
            //print("Hit nothing");
        }

        //// Detectar enemigo
        if (Physics.Raycast(ray, out enemyHitInfo, 1.5f, enemyLayerMask))
        {
            //onHitRange = true;
            //print("Hit something");

            // animacion atacar
            // codigo atacar
        }
        else
        {
            onHitRange = false;
            //print("Hit nothing");
        }
    }

    private void managePickup()
    {
        // Guardar en inventario
        if (pickingUp && Input.GetKeyDown(KeyCode.E) && !pulgon.playerOn)
        {
            //dropResource();

            // animacion pickup > idle
            defaultArms.SetActive(true);
            pickUpArms.SetActive(false);

            if (inventory.Count < maxInventory)
            {
                pickingUp = false;

                IInteractable interactable = resourceCol.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact(this);
                }
            }
            else
            {
                Debug.Log("INVENTARIO LLENO");
            }
        }

        if (onPickupRange && Input.GetKeyDown(KeyCode.Space))
        {
            hasThrown = false;
            if (pickingUp) // si ya tiene un objeto
            {
                // soltar objeto 1
                resourceTrans.parent = resourceContainer;
                resourceRb.isKinematic = false;
                resourceCol.enabled = true;

                // establecer nuevo objeto
                resourceTrans = resourceHitInfo.transform;
                resourceRb = resourceHitInfo.rigidbody;
                resourceCol = resourceHitInfo.collider;
                //print(resourceCol.name);

                // coger nuevo objeto
                resourceTrans.SetParent(antHands);
                resourceTrans.position = antHands.transform.position;
                resourceRb.isKinematic = true;
                resourceCol.enabled = false;
            }
            else
            {
                // establecer nuevo objeto
                resourceTrans = resourceHitInfo.transform;
                resourceRb = resourceHitInfo.rigidbody;
                resourceCol = resourceHitInfo.collider;

                //if (resourceCol.name == "water(Clone)") print(resourceCol.name);

                pickingUp = true;

                // coger nuevo objeto
                resourceTrans.SetParent(antHands);
                resourceTrans.position = antHands.transform.position;
                resourceRb.isKinematic = true;
                resourceCol.enabled = false;

                // animacion idle > pickup
                defaultArms.SetActive(false);
                pickUpArms.SetActive(true);
            }
        }



        
    }

    private void dropResource(bool throwing = false)
    {
        if (pickingUp)
        {
            pickingUp = false;

            resourceTrans.parent = resourceContainer;
            resourceRb.isKinematic = false;
            resourceCol.enabled = true;

            // animacion pickup > idle
            defaultArms.SetActive(true);
            pickUpArms.SetActive(false);

            if (throwing && !hasThrown)
            {
                resourceRb.AddForce(transform.TransformDirection(Vector3.back) * throwForce, ForceMode.Impulse);
                //resourceRb.AddForce(direction * throwForce, ForceMode.Impulse); // fuerza depende de velocidad            
            }
            hasThrown = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            print("DAMAGE");
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    IInteractable interactable = other.GetComponent<IInteractable>();
    //    if (interactable != null)
    //    {
    //        interactable.Interact(this);
    //    }
    //}
}