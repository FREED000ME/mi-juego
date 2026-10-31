// Forja3D — agua que mata: el Aqueronte (sangre hirviendo) y la Estigia (pantano de los iracundos).
// Dante no puede nadar: si cae, se hunde. En la sangre arde entre chispas y humo; en el pantano, las almas
// lo agarran y lo arrastran al fondo. Los enemigos no entran (el NavMesh no pasa por el agua).
using UnityEngine;

namespace Forja3D
{
    public class Forja3DAguaMortal : MonoBehaviour
    {
        public bool fuego; // true: sangre hirviendo (Aqueronte); false: pantano (Estigia)
        static float ultimo = -10f;

        void OnTriggerEnter(Collider c)
        {
            if (!c.CompareTag("Player") || Time.time - ultimo < 1f) return;
            var vida = c.GetComponent<Forja3DHealth>();
            if (!vida || vida.Muerto) return;
            ultimo = Time.time;
            Forja3DGameManager.Mensaje(fuego ? "LA SANGRE HIRVIENTE TE CONSUME" : "LOS IRACUNDOS TE ARRASTRAN AL FONDO");
            var zona = FindFirstObjectByType<Forja3DZonaMuerta>();
            if (zona) zona.Ahogar(fuego);
            vida.Matar();
        }
    }
}
