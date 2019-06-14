﻿using System;
using System.IO;
using System.IO.Streams;
using Microsoft.Xna.Framework;
using PvPModifier.Utilities;
using PvPModifier.Variables;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace PvPModifier.Network.Packets {
    public class ProjectileNewArgs : EventArgs {
        
        public GetDataEventArgs Args;
        public TSPlayer Attacker;
        public Item Weapon;
        public Projectile Proj;

        public int Identity;
        public Vector2 Position;
        public Vector2 Velocity;
        public Single Knockback;
        public int Damage;
        public int Owner;
        public int Type;
        public BitsByte AiFlags;
        public float Ai0;
        public float Ai1;
        public float[] Ai;

        public bool ExtractData(GetDataEventArgs args, MemoryStream data, TSPlayer attacker, out ProjectileNewArgs arg) {
            arg = null;
            if (PresetData.ProjectileDummy.Contains(Type)) return false;

            arg = new ProjectileNewArgs {
                Args = args,
                Attacker = attacker,

                Identity = data.ReadInt16(),
                Position = new Vector2(data.ReadSingle(), data.ReadSingle()),
                Velocity = new Vector2(data.ReadSingle(), data.ReadSingle()),
                Knockback = data.ReadSingle(),
                Damage = data.ReadInt16(),
                Owner = data.ReadByte(),
                Type = data.ReadInt16(),
                AiFlags = (BitsByte)data.ReadByte(),
                Ai0 = AiFlags[0] ? Ai0 = data.ReadSingle() : 0,
                Ai1 = AiFlags[1] ? data.ReadSingle() : 0,

                Ai = new float[Projectile.maxAI]
            };

            arg.Proj = new Projectile().SetType(arg.Type).SetIdentity(arg.Identity);
            arg.Weapon = ProjectileUtils.GetProjectileWeapon(arg.Attacker, arg.Type);

            return true;
        }
    }
}
