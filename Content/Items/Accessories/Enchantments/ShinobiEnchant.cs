using FargowiltasSouls.Content.Buffs.Souls;
using FargowiltasSouls.Content.Items.Accessories.Masomode;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.Systems;
using FargowiltasSouls.Core.Toggler;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace FargowiltasSouls.Content.Items.Accessories.Enchantments
{
	public class ShinobiEnchant : BaseEnchant
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            // DisplayName.SetDefault("Shinobi Infiltrator Enchantment");

            // Tooltip.SetDefault(tooltip);
        }

        protected override Color nameColor => new(147, 91, 24);
        

        public override void SetDefaults()
        {
            base.SetDefaults();

            Item.rare = ItemRarityID.Yellow;
            Item.value = 250000;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            AddEffects(player, Item);
        }
        public static void AddEffects(Player player, Item item)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            player.AddEffect<ShinobiThroughWalls>(item);
            modPlayer.ShinobiEnchantActive = true;
            bool dashCheck = !modPlayer.HasDash;
            if (modPlayer.FargoDash == DashManager.DashType.Monk && !player.HasBuff<MonkBuff>())
                dashCheck = true;
            bool effectCheck = player.AddEffect<ShinobiDashEffect>(item);
            if (dashCheck && effectCheck)
            {
                modPlayer.HasDash = true;
                modPlayer.FargoDash = DashManager.DashType.Shinobi;
            }
            MonkEnchant.AddEffects(player, item);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ItemID.MonkAltHead)
            .AddIngredient(ItemID.MonkAltShirt)
            .AddIngredient(ItemID.MonkAltPants)
            .AddIngredient(null, "MonkEnchant")
            .AddIngredient(ItemID.ChainGuillotines)
            .AddIngredient(ItemID.PsychoKnife)
            //code 2
            //flower pow
            //stynger

            .AddTile(TileID.CrystalBall)
            .Register();
        }
    }
    public class ShinobiDashEffect : AccessoryEffect
    {
        
        public override Header ToggleHeader => Header.GetHeader<ShadowHeader>();
        public static void ShinobiDash(Player player, int direction)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();

            modPlayer.dashCD = 90;
            player.dashDelay = modPlayer.dashCD;

            var teleportPos = player.position;

            const int length = 400; //make sure this is divisible by 16 btw

            if (player.HasEffect<ShinobiThroughWalls>()) //go through walls
            {
                teleportPos.X += length * direction;

                while (Collision.SolidCollision(teleportPos, player.width, player.height))
                {
                    if (direction == 1)
                    {
                        teleportPos.X++;
                    }
                    else
                    {
                        teleportPos.X--;
                    }
                }
            }
            else
            {
                for (int i = 0; i < length; i += 16)
                {
                    if (player.HasEffect<DeerclawpsEffect>())
                    {
                        DeerclawpsEffect.DeerclawpsAttack(player, Vector2.UnitX * teleportPos.X + Vector2.UnitY * player.Bottom.Y);
                    }

                    teleportPos.X += 16 * direction;

                    if (Collision.SolidCollision(teleportPos, player.width, player.height))
                    {
                        teleportPos.X -= 16 * direction;
                        break;
                    }
                }
            }

            if (teleportPos.X > 50 && teleportPos.X < (double)(Main.maxTilesX * 16 - 50) && teleportPos.Y > 50 && teleportPos.Y < (double)(Main.maxTilesY * 16 - 50))
            {
                FargoSoulsUtil.GrossVanillaDodgeDust(player);
                player.Teleport(teleportPos, 1);
                FargoSoulsUtil.GrossVanillaDodgeDust(player);
                NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, teleportPos.X, teleportPos.Y, 1);

                player.velocity.X = 12f * direction;
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI);
            }
        }
    }
    public class ShinobiThroughWalls : AccessoryEffect
    {
        
        public override Header ToggleHeader => Header.GetHeader<ShadowHeader>();
    }
}
