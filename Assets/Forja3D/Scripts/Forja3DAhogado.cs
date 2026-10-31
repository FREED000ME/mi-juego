// Forja3D — condenado que se ahoga en la Estigia (decoración): medio hundido, sube y baja, manotea y se hunde
// de vez en cuando para volver a salir. Sin colisión ni IA.
using UnityEngine;

namespace Forja3D
{
    public class Forja3DAhogado : MonoBehaviour
    {
        Vector3 basePos; float fase, proximo; Animator anim;
        void Start()
        {
            basePos = transform.position; fase = Random.Range(0f, 10f); proximo = Time.time + Random.Range(1f, 4f);
            anim = GetComponentInChildren<Animator>(); if (anim) anim.applyRootMotion = false;
        }
        void Update()
        {
            float t = Time.time + fase;
            // Ciclo: asoma (manotea) unos segundos y se hunde un momento
            float ciclo = Mathf.Repeat(t, 9f), hundir = ciclo > 7f ? Mathf.Sin((ciclo - 7f) / 2f * Mathf.PI) * 1.4f : 0f;
            transform.position = basePos + Vector3.up * (Mathf.Sin(t * 2.3f) * 0.18f - hundir);
            transform.rotation = Quaternion.Euler(Mathf.Sin(t * 1.7f) * 10f, t * 20f, Mathf.Cos(t * 1.3f) * 10f);
            if (anim && Time.time > proximo) { proximo = Time.time + Random.Range(1.2f, 3f); Forja3DAnim.Trigger(anim, Random.value < 0.5f ? "Hit" : "Attack"); }
            if (Random.value < 1.5f * Time.deltaTime) Forja3DFX.Polvo(basePos + Vector3.up * 0.2f, 0.5f);
        }
    }
}
