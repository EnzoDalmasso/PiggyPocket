using System.Collections.Generic;
using UnityEngine;

// Capa individual de fondo con parallax.
// Se coloca en un GameObject con SpriteRenderer y se mueve segun la camara.
[RequireComponent(typeof(SpriteRenderer))]
public class ParallaxBackgroundLayer : MonoBehaviour
{
    [SerializeField] private Transform camara;
    [SerializeField] private Vector2 factorParallax = new Vector2(0.1f, 1f);
    [SerializeField] private bool repetirHorizontal = true;
    [SerializeField] private int copiasLaterales = 2;
    [SerializeField] private bool ajustarAlAltoCamara = true;
    [SerializeField] private float altoDiseno = 1.8f;
    [SerializeField] private float escalaExtra = 1.05f;

    private readonly List<GameObject> copias = new List<GameObject>();
    private SpriteRenderer spriteRenderer;
    private Vector3 posicionInicial;
    private Vector3 posicionCamaraInicial;
    private float anchoMundo;
    private float offsetRepeticionX;

    void Start()
    {
        AsegurarReferencias();
        AjustarEscala();
        posicionInicial = transform.position;
        posicionCamaraInicial = camara != null ? camara.position : Vector3.zero;
        CalcularAnchoMundo();
        CrearCopiasLaterales();
    }

    void LateUpdate()
    {
        if(camara == null)
        {
            AsegurarReferencias();
        }

        if(camara == null)
        {
            return;
        }

        Vector3 deltaCamara = camara.position - posicionCamaraInicial;
        Vector3 nuevaPosicion = posicionInicial + new Vector3(deltaCamara.x * factorParallax.x, deltaCamara.y * factorParallax.y, 0f);

        if(repetirHorizontal && anchoMundo > 0)
        {
            nuevaPosicion.x += CalcularOffsetRepeticion(nuevaPosicion.x);
        }

        nuevaPosicion.z = posicionInicial.z;
        transform.position = nuevaPosicion;
    }

    public void Configurar(Transform nuevaCamara, Vector2 nuevoFactorParallax, bool repetir, int nuevasCopiasLaterales, bool ajustarAlto, float nuevoAltoDiseno, float nuevaEscalaExtra)
    {
        camara = nuevaCamara;
        factorParallax = nuevoFactorParallax;
        repetirHorizontal = repetir;
        copiasLaterales = nuevasCopiasLaterales;
        ajustarAlAltoCamara = ajustarAlto;
        altoDiseno = nuevoAltoDiseno;
        escalaExtra = nuevaEscalaExtra;
    }

    private void AsegurarReferencias()
    {
        if(spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if(camara == null && Camera.main != null)
        {
            camara = Camera.main.transform;
        }
    }

    private void AjustarEscala()
    {
        if(!ajustarAlAltoCamara || camara == null || spriteRenderer == null || spriteRenderer.sprite == null)
        {
            return;
        }

        Camera componenteCamara = camara.GetComponent<Camera>();

        if(componenteCamara == null || !componenteCamara.orthographic || altoDiseno <= 0)
        {
            return;
        }

        float altoCamara = componenteCamara.orthographicSize * 2f;
        float escala = altoCamara / altoDiseno * escalaExtra;
        transform.localScale = new Vector3(escala, escala, transform.localScale.z);
    }

    private void CalcularAnchoMundo()
    {
        if(spriteRenderer == null || spriteRenderer.sprite == null)
        {
            anchoMundo = 0;
            return;
        }

        anchoMundo = spriteRenderer.bounds.size.x;
    }

    private void CrearCopiasLaterales()
    {
        if(!repetirHorizontal || spriteRenderer == null || spriteRenderer.sprite == null || copiasLaterales <= 0)
        {
            return;
        }

        float anchoLocal = spriteRenderer.sprite.bounds.size.x;

        for(int i = 1; i <= copiasLaterales; i++)
        {
            CrearCopia(-i, anchoLocal);
            CrearCopia(i, anchoLocal);
        }
    }

    private void CrearCopia(int direccion, float anchoLocal)
    {
        GameObject copia = new GameObject(spriteRenderer.name + "_Copy_" + direccion);
        copia.layer = gameObject.layer;
        copia.transform.SetParent(transform, false);
        copia.transform.localPosition = new Vector3(anchoLocal * direccion, 0f, 0f);

        SpriteRenderer copiaRenderer = copia.AddComponent<SpriteRenderer>();
        copiaRenderer.sprite = spriteRenderer.sprite;
        copiaRenderer.color = spriteRenderer.color;
        copiaRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        copiaRenderer.sortingOrder = spriteRenderer.sortingOrder;
        copiaRenderer.flipX = spriteRenderer.flipX;
        copiaRenderer.flipY = spriteRenderer.flipY;
        copiaRenderer.material = spriteRenderer.sharedMaterial;

        copias.Add(copia);
    }

    private float CalcularOffsetRepeticion(float posicionBaseX)
    {
        float centroCapa = posicionBaseX + anchoMundo * 0.5f + offsetRepeticionX;
        float distanciaCamara = camara.position.x - centroCapa;

        while(distanciaCamara > anchoMundo)
        {
            offsetRepeticionX += anchoMundo;
            distanciaCamara -= anchoMundo;
        }

        while(distanciaCamara < -anchoMundo)
        {
            offsetRepeticionX -= anchoMundo;
            distanciaCamara += anchoMundo;
        }

        return offsetRepeticionX;
    }
}
