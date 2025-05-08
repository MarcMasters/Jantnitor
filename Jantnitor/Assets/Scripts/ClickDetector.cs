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
            // Crear un PointerEventData con la posición del cursor
            PointerEventData pointerData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };

            // Lista donde se almacenarán los resultados del raycast
            List<RaycastResult> results = new List<RaycastResult>();

            // Lanzar raycast gráfico
            raycaster.Raycast(pointerData, results);

            // Recorrer los resultados
            foreach (RaycastResult result in results)
            {
                clickedGO = result.gameObject; // Si por lo que sea son más de 1, se sobreescribirán (y se printearán)

                if (results.Count > 1)
                {
                    Debug.Log("Clic detectado en: " + clickedGO.GetComponent<Image>().sprite.name);
                }
            }
        }
    }
}

