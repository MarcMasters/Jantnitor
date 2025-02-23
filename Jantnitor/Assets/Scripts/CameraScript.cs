using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] private Transform ant;
    private Vector3 offset;
    [SerializeField] private float yOffset = 10f;
    [SerializeField] private float zOffset = -10f;
    private float smooth = .2f; // velocidad de seguimiento

    private Vector3 speed = Vector3.zero;

    // Se ejecuta después de cualquier update (imprescindible para movimientos de cámara que siguen objetos que se mueven en update)
    private void LateUpdate()
    {
        offset = new Vector3(0, yOffset, zOffset);
        Vector3 desiredPos = ant.position + offset;
        //transform.position = desiredPos;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref speed, smooth);
    }
}
