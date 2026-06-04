using UnityEngine;

// Centraliza los sonidos del juego para que cada sistema solo pida "reproducir X".
// Si algun clip no esta asignado en el Inspector, simplemente no reproduce nada.
[DisallowMultipleComponent]
public class GameAudioManager : MonoBehaviour
{
    private static GameAudioManager instancia;

    [Header("Fuentes")]
    [SerializeField] private AudioSource fuenteSfx;
    [SerializeField] private AudioSource fuenteMusica;
    [Range(0f, 1f)]
    [SerializeField] private float volumenSfx = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float volumenMusica = 0.45f;

    [Header("Musica")]
    [SerializeField] private AudioClip musicaFondo;
    [SerializeField] private bool reproducirMusicaAlIniciar = true;

    [Header("Player")]
    [SerializeField] private AudioClip sonidoSalto;
    [SerializeField] private AudioClip sonidoAtaque1;
    [SerializeField] private AudioClip sonidoAtaque2;
    [SerializeField] private AudioClip sonidoDanoJugador;
    [SerializeField] private AudioClip sonidoMuerteJugador;

    [Header("Collectibles")]
    [SerializeField] private AudioClip sonidoMoneda;
    [SerializeField] private AudioClip sonidoVida;

    [Header("Objetos y hazards")]
    [SerializeField] private AudioClip sonidoRompible;
    [SerializeField] private AudioClip sonidoExplosion;
    [SerializeField] private AudioClip sonidoVeneno;

    [Header("Nivel y UI")]
    [SerializeField] private AudioClip sonidoVictoria;
    [SerializeField] private AudioClip sonidoDerrota;
    [SerializeField] private AudioClip sonidoClickUI;

    void Awake()
    {
        if(instancia != null && instancia != this)
        {
            enabled = false;
            return;
        }

        instancia = this;
        AsegurarFuentes();
    }

    void Start()
    {
        if(reproducirMusicaAlIniciar)
        {
            ReproducirMusicaFondo();
        }
    }

    void OnDestroy()
    {
        if(instancia == this)
        {
            instancia = null;
        }
    }

    public static void ReproducirSalto()
    {
        GameAudioManager manager = ObtenerInstancia();
        manager?.ReproducirClip(manager.sonidoSalto);
    }

    public static void ReproducirAtaque(int numeroAtaque)
    {
        GameAudioManager manager = ObtenerInstancia();

        if(manager == null)
        {
            return;
        }

        AudioClip clip = numeroAtaque == 2 && manager.sonidoAtaque2 != null
            ? manager.sonidoAtaque2
            : manager.sonidoAtaque1;

        manager.ReproducirClip(clip);
    }

    public static void ReproducirDanoJugador()
    {
        GameAudioManager manager = ObtenerInstancia();
        manager?.ReproducirClip(manager.sonidoDanoJugador);
    }

    public static void ReproducirMuerteJugador()
    {
        GameAudioManager manager = ObtenerInstancia();
        manager?.ReproducirClip(manager.sonidoMuerteJugador);
    }

    public static void ReproducirMoneda(AudioClip sonidoPersonalizado = null)
    {
        GameAudioManager manager = ObtenerInstancia();

        if(manager == null)
        {
            return;
        }

        manager.ReproducirClip(sonidoPersonalizado != null ? sonidoPersonalizado : manager.sonidoMoneda);
    }

    public static void ReproducirVida(AudioClip sonidoPersonalizado = null)
    {
        GameAudioManager manager = ObtenerInstancia();

        if(manager == null)
        {
            return;
        }

        manager.ReproducirClip(sonidoPersonalizado != null ? sonidoPersonalizado : manager.sonidoVida);
    }

    public static void ReproducirRompible()
    {
        GameAudioManager manager = ObtenerInstancia();
        manager?.ReproducirClip(manager.sonidoRompible);
    }

    public static void ReproducirExplosion()
    {
        GameAudioManager manager = ObtenerInstancia();
        manager?.ReproducirClip(manager.sonidoExplosion);
    }

    public static void ReproducirVeneno()
    {
        GameAudioManager manager = ObtenerInstancia();
        manager?.ReproducirClip(manager.sonidoVeneno);
    }

    public static void ReproducirVictoria()
    {
        GameAudioManager manager = ObtenerInstancia();
        manager?.ReproducirClip(manager.sonidoVictoria);
    }

    public static void ReproducirDerrota()
    {
        GameAudioManager manager = ObtenerInstancia();
        manager?.ReproducirClip(manager.sonidoDerrota);
    }

    public static void ReproducirClickUI()
    {
        GameAudioManager manager = ObtenerInstancia();
        manager?.ReproducirClip(manager.sonidoClickUI);
    }

    public static void PausarMusicaFondo()
    {
        GameAudioManager manager = ObtenerInstancia();
        manager?.PausarMusica();
    }

    public static void ReanudarMusicaFondo()
    {
        GameAudioManager manager = ObtenerInstancia();
        manager?.ReanudarMusica();
    }

    public static void DetenerMusicaFondo()
    {
        GameAudioManager manager = ObtenerInstancia();
        manager?.DetenerMusica();
    }

    public void ReproducirMusicaFondo()
    {
        if(musicaFondo == null)
        {
            return;
        }

        AsegurarFuentes();

        if(fuenteMusica.clip == musicaFondo && fuenteMusica.isPlaying)
        {
            return;
        }

        fuenteMusica.clip = musicaFondo;
        fuenteMusica.volume = volumenMusica;
        fuenteMusica.loop = true;
        fuenteMusica.Play();
    }

    private void PausarMusica()
    {
        if(fuenteMusica != null && fuenteMusica.isPlaying)
        {
            fuenteMusica.Pause();
        }
    }

    private void ReanudarMusica()
    {
        if(fuenteMusica != null && fuenteMusica.clip != null && !fuenteMusica.isPlaying)
        {
            fuenteMusica.UnPause();
        }
    }

    private void DetenerMusica()
    {
        if(fuenteMusica != null)
        {
            fuenteMusica.Stop();
        }
    }

    private static GameAudioManager ObtenerInstancia()
    {
        if(instancia == null)
        {
            instancia = Object.FindFirstObjectByType<GameAudioManager>();
        }

        return instancia;
    }

    private void ReproducirClip(AudioClip clip)
    {
        if(clip == null)
        {
            return;
        }

        AsegurarFuentes();
        fuenteSfx.PlayOneShot(clip, volumenSfx);
    }

    private void AsegurarFuentes()
    {
        if(fuenteSfx == null)
        {
            fuenteSfx = gameObject.AddComponent<AudioSource>();
        }

        fuenteSfx.playOnAwake = false;
        fuenteSfx.loop = false;

        if(fuenteMusica == null)
        {
            fuenteMusica = gameObject.AddComponent<AudioSource>();
        }

        fuenteMusica.playOnAwake = false;
        fuenteMusica.loop = true;
        fuenteMusica.volume = volumenMusica;
    }
}
