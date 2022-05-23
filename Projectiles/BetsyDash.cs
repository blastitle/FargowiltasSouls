using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Projectiles
{
    public class BetsyDash : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_686";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Dash");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Player.defaultHeight;
            Projectile.height = Player.defaultHeight;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ignoreWater = true;
            Projectile.alpha = 60;
            Projectile.timeLeft = 15;
            Projectile.penetrate = -1;
            Projectile.GetGlobalProjectile<FargoSoulsGlobalProjectile>().CanSplit = false;
            Projectile.GetGlobalProjectile<FargoSoulsGlobalProjectile>().TimeFreezeImmune = true;
            Projectile.GetGlobalProjectile<FargoSoulsGlobalProjectile>().DeletionImmuneRank = 2;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            if (player.HasBuff(ModContent.BuffType<Buffs.Souls.TimeFrozen>()))
            {
                Projectile.Kill();
                return;
            }

            player.GetModPlayer<FargoSoulsPlayer>().BetsyDashing = true;
            player.GetModPlayer<FargoSoulsPlayer>().dashCD = 5;
            player.GetModPlayer<FargoSoulsPlayer>().IsDashingTimer = 0;

            player.Center = Projectile.Center;
            if (Projectile.timeLeft > 1) //trying to avoid wallclipping
                player.position += Projectile.velocity;
            player.velocity = Projectile.velocity * .5f;
            player.direction = Projectile.velocity.X > 0 ? 1 : -1;

            /*player.controlLeft = false;
            player.controlRight = false;
            player.controlJump = false;
            player.controlDown = false;*/
            player.controlUseItem = false;
            player.controlUseTile = false;
            player.controlHook = false;
            player.controlMount = false;

            if (player.mount.Active)
                player.mount.Dismount(player);

            player.immune = true;
            player.immuneTime = Math.Max(player.immuneTime, 2);
            player.hurtCooldowns[0] = Math.Max(player.hurtCooldowns[0], 2);
            player.hurtCooldowns[1] = Math.Max(player.hurtCooldowns[1], 2);

            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2;

            if (Projectile.localAI[0] == 0)
            {
                Projectile.localAI[0] = 1;
                SoundEngine.PlaySound(SoundHelper.LegacySoundStyle("Item", 14), Projectile.Center);
                for (int i = 0; i < 30; i++)
                {
                    int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 87, 0, 0, 0, default, 2.5f);
                    Main.dust[d].noGravity = true;
                    Main.dust[d].velocity *= 4f;
                }
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(BuffID.BetsysCurse, 600);
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundHelper.LegacySoundStyle("Item", 14), Projectile.Center);
            for (int i = 0; i < 30; i++)
            {
                int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 87, 0, 0, 0, default, 2.5f);
                Main.dust[d].noGravity = true;
                Main.dust[d].velocity *= 4f;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false; //dont kill proj when hits tiles
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.localAI[0] != 0)
            {
                Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
                int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
                int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
                Rectangle rectangle = new Rectangle(0, y3, texture2D13.Width, num156);
                Vector2 origin2 = rectangle.Size() / 2f;

                Color color26 = Color.White;
                color26 = Projectile.GetAlpha(color26);
                color26.A = (byte)Projectile.alpha;

                SpriteEffects effects = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
                {
                    float lerpamount = 0;
                    if (i > 3 && i < 5)
                        lerpamount = 0.6f;
                    if (i >= 5)
                        lerpamount = 0.8f;

                    Color color27 = Color.Lerp(Color.White, Color.Purple, lerpamount) * 0.75f * 0.5f;
                    color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                    float scale = Projectile.scale * (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                    Vector2 value4 = Projectile.oldPos[i];
                    float num165 = Projectile.oldRot[i];
                    Main.EntitySpriteDraw(texture2D13, value4 + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, num165, origin2, scale, effects, 0);
                }

                Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color26, Projectile.rotation, origin2, Projectile.scale, effects, 0);
            }
            return false;
        }
    }
}