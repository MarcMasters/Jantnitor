using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ClickDetector : MonoBehaviour
{
    [SerializeField] private GraphicRaycaster raycaster;
    [SerializeField] private EventSystem eventSystem;
    public GameObject clickedGO;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Crear un PointerEventData con la posici�n del cursor
            PointerEventData pointerData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };

            // Lista donde se almacenar�n los resultados del raycast
            List<RaycastResult> results = new List<RaycastResult>();

            // Lanzar raycast gr�fico
            raycaster.Raycast(pointerData, results);

            // Recorrer los resultados
            foreach (RaycastResult result in results)
            {
                clickedGO = result.gameObject; // Si por lo que sea son m�s de 1, se sobreescribir�n (y se printear�n)

                if (results.Count > 1)
                {
                    Debug.Log("Clic detectado en: " + clickedGO.GetComponent<Image>().sprite.name);
                }
            }
        }
    }
}

