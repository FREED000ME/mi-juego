// Forja3D — reliquias del nivel (antes monedas): al recogerlas se guardan y se ven en la pausa.
// (cada componente en su propio archivo: si no, Unity no lo guarda bien en las escenas del APK)
using System.Collections;
using UnityEngine;

namespace Forja3D
{
    public class Forja3DCollectible : MonoBehaviour
    {
        public string nombre = "Reliquia";
        [TextArea] public string descripcion;
        Vector3 inicio;
        void Start() { Forja3DGameManager.RegistrarMoneda(); inicio = transform.position; }
        void Update()
        {
            transform.Rotate(0, 90f * Time.deltaTime, 0, Space.World);
            transform.position = inicio + Vector3.up * Mathf.Sin(Time.time * 2f) * 0.15f;
        }
        void OnTriggerEnter(Collider c)
        {
            if (!c.CompareTag("Player")) return;
            Forja3DProgreso.GuardarReliquia(nombre, descripcion);
            transform.position = inicio; Forja3DRetrato.Capturar(gameObject, nombre, false);
            Forja3DGameManager.Mensaje("RECOGISTE: " + nombre.ToUpperInvariant());
            Forja3DFX.Chispas(transform.position, 24, true);
            Forja3DAudio.Sonar("victoria", transform.position, 0.5f, 0f, 1.5f);
            Forja3DGameManager.MonedaRecogida();
            Destroy(gameObject);
        }
    }
}
