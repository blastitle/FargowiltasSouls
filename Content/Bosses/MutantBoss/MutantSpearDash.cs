﻿using FargowiltasSouls.Assets.Sounds;
using FargowiltasSouls.Content.Buffs.Boss;
using FargowiltasSouls.Content.Buffs.Masomode;
using FargowiltasSouls.Core.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Bosses.MutantBoss
{
    public abstract class MutantSpearAttack : ModProjectile
    {
        protected NPC npc;

        public override bool? CanDamage()
        {
            Projectile.maxPenetrate = 1;
            return null;
        }

        protected void TryLifeSteal(Vector2 pos, int playerWhoAmI)
        {
            if (WorldSavingSystem.MasochistModeReal && npc is NPC)
            {
                int totalHealPerHit = npc.lifeMax / 100 * 5;

                const int max = 20;
                for (int i = 0; i < max; i++)
                {
                    Vector2 vel = Main.rand.NextFloat(2f, 9f) * -Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi);
                    float ai0 = npc.whoAmI;
                    float ai1 = vel.Length() / Main.rand.Next(30, 90); //window in which they begin homing in

                    int healPerOrb = (int)(totalHealPerHit / max * Main.rand.NextFloat(0.95f, 1.05f));

                    if (playerWhoAmI == Main.myPlayer && Main.player[playerWhoAmI].ownedProjectileCounts[ModContent.ProjectileType<MutantHeal>()] < 10)
                    {
                        Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), pos, vel, ModContent.ProjectileType<MutantHeal>(), healPerOrb, 0f, Main.myPlayer, ai0, ai1);

                        SoundEngine.PlaySound(SoundID.Item27, pos);
                    }
                }
            }
        }
    }

    public class MutantSpearDash : MutantSpearAttack
    {
        public override string Texture => FargoSoulsUtil.AprilFools ?
            "FargowiltasSouls/Content/Bosses/MutantBoss/MutantSpear_April" :
            "FargowiltasSouls/Content/Projectiles/BossWeapons/Penetrator";

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("The Penetrator");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            CooldownSlot = 1;
            Projectile.FargoSouls().TimeFreezeImmune = true;
            Projectile.FargoSouls().DeletionImmuneRank = 2;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (projHitbox.Intersects(targetHitbox))
                return true;

            float length = 200;
            float dummy = 0f;
            Vector2 offset = length / 2 * Projectile.scale * (Projectile.rotation - MathHelper.ToRadians(135f)).ToRotationVector2();
            Vector2 end = Projectile.Center - offset;
            Vector2 tip = Projectile.Center + offset;

            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), end, tip, 8f * Projectile.scale, ref dummy))
                return true;

            return false;
        }

        public override void OnSpawn(IEntitySource source)
        {
            if (source is EntitySource_Parent parent && parent.Entity is NPC sourceNPC)
                npc = sourceNPC;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(npc is NPC ? npc.whoAmI : -1);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            npc = FargoSoulsUtil.NPCExists(reader.Read7BitEncodedInt());
        }

        float scaletimer;
        public override void AI()
        {
            if (Projectile.localAI[1] == 0f)
            {
                Projectile.localAI[1] = 1f;
                if (!WorldSavingSystem.masochistModeReal)
                {   
                    //bullshit to make it play the maso sound on the final throw of predictives
                    if (Projectile.ai[1] != -2)
                    {
                        SoundEngine.PlaySound(FargosSoundRegistry.PenetratorThrow, Projectile.Center);
                    }
                    if (Projectile.ai[1] == -2)
                    {
                        SoundEngine.PlaySound(FargosSoundRegistry.PenetratorExplosion, Projectile.Center);
                    }

                }
                else 
                {
                    SoundEngine.PlaySound(FargosSoundRegistry.PenetratorExplosion, Projectile.Center);
                }
            }

            NPC mutant = Main.npc[(int)Projectile.ai[0]];
            if (mutant.active && mutant.type == ModContent.NPCType<MutantBoss>() && (mutant.ai[0] == 6 || mutant.ai[0] == 15 || mutant.ai[0] == 23))
            {
                Projectile.velocity = Vector2.Normalize(mutant.velocity);
                Projectile.position -= Projectile.velocity;
                Projectile.rotation = mutant.velocity.ToRotation() + MathHelper.ToRadians(135f);
                Projectile.Center = mutant.Center + mutant.velocity;
                if ((Projectile.ai[1] <= 0f || WorldSavingSystem.MasochistModeReal) && --Projectile.localAI[0] < 0)
                {
                    if (Projectile.ai[1] == -2)
                    {
                        Projectile.localAI[0] = 1;

                        for (int i = -1; i <= 1; i += 2)
                        {
                            if (FargoSoulsUtil.HostCheck)
                            {
                                int p = Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, 16f * Vector2.Normalize(mutant.velocity).RotatedBy(MathHelper.PiOver2 * i),
                                ModContent.ProjectileType<MutantSphereSmall>(), Projectile.damage, 0f, Projectile.owner, -1);
                                if (p != Main.maxProjectiles)
                                    Main.projectile[p].timeLeft = 15;
                            }
                        }
                    }
                    else if (WorldSavingSystem.MasochistModeReal)
                    {
                        Projectile.localAI[0] = 2;

                        for (int i = -1; i <= 1; i += 2)
                        {
                            if (FargoSoulsUtil.HostCheck)
                            {
                                int p = Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, 16f / 2f * Vector2.Normalize(mutant.velocity).RotatedBy(MathHelper.PiOver2 * i),
                                ModContent.ProjectileType<MutantSphereSmall>(), Projectile.damage, 0f, Projectile.owner, -1);
                                if (p != Main.maxProjectiles)
                                    Main.projectile[p].timeLeft = 15;
                            }
                        }
                    }
                    else
                    {
                        Projectile.localAI[0] = 2;

                        if (FargoSoulsUtil.HostCheck)
                            Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<MutantSphereSmall>(), Projectile.damage, 0f, Projectile.owner, mutant.target);
                    }
                }
            }
            else
            {
                Projectile.Kill();
            }

            scaletimer++;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Projectile.NewProjectile(Terraria.Entity.InheritSource(Projectile), target.Center + Main.rand.NextVector2Circular(100, 100), Vector2.Zero, ModContent.ProjectileType<MutantBombSmall>(), 0, 0f, Projectile.owner);
            if (WorldSavingSystem.EternityMode)
            {
                target.FargoSouls().MaxLifeReduction += 100;
                target.AddBuff(ModContent.BuffType<OceanicMaulBuff>(), 5400);
                target.AddBuff(ModContent.BuffType<MutantFangBuff>(), 180);
            }
            target.AddBuff(ModContent.BuffType<CurseoftheMoonBuff>(), 600);

            TryLifeSteal(target.Center, target.whoAmI);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.zenithWorld)
                TryLifeSteal(target.Center, Main.myPlayer);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("FargowiltasSouls/Content/Bosses/MutantBoss/MutantEye_Glow", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            int rect1 = glow.Height / Main.projFrames[Projectile.type];
            int rect2 = rect1 * Projectile.frame;
            Rectangle glowrectangle = new(0, rect2, glow.Width, rect1);
            Vector2 gloworigin2 = glowrectangle.Size() / 2f;
            Color glowcolor = Color.Lerp(FargoSoulsUtil.AprilFools ? new Color(255, 191, 51, 0) : new Color(51, 255, 191, 0), Color.Transparent, 0.82f);
            Color glowcolor2 = Color.Lerp(FargoSoulsUtil.AprilFools ? new Color(255, 242, 194, 0) : new Color(194, 255, 242, 0), Color.Transparent, 0.6f);
            glowcolor = Color.Lerp(glowcolor, glowcolor2, 0.5f + (float)Math.Sin(scaletimer / 7) / 2); //make it shift between the 2 colors
            Vector2 drawCenter = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 28;

            float rotationModifier = -MathHelper.ToRadians(135f) + MathHelper.PiOver2;

            for (int i = 0; i < 3; i++) //create multiple transparent trail textures ahead of the projectile
            {
                Vector2 drawCenter2 = drawCenter + (Projectile.velocity.SafeNormalize(Vector2.UnitX) * 20).RotatedBy(MathHelper.Pi / 5 - i * MathHelper.Pi / 5); //use a normalized version of the projectile's velocity to offset it at different angles
                drawCenter2 -= Projectile.velocity.SafeNormalize(Vector2.UnitX) * 20; //then move it backwards
                float scale = Projectile.scale;
                scale += (float)Math.Sin(scaletimer / 7) / 7; //pulsate slightly so it looks less static
                Main.EntitySpriteDraw(glow, drawCenter2 - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(glowrectangle),
                    glowcolor, Projectile.rotation + rotationModifier, gloworigin2, scale * 1.25f, SpriteEffects.None, 0);
            }

            for (int i = ProjectileID.Sets.TrailCacheLength[Projectile.type] - 1; i > 0; i--) //scaling trail
            {
                Color color27 = glowcolor;
                color27 *= (float)(ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                float scale = Projectile.scale * (ProjectileID.Sets.TrailCacheLength[Projectile.type] - i) / ProjectileID.Sets.TrailCacheLength[Projectile.type];
                scale += (float)Math.Sin(scaletimer / 7) / 7; //pulsate slightly so it looks less static
                Vector2 value4 = Projectile.oldPos[i] - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 14;
                Main.EntitySpriteDraw(glow, value4 + Projectile.Size / 2f - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(glowrectangle), color27,
                    Projectile.oldRot[i] + rotationModifier, gloworigin2, scale * 1.25f, SpriteEffects.None, 0);
            }

            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame; //ypos of upper left corner of sprite to draw
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;
            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, SpriteEffects.None, 0);
        }
    }
}