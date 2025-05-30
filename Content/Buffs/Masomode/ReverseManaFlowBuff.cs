﻿using Terraria;
using Terraria.ModLoader;

namespace FargowiltasSouls.Content.Buffs.Masomode
{
    public class ReverseManaFlowBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Reverse Mana Flow");
            // Description.SetDefault("Your magic weapons cost life instead of mana");
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            //DisplayName.AddTranslation((int)GameCulture.CultureName.Chinese, "反魔力流");
            //Description.AddTranslation((int)GameCulture.CultureName.Chinese, "魔法武器消耗生命,而不是法力");
        }

        public override void Update(Player player, ref int buffIndex)
        {
            //mana cost also damages
            player.FargoSouls().ReverseManaFlow = true;

            player.GetDamage(DamageClass.Summon) *= 0.6f;
        }
    }
}