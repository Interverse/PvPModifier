using TShockAPI;
using Terraria;

namespace PvPModifier.CustomWeaponAPI {
    public static class CustomWeaponDropper {
        private static byte GetAlterStartFlags(CustomWeapon weapon) {
            byte flagByte = 0;
            if (weapon.Color != null) {
                flagByte += 1;
            }

            if (weapon.Damage != null) {
                flagByte += 2;
            }

            if (weapon.Knockback != null) {
                flagByte += 4;
            }

            if (weapon.UseAnimation != null) {
                flagByte += 8;
            }

            if (weapon.UseTime != null) {
                flagByte += 16;
            }

            if (weapon.ShootProjectileId != null) {
                flagByte += 32;
            }

            if (weapon.ShootSpeed != null) {
                flagByte += 64;
            }

            var nextFlags = weapon.DropAreaWidth != null || weapon.DropAreaHeight != null || weapon.Scale != null ||
                            weapon.AmmoIdentifier != null || weapon.UseAmmoIdentifier != null || weapon.NotAmmo != null;
            if (nextFlags) {
                flagByte += 128;
            }

            return flagByte;
        }

        private static byte GetAlterEndFlags(CustomWeapon weapon) {
            byte flagByte = 0;

            if (weapon.DropAreaWidth != null) {
                flagByte += 1;
            }

            if (weapon.DropAreaHeight != null) {
                flagByte += 2;
            }

            if (weapon.Scale != null) {
                flagByte += 4;
            }

            if (weapon.AmmoIdentifier != null) {
                flagByte += 8;
            }

            if (weapon.UseAmmoIdentifier != null) {
                flagByte += 16;
            }

            if (weapon.NotAmmo != null) {
                flagByte += 32;
            }

            return flagByte;
        }

        private static void SetAlterItemDropNextFlags(PacketWriter alterItemDrop, CustomWeapon weapon) {
            alterItemDrop.PackByte(GetAlterEndFlags(weapon));

            if (weapon.DropAreaWidth != null) {
                alterItemDrop.PackInt16((short) weapon.DropAreaWidth);
            }

            if (weapon.DropAreaHeight != null) {
                alterItemDrop.PackInt16((short) weapon.DropAreaHeight);
            }

            if (weapon.Scale != null) {
                alterItemDrop.PackSingle((float) weapon.Scale);
            }

            if (weapon.AmmoIdentifier != null) {
                alterItemDrop.PackInt16((short) weapon.AmmoIdentifier);
            }

            if (weapon.UseAmmoIdentifier != null) {
                alterItemDrop.PackInt16((short) weapon.UseAmmoIdentifier);
            }

            if (weapon.NotAmmo != null) {
                alterItemDrop.PackByte((byte) (((bool) weapon.NotAmmo) ? 1 : 0));
            }
        }

        private static byte[] GetAlterItemDropPacket(CustomWeapon weapon, int itemIndex) {
            var flags1 = GetAlterStartFlags(weapon);
            var alterItemDrop = new PacketWriter()
                .SetType(88)
                .PackInt16((short) itemIndex)
                .PackByte(flags1);

            if (weapon.Color != null) {
                var color = 0xFF000000;
                color += (uint) weapon.Color?.R;
                color += (uint) weapon.Color?.G << 8;
                color += (uint) weapon.Color?.B << 16;
                alterItemDrop.PackUInt32(color);
            }

            if (weapon.Damage != null) {
                alterItemDrop.PackUInt16((ushort) weapon.Damage);
            }

            if (weapon.Knockback != null) {
                alterItemDrop.PackSingle((float) weapon.Knockback);
            }

            if (weapon.UseAnimation != null) {
                alterItemDrop.PackUInt16((ushort) weapon.UseAnimation);
            }

            if (weapon.UseTime != null) {
                alterItemDrop.PackUInt16((ushort) weapon.UseTime);
            }

            if (weapon.ShootProjectileId != null) {
                alterItemDrop.PackInt16((short) weapon.ShootProjectileId);
            }

            if (weapon.ShootSpeed != null) {
                alterItemDrop.PackSingle((float) weapon.ShootSpeed);
            }

            if ((flags1 & 128) != 0) {
                SetAlterItemDropNextFlags(alterItemDrop, weapon);
            }

            return alterItemDrop.GetByteData();
        }

        public static void DropItem(TSPlayer player, CustomWeapon weapon) {
            int freeIndex = 400;
            for (int i = 0; i < 400; ++i) {
                if (!Main.item[i].active && Main.itemLockoutTime[i] == 0) {
                    freeIndex = i;
                    break;
                }
            }

            Main.itemLockoutTime[freeIndex] = 0;
            Main.item[freeIndex] = new Item();
            Main.item[freeIndex].active = true;

            var itemDrop = new PacketWriter()
                .SetType((int) PacketTypes.UpdateItemDrop)
                .PackInt16((short) freeIndex)
                .PackSingle(player.TPlayer.position.X - short.MaxValue / 2f) //This line of code was modified from the API
                .PackSingle(player.TPlayer.position.Y - short.MaxValue / 2f) //This line of code was modified from the API
                .PackSingle(0)
                .PackSingle(0)
                .PackInt16(weapon.Stack ?? 1)
                .PackByte(weapon.Prefix ?? 0)
                .PackByte(0)
                .PackInt16(weapon.ItemNetId)
                .GetByteData();
            var itemOwner = new PacketWriter()
                .SetType((int) PacketTypes.ItemOwner)
                .PackInt16((short) freeIndex)
                .PackByte((byte) player.Index)
                .GetByteData();

            player.SendRawData(itemDrop);
            player.SendRawData(GetAlterItemDropPacket(weapon, freeIndex));
            player.SendRawData(itemOwner);
        }
    }
}
