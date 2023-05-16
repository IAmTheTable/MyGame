using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Rpc : NetworkBehaviour
{

    [ServerRpc(RequireOwnership = false)]
    public void OnClientConnectServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (NetworkManager.ConnectedClients.ContainsKey(serverRpcParams.Receive.SenderClientId))
            PlayerCountUpdateClientRpc(NetworkManager.ConnectedClients.Count);
    }

    [ClientRpc]
    public void PlayerCountUpdateClientRpc(int count)
    {
        var playerCount = GameObject.Find("Canvas").GetComponent<UIMan>().playerCount;
        playerCount.SetText($"Online: {count}");
    }

    [ClientRpc]
    public void LeaderboardUpdateClientRpc()
    {
        var list = GameObject.FindGameObjectsWithTag("lb").OrderBy(x => x.name).ToList();
        var players = GameObject.FindGameObjectsWithTag("Player").ToArray();
        var plrMngr = players.Select(x => x.GetComponent<PlayerManager>()).OrderByDescending(x => x.Score.Value).ToArray();

        for (int i = 0; i < 10; i++)
        {
            if (i < players.Count())
            {
                list[i].GetComponent<TextMeshProUGUI>().text = $"{plrMngr[i].playerName.Value} - {ScoreCompress(plrMngr[i].Score.Value)}";
            }
            else
                list[i].GetComponent<TextMeshProUGUI>().text = "";
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RenameServerRpc(string name, ServerRpcParams serverRpcParams = default)
    {
        if (NetworkManager.ConnectedClients.ContainsKey(serverRpcParams.Receive.SenderClientId))
        {
            var player = NetworkManager.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject.gameObject;
            player.GetComponent<PlayerManager>().playerName.Value = name;
        }
    }

    // turn a number into a more readable form ex
    // 10453 -> 10.4k
    private string ScoreCompress(int value)
    {
        if (value < 1000)
            return value.ToString();
        else if (value < 1000000)
            return (value / 1000).ToString() + "k";
        else if (value < 1000000000)
            return (value / 1000000).ToString() + "m";
        else
            return (value / 1000000000).ToString() + "b";
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestMoveServerRpc(Vector3 direction, ServerRpcParams serverRpcParams = default)
    {
        if (NetworkManager.ConnectedClients.ContainsKey(serverRpcParams.Receive.SenderClientId))
        {
            var client = NetworkManager.ConnectedClients[serverRpcParams.Receive.SenderClientId];
            client.PlayerObject.GetComponent<Rigidbody2D>().AddForce(direction);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnEatServerRpc(ulong id, ServerRpcParams serverRpcParams = default)
    {
        // get our player object
        var player = NetworkManager.ConnectedClients[id].PlayerObject;
        // leveling system
        var plrMgr = player.GetComponent<PlayerManager>();
        plrMgr.Score.Value++;
        plrMgr.XP.Value += 5;

        if (plrMgr.Level.Value < 50)
        {
            // check if the player leveled up
            if (DoLevelUp(plrMgr.Level.Value, plrMgr.XP.Value))
            {
                plrMgr.Level.Value++;
                plrMgr.UpgradePoints.Value++;
            }
        }

        LeaderboardUpdateClientRpc();
        NetworkObject.Despawn(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetShootServerRpc(bool m) => NetworkObject.GetComponent<PlayerManager>().CanShoot.Value = m;

    // called on the client that shot
    [ServerRpc(RequireOwnership = false)]
    public void ClientShootServerRpc(Ray lookPos, ServerRpcParams serverRpcParams = default)
    {
        if (NetworkManager.ConnectedClients.ContainsKey(serverRpcParams.Receive.SenderClientId))
        {
            var client = NetworkManager.ConnectedClients[serverRpcParams.Receive.SenderClientId];

            var localPlayer = client.PlayerObject;

            var _bullet = Instantiate(localPlayer.GetComponent<PlayerManager>().bulletPrefab, lookPos.origin + lookPos.direction.normalized * 3, Quaternion.identity);
            var bullet = _bullet.GetComponent<NetworkObject>();

            bullet.SpawnWithOwnership(serverRpcParams.Receive.SenderClientId);
            bullet.transform.position = new Vector3(bullet.transform.position.x, bullet.transform.position.y, 0);
            bullet.GetComponent<Rigidbody2D>().velocity = (lookPos.direction.normalized * 3) * (10 + (localPlayer.GetComponent<PlayerManager>().u_BulletSpeed.Value));

            StartCoroutine(wait(bullet));
        }
    }

    IEnumerator wait(NetworkObject bullet)
    {
        if (!IsServer) yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(3);
        bullet.Despawn(true);
    }

    // called on the client that got hit
    [ClientRpc]
    public void OnBulletHitClientRpc(ClientRpcParams clientRpcParams = default)
    {
        // if we already combat logged and got hit
        if (NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerManager>().CombatLogged.Value == true)
            StopCoroutine(DisableCombatLog());
        
        Debug.Log($"Called on client: {string.Join(",", clientRpcParams.Send.TargetClientIds)}");
        Debug.Log($"Called on client2: {OwnerClientId}");
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerManager>().CombatLogged.Value = true;
        StartCoroutine(DisableCombatLog());
    }

    private IEnumerator DisableCombatLog()
    {
        yield return new WaitForSeconds(10.0f);
        NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerManager>().CombatLogged.Value = true;
    }

    // called from the player who shot the player
    [ServerRpc(RequireOwnership = false)]
    public void OnBulletHitServerRpc(ulong personHit, ServerRpcParams serverRpcParams = default)
    {
        if (NetworkManager.ConnectedClients.ContainsKey(serverRpcParams.Receive.SenderClientId))
        {
            Debug.Log($"{serverRpcParams.Receive.SenderClientId} attacked {personHit}");

            var shooter = NetworkManager.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;

            OnBulletHitClientRpc(new ClientRpcParams() { Send = new ClientRpcSendParams() { TargetClientIds = new[] { personHit } } });
            // get the player, change the health, set the last attacker for death
            var player = NetworkManager.ConnectedClients[personHit].PlayerObject;
            player.GetComponent<PlayerManager>().Health.Value -= (2 * shooter.GetComponent<PlayerManager>().u_Damage.Value);
            player.GetComponent<PlayerManager>().LastAttacker.Value = serverRpcParams.Receive.SenderClientId;
            player.GetComponent<PlayerManager>().i_lastShot.Value = DateTime.UnixEpoch.Ticks;
        }
    }

    // called from the player who died
    [ServerRpc(RequireOwnership = false)]
    public void PlayerDeathServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (NetworkManager.ConnectedClients.ContainsKey(serverRpcParams.Receive.SenderClientId))
        {
            var player = NetworkManager.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;
            var mgr = player.GetComponent<PlayerManager>();
            // chcek if the shooter(person who killed the player) is in the game
            if (!NetworkManager.ConnectedClients.ContainsKey(mgr.LastAttacker.Value))
                goto reset;

            // add the score to the player
            var attacker = NetworkManager.ConnectedClients[mgr.LastAttacker.Value].PlayerObject;
            attacker.GetComponent<PlayerManager>().Score.Value += mgr.Score.Value;

            // leveling system
            var plrMgr = attacker.GetComponent<PlayerManager>();
            plrMgr.XP.Value += (int)(mgr.Score.Value * 0.5f);

            if (plrMgr.Level.Value < 50)
            {
                // check if the player leveled up
                if (DoLevelUp(plrMgr.Level.Value, plrMgr.XP.Value))
                {
                    plrMgr.Level.Value++;
                    plrMgr.UpgradePoints.Value++;
                }
            }

            PlayDeathSoundClientRpc();

        reset:
            RespawnServerRpc(serverRpcParams);
        }
    }

    [ClientRpc]
    public void PlayDeathSoundClientRpc()
    {
        var player = NetworkManager.ConnectedClients[OwnerClientId].PlayerObject;
        player.GetComponent<AudioSource>().Play();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TestExplosionServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var player = NetworkManager.ConnectedClients[OwnerClientId].PlayerObject;
        var playerManager = player.GetComponent<PlayerManager>();

        // spawn explosion
        var explosion = Instantiate(playerManager.ExplosionPrefab, player.transform.position, Quaternion.identity);
        var explosionNetworkObject = explosion.GetComponent<NetworkObject>();
        explosionNetworkObject.Spawn();
    }


    public static float GetXPRate() => 200.0f;

    public bool DoLevelUp(int level, int xp)
    {
        int maxXP = (int)(GetXPRate() * level);
        if (xp >= maxXP)
            return true;
        else
            return false;
    }


    [ServerRpc(RequireOwnership = false)]
    public void RespawnServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (NetworkManager.ConnectedClients.ContainsKey(serverRpcParams.Receive.SenderClientId))
        {
            var player = NetworkManager.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;
            var mgr = player.GetComponent<PlayerManager>();
            mgr.Health.Value = 100;
            mgr.Score.Value = 0;

            mgr.CombatLogged.Value = false;
            // reset upgrades
            mgr.u_Health.Value = 0;
            mgr.u_Regen.Value = 0;
            mgr.u_Firerate.Value = 0;
            mgr.u_BulletSpeed.Value = 0;
            mgr.u_Damage.Value = 0;
            mgr.u_Speed.Value = 0;
            // reset properties
            mgr.XP.Value = 0;
            mgr.Level.Value = 0;
            mgr.LastAttacker.Value = 0;

            player.transform.position = new Vector3(0, 0, 0);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void FoodDestroyServerRpc()
    {
        GameObject.Find("GameManager(Clone)").GetComponent<GameManager>().availableFood.Value--;
        NetworkObject.Despawn(true);
    }


    public enum UpgradeType : int
    {
        Health,
        Regen,
        Firerate,
        Damage,
        MoveSpeed,
        BulletSpeed
    }


    [ServerRpc(RequireOwnership = false)]
    public void RequestUpgradeServerRpc(UpgradeType upgradeType, ServerRpcParams serverRpcParams = default)
    {
        if (NetworkManager.ConnectedClients.ContainsKey(serverRpcParams.Receive.SenderClientId))
        {
            var player = NetworkManager.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;
            var mgr = player.GetComponent<PlayerManager>();

            // check if the player has points
            if (mgr.UpgradePoints.Value > 0)
            {
                switch (upgradeType)
                {
                    case UpgradeType.Health:
                        if (mgr.u_Health.Value + 1 <= 10)
                            mgr.u_Health.Value += 1;
                        mgr.MaxHealth.Value = 100 + (mgr.u_Health.Value * 10);
                        break;
                    case UpgradeType.Regen:
                        if (mgr.u_Regen.Value + 1 <= 10)
                            mgr.u_Regen.Value += 1;
                        break;
                    case UpgradeType.Firerate:
                        if (mgr.u_Firerate.Value + 1 <= 10)
                            mgr.u_Firerate.Value += 1;
                        break;
                    case UpgradeType.Damage:
                        if (mgr.u_Damage.Value + 1 <= 10)
                            mgr.u_Damage.Value += 1;
                        break;
                    case UpgradeType.MoveSpeed:
                        if (mgr.u_Speed.Value + 1 <= 10)
                            mgr.u_Speed.Value += 1;
                        break;
                    case UpgradeType.BulletSpeed:
                        if (mgr.u_BulletSpeed.Value + 1 <= 10)
                            mgr.u_BulletSpeed.Value += 1;
                        break;
                }
                //remove the point
                mgr.UpgradePoints.Value--;
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestCombatLogServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (NetworkManager.ConnectedClients.ContainsKey(serverRpcParams.Receive.SenderClientId))
        {
            var player = NetworkManager.ConnectedClients[serverRpcParams.Receive.SenderClientId].PlayerObject;
            var mgr = player.GetComponent<PlayerManager>();
            if (DateTime.Now.Ticks - mgr.i_lastShot.Value > TimeSpan.TicksPerSecond * 10)
                mgr.CombatLogged.Value = false;
        }
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

}
