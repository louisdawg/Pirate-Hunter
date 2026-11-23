using UnityEngine;

/// <summary>
/// Explosivkanone des Spielers. Beim Aufprall werden per OverlapCircleAll
/// alle Gegner im Umkreis von zwei Metern zerstört.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class ExplosiveCannonball : MonoBehaviour
{
    public float blastRadius = 2f;
    public float lifetime = 4f;

    private Rigidbody2D _body;
    private bool _exploded;

    public void Initialize(Vector2 direction, float speed)
    {
        _body = GetComponent<Rigidbody2D>();
        _body.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_exploded || other.GetComponentInParent<PlayerController>() != null)
        {
            return;
        }

        bool hitEnemy = other.CompareTag("Enemy");
        bool hitObstacle = other.gameObject.layer == GameLayers.Obstacle;

        if (hitEnemy || hitObstacle)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (_exploded)
        {
            return;
        }

        _exploded = true;
        Vector3 center = transform.position;
        GameBootstrap.Instance.SpawnExplosion(center, true);

        // Alle Collider auf dem Enemy-Layer im Radius von 2 Metern finden.
        Collider2D[] collidersInRange = Physics2D.OverlapCircleAll(center, blastRadius, GameLayers.EnemyMask);
        foreach (Collider2D hit in collidersInRange)
        {
            if (hit == null)
            {
                continue;
            }

            GameObject enemy = hit.attachedRigidbody != null
                ? hit.attachedRigidbody.gameObject
                : hit.gameObject;

            GameBootstrap.Instance.SpawnExplosion(enemy.transform.position);
            Destroy(enemy);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0.1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, blastRadius);
    }
}
