using UnityEngine;

// Contrato simple para cualquier objeto que pueda recibir dano.
// Un enemigo, jefe u objeto rompible puede implementar esta interfaz.
public interface IDamageable
{
    void RecibirDano(int cantidad, Vector2 puntoGolpe, GameObject atacante);
}
