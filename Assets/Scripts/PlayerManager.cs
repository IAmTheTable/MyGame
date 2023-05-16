using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Netcode;
using Unity.Networking.Transport;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using static Rpc;

public class NetworkString : INetworkSerializable, IComparable
{
    public string value = "";

    int IComparable.CompareTo(object obj)
    {
        throw new NotImplementedException();
    }

    void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        serializer.SerializeValue(ref value);
    }

    public static implicit operator NetworkString(string s)
    {
        return new NetworkString { value = s };
    }

    public static implicit operator string(NetworkString s)
    {
        return s.value;
    }

    public override string ToString()
    {
        return value;
    }
}

/*
 *  LEVELING SYSTEM:
*  - Each level gives you 1 UpgradePoint
*  - The max level would be 50 (50 UpgradePoints
 
 */


public class PlayerManager : NetworkBehaviour
{
    // player properties
    public NetworkVariable<int> Score = new();
    public NetworkVariable<int> XP = new();
    public NetworkVariable<int> Level = new();
    public NetworkVariable<int> UpgradePoints = new();
    public NetworkVariable<float> XPRatio = new();
    public NetworkVariable<float> Health = new(100);
    public NetworkVariable<int> MaxHealth = new(100);
    public NetworkVariable<ulong> LastAttacker = new();
    public NetworkVariable<bool> CanShoot = new(false);
    public NetworkVariable<NetworkString> playerName = new("Player");
    public NetworkVariable<bool> CombatLogged = new(false);
    public NetworkVariable<long> i_lastShot = new(DateTime.UnixEpoch.Ticks);
    // upgrades
    public NetworkVariable<int> u_Health = new(0);
    public NetworkVariable<int> u_Regen = new(0);
    public NetworkVariable<int> u_Firerate = new(0);
    public NetworkVariable<int> u_Damage = new(0);
    public NetworkVariable<int> u_Speed = new(0);
    public NetworkVariable<int> u_BulletSpeed = new(0);
    
    public Rigidbody2D rb;
    private Camera camera;
    public Physics2DRaycaster raycaster;
    public GameObject bulletPrefab;
    public RectTransform healthUI;

    public GameObject ExplosionPrefab;
    public GameObject barrel;
    public GameObject player;

    public TextMeshProUGUI nametext;


    public int speedMultiplier = 15;
    public int boostMultiplier = 25;

    public Rpc _rpc;
    private int _boostCount = 5;

    /*public new GameObject gameObject
    {
        get
        {
            return player;
        }
    }*/
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
            Health.Value = 100;
        // z index to 2, because layers
        gameObject.tag = "Player"; //player tag for server uses

        Debug.Log("Player Spawned: id " + NetworkManager.LocalClientId);
        //Camera.main.transform.SetParent(transform);

        // change the network ownership of our player to our client
        // this will allow us to control our player object

        Health.OnValueChanged += (old, _new) =>
        {
            healthUI.sizeDelta = new Vector2((float)_new / MaxHealth.Value, healthUI.sizeDelta.y);
            if (_new <= 0)
            {
                _rpc.PlayerDeathServerRpc();
                _rpc.TestExplosionServerRpc();
            }
        };

        MaxHealth.OnValueChanged += (old, _new) =>
        {
            healthUI.sizeDelta = new Vector2((float)Health.Value / MaxHealth.Value, healthUI.sizeDelta.y);
        };


        nametext.text = playerName.Value;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _rpc = GetComponent<Rpc>();
        if (IsOwner)
        {
            camera = Camera.main;
        }

        Score.OnValueChanged += OnScoreChanged;
    }

    private void OnScoreChanged(int previousValue, int newValue)
    {
        if (IsOwner)
            GameObject.Find("Canvas").GetComponent<UIMan>().scoreText.SetText($"Score: {newValue}");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // client only camera manipulation
        if (IsOwner)
        {
            var cameraPosition = camera.transform.position;
            var playerPosition = transform.position;

            cameraPosition.x = Mathf.Lerp(cameraPosition.x, playerPosition.x, 0.1f);
            cameraPosition.y = Mathf.Lerp(cameraPosition.y, playerPosition.y, 0.1f);

            camera.transform.position = cameraPosition;
        }
        


        nametext.SetText(playerName.Value);

        // basic WASD key movement
        Move();

        // make sure i_lastShot has been atleast 10 seconds ago
        if (DateTime.Now.Ticks - i_lastShot.Value > TimeSpan.TicksPerSecond * 10)
            _rpc.RequestCombatLogServerRpc();

        if (u_Regen.Value > 0 && Health.Value < MaxHealth.Value && !CombatLogged.Value)
        {
            Health.Value += (u_Regen.Value * 0.025f);
        }


        if (gameObject.transform.position.x < GameManager.instance.spawnArea.minX || gameObject.transform.position.x > GameManager.instance.spawnArea.maxX || gameObject.transform.position.y < GameManager.instance.spawnArea.minY || gameObject.transform.position.y > GameManager.instance.spawnArea.maxY)
        {
            // just clip their position to the spawn area
            gameObject.transform.position = new Vector3(Mathf.Clamp(gameObject.transform.position.x, GameManager.instance.spawnArea.minX + 5, GameManager.instance.spawnArea.maxX - 5), Mathf.Clamp(gameObject.transform.position.y, GameManager.instance.spawnArea.minY + 5, GameManager.instance.spawnArea.maxY - 5), gameObject.transform.position.z);
        }
    }
    bool canshoot = true;
    private void Update()
    {
        // client only
        if (!IsOwner) return;

        var mousePosition = Input.mousePosition;
        var test = camera.ScreenPointToRay(mousePosition);
        // line = origin + dir
        // test.origin = transform.position + dir
        // test.origin - transform.position = dir

        // get direction from player to mouse
        var playerRot = new Ray(transform.position, test.origin - transform.position);
        var playerToMouseRay = new Ray(barrel.transform.position, test.origin - barrel.transform.position);
        // if space and can shoot, shoot function
        if (Input.GetKey(KeyCode.Space) && canshoot)
        {
            canshoot = false;
            StartCoroutine(Shoot(playerToMouseRay));
        }

        // set our player's rotation
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(playerRot.direction.y, playerRot.direction.x) * Mathf.Rad2Deg - 90);
        barrel.transform.rotation = transform.rotation;
        var bounds = GetComponent<CircleCollider2D>().bounds; // circle bounds of the player

        nametext.transform.position = new Vector3(transform.position.x, transform.position.y + bounds.size.y + 0.75f); //top of the player
        healthUI.position = new Vector3(transform.position.x, transform.position.y - bounds.extents.y - 0.75f); // bottom of the player
        healthUI.localRotation = Quaternion.Euler(-transform.rotation.eulerAngles);
        nametext.transform.localRotation = Quaternion.Euler(-transform.rotation.eulerAngles);
    }

    public int GetUpgradeValue(UpgradeType type)
    {
        return type switch
        {
            UpgradeType.Health => u_Health.Value,
            UpgradeType.Regen => u_Regen.Value,
            UpgradeType.Firerate => u_Firerate.Value,
            UpgradeType.Damage => u_Damage.Value,
            UpgradeType.MoveSpeed => u_Speed.Value,
            UpgradeType.BulletSpeed => u_BulletSpeed.Value,
            _ => -1,
        };
    }
    
    void Move()
    {
        if (!IsOwner) return;
        if (Input.GetKey(KeyCode.W))
        {
            // move using rigid body
            _rpc.RequestMoveServerRpc(Vector3.up * (speedMultiplier + u_Speed.Value));
        }
        if (Input.GetKey(KeyCode.A))
        {
            _rpc.RequestMoveServerRpc(Vector2.left * (speedMultiplier + u_Speed.Value));
        }
        if (Input.GetKey(KeyCode.S))
        {
            _rpc.RequestMoveServerRpc(Vector2.down * (speedMultiplier + u_Speed.Value));
        }
        if (Input.GetKey(KeyCode.D))
        {
            _rpc.RequestMoveServerRpc(Vector2.right * (speedMultiplier + u_Speed.Value));
        }

        if(Input.GetKey(KeyCode.KeypadMinus))
        {
            _rpc.TestExplosionServerRpc();
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (_boostCount > 0)
            {
                _rpc.RequestMoveServerRpc(new Vector3(rb.velocity.x + boostMultiplier, rb.velocity.y + boostMultiplier));
                _boostCount--;
            }
        }
        else
        {
            _boostCount = 3;
        }
    }

    IEnumerator Shoot(Ray pos)
    {
        _rpc.ClientShootServerRpc(pos);
        yield return new WaitForSeconds(0.5f - (0.025f * u_Firerate.Value));
        canshoot = true;
    }
}
