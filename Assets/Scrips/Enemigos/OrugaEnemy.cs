using UnityEngine;

// Enemigo concreto de la oruga/sandbug.
// Por ahora usa la logica comun de EnemyBase; aca se agregaria IA, animaciones o ataques propios.
[RequireComponent(typeof(EnemyPatrolMovement))]
public class OrugaEnemy : EnemyBase
{
}
