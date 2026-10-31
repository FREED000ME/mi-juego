// Forja3D — la lámina de una cascada: hace correr la textura del líquido hacia abajo (sin textura, pulsa el brillo).
using UnityEngine;

namespace Forja3D
{
    public class Forja3DCascada : MonoBehaviour
    {
        Material m; Color emision; bool conEmision;
        void Start()
        {
            var r = GetComponent<Renderer>(); if (!r) return;
            m = r.material; // copia propia
            conEmision = m.HasProperty("_EmissionColor"); if (conEmision) emision = m.GetColor("_EmissionColor");
        }
        void Update()
        {
            if (!m) return;
            m.mainTextureOffset = new Vector2(0, Time.time * 1.6f);
            if (conEmision) m.SetColor("_EmissionColor", emision * (1f + Mathf.Sin(Time.time * 9f) * 0.25f));
        }
    }
}
