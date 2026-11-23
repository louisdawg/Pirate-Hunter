using UnityEngine;

/// <summary>
/// Baut die Spielwelt beim Start der Szene auf. In der Szene selbst liegt nur
/// dieses eine GameObject, damit man alle Gegner, Inseln und Projektile an
/// einer Stelle im Code findet.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    public static GameBootstrap Instance { get; private set; }

    [Header("Grafiken")]
    [SerializeField] private Sprite _playerSprite;
    [SerializeField] private Sprite _pirateSprite;
    [SerializeField] private Sprite _islandSprite;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private GameObject _bigExplosionPrefab;

    public Sprite PlayerSprite => _playerSprite;
    public Sprite PirateSprite => _pirateSprite;
    public GameObject ExplosionPrefab => _explosionPrefab;
    public GameObject BigExplosionPrefab => _bigExplosionPrefab;

    public const float WorldHalfWidth = 26f;
    public const float WorldHalfHeight = 16f;

    private void Awake()
    {
        Instance = this;
        GameSession.Reset();

        BuildPlayer();
        BuildIslands();
        BuildBoundaries();
        BuildCannons();
        BuildPirates();
        BuildMines();
        SetupCamera();

        GameSession.CaptureEnemyCount();
        gameObject.AddComponent<GameHud>();
    }

    private void BuildPlayer()
    {
        var ship = new GameObject("Player");
        ship.tag = "Player";
        ship.layer = GameLayers.Player;
        ship.transform.position = Vector3.zero;
        ship.transform.localScale = Vector3.one * 1.6f;

        var renderer = ship.AddComponent<SpriteRenderer>();
        renderer.sprite = _playerSprite;
        renderer.sortingOrder = 4;

        var body = ship.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var collider = ship.AddComponent<CircleCollider2D>();
        collider.radius = 0.3f;

        ship.AddComponent<PlayerController>();
        GameSession.Player = ship.transform;
    }

    private void BuildIslands()
    {
        CreateIsland(new Vector2(-6.2f, -0.4f), 1.8f, 5.2f);
        CreateIsland(new Vector2(6.2f, -0.4f), 1.8f, 5.2f);
        CreateIsland(new Vector2(-10.5f, -7.2f), 1.6f, 3.8f);
        CreateIsland(new Vector2(10.5f, -7.2f), 1.6f, 3.8f);
    }

    /// <summary>Platziert eine Kapsel-Insel aus tiles_sheet.png.</summary>
    private void CreateIsland(Vector2 position, float width, float height)
    {
        var island = new GameObject("Island");
        island.transform.position = position;

        // Der Kapsel-Sprite ist 44 x 108 Pixel groß; bei 64 PPU entspricht das 0,6875 x 1,6875 Units.
        island.transform.localScale = new Vector3(width / 0.6875f, height / 1.6875f, 1f);
        island.layer = GameLayers.Obstacle;

        var renderer = island.AddComponent<SpriteRenderer>();
        renderer.sprite = _islandSprite != null ? _islandSprite : SpriteFactory.Island;
        renderer.sortingOrder = -5;

        var collider = island.AddComponent<CapsuleCollider2D>();
        collider.direction = CapsuleDirection2D.Vertical;
        collider.size = new Vector2(0.6f, 1.55f);
    }

    private void BuildCannons()
    {
        CreateCannon(new Vector2(6.5f, 5.5f), Vector2.left);
        CreateCannon(new Vector2(-6.5f, 6f), Vector2.right);
        CreateCannon(new Vector2(7.5f, -6f), Vector2.left);
        CreateCannon(new Vector2(-7f, -6f), Vector2.right);
    }

    private void CreateCannon(Vector2 position, Vector2 facing)
    {
        var cannon = new GameObject("Enemy Cannon");
        cannon.transform.position = position;
        cannon.layer = GameLayers.Enemy;
        cannon.tag = "Enemy";
        cannon.AddComponent<CircleCollider2D>();
        cannon.AddComponent<EnemyCannon>().Configure(facing);
    }

    /// <summary>Erzeugt eine feindliche Kanonenkugel, die geradeaus fliegt.</summary>
    public void SpawnCannonball(Vector2 position, Vector2 direction)
    {
        var projectile = new GameObject("Cannonball");
        projectile.transform.position = position;
        projectile.transform.localScale = Vector3.one * 0.3f;

        var renderer = projectile.AddComponent<SpriteRenderer>();
        renderer.sprite = SpriteFactory.Circle;
        renderer.color = new Color(0.12f, 0.12f, 0.15f);
        renderer.sortingOrder = 7;

        var body = projectile.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.bodyType = RigidbodyType2D.Kinematic;
        // Ohne diese Option meldet ein kinematischer Trigger keine Treffer an
        // statischen Collidern (Inseln, Kanonen), sondern fliegt hindurch.
        body.useFullKinematicContacts = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var collider = projectile.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;
        collider.isTrigger = true;

        var script = projectile.AddComponent<Cannonball>();
        script.Initialize(direction, 7.5f);
    }

    private void BuildPirates()
    {
        CreatePirate(new Vector2(17f, 2f));
        CreatePirate(new Vector2(-17f, -2f));
    }

    private void CreatePirate(Vector2 position)
    {
        var ship = new GameObject("Pirate Ship");
        ship.tag = "Enemy";
        ship.layer = GameLayers.Enemy;
        ship.transform.position = position;
        ship.transform.localScale = Vector3.one * 1.7f;

        var renderer = ship.AddComponent<SpriteRenderer>();
        renderer.sprite = _pirateSprite;
        renderer.sortingOrder = 4;

        var body = ship.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var collider = ship.AddComponent<CircleCollider2D>();
        collider.radius = 0.3f;

        ship.AddComponent<ChasingPirate>();
    }

    /// <summary>Erzeugt die Explosivkanonenkugel des Spielers.</summary>
    public void SpawnExplosiveCannonball(Vector2 position, Vector2 direction)
    {
        var projectile = new GameObject("Explosive Cannonball");
        projectile.transform.position = position;
        projectile.transform.localScale = Vector3.one * 0.45f;

        var renderer = projectile.AddComponent<SpriteRenderer>();
        renderer.sprite = SpriteFactory.Circle;
        renderer.color = new Color(1f, 0.55f, 0.1f);
        renderer.sortingOrder = 7;

        var glow = new GameObject("Glow");
        glow.transform.SetParent(projectile.transform, false);
        glow.transform.localScale = Vector3.one * 2f;
        var glowRenderer = glow.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = SpriteFactory.Ring;
        glowRenderer.color = new Color(1f, 0.35f, 0.1f, 0.5f);
        glowRenderer.sortingOrder = 6;

        var body = projectile.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.bodyType = RigidbodyType2D.Kinematic;
        body.useFullKinematicContacts = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var collider = projectile.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;
        collider.isTrigger = true;

        projectile.AddComponent<ExplosiveCannonball>().Initialize(direction, 9f);
    }

    private void BuildMines()
    {
        CreateMine(new Vector2(4.5f, -3f));
        CreateMine(new Vector2(-5f, 2.5f));
        CreateMine(new Vector2(12f, 3f));
        CreateMine(new Vector2(-12f, 3f));
    }

    private void CreateMine(Vector2 position)
    {
        var mine = new GameObject("Sea Mine");
        mine.transform.position = position;

        var collider = mine.AddComponent<CircleCollider2D>();
        collider.radius = 0.4f;
        collider.isTrigger = true;

        mine.AddComponent<SeaMine>();
    }

    /// <summary>Erzeugt einen zielsuchenden Torpedo.</summary>
    public void SpawnTorpedo(Vector2 position, Vector2 direction)
    {
        var torpedo = new GameObject("Torpedo");
        torpedo.transform.position = position;
        torpedo.transform.localScale = Vector3.one * 1.1f;

        var renderer = torpedo.AddComponent<SpriteRenderer>();
        renderer.sprite = SpriteFactory.Torpedo;
        renderer.sortingOrder = 7;

        var body = torpedo.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.bodyType = RigidbodyType2D.Kinematic;
        body.useFullKinematicContacts = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var collider = torpedo.AddComponent<CircleCollider2D>();
        collider.radius = 0.22f;
        collider.isTrigger = true;

        torpedo.AddComponent<Torpedo>().Initialize(direction);
    }

    /// <summary>Spielt das Explosion-Prefab an einer Position ab.</summary>
    public void SpawnExplosion(Vector3 position, bool big = false)
    {
        GameObject prefab = big ? _bigExplosionPrefab : _explosionPrefab;
        if (prefab != null)
        {
            var explosion = Instantiate(prefab, position, Quaternion.identity);
            if (big)
            {
                explosion.transform.localScale = prefab.transform.localScale * 4f;
            }

            return;
        }

        // Fallback, damit Treffer auch ohne zugewiesenes Prefab sichtbar sind.
        var burstObject = new GameObject(big ? "BigExplosion" : "Explosion");
        burstObject.transform.position = position;

        var renderer = burstObject.AddComponent<SpriteRenderer>();
        renderer.sprite = SpriteFactory.Circle;
        renderer.sortingOrder = 8;

        var burst = burstObject.AddComponent<ExplosionBurst>();
        burst.Initialize(
            new Color(1f, 0.55f, 0.12f, 1f),
            big ? 1f : 0.5f,
            big ? 8f : 3f);
    }

    private void BuildBoundaries()
    {
        CreateWall("Boundary West", new Vector2(-WorldHalfWidth - 0.5f, 0f), new Vector2(1f, WorldHalfHeight * 2f + 4f));
        CreateWall("Boundary East", new Vector2(WorldHalfWidth + 0.5f, 0f), new Vector2(1f, WorldHalfHeight * 2f + 4f));
        CreateWall("Boundary North", new Vector2(0f, WorldHalfHeight + 0.5f), new Vector2(WorldHalfWidth * 2f + 4f, 1f));
        CreateWall("Boundary South", new Vector2(0f, -WorldHalfHeight - 0.5f), new Vector2(WorldHalfWidth * 2f + 4f, 1f));
    }

    private void CreateWall(string name, Vector2 position, Vector2 size)
    {
        var wall = new GameObject(name);
        wall.transform.position = position;
        wall.layer = GameLayers.Obstacle;

        var collider = wall.AddComponent<BoxCollider2D>();
        collider.size = size;
    }

    private void SetupCamera()
    {
        var camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        camera.orthographicSize = 6.2f;
        camera.backgroundColor = new Color(0.12f, 0.44f, 0.56f, 1f);
        camera.transform.position = new Vector3(0f, 0f, -10f);

        var follow = camera.GetComponent<CameraFollow>();
        if (follow == null)
        {
            follow = camera.gameObject.AddComponent<CameraFollow>();
        }
        follow.SetTarget(GameSession.Player);
    }
}
