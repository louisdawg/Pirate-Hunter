using UnityEngine;

/// <summary>
/// Feindliche Kanone. Sie prüft per Raycast, ob der Spieler im Sichtbereich
/// und im Schussfeld ist, richtet das Rohr langsam auf ihn aus und feuert in
/// festen Abständen Cannonballs ab.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class EnemyCannon : MonoBehaviour
{
    [Header("Sicht")]
    public float sightRange = 7.5f;
    public float fieldOfView = 140f;
    public float turnSpeed = 140f;

    [Header("Feuer")]
    public float fireInterval = 1.8f;

    private Transform _barrelPivot;
    private Transform _muzzle;
    private SpriteRenderer _warningRing;
    private Vector2 _idleFacing = Vector2.left;
    private float _nextFireTime;

    private void Awake()
    {
        gameObject.tag = "Enemy";
        gameObject.layer = GameLayers.Enemy;
        transform.localScale = Vector3.one * 1.25f;

        var collider = GetComponent<CircleCollider2D>();
        collider.radius = 0.55f;

        BuildVisuals();
        GameSession.RegisterEnemy(gameObject);
        _nextFireTime = Time.time + 1.2f;
    }

    private void OnDestroy()
    {
        GameSession.UnregisterEnemy(gameObject);
    }

    /// <summary>Legt die Blickrichtung fest, in die die Kanone ohne Ziel zeigt.</summary>
    public void Configure(Vector2 facing)
    {
        _idleFacing = facing.sqrMagnitude > 0.001f ? facing.normalized : Vector2.left;
        _barrelPivot.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(_idleFacing.y, _idleFacing.x) * Mathf.Rad2Deg);
    }

    private void BuildVisuals()
    {
        var baseRenderer = gameObject.AddComponent<SpriteRenderer>();
        baseRenderer.sprite = SpriteFactory.Circle;
        baseRenderer.color = new Color(0.28f, 0.34f, 0.4f);
        baseRenderer.sortingOrder = 3;

        var barrelPivot = new GameObject("BarrelPivot");
        barrelPivot.transform.SetParent(transform, false);
        _barrelPivot = barrelPivot.transform;

        var barrel = new GameObject("Barrel");
        barrel.transform.SetParent(_barrelPivot, false);
        barrel.transform.localPosition = new Vector3(0.5f, 0f, 0f);
        barrel.transform.localScale = new Vector3(1.1f, 0.3f, 1f);
        var barrelRenderer = barrel.AddComponent<SpriteRenderer>();
        barrelRenderer.sprite = SpriteFactory.Circle;
        barrelRenderer.color = new Color(0.32f, 0.22f, 0.16f);
        barrelRenderer.sortingOrder = 5;

        var muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(_barrelPivot, false);
        muzzle.transform.localPosition = new Vector3(1.05f, 0f, 0f);
        _muzzle = muzzle.transform;

        var warning = new GameObject("WarningRing");
        warning.transform.SetParent(transform, false);
        warning.transform.localScale = Vector3.one * 1.7f;
        _warningRing = warning.AddComponent<SpriteRenderer>();
        _warningRing.sprite = SpriteFactory.Ring;
        _warningRing.color = new Color(1f, 0.25f, 0.2f, 0.85f);
        _warningRing.sortingOrder = 2;
        _warningRing.enabled = false;
    }

    private void Update()
    {
        if (GameSession.IsGameOver || GameSession.Player == null)
        {
            _warningRing.enabled = false;
            RotateBarrel(_idleFacing);
            return;
        }

        Vector2 origin = _barrelPivot.position;
        Vector2 toPlayer = (Vector2)GameSession.Player.position - origin;
        float distance = toPlayer.magnitude;
        if (distance < 0.01f)
        {
            return;
        }

        Vector2 direction = toPlayer / distance;
        bool inRange = distance <= sightRange;
        bool inFieldOfView = Vector2.Angle(_barrelPivot.right, direction) <= fieldOfView * 0.5f;

        // Layermask: Player + Obstacle. Die Kanone selbst liegt auf Enemy und
        // kann deshalb nie vom eigenen Raycast getroffen werden. Inseln
        // blockieren die Sichtlinie.
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, sightRange, GameLayers.PlayerOrObstacleMask);
        bool seesPlayer = inRange && inFieldOfView && hit.collider != null && hit.collider.CompareTag("Player");

        if (seesPlayer)
        {
            Debug.DrawLine(origin, hit.point, Color.magenta);
            RotateBarrel(direction);
            _warningRing.enabled = true;

            float barrelAngle = _barrelPivot.eulerAngles.z;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bool aligned = Mathf.Abs(Mathf.DeltaAngle(barrelAngle, targetAngle)) < 10f;

            if (aligned && Time.time >= _nextFireTime)
            {
                Fire(direction);
                _nextFireTime = Time.time + fireInterval;
            }
        }
        else
        {
            _warningRing.enabled = false;
            RotateBarrel(_idleFacing);
        }
    }

    private void RotateBarrel(Vector2 direction)
    {
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _barrelPivot.rotation = Quaternion.RotateTowards(
            _barrelPivot.rotation,
            Quaternion.Euler(0f, 0f, targetAngle),
            turnSpeed * Time.deltaTime);
    }

    private void Fire(Vector2 direction)
    {
        GameBootstrap.Instance.SpawnCannonball(_muzzle.position, direction);
    }
}
