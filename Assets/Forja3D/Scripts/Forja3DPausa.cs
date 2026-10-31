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
        Transform canvas; GameObject panel, lista; Text listaTitulo; RectTransform contenido;

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
                Boton(panel.transform, "LIBRO", new Vector2(0.5f, y), Vector2.zero, new Vector2(560, 100), 42, () => MostrarLibro(true)); y -= 0.13f;
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

        // Libro: dos pestañas (reliquias y personajes); cada entrada con su retrato, nombre y descripción
        bool pestanaReliquias = true;
        void MostrarLibro(bool reliquias)
        {
            pestanaReliquias = reliquias;
            if (!lista)
            {
                lista = Fondo(canvas, "Libro", 0.96f);
                listaTitulo = Forja3DGameManager.Texto(lista.transform, "", 60, TextAnchor.MiddleCenter);
                listaTitulo.color = new Color(0.92f, 0.75f, 0.42f); listaTitulo.fontStyle = FontStyle.Bold;
                Anclar(listaTitulo.rectTransform, new Vector2(0, 0.88f), new Vector2(1, 0.98f));
                Boton(lista.transform, "RELIQUIAS", new Vector2(0.36f, 0.83f), Vector2.zero, new Vector2(360, 76), 34, () => MostrarLibro(true));
                Boton(lista.transform, "PERSONAJES", new Vector2(0.64f, 0.83f), Vector2.zero, new Vector2(360, 76), 34, () => MostrarLibro(false));
                var vista = new GameObject("Vista", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
                vista.transform.SetParent(lista.transform, false);
                Anclar(vista.GetComponent<RectTransform>(), new Vector2(0.06f, 0.15f), new Vector2(0.94f, 0.78f));
                vista.GetComponent<Image>().color = new Color(1, 1, 1, 0.02f); vista.GetComponent<Mask>().showMaskGraphic = false;
                var cont = new GameObject("Contenido", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
                cont.transform.SetParent(vista.transform, false);
                contenido = cont.GetComponent<RectTransform>();
                contenido.anchorMin = new Vector2(0, 1); contenido.anchorMax = new Vector2(1, 1); contenido.pivot = new Vector2(0.5f, 1); contenido.sizeDelta = Vector2.zero;
                var vl = cont.GetComponent<VerticalLayoutGroup>(); vl.spacing = 24; vl.childControlHeight = true; vl.childControlWidth = true; vl.childForceExpandHeight = false; vl.childForceExpandWidth = true;
                cont.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                var sr = vista.GetComponent<ScrollRect>(); sr.content = contenido; sr.horizontal = false; sr.movementType = ScrollRect.MovementType.Clamped;
                Boton(lista.transform, "VOLVER", new Vector2(0.5f, 0.07f), Vector2.zero, new Vector2(360, 90), 40, () => lista.SetActive(false));
            }
            listaTitulo.text = reliquias ? "LIBRO · RELIQUIAS" : "LIBRO · PERSONAJES";
            for (int i = contenido.childCount - 1; i >= 0; i--) Destroy(contenido.GetChild(i).gameObject);
            var datos = reliquias ? Forja3DProgreso.Reliquias : Forja3DProgreso.Personajes;
            if (datos.Count == 0)
            {
                var v = Forja3DGameManager.Texto(contenido, reliquias ? "<i>Aún no has recogido reliquias.</i>" : "<i>Aún no has conocido a nadie.</i>", 36, TextAnchor.MiddleCenter);
                v.supportRichText = true; v.gameObject.AddComponent<LayoutElement>().preferredHeight = 120;
            }
            foreach (var (n, d) in datos) Entrada(n, d);
            contenido.anchoredPosition = Vector2.zero;
            lista.SetActive(true); lista.transform.SetAsLastSibling();
        }

        void Entrada(string nombre, string desc)
        {
            var fila = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            fila.transform.SetParent(contenido, false);
            fila.GetComponent<Image>().color = new Color(0.2f, 0.12f, 0.08f, 0.55f);
            var hl = fila.GetComponent<HorizontalLayoutGroup>(); hl.padding = new RectOffset(16, 16, 16, 16); hl.spacing = 28;
            hl.childControlWidth = true; hl.childControlHeight = true; hl.childForceExpandWidth = false; hl.childForceExpandHeight = false;
            hl.childAlignment = TextAnchor.MiddleLeft;
            // Retrato
            var img = new GameObject("Retrato", typeof(RectTransform), typeof(RawImage), typeof(LayoutElement));
            img.transform.SetParent(fila.transform, false);
            var le = img.GetComponent<LayoutElement>(); le.preferredWidth = le.minWidth = 220; le.preferredHeight = le.minHeight = 220;
            var raw = img.GetComponent<RawImage>(); var tex = Forja3DRetrato.Cargar(nombre);
            if (tex) raw.texture = tex;
            else
            {
                raw.color = new Color(0.12f, 0.08f, 0.06f);
                var ini = Forja3DGameManager.Texto(img.transform, nombre.Substring(0, 1).ToUpperInvariant(), 110, TextAnchor.MiddleCenter);
                ini.color = new Color(0.92f, 0.75f, 0.42f, 0.6f);
            }
            // Nombre y descripción
            var txt = Forja3DGameManager.Texto(fila.transform, $"<color=#ebbf6b><b>{nombre}</b></color>\n<i>{desc}</i>", 34, TextAnchor.MiddleLeft);
            txt.supportRichText = true; txt.horizontalOverflow = HorizontalWrapMode.Wrap; txt.verticalOverflow = VerticalWrapMode.Overflow;
            var lt = txt.gameObject.AddComponent<LayoutElement>(); lt.flexibleWidth = 1; lt.minHeight = 220;
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
