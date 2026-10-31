// Forja3D — meta del nivel. Oculta hasta cumplir el objetivo: entonces surge del suelo con temblor
// de cámara, o (la barca de Carón) llega navegando por el río.
// (cada componente en su propio archivo: si no, Unity no lo guarda bien en las escenas del APK)
using System.Collections;
using UnityEngine;

namespace Forja3D
{
    public class Forja3DGoal : MonoBehaviour
    {
        public string modo = "surgir"; // "surgir": sube del suelo; "navegar": la barca llega por el río
        public float alto = 6f;
        Transform visual; Vector3 posFinal; bool visible; Collider disparador;

        void Awake()
        {
            visual = transform.childCount > 0 ? new GameObject("VisualMeta").transform : null;
            if (visual)
            {
                visual.SetParent(transform, false);
                var hijos = new System.Collections.Generic.List<Transform>();
                foreach (Transform t in transform) if (t != visual) hijos.Add(t);
                foreach (var t in hijos) t.SetParent(visual, true);
                posFinal = visual.position;
            }
            disparador = GetComponent<Collider>();
            Forja3DGameManager.AlCumplirObjetivo += Revelar;
        }
        void OnDestroy() => Forja3DGameManager.AlCumplirObjetivo -= Revelar;

        IEnumerator Start()
        {
            if (disparador) disparador.enabled = false;
            if (visual) visual.gameObject.SetActive(false);
            yield return null; // espera a que todos los enemigos, jefes y reliquias se registren
            if (!visible && Forja3DGameManager.ObjetivoCumplido)
            {   // objetivo "meta": visible desde el principio
                visible = true;
                if (visual) visual.gameObject.SetActive(true);
                if (disparador) disparador.enabled = true;
            }
        }

        void Revelar()
        {
            if (visible) return;
            visible = true;
            StartCoroutine(modo == "navegar" ? Navegar() : Surgir());
        }

        IEnumerator Surgir()
        {
            yield return new WaitForSeconds(1.2f);
            if (visual)
            {
                visual.gameObject.SetActive(true);
                var abajo = posFinal - Vector3.up * alto;
                Forja3DFX.Temblor(0.45f, 2.6f);
                Forja3DAudio.Sonar("impacto_suelo", posFinal, 1f, 0f, 0.6f);
                Forja3DGameManager.Mensaje("¡LA SALIDA SURGE DE LA TIERRA!");
                for (float t = 0; t < 1f; t += Time.deltaTime / 2.4f)
                {
                    float k = 1f - Mathf.Pow(1f - t, 3f);
                    visual.position = Vector3.Lerp(abajo, posFinal, k) + Random.insideUnitSphere * 0.05f * (1f - t);
                    if (Random.value < 8f * Time.deltaTime) Forja3DFX.Polvo(posFinal + Random.insideUnitSphere * 2f, 2f);
                    yield return null;
                }
                visual.position = posFinal;
                Forja3DFX.Polvo(posFinal, 3.5f);
                Forja3DAudio.Sonar("impacto_suelo", posFinal, 0.8f, 0f, 0.8f);
            }
            if (disparador) disparador.enabled = true;
        }

        // La barca de Carón cruza la escena por el río hasta la orilla
        IEnumerator Navegar()
        {
            yield return new WaitForSeconds(0.8f);
            if (visual)
            {
                visual.gameObject.SetActive(true);
                var desde = posFinal + Vector3.left * 45f;
                Forja3DGameManager.Mensaje("UNA BARCA SE ACERCA POR EL RÍO");
                for (float t = 0; t < 1f; t += Time.deltaTime / 7f)
                {
                    float k = Mathf.SmoothStep(0f, 1f, t);
                    visual.position = Vector3.Lerp(desde, posFinal, k) + Vector3.up * Mathf.Sin(Time.time * 1.6f) * 0.08f;
                    visual.rotation = transform.rotation * Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 1.1f) * 2f);
                    yield return null;
                }
                visual.position = posFinal; visual.rotation = transform.rotation;
            }
            if (disparador) disparador.enabled = true;
        }

        void OnTriggerEnter(Collider c) { if (visible && c.CompareTag("Player")) Forja3DGameManager.MetaAlcanzada(); }
    }
}
