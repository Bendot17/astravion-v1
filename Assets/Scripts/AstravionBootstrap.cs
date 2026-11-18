
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class AstravionBootstrap : MonoBehaviour
{
    private const string FirebaseApiKey = "AIzaSyC6b_-kXqhYabmVjIjbXbcXl1LERa-ODmg";
    private const string FirebaseProjectId = "astravion-14175";

    [Serializable] private class AuthRequest
    {
        public string email;
        public string password;
        public bool returnSecureToken = true;
    }

    [Serializable] private class AuthResponse
    {
        public string idToken;
        public string localId;
        public string email;
        public AuthError error;
    }

    [Serializable] private class AuthError
    {
        public string message;
    }

    private string email = "";
    private string password = "";
    private string playerName = "Player";
    private string status = "ASTRAVION - Legends of Eternal War";
    private string idToken = "";
    private string playerId = "";

    private bool loggedIn;
    private bool showRegister;
    private bool showBattle;
    private bool showHeroes;
    private bool showSettings;
    private bool busy;

    private GameObject arena;
    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;

    private void Start()
    {
        Application.targetFrameRate = 60;
        CreateLighting();
    }

    private void CreateLighting()
    {
        if (Camera.main == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";

            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();

            camera.transform.position = new Vector3(0, 14, -19);
            camera.transform.rotation = Quaternion.Euler(32, 0, 0);
            camera.fieldOfView = 55;
        }

        RenderSettings.ambientLight = new Color(0.28f, 0.32f, 0.42f);
    }

    private void OnGUI()
    {
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            titleStyle.normal.textColor = new Color(1f, 0.78f, 0.25f);

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fixedHeight = 48
            };
        }

        float width = Mathf.Min(560, Screen.width - 32);

        GUILayout.BeginArea(
            new Rect((Screen.width - width) / 2, 20, width, Screen.height - 40),
            GUI.skin.box
        );

        GUILayout.Label("ASTRAVION", titleStyle);
        GUILayout.Label("LEGENDS OF ETERNAL WAR", Centered());
        GUILayout.Space(12);
        GUILayout.Label(status, Centered());

        if (busy)
            GUILayout.Label("Menghubungkan ke Firebase...", Centered());

        if (!loggedIn)
        {
            GUILayout.Label(showRegister ? "BUAT AKUN" : "LOGIN PEMAIN", Centered());

            GUILayout.Label("Email");
            email = GUILayout.TextField(email, 64);

            GUILayout.Label("Password");
            password = GUILayout.PasswordField(password, '*', 64);

            if (showRegister)
            {
                GUILayout.Label("Nama pemain");
                playerName = GUILayout.TextField(playerName, 24);
            }

            GUI.enabled = !busy;

            if (GUILayout.Button(showRegister ? "DAFTAR" : "MASUK", buttonStyle))
                StartCoroutine(AuthRoutine(showRegister));

            if (GUILayout.Button(
                showRegister ? "Kembali ke Login" : "Buat Akun Baru",
                buttonStyle))
            {
                showRegister = !showRegister;
            }

            GUI.enabled = true;
            GUILayout.EndArea();
            return;
        }

        GUILayout.Label(
            "Selamat datang, " +
            (string.IsNullOrEmpty(playerName) ? email : playerName),
            Centered()
        );

        GUILayout.Label("Player ID: " + playerId, Centered());

        if (GUILayout.Button("PILIH HERO", buttonStyle))
        {
            showHeroes = true;
            showBattle = false;
            showSettings = false;
        }

        if (GUILayout.Button("MULAI LATIHAN 3D", buttonStyle))
        {
            showBattle = true;
            showHeroes = false;
            showSettings = false;
            CreateArena();
        }

        if (GUILayout.Button("ROOM CUSTOM / ONLINE (TAHAP BERIKUTNYA)", buttonStyle))
        {
            status = "Room online memerlukan layanan multiplayer server.";
        }

        if (GUILayout.Button("PENGATURAN", buttonStyle))
        {
            showSettings = true;
            showHeroes = false;
            showBattle = false;
        }

        if (showHeroes)
            GUILayout.Label("KAEL - Pejuang Api | Hero contoh awal", Centered());

        if (showBattle)
            GUILayout.Label(
                "Arena latihan lokal aktif. Ini belum pertandingan online 5v5.",
                Centered()
            );

        if (showSettings)
            GUILayout.Label("ASTRAVION V1 | Android | Bahasa Indonesia", Centered());

        if (GUILayout.Button("KELUAR AKUN", buttonStyle))
        {
            loggedIn = false;
            idToken = "";
            playerId = "";
            status = "Silakan login kembali.";
        }

        GUILayout.EndArea();
    }

    private GUIStyle Centered()
    {
        return new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            fontSize = 15
        };
    }

    private IEnumerator AuthRoutine(bool register)
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            status = "Email dan password wajib diisi.";
            yield break;
        }

        if (password.Length < 6)
        {
            status = "Password Firebase minimal 6 karakter.";
            yield break;
        }

        busy = true;

        string method = register ? "signUp" : "signInWithPassword";

        string url =
            "https://identitytoolkit.googleapis.com/v1/accounts:" +
            method + "?key=" + FirebaseApiKey;

        AuthRequest payload = new AuthRequest
        {
            email = email.Trim(),
            password = password,
            returnSecureToken = true
        };

        string json = JsonUtility.ToJson(payload);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            AuthResponse response = null;

            if (!string.IsNullOrEmpty(request.downloadHandler.text))
            {
                try
                {
                    response = JsonUtility.FromJson<AuthResponse>(
                        request.downloadHandler.text
                    );
                }
                catch
                {
                    response = null;
                }
            }

            if (request.result == UnityWebRequest.Result.Success &&
                response != null &&
                !string.IsNullOrEmpty(response.idToken))
            {
                idToken = response.idToken;
                playerId = response.localId;
                loggedIn = true;
                showRegister = false;

                status = "Login Firebase berhasil.";

                PlayerPrefs.SetString("astravion_uid", playerId);
                PlayerPrefs.Save();

                StartCoroutine(SavePlayerProfile());
            }
            else
            {
                string error =
                    response != null && response.error != null
                    ? response.error.message
                    : request.error;

                status = "Firebase: " +
                    (string.IsNullOrEmpty(error)
                    ? "Login gagal. Periksa koneksi dan konfigurasi."
                    : error);
            }
        }

        busy = false;
    }

    private IEnumerator SavePlayerProfile()
    {
        string url =
            "https://firestore.googleapis.com/v1/projects/" +
            FirebaseProjectId +
            "/databases/(default)/documents/players/" +
            playerId + "?key=" + FirebaseApiKey;

        string safeName = string.IsNullOrWhiteSpace(playerName)
            ? email
            : playerName;

        string escapedName = safeName
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");

        string escapedEmail = email.Trim()
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");

        string body =
            "{\"fields\":{\"uid\":{\"stringValue\":\"" + playerId +
            "\"},\"displayName\":{\"stringValue\":\"" + escapedName +
            "\"},\"email\":{\"stringValue\":\"" + escapedEmail + "\"}}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
        {
            request.uploadHandler = new UploadHandlerRaw(
                Encoding.UTF8.GetBytes(body)
            );

            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + idToken);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                status =
                    "Login berhasil; penyimpanan profil Firestore perlu pemeriksaan Rules: " +
                    request.responseCode;
            }
        }
    }

    private void CreateArena()
    {
        if (arena != null)
            return;

        arena = new GameObject("ASTRAVION_TRAINING_ARENA");

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Arena Ground";
        ground.transform.SetParent(arena.transform);
        ground.transform.localScale = new Vector3(3, 1, 3);
        ground.GetComponent<Renderer>().material =
            MakeMaterial(new Color(0.045f, 0.075f, 0.12f));

        GameObject hero = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        hero.name = "KAEL";
        hero.transform.SetParent(arena.transform);
        hero.transform.position = new Vector3(0, 1, -3);
        hero.GetComponent<Renderer>().material =
            MakeMaterial(new Color(0.72f, 0.08f, 0.06f));

        GameObject sword = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sword.name = "KAEL Red Flame Sword";
        sword.transform.SetParent(hero.transform);
        sword.transform.localPosition = new Vector3(0.65f, 0.1f, 0.25f);
        sword.transform.localRotation = Quaternion.Euler(0, 0, -25);
        sword.transform.localScale = new Vector3(0.12f, 1.15f, 0.16f);
        sword.GetComponent<Renderer>().material =
            MakeMaterial(new Color(1f, 0.18f, 0.02f));

        for (int i = 0; i < 4; i++)
        {
            GameObject pillar =
                GameObject.CreatePrimitive(PrimitiveType.Cylinder);

            pillar.name = "Arena Pillar " + i;
            pillar.transform.SetParent(arena.transform);

            float angle = i * Mathf.PI / 2;

            pillar.transform.position = new Vector3(
                Mathf.Cos(angle) * 11,
                1.4f,
                Mathf.Sin(angle) * 11
            );

            pillar.transform.localScale = new Vector3(0.8f, 1.4f, 0.8f);

            pillar.GetComponent<Renderer>().material =
                MakeMaterial(new Color(0.12f, 0.32f, 0.55f));
        }

        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(0, 14, -19);
            Camera.main.transform.rotation = Quaternion.Euler(32, 0, 0);
        }
    }

    private Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");

        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        material.color = color;

        return material;
    }
}
