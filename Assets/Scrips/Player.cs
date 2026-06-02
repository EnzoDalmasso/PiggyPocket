using UnityEngine;

public class Player : MonoBehaviour
{
    //Fisicas
    private Rigidbody2D rb;

    //Velocidad de movimiento
    [SerializeField] public float velocidad = 5;
    private float movimientoH;

    //Salto
    public float fuerzaSalto = 4;
    private bool estaSuelo;
    public Transform checkSuelo;
    public float radioSuelo= 0.1f;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

 
    void Update()
    {
        movimientoH = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(movimientoH * velocidad,rb.linearVelocity.y);

        if(movimientoH != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(movimientoH), 1, 1);
        }
    }
}
