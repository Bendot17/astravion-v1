
using System.Collections.Generic;
using UnityEngine;

public class AstravionBootstrap : MonoBehaviour
{
    class Unit
    {
        public GameObject obj;
        public float hp;
        public float speed;
        public float attackCooldown;
        public bool enemy;
    }

    class Projectile
    {
        public GameObject obj;
        public Vector3 direction;
        public float life;
        public float damage;
        public bool ultimate;
        public float trailTimer;
    }

    GameObject hero;
    GameObject enemyBase;
    Camera gameCamera;

    readonly List<Unit> units = new List<Unit>();
    readonly List<Projectile> projectiles = new List<Projectile>();

    Material blueMat, redMat, groundMat, goldMat, fireMat;
    Vector2 moveInput;
    float heroHP = 1000;
    float heroMaxHP = 1000;
    float attackTimer;
    float skill1Timer;
    float skill2Timer;
    float ultimateTimer;
    float spawnTimer;
    int level = 1;
    int kills;
    string statusText = "ASTRAVION - MODE LATIHAN";

    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;

        CreateMaterials();
        CreateArena();
        CreateHero();
        CreateCamera();
        SpawnWave();
    }

    void CreateMaterials()
    {
        blueMat = MakeMaterial(new Color(0.05f, 0.35f, 1f));
        redMat = MakeMaterial(new Color(1f, 0.08f, 0.04f));
        groundMat = MakeMaterial(new Color(0.12f, 0.23f, 0.16f));
        goldMat = MakeMaterial(new Color(1f, 0.65f, 0.08f));
        fireMat = MakeMaterial(new Color(1f, 0.12f, 0.01f));
    }

    Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material m = new Material(shader);
        m.color = color;
        return m;
    }

    GameObject Cube(string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.position = pos;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().material = mat;
        return g;
    }

    GameObject Sphere(string name, Vector3 pos, float size, Material mat)
    {
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        g.name = name;
        g.transform.position = pos;
        g.transform.localScale = Vector3.one * size;
        g.GetComponent<Renderer>().material = mat;
        return g;
    }

    void CreateArena()
    {
        Cube("Battlefield", new Vector3(0, -0.6f, 0),
            new Vector3(75, 1, 75), groundMat);

        // Three battle lanes
        Material lane = MakeMaterial(new Color(0.34f, 0.32f, 0.25f));
        Cube("Top Lane", new Vector3(0, -0.04f, 22),
            new Vector3(62, 0.12f, 5), lane);
        Cube("Middle Lane", new Vector3(0, -0.03f, 0),
            new Vector3(62, 0.12f, 5), lane);
        Cube("Bottom Lane", new Vector3(0, -0.04f, -22),
            new Vector3(62, 0.12f, 5), lane);

        // Decorative jungle areas
        for (int i = 0; i < 18; i++)
        {
            float x = Random.Range(-27, 27);
            float z = Random.Range(-30, 30);
            if (Mathf.Abs(z) < 5 || Mathf.Abs(z - 22) < 4 ||
                Mathf.Abs(z + 22) < 4) continue;

            GameObject tree = Sphere("Jungle Tree",
                new Vector3(x, 1.2f, z), 2.5f, groundMat);
            tree.transform.localScale = new Vector3(1.2f, 2.2f, 1.2f);
        }

        // Bases
        Cube("Blue Base", new Vector3(-29, 1, 0),
            new Vector3(5, 3, 8), blueMat);

        enemyBase = Cube("Red Base", new Vector3(29, 1, 0),
            new Vector3(5, 3, 8), redMat);

        // Turrets on middle lane
        CreateTower(-17, blueMat);
        CreateTower(-5, blueMat);
        CreateTower(17, redMat);
        CreateTower(5, redMat);

        // Outer arena walls
        Cube("North Wall", new Vector3(0, 1, 37),
            new Vector3(75, 3, 1), groundMat);
        Cube("South Wall", new Vector3(0, 1, -37),
            new Vector3(75, 3, 1), groundMat);
    }

    void CreateTower(float x, Material mat)
    {
        GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tower.name = "Battle Turret";
        tower.transform.position = new Vector3(x, 1.5f, 0);
        tower.transform.localScale = new Vector3(2.5f, 2, 2.5f);
        tower.GetComponent<Renderer>().material = mat;

        Sphere("Turret Crystal", new Vector3(x, 4, 0),
            1.3f, goldMat);
    }

    void CreateHero()
    {
        hero = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        hero.name = "KAEL";
        hero.transform.position = new Vector3(-22, 1, 0);
        hero.transform.localScale = new Vector3(1.2f, 1.5f, 1.2f);
        hero.GetComponent<Renderer>().material = blueMat;

        GameObject head = Sphere("KAEL Helmet",
            hero.transform.position + Vector3.up * 1.5f,
            0.8f, goldMat);
        head.transform.SetParent(hero.transform);
        head.transform.localPosition = new Vector3(0, 1, 0);

        GameObject sword = Cube("KAEL Fire Sword",
            hero.transform.position + new Vector3(0.8f, 0.3f, 0),
            new Vector3(0.25f, 1.5f, 0.3f), fireMat);
        sword.transform.SetParent(hero.transform);
        sword.transform.localPosition = new Vector3(0.8f, 0.2f, 0);
        sword.transform.localRotation = Quaternion.Euler(0, 0, -25);
    }

    void CreateCamera()
    {
        gameCamera = Camera.main;
        if (gameCamera == null)
        {
            GameObject cam = new GameObject("Main Camera");
            gameCamera = cam.AddComponent<Camera>();
            cam.tag = "MainCamera";
        }

        gameCamera.transform.position = hero.transform.position +
            new Vector3(-8, 17, -13);
        gameCamera.transform.LookAt(hero.transform.position);
        gameCamera.fieldOfView = 60;
        gameCamera.backgroundColor = new Color(0.08f, 0.12f, 0.18f);
    }

    void Update()
    {
        if (hero == null) return;

        ReadMovement();
        MoveHero();

        attackTimer -= Time.deltaTime;
        skill1Timer -= Time.deltaTime;
        skill2Timer -= Time.deltaTime;
        ultimateTimer -= Time.deltaTime;
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0)
        {
            SpawnWave();
            spawnTimer = 12f;
        }

        UpdateUnits();
        UpdateProjectiles();
    }

    void ReadMovement()
    {
        moveInput = Vector2.zero;

        // Touch joystick on the left side of the screen
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            Vector2 p = t.position;

            if (p.x < Screen.width * 0.43f &&
                p.y < Screen.height * 0.75f)
            {
                Vector2 center = new Vector2(
                    Screen.width * 0.17f,
                    Screen.height * 0.22f);

                moveInput = Vector2.ClampMagnitude(
                    (p - center) / 100f, 1f);
            }
        }

        // Keyboard support for editor testing
        if (Input.GetKey(KeyCode.W)) moveInput.y = 1;
        if (Input.GetKey(KeyCode.S)) moveInput.y = -1;
        if (Input.GetKey(KeyCode.A)) moveInput.x = -1;
        if (Input.GetKey(KeyCode.D)) moveInput.x = 1;
    }

    void MoveHero()
    {
        Vector3 direction = new Vector3(
            moveInput.x, 0, moveInput.y);

        if (direction.magnitude > 0.1f)
        {
            hero.transform.position += direction.normalized *
                8f * Time.deltaTime;

            hero.transform.rotation =
                Quaternion.LookRotation(direction);
        }

        Vector3 p = hero.transform.position;
        p.x = Mathf.Clamp(p.x, -33, 33);
        p.z = Mathf.Clamp(p.z, -33, 33);
        p.y = 1;
        hero.transform.position = p;
    }

    void LateUpdate()
    {
        if (gameCamera == null || hero == null) return;

        Vector3 target = hero.transform.position +
            new Vector3(-8, 17, -13);

        gameCamera.transform.position = Vector3.Lerp(
            gameCamera.transform.position, target,
            Time.deltaTime * 5f);

        gameCamera.transform.LookAt(hero.transform.position);
    }

    void SpawnWave()
    {
        for (int i = 0; i < 3; i++)
        {
            CreateUnit(new Vector3(18 + i * 1.5f, 1, -2 + i * 2),
                true, 100);
        }
    }

    void CreateUnit(Vector3 position, bool enemy, float hp)
    {
        GameObject obj = GameObject.CreatePrimitive(
            PrimitiveType.Capsule);

        obj.name = enemy ? "Enemy Minion" : "Ally Minion";
        obj.transform.position = position;
        obj.transform.localScale = Vector3.one * 0.8f;
        obj.GetComponent<Renderer>().material =
            enemy ? redMat : blueMat;

        Unit u = new Unit();
        u.obj = obj;
        u.hp = hp;
        u.speed = 2f;
        u.enemy = enemy;
        units.Add(u);
    }

    void UpdateUnits()
    {
        for (int i = units.Count - 1; i >= 0; i--)
        {
            Unit u = units[i];

            if (u.obj == null)
            {
                units.RemoveAt(i);
                continue;
            }

            Vector3 target = u.enemy
                ? hero.transform.position
                : enemyBase.transform.position;

            Vector3 dir = target - u.obj.transform.position;
            dir.y = 0;

            if (dir.magnitude > 2.2f)
            {
                u.obj.transform.position += dir.normalized *
                    u.speed * Time.deltaTime;

                if (dir.sqrMagnitude > 0.1f)
                    u.obj.transform.rotation =
                        Quaternion.LookRotation(dir);
            }
            else if (u.enemy)
            {
                u.attackCooldown -= Time.deltaTime;

                if (u.attackCooldown <= 0)
                {
                    heroHP -= 15;
                    u.attackCooldown = 1.5f;

                    if (heroHP <= 0)
                    {
                        heroHP = heroMaxHP;
                        hero.transform.position =
                            new Vector3(-22, 1, 0);
                        statusText = "KAEL KEMBALI KE MARKAS!";
                    }
                }
            }
        }
    }

    Unit FindNearestEnemy()
    {
        Unit best = null;
        float distance = 100f;

        foreach (Unit u in units)
        {
            if (!u.enemy || u.obj == null) continue;

            float d = Vector3.Distance(
                hero.transform.position, u.obj.transform.position);

            if (d < distance)
            {
                distance = d;
                best = u;
            }
        }

        return best;
    }

    void Attack()
    {
        if (attackTimer > 0) return;

        Unit target = FindNearestEnemy();

        if (target != null &&
            Vector3.Distance(hero.transform.position,
                target.obj.transform.position) < 5f)
        {
            target.hp -= 35;
            attackTimer = 0.5f;
            statusText = "SERANGAN KAEL!";

            if (target.hp <= 0)
            {
                Destroy(target.obj);
                units.Remove(target);
                kills++;
                level++;
                statusText = "MUSUH DIKALAHKAN!";
            }
        }
        else
        {
            statusText = "DEKATI MINION MUSUH!";
            attackTimer = 0.3f;
        }
    }

    void Skill1()
    {
        if (skill1Timer > 0) return;

        skill1Timer = 4f;
        ShootFire(12, 35, false);
        statusText = "SKILL 1 - FIRE BLAST!";
    }

    void Skill2()
    {
        if (skill2Timer > 0) return;

        skill2Timer = 7f;
        ShootFire(16, 65, false);
        statusText = "SKILL 2 - FIRE STRIKE!";
    }

    void Ultimate()
    {
        if (ultimateTimer > 0) return;

        ultimateTimer = 20f;
        ShootFire(24, 180, true);
        statusText = "ULTIMATE KAEL - NAGA API MERAH!";
    }

    void ShootFire(float speed, float damage, bool ultimate)
    {
        Vector3 direction = hero.transform.forward;

        if (direction.sqrMagnitude < 0.1f)
            direction = Vector3.right;

        GameObject fire = Sphere(
            ultimate ? "RED FIRE DRAGON" : "Fire Projectile",
            hero.transform.position + Vector3.up,
            ultimate ? 2.2f : 0.9f,
            fireMat);

        Projectile p = new Projectile();
        p.obj = fire;
        p.direction = direction.normalized;
        p.life = ultimate ? 3f : 2f;
        p.damage = damage;
        p.ultimate = ultimate;
        projectiles.Add(p);
    }

    void UpdateProjectiles()
    {
        for (int i = projectiles.Count - 1; i >= 0; i--)
        {
            Projectile p = projectiles[i];

            if (p.obj == null)
            {
                projectiles.RemoveAt(i);
                continue;
            }

            p.obj.transform.position += p.direction *
                (p.ultimate ? 17f : 13f) * Time.deltaTime;

            p.life -= Time.deltaTime;
            p.trailTimer -= Time.deltaTime;

            if (p.ultimate && p.trailTimer <= 0)
            {
                Sphere("Dragon Fire Trail",
                    p.obj.transform.position,
                    0.8f, fireMat);
                p.trailTimer = 0.08f;
            }

            for (int j = units.Count - 1; j >= 0; j--)
            {
                Unit u = units[j];
                if (!u.enemy || u.obj == null) continue;

                if (Vector3.Distance(p.obj.transform.position,
                    u.obj.transform.position) < 2.5f)
                {
                    u.hp -= p.damage;

                    if (u.hp <= 0)
                    {
                        Destroy(u.obj);
                        units.RemoveAt(j);
                        kills++;
                    }

                    p.life = 0;
                    break;
                }
            }

            if (p.life <= 0)
            {
                Destroy(p.obj);
                projectiles.RemoveAt(i);
            }
        }
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = 22;
        GUI.skin.button.fontSize = 20;
        GUI.skin.box.fontSize = 18;

        // Top information bar
        GUI.Box(new Rect(10, 10, 330, 105),
            "ASTRAVION  |  MODE LATIHAN");

        GUI.Label(new Rect(25, 45, 300, 30),
            "KAEL  LV." + level + "    KILL: " + kills);

        GUI.Label(new Rect(25, 75, 300, 30),
            "HP: " + Mathf.CeilToInt(heroHP) + " / 1000");

        GUI.Box(new Rect(350, 10, 300, 45), statusText);

        // Joystick guide
        GUI.Box(new Rect(25, Screen.height - 175, 155, 145),
            "JOYSTICK");

        // Action buttons
        float right = Screen.width - 110;

        if (GUI.Button(new Rect(right - 75,
            Screen.height - 100, 85, 85), "ATTACK"))
            Attack();

        if (GUI.Button(new Rect(right - 180,
            Screen.height - 85, 85, 70), "SKILL 1"))
            Skill1();

        if (GUI.Button(new Rect(right - 280,
            Screen.height - 85, 85, 70), "SKILL 2"))
            Skill2();

        if (GUI.Button(new Rect(right - 385,
            Screen.height - 85, 90, 70), "ULTIMATE"))
            Ultimate();

        GUI.Label(new Rect(20, Screen.height - 25, 400, 25),
            "Gerakkan KAEL dengan sentuhan di sisi kiri layar");
    }
}
