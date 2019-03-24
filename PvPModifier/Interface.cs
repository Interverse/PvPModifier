using Microsoft.Xna.Framework;
using System;
using System.Text;
using PvPModifier.DataStorage;
using PvPModifier.Utilities;
using PvPModifier.Variables;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using TShockAPI;

namespace PvPModifier {
    public class Interface {
        /// <summary>
        /// Brings a brief text pop-up above a person displaying a message.
        /// </summary>
        public static void PlayerTextPopup(PvPPlayer player, string message, Color color) {
            NetMessage.SendData(119, player.Index, -1, NetworkText.FromLiteral(message), (int)color.PackedValue, player.X, player.Y + 10);
        }

        /// <summary>
        /// Brings a brief text pop-up above a person displaying a message.
        /// </summary>
        public static void PlayerTextPopup(PvPPlayer player, PvPPlayer target, string message, Color color) {
            NetMessage.SendData(119, player.Index, -1, NetworkText.FromLiteral(message), (int)color.PackedValue, target.X, target.Y + 10);
        }
    }
}
