// Forja3D — retratos para el libro (pausa): al conocer a un personaje o recoger una reliquia se le toma una foto
// en ese momento (cámara temporal que solo ve ese objeto) y se guarda en el teléfono. Sin créditos ni assets extra.
// EXPANSIÓN FUTURA: en PC, retratos a mayor resolución.
using System.IO;
using UnityEngine;

namespace Forja3D
{
    public static class Forja3DRetrato
    {
        const int Capa = 31, Tam = 256;

        public static string Ruta(string nombre)
        {
            var limpio = new System.Text.StringBuilder();
            foreach (var ch in (nombre ?? "x").ToLowerInvariant()) limpio.Append(char.IsLetterOrDigit(ch) ? ch : '_');
            return Path.Combine(Application.persistentDataPath, "retratos", limpio + ".png");
        }

        public static Texture2D Cargar(string nombre)
        {
            var r = Ruta(nombre);
            if (!File.Exists(r)) return null;
            var t = new Texture2D(2, 2); return t.LoadImage(File.ReadAllBytes(r)) ? t : null;
        }

        // retratoPersonaje: encuadra la parte de arriba (cara y torso); si no, el objeto entero
        public static void Capturar(GameObject obj, string nombre, bool retratoPersonaje)
        {
            if (!obj || string.IsNullOrEmpty(nombre)) return;
            var ruta = Ruta(nombre);
            if (File.Exists(ruta)) return;
            var capas = new System.Collections.Generic.List<(Transform t, int capa)>();
            GameObject camGo = null, luzGo = null; RenderTexture rt = null;
            try
            {
                Bounds b = default; bool hay = false;
                foreach (var r in obj.GetComponentsInChildren<Renderer>())
                {
                    if (r is ParticleSystemRenderer || r is LineRenderer || !r.enabled) continue;
                    if (!hay) { b = r.bounds; hay = true; } else b.Encapsulate(r.bounds);
                }
                if (!hay) return;
                foreach (var t in obj.GetComponentsInChildren<Transform>(true)) { capas.Add((t, t.gameObject.layer)); t.gameObject.layer = Capa; }

                var centro = b.center; float alto = b.size.y, ancho = Mathf.Max(b.size.x, b.size.z);
                float marco = Mathf.Max(alto, ancho);
                if (retratoPersonaje && alto > ancho * 0.8f) { centro.y = b.max.y - alto * 0.3f; marco = alto * 0.62f; }
                var frente = obj.transform.forward; frente.y = 0; if (frente.sqrMagnitude < 0.01f) frente = Vector3.forward; frente.Normalize();
                var lado = Quaternion.Euler(0, retratoPersonaje ? 20f : 35f, 0) * frente;
                float dist = marco * 0.5f / Mathf.Tan(15f * Mathf.Deg2Rad) * 1.15f;

                camGo = new GameObject("CamRetrato"); var cam = camGo.AddComponent<Camera>();
                cam.enabled = false; cam.cullingMask = 1 << Capa; cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.16f, 0.1f, 0.07f, 1f); cam.fieldOfView = 30f; cam.nearClipPlane = 0.05f; cam.farClipPlane = dist * 4f;
                cam.transform.position = centro + lado * dist + Vector3.up * marco * 0.12f;
                cam.transform.LookAt(centro);
                luzGo = new GameObject("LuzRetrato"); var luz = luzGo.AddComponent<Light>();
                luz.type = LightType.Directional; luz.cullingMask = 1 << Capa; luz.intensity = 1.25f; luz.color = new Color(1f, 0.92f, 0.82f);
                luz.transform.rotation = Quaternion.LookRotation(centro - (cam.transform.position + Vector3.up * marco));

                bool niebla = RenderSettings.fog; RenderSettings.fog = false;
                rt = RenderTexture.GetTemporary(Tam, Tam, 16);
                cam.targetTexture = rt; cam.Render();
                RenderSettings.fog = niebla;
                var previa = RenderTexture.active; RenderTexture.active = rt;
                var tex = new Texture2D(Tam, Tam, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, Tam, Tam), 0, 0); tex.Apply();
                RenderTexture.active = previa;
                Directory.CreateDirectory(Path.GetDirectoryName(ruta));
                File.WriteAllBytes(ruta, tex.EncodeToPNG());
                Object.Destroy(tex);
            }
            catch (System.Exception e) { Debug.LogWarning("[Forja3D] Retrato de " + nombre + ": " + e.Message); }
            finally
            {
                foreach (var (t, capa) in capas) if (t) t.gameObject.layer = capa;
                if (rt) RenderTexture.ReleaseTemporary(rt);
                if (camGo) Object.Destroy(camGo);
                if (luzGo) Object.Destroy(luzGo);
            }
        }
    }
}
