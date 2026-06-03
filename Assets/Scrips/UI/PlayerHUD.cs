using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Actualiza el HUD principal del jugador desde un Canvas armado manualmente.
// La UI vive en la escena; este script solo escucha vida/monedas y refresca los elementos.
public class PlayerHUD : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerWallet playerWallet;

    [Header("Vida")]
    [SerializeField] private Image[] iconosVida;
    [SerializeField] private Sprite iconoVidaLlena;
    [SerializeField] private Sprite iconoVidaVacia;
    [SerializeField] private Color colorVidaLlena = Color.white;
    [SerializeField] private Color colorVidaVacia = new Color(1f, 1f, 1f, 0.3f);

    [Header("Monedas")]
    [SerializeField] private Image iconoMoneda;
    [SerializeField] private Sprite spriteMoneda;
    [SerializeField] private TMP_Text textoMonedas;
    [SerializeField] private Text textoMonedasLegacy;
    [SerializeField] private string prefijoMonedas = "x";

    private bool inicializado;
    private bool eventosSuscritos;

    void OnEnable()
    {
        if(inicializado)
        {
            SuscribirEventos();
            RefrescarHUD();
        }
    }

    void Start()
    {
        inicializado = true;
        AsegurarReferencias();
        SuscribirEventos();
        RefrescarHUD();
    }

    void OnDisable()
    {
        DesuscribirEventos();
    }

    private void AsegurarReferencias()
    {
        if(!EsReferenciaDeEscena(playerHealth))
        {
            playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        }

        if(!EsReferenciaDeEscena(playerWallet))
        {
            playerWallet = Object.FindFirstObjectByType<PlayerWallet>();
        }

        if(textoMonedas == null)
        {
            textoMonedas = GetComponentInChildren<TMP_Text>();
        }

        if(textoMonedasLegacy == null)
        {
            textoMonedasLegacy = GetComponentInChildren<Text>();
        }

        if(playerHealth == null)
        {
            Debug.LogWarning("PlayerHUD no encontro PlayerHealth. Asignalo desde el Inspector.", this);
        }

        if(playerWallet == null)
        {
            Debug.LogWarning("PlayerHUD no encontro PlayerWallet. Asignalo desde el Inspector.", this);
        }
    }

    private bool EsReferenciaDeEscena(Component referencia)
    {
        return referencia != null && referencia.gameObject.scene.IsValid();
    }

    private void ActualizarVida(int vidaActual, int vidaMaxima)
    {
        if(iconosVida == null)
        {
            return;
        }

        for(int i = 0; i < iconosVida.Length; i++)
        {
            if(iconosVida[i] == null)
            {
                continue;
            }

            bool dentroDeVidaMaxima = i < vidaMaxima;
            bool tieneVida = i < vidaActual;

            iconosVida[i].gameObject.SetActive(dentroDeVidaMaxima);

            if(!dentroDeVidaMaxima)
            {
                continue;
            }

            iconosVida[i].sprite = tieneVida || iconoVidaVacia == null ? iconoVidaLlena : iconoVidaVacia;
            iconosVida[i].color = tieneVida ? colorVidaLlena : colorVidaVacia;
        }
    }

    private void ActualizarMonedas(int monedas)
    {
        if(textoMonedas != null)
        {
            textoMonedas.text = prefijoMonedas + monedas;
        }
        else if(textoMonedasLegacy != null)
        {
            textoMonedasLegacy.text = prefijoMonedas + monedas;
        }

        if(iconoMoneda != null && spriteMoneda != null)
        {
            iconoMoneda.sprite = spriteMoneda;
        }
    }

    private void RefrescarHUD()
    {
        if(playerHealth != null)
        {
            ActualizarVida(playerHealth.VidaActual, playerHealth.VidaMaxima);
        }

        if(playerWallet != null)
        {
            ActualizarMonedas(playerWallet.Monedas);
        }
    }

    private void SuscribirEventos()
    {
        if(eventosSuscritos)
        {
            return;
        }

        if(playerHealth != null)
        {
            playerHealth.VidaCambiada += ActualizarVida;
        }

        if(playerWallet != null)
        {
            playerWallet.MonedasCambiadas += ActualizarMonedas;
        }

        eventosSuscritos = true;
    }

    private void DesuscribirEventos()
    {
        if(!eventosSuscritos)
        {
            return;
        }

        if(playerHealth != null)
        {
            playerHealth.VidaCambiada -= ActualizarVida;
        }

        if(playerWallet != null)
        {
            playerWallet.MonedasCambiadas -= ActualizarMonedas;
        }

        eventosSuscritos = false;
    }
}
