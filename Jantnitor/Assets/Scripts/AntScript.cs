using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class AntScript : MonoBehaviour
{
    // Movement
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    private Rigidbody rb;
    private Vector3 direction;
    [SerializeField] ParticleSystem moveParticles;

    // Pickup
    public bool pickingUp = false;
    private bool onPickupRange = false;

    // Resources raycast
    [Header("Raycast")]
    RaycastHit resourceHitInfo;
    [SerializeField] LayerMask layerMask;
    private Vector3 rayPosition;
    [SerializeField] private Vector3 rayDirection;
    [SerializeField] private Transform antHands;
    [SerializeField] private Transform resourceContainer; // objeto padre en drop
    private Transform resourceTrans;
    private Rigidbody resourceRb;
    private Collider resourceCol;
    [SerializeField] private Vector3 rayROffset;
    [SerializeField] private Vector3 rayLOffset;

    // Throwing
    [Header("Throwing")]
    [SerializeField] private float throwForce;

    // Combat
    [Header("Combat")]
    [SerializeField] private GameObject broomAttackCol;

    [SerializeField] LayerMask enemyLayerMask;
    RaycastHit enemyHitInfo;

    [Header("Camera")]
    [SerializeField] Camera mainCamera;

    // Almacenamiento de items en pulgon ??
    private PulgonScript pulgon;

    // Inventory
    [Header("Inventory")]
    private List<Item> inventory = new List<Item>();
    [SerializeField] private int maxInventory = 4;

    [SerializeField] private GameObject itemIconPrefab;
    [SerializeField] private GameObject inventoryContent;
    private List<GameObject> uiInventory;
    private bool inventoryOn = false;

    private ClickDetector click;
    [SerializeField] private GameObject[] itemPrefabs;

    // Animations
    private Animator anim;
    private bool isAttacking;
    private bool pickUpThrowAnimOn = false;

    // Smash mecanics
    [Header("Smash")]
    [SerializeField] private float targetMashCount = 10f;      // Cuántas veces hay que pulsar
    [SerializeField] private float mashDecayRate = 2f;         // Cuánto se reduce el progreso por segundo
    [SerializeField] private Image mashProgressBar;            // Barra de progreso opcional
    [SerializeField] private Image mashProgressBarBorder;
    private float mashCount = 0f;
    private bool smashGameActive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (GameObject.FindGameObjectWithTag("Pulgon") != null)
        {
            pulgon = GameObject.FindGameObjectWithTag("Pulgon").GetComponent<PulgonScript>();
        }
        //anim = transform.GetChild(0).gameObject.GetComponent<Animator>();
        anim = GetComponent<Animator>();
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
            pickUpThrowAnimOn = true;
            anim.SetBool("hasPicked", true);
            anim.SetBool("isPicking", true);
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
        //////////// DEBUG //////////////
        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    for (int i = 0; i < inventory.Count; i++)
        //    {
        //        Debug.Log(inventory[i]);
        //    }
        //}

        //////////// RAYCAST ////////////
        rayPosition = new Vector3(transform.position.x, transform.position.y + 2.5f, transform.position.z);
        raycastRays();

        //////////// RECOGIDA ////////////
        if (Input.GetKeyDown(KeyCode.Space)) managePickup();

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

        //////////// ATAQUE ////////////
        print(click.clickedGO);
        if (Input.GetMouseButtonDown(0) && click.clickedGO == null && !isAttacking)
        {
            // Mirar en la dirección del click
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Vector3 targetPosition = hit.point;
                // Calcular la dirección desde el personaje hacia el punto
                Vector3 direction = targetPosition - transform.position;
                direction.y = 0f; // Evitar que gire hacia arriba o abajo

                // Calcular el ángulo con el +180 para compensar el modelo invertido
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + 180f;

                // Aplicar la rotación en Y
                transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
            }

            if (!pickingUp)
            {
                broomAttack();
            }
            else
            {
                dropResource(true);
            }
        }
        //print(click.clickedGO);

        // Smash game
        if (smashGameActive) smashGame();
        //if (resourceHitInfo.collider) if (resourceHitInfo.collider.gameObject.name == "savia") 

        //////////// PULGÓN ////////////
        //if (Input.GetKey(KeyCode.E) && pulgon.playerOn)
        //{

        //}
    }

    private void moveAnt()
    {
        direction = Vector3.zero;
        if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) && !isAttacking && !pickUpThrowAnimOn)
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

    private void raycastRays()
    {
        /// Determina si el player está en rango o no (de recoger item, golpear enemigo, etc)
        // Creo un rayo que apunte desde arriba hacia abajo en diagonal para atravesar siempre el collider de los items
        
        Ray rayC = new Ray(rayPosition, transform.TransformDirection(rayDirection));
        Ray rayR = new Ray(rayPosition, transform.TransformDirection(rayDirection + rayROffset));
        Ray rayL = new Ray(rayPosition, transform.TransformDirection(rayDirection + rayLOffset));

        Debug.DrawRay(    rayPosition, transform.TransformDirection(rayDirection) * 1.5f, Color.green);
        Debug.DrawRay(    rayPosition, transform.TransformDirection(rayDirection + rayROffset) * 1.5f, Color.blue);
        Debug.DrawRay(    rayPosition, transform.TransformDirection(rayDirection + rayLOffset) * 1.5f, Color.blue);

        if (Physics.Raycast(rayC, out resourceHitInfo, 2.5f, layerMask) || Physics.Raycast(rayR, out resourceHitInfo, 2.5f, layerMask) || Physics.Raycast(rayL, out resourceHitInfo, 2.5f, layerMask))
        {
            onPickupRange = true;
            //print("Hit something");
        }
        else
        {
            onPickupRange = false;
            //print("Hit nothing");
        }


        //// Detectar enemigo (útil para hacer algo al enemigo al acercarte (mordisco))
        //if (Physics.Raycast(ray, out enemyHitInfo, 2.5f, enemyLayerMask))
        //{
        //    //onHitRange = true;
        //    //print("Hit something");

        //    // animacion atacar
        //    // codigo atacar
        //}
        //else
        //{
        //    onHitRange = false;
        //    //print("Hit nothing");
        //}
    }

    private void managePickup()
    {
        if (onPickupRange)
        {
            if (resourceHitInfo.collider.gameObject.name == "savia")      // Recoger savia
            {
                mashProgressBar.gameObject.SetActive(true);
                mashProgressBarBorder.gameObject.SetActive(true);
                smashGameActive = true;
                mashCount += 1f;
            }
            else                                                          // Recoger item del suelo
            {
                spawnAtHands(null, resourceHitInfo);

            }
        }
        else if (pickingUp)                                     // Guardar en inventario
        {
            //dropResource();

            if (inventory.Count < maxInventory)
            {
                pickingUp = false;
                anim.SetBool("isPicking", false);

                IInteractable interactable = resourceCol.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact(this, 0);
                }
            }
            else
            {
                Debug.Log("INVENTARIO LLENO");
            }
        }
    }

    private void dropResource(bool throwing = false)
    {
        pickingUp = false;

        resourceTrans.parent = resourceContainer;
        resourceRb.isKinematic = false;
        resourceCol.enabled = true;

        pickUpThrowAnimOn = true;
        anim.SetBool("hasThrown", true);
        anim.SetBool("isPicking", false);

        if (throwing)
        {
            resourceRb.AddForce(transform.TransformDirection(Vector3.back) * throwForce, ForceMode.Impulse);
            //resourceRb.AddForce(direction * throwForce, ForceMode.Impulse); // fuerza depende de velocidad            
        }
    }

    private void broomAttack()
    {
        isAttacking = true;
        anim.SetBool("isAttacking", true);
        //broomAttackCol.SetActive(true);
    }

    public void setBroomCollider()
    {
        broomAttackCol.SetActive(true);
    }

    public void endBroomAttack()
    {
        isAttacking = false;
        anim.SetBool("isAttacking", false);
        broomAttackCol.SetActive(false);
    }

    private void endThrowAnim()
    {
        anim.SetBool("hasThrown", false);
        pickUpThrowAnimOn = false;
    }
    private void endPickAnim()
    {
        anim.SetBool("hasPicked", false);
        pickUpThrowAnimOn = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            print("DAMAGE");
        }
    }

    private void smashGame()
    {
        void endGame()
        {
            smashGameActive = false;
            mashProgressBar.gameObject.SetActive(false);
            mashProgressBarBorder.gameObject.SetActive(false);
            mashCount = 0f;
        }

        // Si no detecta collider (te has alejado de la savia), se desactiva
        if (!resourceHitInfo.collider) {
            endGame();
        }

        // Hacer que el contador baje con el tiempo si no se pulsa
        if (mashCount > 0)
        {
            mashCount -= mashDecayRate * Time.deltaTime;
            mashCount = Mathf.Max(0f, mashCount); // Evitar valores negativos
        }

        // Actualizar barra de progreso
        if (mashProgressBar != null)
        {
            mashProgressBar.fillAmount = mashCount / targetMashCount;
        }

        // Comprobar si se alcanzó el objetivo
        if (mashCount >= targetMashCount)
        {
            GameObject spawnedItem = Instantiate(itemPrefabs[2], resourceContainer.position, Quaternion.identity, resourceContainer);
            spawnAtHands(spawnedItem);
            endGame();
        }
    }
}