using Terraria;
using TShockAPI;
using TerrariaApi.Server;
using Microsoft.Xna.Framework;

namespace GodmodeModerator
{
    [ApiVersion(2, 1)]
    public class GodmodeModerator : TerrariaPlugin
    {
        private Dictionary<int, int> playerHealthHistory = new Dictionary<int, int>();
        private Dictionary<int, DateTime> lastCheckTime = new Dictionary<int, DateTime>();
        private const double CheckDelay = 670;
        private Dictionary<int, int> projectileHealthHistory = new Dictionary<int, int>();
        private Dictionary<int, int> noDamageCount = new Dictionary<int, int>();
        public override string Name => "Godmode Moderator";
        public override string Author => "Nuffy";
        public override string Description => "A basic plugin that kicks players suspected of godmode.";

        public GodmodeModerator(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);
            }
            base.Dispose(disposing);
        }

        private void OnUpdate(EventArgs args)
        {
            CheckHitboxOverlap();
        }

        private void CheckHitboxOverlap()
        {
            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && player.Active)
                {
                    if (lastCheckTime.ContainsKey(player.Index) && (DateTime.Now - lastCheckTime[player.Index]).TotalMilliseconds < CheckDelay)
                    {
                        continue;
                    }

                    lastCheckTime[player.Index] = DateTime.Now;

                    Rectangle playerHitbox = player.TPlayer.getRect();
                    int currentHealth = player.TPlayer.statLife;

                    if (currentHealth <= 0)
                    {
                        noDamageCount[player.Index] = 0;
                        continue;
                    }

                    int previousHealth;
                    int healthDifference;

                    bool hitByNpc = false;
                    bool hitByProjectile = false;

                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.active && npc.damage > 0 && !npc.friendly)
                        {
                            Rectangle npcHitbox = npc.getRect();

                            if (playerHitbox.Intersects(npcHitbox) && !hitByNpc)
                            {
                                previousHealth = playerHealthHistory.ContainsKey(player.Index) ? playerHealthHistory[player.Index] : currentHealth;
                                playerHealthHistory[player.Index] = currentHealth;

                                healthDifference = previousHealth - currentHealth;

                                if (healthDifference > 0)
                                {
                                    noDamageCount[player.Index] = 0;
                                }
                                else if (healthDifference < 0)
                                {
                                    noDamageCount[player.Index] = 0;
                                }
                                else
                                {

                                    if (!noDamageCount.ContainsKey(player.Index))
                                    {
                                        noDamageCount[player.Index] = 0;
                                    }
                                    noDamageCount[player.Index]++;


                                    if (noDamageCount[player.Index] >= 15 && !player.HasPermission("ac.gmwl"))
                                    {
                                        noDamageCount[player.Index] = 0; 
                                        player.Kick("Godmode Detected");
                                    }
                                }
                                hitByNpc = true; 
                            }
                        }
                    }


                    foreach (Projectile projectile in Main.projectile)
                    {
                        if (projectile.active && playerHitbox.Intersects(projectile.getRect()) && projectile.hostile && !hitByProjectile)
                        {

                            if (projectile.owner != 255 && Main.player[projectile.owner].whoAmI == player.Index)
                            {
                                continue;
                            }

                            int projectileDamage = projectile.damage;
                            previousHealth = projectileHealthHistory.ContainsKey(player.Index) ? projectileHealthHistory[player.Index] : currentHealth;
                            healthDifference = previousHealth - currentHealth;

                            if (healthDifference > 0)
                            {
                                noDamageCount[player.Index] = 0;
                            }
                            else if (healthDifference < 0)
                            {
                                noDamageCount[player.Index] = 0;
                            }
                            else
                            {

                                if (!noDamageCount.ContainsKey(player.Index))
                                {
                                    noDamageCount[player.Index] = 0;
                                }
                                noDamageCount[player.Index]++;


                                if (noDamageCount[player.Index] >= 15 && !player.HasPermission("ac.gmwl"))
                                {
                                    noDamageCount[player.Index] = 0; 
                                    player.Kick("Godmode Detected.");
                                }
                            }
                            hitByProjectile = true;
                            projectileHealthHistory[player.Index] = currentHealth; 
                        }
                    }
                }
            }
        }



    }
}