using Microsoft.Xna.Framework;

namespace PvPModifier.CustomWeaponAPI {
    /// <summary>
    /// Made by Rofle(popstarfreas) from DarkGaming
    /// </summary>
    public class CustomWeapon {
        public byte? Prefix;
        public short ItemNetId;
        public Color? Color;
        public short? Stack;
        public ushort? Damage;
        public float? Knockback;
        public ushort? UseAnimation;
        public ushort? UseTime;
        public short? ShootProjectileId;
        public float? ShootSpeed;
        public float? Scale;
        public short? AmmoIdentifier;
        public short? UseAmmoIdentifier;
        public bool? NotAmmo;
        public short? DropAreaWidth;
        public short? DropAreaHeight;

        public CustomWeapon(CustomWeapon weapon) {
            Prefix = weapon.Prefix;
            ItemNetId = weapon.ItemNetId;
            if (weapon.Color != null) {
                Color = new Color(((Color) weapon.Color).R, ((Color) weapon.Color).G, ((Color) weapon.Color).B);
            }

            Stack = weapon.Stack;
            Damage = weapon.Damage;
            Knockback = weapon.Knockback;
            UseAnimation = weapon.UseAnimation;
            UseTime = weapon.UseTime;
            ShootProjectileId = weapon.ShootProjectileId;
            ShootSpeed = weapon.ShootSpeed;
            Scale = weapon.Scale;
            AmmoIdentifier = weapon.AmmoIdentifier;
            UseAmmoIdentifier = weapon.UseAmmoIdentifier;
            NotAmmo = weapon.NotAmmo;
            DropAreaHeight = weapon.DropAreaHeight; 
            DropAreaWidth = weapon.DropAreaHeight; 
        }

        public override bool Equals(object obj) {
            CustomWeapon custwep = obj as CustomWeapon;
            if (custwep == null) return false;
            return Equals(custwep);
        }

        public bool Equals(CustomWeapon other) {
            if (other == null) return false;
            return ItemNetId == other.ItemNetId;
        }

        public CustomWeapon() { }
    }
}
