﻿using FargowiltasSouls.Content.Items.Accessories.Expert;
using FargowiltasSouls.Core.AccessoryEffectSystem;
using FargowiltasSouls.Core.ModPlayers;
using FargowiltasSouls.Core.Toggler.Content;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Items.Accessories.Souls
{
    [AutoloadEquip(EquipType.Wings)]
    public class FlightMasterySoul : FlightMasteryWings
    {

        public static readonly Color ItemColor = new(56, 134, 255);
        protected override Color? nameColor => ItemColor;

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            AddEffects(player, Item);
        }
        public static void AddEffects(Player player, Item item)
        {
            FargoSoulsPlayer modPlayer = player.FargoSouls();
            player.AddEffect<JumpsDisabled>(item);
            //if (player.AddEffect<JumpsDisabled>(item))
                //modPlayer.JumpsDisabled = modPlayer.JumpsDisabledBuffer = true;

            modPlayer.FlightMasterySoul = true;
            player.wingTimeMax = 999999;
            player.wingTime = player.wingTimeMax;
            player.ignoreWater = true;

            player.AddEffect<FlightMasteryInsignia>(item);
            player.AddEffect<FlightMasteryGravity>(item);

            //hover
            if (player.controlDown && player.controlJump && !player.mount.Active)
            {
                player.position.Y -= player.velocity.Y;
                if (player.velocity.Y > 0.1f)
                    player.velocity.Y = 0.1f;
                else if (player.velocity.Y < -0.1f)
                    player.velocity.Y = -0.1f;
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe()

            .AddIngredient(ItemID.EmpressFlightBooster) //soaring insignia

            .AddIngredient(ItemID.BatWings) //bat wings
            .AddIngredient(ItemID.CreativeWings) //fledgling wings
            .AddIngredient(ItemID.FairyWings)
            .AddIngredient(ItemID.HarpyWings)
            .AddIngredient(ItemID.BoneWings)
            .AddIngredient(ItemID.FrozenWings)
            .AddIngredient(ItemID.FlameWings)
            .AddIngredient(ItemID.TatteredFairyWings)
            .AddIngredient(ItemID.FestiveWings)
            .AddIngredient(ItemID.BetsyWings)
            .AddIngredient(ItemID.FishronWings)
            .AddIngredient(ItemID.RainbowWings) //empress wings
            .AddIngredient(ItemID.LongRainbowTrailWings) //celestial starboard

            .AddIngredient(ItemID.GravityGlobe)

            .AddTile(ModContent.Find<ModTile>("Fargowiltas", "CrucibleCosmosSheet"))


            .Register();
        }
    }
    public class FlightMasteryInsignia : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<FlightMasteryHeader>();
        public override int ToggleItemType => ItemID.EmpressFlightBooster;
        
        public override void PostUpdateEquips(Player player)
        {
            player.empressBrooch = true;
        }
    }
    public class FlightMasteryGravity : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<FlightMasteryHeader>();
        public override int ToggleItemType => ModContent.ItemType<FlightMasterySoul>();
        
        public override void PostUpdateEquips(Player player)
        {
            player.gravity = Player.defaultGravity * 1.5f;
        }
    }
    public class JumpsDisabled : AccessoryEffect
    {
        public override Header ToggleHeader => Header.GetHeader<FlightMasteryHeader>();
        public override int ToggleItemType => ModContent.ItemType<FlightMasterySoul>();
        public override void PostUpdateEquips(Player player)
        {
            player.ConsumeAllExtraJumps();
        }
    }
}
