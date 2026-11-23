using UnityEngine;

/// <summary>
/// Feindliche Kanone. Sie prüft per Raycast, ob der Spieler im Sichtbereich
/// und im Schussfeld ist, richtet sich langsam auf ihn aus und feuert in
/// festen Abständen Cannonballs ab. Die Kanone und der Warnring sind Sprites
/// aus dem Art-Ordner.
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

    private Transform _muzzle;
    private SpriteRenderer _warningRing;
    private Vector2 _idleFacing = Vector2.left;
    private float _nextFireTime;
    private Sprite _cannonSprite;
    private Sprite _warningSprite;

    private void Awake()
    {
        gameObject.tag = "Enemy";
        gameObject.layer = GameLayers.Enemy;
        transform.localScale = Vector3.one * 1.8f;

        var collider = GetComponent<CircleCollider2D>();
        collider.radius = 0.3f;

        GameSession.RegisterEnemy(gameObject);
        _nextFireTime = Time.time + 1.2f;
    }

    private void OnDestroy()
    {
        GameSession.UnregisterEnemy(gameObject);
    }

    /// <summary>Setzt Blickrichtung sowie Kanonen- und Warnring-Sprite.</summary>
    public void Configure(Vector2 facing, Sprite cannonSprite, Sprite warningSprite)
    {
        _cannonSprite = cannonSprite;
        _warningSprite = warningSprite;
        _idleFacing = facing.sqrMagnitude > 0.001f ? facing.normalized : Vector2.left;

        BuildVisuals();
        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            Mathf.Atan2(_idleFacing.y, _idleFacing.x) * Mathf.Rad2Deg);
    }

    private void BuildVisuals()
    {
        var renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = _cannonSprite != null ? _cannonSprite : SpriteFactory.Circle;
        renderer.color = _cannonSprite != null ? Color.white : new Color(0.28f, 0.34f, 0.4f);
        renderer.sortingOrder = 3;

        var muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(transform, false);
        muzzle.transform.localPosition = new Vector3(0.3f, 0f, 0f);
        _muzzle = muzzle.transform;

        var warning = new GameObject("WarningRing");
        warning.transform.SetParent(transform, false);

        // Das Ring-Sprite ist 52 px groß (0,8125 Units bei 64 PPU). Die Kanone
        // ist auf 1.8 skaliert, daher wird die lokale Ringgröße angepasst.
        float ringSize = _warningSprite != null ? 0.8125f : 1f;
        warning.transform.localScale = Vector3.one * (1.9f / (ringSize * 1.8f));

        _warningRing = warning.AddComponent<SpriteRenderer>();
        _warningRing.sprite = _warningSprite != null ? _warningSprite : SpriteFactory.Ring;
        _warningRing.color = new Color(1f, 0.25f, 0.2f, 0.85f);
        _warningRing.sortingOrder = 2;
        _warningRing.enabled = false;
    }

    private void Update()
    {
        if (GameSession.IsGameOver || GameSession.Player == null)
        {
            SetWarning(false);
            Face(_idleFacing);
            return;
        }

        Vector2 origin = transform.position;
        Vector2 toPlayer = (Vector2)GameSession.Player.position - origin;
        float distance = toPlayer.magnitude;
        if (distance < 0.01f)
        {
            return;
        }

        Vector2 direction = toPlayer / distance;
        bool inRange = distance <= sightRange;
        bool inFieldOfView = Vector2.Angle(transform.right, direction) <= fieldOfView * 0.5f;

        // Layermask: Player + Obstacle. Die Kanone selbst liegt auf Enemy und
        // kann deshalb nie vom eigenen Raycast getroffen werden. Inseln
        // blockieren die Sichtlinie.
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, sightRange, GameLayers.PlayerOrObstacleMask);
        bool seesPlayer = inRange && inFieldOfView && hit.collider != null && hit.collider.CompareTag("Player");

        if (seesPlayer)
        {
            Debug.DrawLine(origin, hit.point, Color.magenta);
            Face(direction);
            SetWarning(true);

            float barrelAngle = transform.eulerAngles.z;
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
            SetWarning(false);
            Face(_idleFacing);
        }
    }

    private void Face(Vector2 direction)
    {
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            Quaternion.Euler(0f, 0f, targetAngle),
            turnSpeed * Time.deltaTime);
    }

    private void SetWarning(bool visible)
    {
        if (_warningRing != null)
        {
            _warningRing.enabled = visible;
        }
    }

    private void Fire(Vector2 direction)
    {
        if (_muzzle == null)
        {
            return;
        }

        GameBootstrap.Instance.SpawnCannonball(_muzzle.position, direction);
    }
}
