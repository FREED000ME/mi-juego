// Forja3D — zona muerta: el anillo de tierra maldita que rodea cada nivel (en lugar de muros en la orilla).
// Si Dante la pisa, las almas surgen del suelo a su alrededor, lo frenan y le quitan vida poco a poco;
// si muere ahí, lo arrastran hacia abajo. Al salir, vuelven a hundirse.
// EXPANSIÓN FUTURA: en PC/consola, vibración del mando y sonido envolvente de lamentos.
using System.Collections.Generic;
using UnityEngine;

namespace Forja3D
{
    public class Forja3DZonaMuerta : MonoBehaviour
    {
        public string[] mapa;   // 'z' = zona muerta, '.' = resto
        public float celda = 3f;
        public int filas, cols;
        public float intervaloDanio = 0.75f;
        public float frenado = 0.45f;

        Transform jugador; Forja3DHealth vida; Forja3DPlayerController control; CharacterController cc;
        readonly List<Transform> almas = new List<Transform>();
        readonly List<float> angulos = new List<float>();
        float proximoDanio, emergencia, baseTrote, baseCorrer, arrastre = -1f;
        bool dentro, avisado, ardiendo;

        // Caer al agua (Forja3DAguaMortal): las almas surgen y lo hunden; en la sangre, además, arde
        public void Ahogar(bool fuego)
        {
            ardiendo = fuego; dentro = true; arrastre = Time.time; emergencia = 1f;
            Forja3DAudio.Sonar("muerte_dante", jugador ? jugador.position : transform.position, 1f, 0f, fuego ? 0.8f : 0.7f);
        }

        void Start()
        {
            var p = GameObject.FindWithTag("Player");
            if (p) { jugador = p.transform; vida = p.GetComponent<Forja3DHealth>(); control = p.GetComponent<Forja3DPlayerController>(); cc = p.GetComponent<CharacterController>(); }
            if (control) { baseTrote = control.velocidadTrote; baseCorrer = control.velocidadCorrer; }
            foreach (Transform t in transform) if (t.name == "AlmaZona") { almas.Add(t); angulos.Add(Random.Range(0f, 360f)); }
            if (vida) vida.AlMorir += () => { if (dentro && arrastre < 0f) { arrastre = Time.time; Forja3DGameManager.Mensaje("LAS ALMAS TE ARRASTRAN AL FONDO"); } };
        }

        bool EnZona(Vector3 pos)
        {
            int col = Mathf.RoundToInt(pos.x / celda), f = filas - 1 - Mathf.RoundToInt(pos.z / celda);
            if (f < 0 || col < 0 || f >= filas || col >= cols) return true;
            var fila = mapa[f];
            return col < fila.Length && fila[col] == 'z';
        }

        void Update()
        {
            if (!jugador || mapa == null) return;
            if (arrastre > 0f) { Arrastrar(); return; }
            if (Forja3DGameManager.Bloqueado) return;
            bool ahora = EnZona(jugador.position);
            if (ahora != dentro)
            {
                dentro = ahora;
                if (dentro)
                {
                    if (!avisado) { avisado = true; Forja3DGameManager.Mensaje("¡ZONA MUERTA! LAS ALMAS TE RECLAMAN: VUELVE AL CAMINO"); }
                    proximoDanio = Time.time + intervaloDanio * 1.5f;
                    Forja3DAudio.Sonar("voz_condenado", jugador.position, 0.9f, 0.15f, 0.7f);
                }
                if (control) { control.velocidadTrote = baseTrote * (dentro ? frenado : 1f); control.velocidadCorrer = baseCorrer * (dentro ? frenado : 1f); }
            }
            emergencia = Mathf.MoveTowards(emergencia, dentro ? 1f : 0f, Time.deltaTime * (dentro ? 0.9f : 1.6f));
            ColocarAlmas(emergencia, 0f);
            if (dentro && vida && !vida.Muerto && Time.time >= proximoDanio)
            {
                proximoDanio = Time.time + intervaloDanio;
                vida.Danio(1, null, true); // imparable: no se bloquea
                Forja3DFX.Sangre(jugador.position + Vector3.up * 0.3f, Vector3.up, 10);
                Forja3DAudio.Sonar("voz_condenado", jugador.position, 0.7f, 0.2f, 0.8f);
            }
        }

        // Las almas salen del suelo en círculo alrededor de Dante y se le acercan cuanto más tiempo se queda
        void ColocarAlmas(float k, float hundir)
        {
            for (int i = 0; i < almas.Count; i++)
            {
                var a = almas[i];
                bool visible = k > 0.01f;
                if (a.gameObject.activeSelf != visible) a.gameObject.SetActive(visible);
                if (!visible) continue;
                angulos[i] += Time.deltaTime * 12f;
                float ang = (angulos[i] + i * 60f) * Mathf.Deg2Rad, radio = Mathf.Lerp(2.6f, 0.9f, k);
                var p = jugador.position + new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang)) * radio;
                if (Physics.Raycast(p + Vector3.up * 4f, Vector3.down, out var suelo, 10f, ~LayerMask.GetMask("Ignore Raycast"), QueryTriggerInteraction.Ignore) && suelo.collider.transform != jugador) p.y = suelo.point.y;
                p.y += Mathf.Lerp(-1.9f, -0.55f, k) - hundir;
                a.position = p;
                var mira = jugador.position - a.position; mira.y = 0;
                if (mira.sqrMagnitude > 0.01f) a.rotation = Quaternion.LookRotation(mira);
            }
        }

        // Muerte en la zona: Dante y las almas se hunden en la tierra
        void Arrastrar()
        {
            float t = Time.time - arrastre;
            if (t > 2.2f) return;
            if (cc && cc.enabled) cc.enabled = false;
            jugador.position += Vector3.down * Time.deltaTime * 0.9f;
            if (ardiendo && Random.value < 20f * Time.deltaTime) { Forja3DFX.Chispas(jugador.position + Vector3.up * (1.6f - t * 0.6f), 6); if (Random.value < 0.3f) Forja3DFX.Polvo(jugador.position + Vector3.up, 1f); }
            if (!ardiendo && Random.value < 6f * Time.deltaTime) Forja3DFX.Polvo(jugador.position + Vector3.up * 0.3f, 0.8f);
            ColocarAlmas(1f, t * 0.9f);
        }
    }
}
