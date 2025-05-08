using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class AntScript : MonoBehaviour
{
    // Movement
    [SerializeField] private float moveSpeed;
    private Rigidbody rb;
    private Vector3 direction;
    [SerializeField] ParticleSystem moveParticles;

    // Pickup
    //[SerializeField] private GameObject pickUpArms;
    //[SerializeField] private GameObject defaultArms;
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

    // Almacenamiento de items en pulgon ??
    private PulgonScript pulgon;

    // Inventory
    private List<Item> inventory = new List<Item>();
    [SerializeField] private int maxInventory = 4;

    [SerializeField] private GameObject itemIconPrefab;
    [SerializeField] private GameObject inventoryContent;
    private List<GameObject> uiInventory;

    private bool inventoryOn = false;

    [SerializeField] private ClickDetector click;
    [SerializeField] private GameObject[] itemPrefabs;

    // Animations
    private Animator anim;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pulgon = GameObject.FindGameObjectWithTag("Pulgon").GetComponent<PulgonScript>();
        anim = transform.GetChild(0).gameObject.GetComponent<Animator>();
        click = GameObject.FindGameObjectWithTag("Logic").GetComponent<ClickDetector>();

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

        // Se instancia la image y se añade gameobject a la lista de uiInventario
        GameObject uiItemIcon = Instantiate(itemIconPrefab, inventoryContent.transform); // genera una image hija de inventoryContent

        Image im = uiItemIcon.GetComponent<Image>();    // image del gameobject
        im.sprite = item.itemIcon;                      // Cambio del sprite de image por el del item (preasignado)
        uiInventory.Add(uiItemIcon);                    // Se añade el icono correspondiente a la lista
    }

    public void RemoveFromPlayerInventory(Item item)
    {
        // El espacio de la lista se queda con null, por tanto elimino todos los null
        //inventory.RemoveAll(item => item == null);

        // Elimino un item de la lista cualquiera (son todos null)
        inventory.RemoveAt(0);

        // Se elimina gameobject de la lista de uiInventario y se elimina icono de la interfaz
        GameObject uiItemIcon = click.clickedGO;
        Image im = uiItemIcon.GetComponent<Image>();    // image del gameobject
        uiInventory.Remove(uiItemIcon);                 // Se elimina el icono correspondiente de la lista (gamobject)

        Destroy(uiItemIcon);
    }

    private void spawnAtHands(GameObject item2spawn = null, RaycastHit? hitInfo = null)
    {
        int option = 0;
        // Spawn desde el inventario
        if (item2spawn != null)
        {
            option = 1;
        }
        // Spawn desde mundo/suelo
        else if (hitInfo.HasValue)
        {
            option = 2;
        }
        else
        {
            Debug.Log("ITEM 2 SPAWN NO DETECTADO");
            return;
        }

        if (!pickingUp)
        {
            // Establecer nuevo objeto (transform,rb y collider) (en funcion del parámetro de entrada)
            if (option == 1)
            {
                resourceTrans = item2spawn.transform;
                resourceRb = item2spawn.GetComponent<Rigidbody>();
                resourceCol = item2spawn.GetComponent<Collider>();
            }
            else if (option == 2)
            {
                resourceTrans = hitInfo.Value.transform;
                resourceRb = hitInfo.Value.rigidbody;
                resourceCol = hitInfo.Value.collider;
            }

            // Coger nuevo objeto
            resourceTrans.SetParent(antHands);
            resourceTrans.position = antHands.transform.position;
            resourceRb.isKinematic = true;
            resourceCol.enabled = false;

            pickingUp = true;

            // animacion idle > pickup
        }
        else
        {
            // soltar objeto 1
            resourceTrans.parent = resourceContainer;
            resourceRb.isKinematic = false;
            resourceCol.enabled = true;

            // Establecer nuevo objeto (transform,rb y collider) (en funcion del parámetro de entrada)
            if (option == 1)
            {
                resourceTrans = item2spawn.transform;
                resourceRb = item2spawn.GetComponent<Rigidbody>();
                resourceCol = item2spawn.GetComponent<Collider>();
            }
            else if (option == 2)
            {
                resourceTrans = hitInfo.Value.transform;
                resourceRb = hitInfo.Value.rigidbody;
                resourceCol = hitInfo.Value.collider;
            }

            // coger nuevo objeto
            resourceTrans.SetParent(antHands);
            resourceTrans.position = antHands.transform.position;
            resourceRb.isKinematic = true;
            resourceCol.enabled = false;
        }
    }

    private void manageItemRemove()
    {
        IInteractable interactable = null;
        GameObject itemToSpawn = null;
        switch (click.clickedGO.GetComponent<Image>().sprite.name)
        {
            case "water":
                interactable = itemPrefabs[0].GetComponent<Collider>().GetComponent<IInteractable>();
                itemToSpawn = itemPrefabs[0];
                break;

            case "leaf":
                interactable = itemPrefabs[1].GetComponent<Collider>().GetComponent<IInteractable>();
                itemToSpawn = itemPrefabs[1];
                break;

            case "trash":
                interactable = itemPrefabs[2].GetComponent<Collider>().GetComponent<IInteractable>();
                itemToSpawn = itemPrefabs[2];
                break;

            default:
                Debug.Log("Item to remove from inventory not found");
                break;
        }

        GameObject spawnedItem = Instantiate(itemToSpawn, resourceContainer.position, Quaternion.identity, resourceContainer);
        spawnAtHands(spawnedItem);

        // Necesito un iinteractable > Collider del item/prefab > Instantiate item > prefab correcto
        if (interactable != null)
        {
            interactable.Interact(this, 1);
        }
    }

    void Update()
    {
        // Imprimir lista de inventory
        if (Input.GetKeyDown(KeyCode.K))
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                Debug.Log(inventory[i]);
            }
        }

        //print(resourceTrans);
        //print(enemyTrans);

        //////////// RAYCAST ////////////
        rayPosition = new Vector3(transform.position.x, transform.position.y - 0.75f, transform.position.z);
        raycast();

        //////////// RECOGIDA ////////////
        managePickup();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            dropResource(true);
        }

        //////////// INVENTARIO ////////////
        if (Input.GetKeyDown(KeyCode.E) && !pulgon.playerOn)
        {
            if (!inventoryOn)
            {
                inventoryOn = true;
                inventoryContent.SetActive(true);
            }
            else
            {
                inventoryOn = false;
                inventoryContent.SetActive(false);
            }
        }

        // Eliminar item del inventario
        if (Input.GetMouseButtonDown(0) && click.clickedGO != null)
        {
            manageItemRemove();
        }

        //////////// PULGÓN ////////////
        //if (Input.GetKey(KeyCode.E) && pulgon.playerOn)
        //{

        //}
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

            anim.SetBool("isMoving",true);

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

            anim.SetBool("isMoving", false);
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
        if (pickingUp && Input.GetKeyDown(KeyCode.Space) && !pulgon.playerOn && !onPickupRange)
        {
            //dropResource();

            // animacion pickup > idle
            //defaultArms.SetActive(true);
            //pickUpArms.SetActive(false);

            if (inventory.Count < maxInventory)
            {
                pickingUp = false;

                IInteractable interactable = resourceCol.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact(this,0);
                }
            }
            else
            {
                Debug.Log("INVENTARIO LLENO");
            }
        }

        // Recoger item del suelo
        if (onPickupRange && Input.GetKeyDown(KeyCode.Space) && !pulgon.playerOn)
        {
            hasThrown = false; // puede lanzar al recoger

            spawnAtHands(null,resourceHitInfo);

            // si ya tiene un objeto
            // o manos vacías
            //if (!pickingUp)
            //{
            //    // establecer nuevo objeto
            //    resourceTrans = resourceHitInfo.transform;
            //    resourceRb = resourceHitInfo.rigidbody;
            //    resourceCol = resourceHitInfo.collider;

            //    //if (resourceCol.name == "water(Clone)") print(resourceCol.name);

            //    pickingUp = true;

            //    // coger nuevo objeto
            //    resourceTrans.SetParent(antHands);
            //    resourceTrans.position = antHands.transform.position;
            //    resourceRb.isKinematic = true;
            //    resourceCol.enabled = false;

            //    // animacion idle > pickup
            //}
            //else
            //{
            //    // soltar objeto 1
            //    resourceTrans.parent = resourceContainer;
            //    resourceRb.isKinematic = false;
            //    resourceCol.enabled = true;

            //    // establecer nuevo objeto
            //    resourceTrans = resourceHitInfo.transform;
            //    resourceRb = resourceHitInfo.rigidbody;
            //    resourceCol = resourceHitInfo.collider;
            //    //print(resourceCol.name);

            //    // coger nuevo objeto
            //    resourceTrans.SetParent(antHands);
            //    resourceTrans.position = antHands.transform.position;
            //    resourceRb.isKinematic = true;
            //    resourceCol.enabled = false;
            //}
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
            //defaultArms.SetActive(true);
            //pickUpArms.SetActive(false);

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
}