using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

// Pantalla final del nivel. Vive dentro de HUDCanvas y se activa cuando LevelGoal confirma la victoria.
public class LevelVictoryScreen : MonoBehaviour
{
    private const string OverlayPath = "VictoryOverlay";
    private const string CardPath = "VictoryOverlay/VictoryCard";

    [Header("Referencias")]
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text textoTitulo;
    [SerializeField] private TMP_Text textoMonedas;
    [SerializeField] private TMP_Text textoDetalle;
    [SerializeField] private Button botonReiniciar;

    [Header("Contenido")]
    [SerializeField] private string titulo = "Nivel completado";
    [SerializeField] private string formatoMonedas = "Monedas recolectadas: {0}";
    [SerializeField] private string detalle = "Llegaste a la meta final.";

    [Header("Comportamiento")]
    [SerializeField] private bool pausarJuegoAlGanar = true;

    private bool visible;
    private float timeScaleAnterior = 1f;

    void Awake()
    {
        AsegurarEventSystem();
        AsegurarReferencias();
        ConfigurarBotones();
        OcultarInstantaneo();
    }

    void OnDestroy()
    {
        if(visible && pausarJuegoAlGanar)
        {
            RestaurarTiempo();
        }
    }

    public void Mostrar(PlayerWallet playerWallet)
    {
        int monedas = playerWallet != null ? playerWallet.Monedas : 0;
        Mostrar(monedas);
    }

    public void Mostrar(int monedasRecolectadas)
    {
        if(visible)
        {
            return;
        }

        visible = true;

        if(textoTitulo != null)
        {
            textoTitulo.text = titulo;
        }

        if(textoMonedas != null)
        {
            textoMonedas.text = string.Format(formatoMonedas, monedasRecolectadas);
        }

        if(textoDetalle != null)
        {
            textoDetalle.text = detalle;
        }

        if(panelVictoria != null)
        {
            panelVictoria.SetActive(true);
        }

        if(canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if(pausarJuegoAlGanar)
        {
            timeScaleAnterior = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    public void ReiniciarNivel()
    {
        RestaurarTiempo();

        Scene escenaActual = SceneManager.GetActiveScene();

        if(escenaActual.buildIndex >= 0)
        {
            SceneManager.LoadScene(escenaActual.buildIndex);
        }
        else
        {
#if UNITY_EDITOR
            EditorSceneManager.LoadScene(escenaActual.path);
#else
            SceneManager.LoadScene(escenaActual.name);
#endif
        }
    }

    private void AsegurarReferencias()
    {
        if(panelVictoria == null)
        {
            Transform overlay = transform.Find(OverlayPath);
            panelVictoria = overlay != null ? overlay.gameObject : null;
        }

        if(canvasGroup == null && panelVictoria != null)
        {
            canvasGroup = panelVictoria.GetComponent<CanvasGroup>();
        }

        if(textoTitulo == null)
        {
            textoTitulo = BuscarTexto(CardPath + "/Titulo");
        }

        if(textoMonedas == null)
        {
            textoMonedas = BuscarTexto(CardPath + "/Monedas");
        }

        if(textoDetalle == null)
        {
            textoDetalle = BuscarTexto(CardPath + "/Detalle");
        }

        if(botonReiniciar == null)
        {
            botonReiniciar = BuscarComponente<Button>(CardPath + "/BotonReiniciar");
        }

        if(panelVictoria == null)
        {
            Debug.LogWarning("LevelVictoryScreen no encontro VictoryOverlay dentro de HUDCanvas.", this);
        }
    }

    private void ConfigurarBotones()
    {
        if(botonReiniciar == null)
        {
            return;
        }

        botonReiniciar.onClick.RemoveListener(ReiniciarNivel);
        botonReiniciar.onClick.AddListener(ReiniciarNivel);
    }

    private void OcultarInstantaneo()
    {
        visible = false;

        if(canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if(panelVictoria != null)
        {
            panelVictoria.SetActive(false);
        }
    }

    private void RestaurarTiempo()
    {
        visible = false;
        Time.timeScale = timeScaleAnterior > 0f ? timeScaleAnterior : 1f;
    }

    private TMP_Text BuscarTexto(string ruta)
    {
        return BuscarComponente<TMP_Text>(ruta);
    }

    private T BuscarComponente<T>(string ruta) where T : Component
    {
        Transform encontrado = transform.Find(ruta);
        return encontrado != null ? encontrado.GetComponent<T>() : null;
    }

    private static void AsegurarEventSystem()
    {
        if(Object.FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<StandaloneInputModule>();
    }

}
