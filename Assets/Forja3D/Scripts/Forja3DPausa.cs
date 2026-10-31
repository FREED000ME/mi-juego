// Forja3D — botón de pausa y menú: continuar, virtudes, reliquias recogidas, personajes conocidos y menú principal.
// EXPANSIÓN FUTURA: en PC se abre también con Escape; opciones de volumen y gráficos.
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Forja3D
{
    public class Forja3DPausa : MonoBehaviour
    {
        public static bool Activa { get; private set; }
        Transform canvas; GameObject panel, lista; Text listaTexto, listaTitulo; RectTransform contenido;

        public void Crear(Transform c)
        {
            canvas = c; Activa = false;
            var b = Boton(c, "II", new Vector2(1, 1), new Vector2(-80, -70), new Vector2(110, 110), 46, Abrir);
            b.GetComponent<Image>().color = new Color(0.1f, 0.06f, 0.05f, 0.65f);
        }

        void Update() { if (Input.GetKeyDown(KeyCode.Escape)) { if (Activa) Cerrar(); else Abrir(); } }

        void Abrir()
        {
            if (Activa || Forja3DGameManager.Terminado || Forja3DPergamino.Activo) return;
            Activa = true; Time.timeScale = 0f;
            if (!panel)
            {
                panel = Fondo(canvas, "Pausa", 0.88f);
                var t = Forja3DGameManager.Texto(panel.transform, "PAUSA", 80, TextAnchor.MiddleCenter);
                t.color = new Color(0.92f, 0.75f, 0.42f); t.fontStyle = FontStyle.Bold;
                Anclar(t.rectTransform, new Vector2(0, 0.8f), new Vector2(1, 0.95f));
                float y = 0.68f;
                Boton(panel.transform, "CONTINUAR", new Vector2(0.5f, y), Vector2.zero, new Vector2(560, 100), 42, Cerrar); y -= 0.13f;
                Boton(panel.transform, "VIRTUDES", new Vector2(0.5f, y), Vector2.zero, new Vector2(560, 100), 42, () => Forja3DVirtudes.Abrir(canvas, () => { })); y -= 0.13f;
                Boton(panel.transform, "RELIQUIAS", new Vector2(0.5f, y), Vector2.zero, new Vector2(560, 100), 42, () => MostrarLista("RELIQUIAS", Forja3DProgreso.Reliquias, "Aún no has recogido reliquias.")); y -= 0.13f;
                Boton(panel.transform, "PERSONAJES", new Vector2(0.5f, y), Vector2.zero, new Vector2(560, 100), 42, () => MostrarLista("PERSONAJES", Forja3DProgreso.Personajes, "Aún no has conocido a nadie.")); y -= 0.13f;
                Boton(panel.transform, "MENÚ PRINCIPAL", new Vector2(0.5f, y), Vector2.zero, new Vector2(560, 100), 42, () => { Activa = false; Time.timeScale = 1f; SceneManager.LoadScene(0); });
            }
            panel.SetActive(true);
            panel.transform.SetAsLastSibling();
        }

        void Cerrar()
        {
            if (lista) lista.SetActive(false);
            if (panel) panel.SetActive(false);
            Activa = false; Time.timeScale = 1f;
        }

        void MostrarLista(string titulo, System.Collections.Generic.List<(string n, string d)> datos, string vacio)
        {
            if (!lista)
            {
                lista = Fondo(canvas, "Lista", 0.95f);
                listaTitulo = Forja3DGameManager.Texto(lista.transform, "", 64, TextAnchor.MiddleCenter);
                listaTitulo.color = new Color(0.92f, 0.75f, 0.42f); listaTitulo.fontStyle = FontStyle.Bold;
                Anclar(listaTitulo.rectTransform, new Vector2(0, 0.86f), new Vector2(1, 0.97f));
                // Lista desplazable
                var vista = new GameObject("Vista", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
                vista.transform.SetParent(lista.transform, false);
                Anclar(vista.GetComponent<RectTransform>(), new Vector2(0.08f, 0.16f), new Vector2(0.92f, 0.85f));
                vista.GetComponent<Image>().color = new Color(1, 1, 1, 0.02f); vista.GetComponent<Mask>().showMaskGraphic = false;
                var cont = new GameObject("Contenido", typeof(RectTransform), typeof(ContentSizeFitter));
                cont.transform.SetParent(vista.transform, false);
                contenido = cont.GetComponent<RectTransform>();
                contenido.anchorMin = new Vector2(0, 1); contenido.anchorMax = new Vector2(1, 1); contenido.pivot = new Vector2(0.5f, 1); contenido.sizeDelta = Vector2.zero;
                cont.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                listaTexto = Forja3DGameManager.Texto(cont.transform, "", 36, TextAnchor.UpperLeft);
                listaTexto.supportRichText = true; listaTexto.horizontalOverflow = HorizontalWrapMode.Wrap; listaTexto.verticalOverflow = VerticalWrapMode.Overflow;
                var lt = listaTexto.rectTransform; lt.anchorMin = new Vector2(0, 1); lt.anchorMax = new Vector2(1, 1); lt.pivot = new Vector2(0.5f, 1);
                var fit = listaTexto.gameObject.AddComponent<ContentSizeFitter>(); fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                var sr = vista.GetComponent<ScrollRect>(); sr.content = contenido; sr.horizontal = false; sr.movementType = ScrollRect.MovementType.Clamped;
                Boton(lista.transform, "VOLVER", new Vector2(0.5f, 0.07f), Vector2.zero, new Vector2(360, 90), 40, () => lista.SetActive(false));
            }
            listaTitulo.text = titulo;
            var sb = new System.Text.StringBuilder();
            if (datos.Count == 0) sb.Append($"<i>{vacio}</i>");
            foreach (var (n, d) in datos) sb.Append($"<color=#ebbf6b><b>{n}</b></color>\n<i>{d}</i>\n\n");
            listaTexto.text = sb.ToString();
            // El contenido mide lo que el texto
            Canvas.ForceUpdateCanvases();
            contenido.sizeDelta = new Vector2(0, listaTexto.preferredHeight + 40);
            contenido.anchoredPosition = Vector2.zero;
            lista.SetActive(true); lista.transform.SetAsLastSibling();
        }

        static GameObject Fondo(Transform padre, string nombre, float alfa)
        {
            var g = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            g.transform.SetParent(padre, false);
            Anclar(g.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
            g.GetComponent<Image>().color = new Color(0.03f, 0.015f, 0.01f, alfa);
            return g;
        }
        static void Anclar(RectTransform r, Vector2 min, Vector2 max) { r.anchorMin = min; r.anchorMax = max; r.offsetMin = r.offsetMax = Vector2.zero; }

        static Button Boton(Transform padre, string texto, Vector2 ancla, Vector2 pos, Vector2 tam, int letra, UnityEngine.Events.UnityAction accion)
        {
            var go = new GameObject(texto, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(padre, false);
            var r = go.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = ancla; r.anchoredPosition = pos; r.sizeDelta = tam;
            go.GetComponent<Image>().color = new Color(0.45f, 0.08f, 0.06f, 0.92f);
            var o = go.AddComponent<Outline>(); o.effectColor = new Color(0.92f, 0.75f, 0.42f, 0.7f); o.effectDistance = new Vector2(2, -2);
            go.GetComponent<Button>().onClick.AddListener(accion);
            Forja3DGameManager.Texto(go.transform, texto, letra, TextAnchor.MiddleCenter);
            return go.GetComponent<Button>();
        }
    }
}
