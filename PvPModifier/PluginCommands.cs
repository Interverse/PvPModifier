using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using PvPModifier.DataStorage;
using PvPModifier.Utilities;
using PvPModifier.Utilities.Extensions;
using PvPModifier.Utilities.PvPConstants;
using Terraria;
using TShockAPI;

namespace PvPModifier {
    public class PluginCommands {
        private static string InvalidSyntax = "Invalid Syntax. ";

        private static string AttributesHelp = "Type /modpvp help [database/config] to get a list of attributes";
        private static string HelpModPvP = InvalidSyntax +
                                           "/modpvp [item/projectile/buff] [ID/name] [attribute] [value]\n" +
                                           "or /modpvp config [config value] [value]\n" +
                                           AttributesHelp;
        private const string ResetList = "Parameters: [config/database/item/projectile/buff]";

        private static string NothingFoundError = InvalidSyntax + "Failed to find something with the given ID or Name.";
        private static string OverflowID = InvalidSyntax + "";

        private static string DatabaseAttributes = string.Join(", ", MiscUtils.GetConstants(typeof(DbConsts)));
        private static string ConfigAttributes = string.Join(", ", MiscUtils.GetConstants(typeof(ConfigConsts)));

        private static string InvalidValue(string attribute, string section) => attribute + " does not exist for " + section;
        private static string ConfigValueFail(string attribute, string value) => "Failed to set " + value + " to " + attribute;

        private static string InvalidCheckStat = InvalidSyntax + "/checkstat [item/projectile/buff] [id/name]";
        private static string ScrollUp = "Press the Up/Down arrow to scroll chat.";
        private static string NegativeExplanation = "-1 means the value was not modified.";

        public static void RegisterCommands() {
            Commands.ChatCommands.Add(new Command("pvpmodifier.modpvp", ModPvP, "modpvp", "mp") { HelpText = "Allows you to mod pvp values" });
            Commands.ChatCommands.Add(new Command("pvpmodifier.resetpvp", ResetPvP, "resetpvp", "rpvp") { HelpText = "Allows you to reset pvp values" });
            Commands.ChatCommands.Add(new Command(CheckStat, "checkstat", "cs") { HelpText = "Checks the stat of an item" });

            Commands.ChatCommands.Add(new Command("pvpcontroller.dpsify", DPSify, "dpsify") { HelpText = "Tries to change all weapon's damage to be around the same dps." });

            Commands.ChatCommands.Add(new Command("pvpmodifier.dev", SQLCommand, "sqlcommand", "sqlcmd") { HelpText = "Allows you to run a SQL command" });
        }

        /// <summary>
        /// Command can reset an item, projectile, buff, config, or database to its default values.
        /// Example usage: /resetpvp database, /resetpvp item 3292, /resetpvp config
        /// </summary>
        private static void ResetPvP(CommandArgs args) {
            var player = args.Player;
            var input = args.Parameters;

            // Sends an error message if player has not typed any arguments or their first argument is not item/projectile/buff
            if (input.Count < 1 || !StringConsts.TryGetSectionFromString(input[0], out var section)) {
                player.SendErrorMessage(InvalidSyntax + ResetList);
                return;
            }

            switch (section) {
                // Runs the database's methods to reset the database
                // Resets every person's inventory in the case that they have the modified items.
                case StringConsts.Database:
                    Database.InitDefaultTables();
                    Cache.Clear();
                    foreach (var pvper in PvPUtils.ActivePlayers)
                        PvPUtils.RefreshInventory(pvper);
                    player.SendSuccessMessage("Reset database to default.");
                    return;

                // Resets the config file's values and writes it in the .json file
                case StringConsts.Config:
                    PvPModifier.Config.ResetConfigValues();
                    PvPModifier.Config.Write(Config.ConfigPath);
                    player.SendSuccessMessage("Reset config values to default.");
                    return;

                // Resets an individual item, projectile, or buff
                case DbTables.ItemTable:
                case DbTables.ProjectileTable:
                case DbTables.BuffTable:
                    int id = -1;
                    // Attempts to get an item from the player's second argument
                    // Note: "item" in this case means any object (Item, Projectile, or Buff)
                    if (input.Count == 2 && !int.TryParse(input[1], out id)) {
                        var foundList = TShock.Utils.GetIdFromInput(section, input[1]);

                        // If no items have been found, exit the method and give an error message.
                        if (foundList.Count == 0) {
                            player.SendErrorMessage(NothingFoundError);
                            return;
                        }

                        // If multiple items have been found, send an error message and list all items found.
                        if (foundList.Count > 1) {
                            player.SendErrorMessage("Found multiple of input");
                            foreach (int items in foundList) {
                                player.SendMessage($"({items}) {MiscUtils.GetNameFromInput(section, items)}", Color.Yellow);
                            }
                            return;
                        }

                        // Stores the found item id, given it has not triggered any errors along the way.
                        id = foundList[0];
                    }

                    // If the ID is out of range for the item/projectile/buff, send an error message.
                    if ((section == DbTables.ItemTable && (id < 0 || id > Terraria.Main.maxItemTypes)) ||
                        (section == DbTables.ProjectileTable && (id < 0 || id > Terraria.Main.maxProjectileTypes)) ||
                        (section == DbTables.BuffTable && (id < 0 || id > Terraria.Main.maxBuffTypes))) {

                        player.SendErrorMessage(OverflowID);
                        return;
                    }

                    // Deletes the data of the item found and writes a new line with default stats.
                    Database.DeleteRow(section, id);
                    Database.Query(Database.GetDefaultValueSqlString(section, id));

                    // Logs the change to an external file and reloads the entire database.
                    string log = "Reset the values of {0}".SFormat(MiscUtils.GetNameFromInput(section, id));
                    player.SendSuccessMessage(log);
                    Cache.Load(section, id);
                    PvPModifier.Config.LogChange($"[{player.Name} ({DateTime.Now})] {log}");

                    // If the item is an Item, reset everyone's instance of that item.
                    if (section == DbTables.ItemTable)
                        foreach (var pvper in PvPUtils.ActivePlayers)
                            PvPUtils.RefreshItem(pvper, id);

                    break;

                default:
                    // The first argument was not valid.
                    player.SendErrorMessage("Invalid parameters. " + ResetList);
                    return;
            }
        }

        /// <summary>
        /// Command shows the current changes of an object in the database by calling the <see cref="DbObject"/>'s ToString() method.
        /// Example usage: /checkstat item daybreak, /checkstat projectile 3, /checkstat buff 92
        /// </summary>
        private static void CheckStat(CommandArgs args) {
            var player = args.Player;
            var input = args.Parameters;

            // Gives an error if the user hasn't given 2 arguments or their first argument is not item/projectile/buff
            if (input.Count != 2 || !StringConsts.TryGetSectionFromString(input[0], out string section)) {
                player.SendErrorMessage(InvalidCheckStat);
                return;
            }

            // If the user doesn't type a numerical id, try to find the ID from the name given
            if (!int.TryParse(input[1], out int id)) {
                var foundItems = TShock.Utils.GetIdFromInput(section, input[1]);
                if (foundItems.Count == 1) {
                    id = foundItems[0];
                } else if (foundItems.Count > 1) {
                    player.SendErrorMessage("Found Multiple items");
                    foreach (int item in foundItems) {
                        player.SendMessage($"({item}) {MiscUtils.GetNameFromInput(section, item)}", Color.Yellow);
                    }
                    return;
                } else {
                    player.SendErrorMessage(NothingFoundError);
                    return;
                }
            }

            // Sends the stats of the item to the player
            switch (section) {
                case DbTables.ItemTable:
                    if (id > 0 && id < Terraria.Main.maxItemTypes) {
                        player.SendMessage(Cache.GetDbObject(section, id).ToString(), Color.YellowGreen);
                        player.SendMessage(ScrollUp + "\n" + NegativeExplanation, Color.Yellow);
                    }
                    return;
                case DbTables.ProjectileTable:
                    if (id > 0 && id < Terraria.Main.maxProjectileTypes) {
                        player.SendMessage(Cache.GetDbObject(section, id).ToString(), Color.YellowGreen);
                        player.SendMessage(ScrollUp + "\n" + NegativeExplanation, Color.Yellow);
                    }
                    return;
                case DbTables.BuffTable:
                    if (id > 0 && id < Terraria.Main.maxBuffTypes) {
                        player.SendMessage(Cache.GetDbObject(section, id).ToString(), Color.YellowGreen);
                        player.SendMessage(ScrollUp + "\n" + NegativeExplanation, Color.Yellow);
                    }
                    return;
                case StringConsts.Config:
                    player.SendErrorMessage(InvalidCheckStat);
                    return;
            }

            // Gives an error message if the ID is out of bounds.
            player.SendErrorMessage(OverflowID);
        }

        /// <summary>
        /// Main command to modify different attributes of an item, projectile, or buff.
        /// The command processes the player's inputs and writes the changes in <see cref="Cache"/> and <see cref="Database"/>.
        /// Example: /modpvp item daybreak usetime 40 shootspeed 10
        /// </summary>
        private static void ModPvP(CommandArgs args) {
            StringBuilder sb = new StringBuilder();
            var player = args.Player;
            var input = args.Parameters;
            int id = -1;
            DbObject dbObject = null;

            // If the player has given no input or they didnt enter item/projectile/buff, give them the help text
            if (input.Count < 1 || !StringConsts.TryGetSectionFromString(input[0], out var section)) {
                player.SendErrorMessage(HelpModPvP);
                return;
            }

            // If the player has entered /modpvp help <database/config>, they get the list of attributes they can modify
            // If the second argument is empty, give them the possible arguments for /modpvp help
            if (section == StringConsts.Help) {
                if (input.Count > 1) {
                    StringConsts.TryGetSectionFromString(input[1], out var helpsection);

                    switch (helpsection) {
                        case StringConsts.Database:
                            player.SendMessage(DatabaseAttributes, Color.Yellow);
                            return;
                        case StringConsts.Config:
                            player.SendMessage(ConfigAttributes, Color.Yellow);
                            return;
                        default:
                            player.SendErrorMessage(InvalidSyntax + AttributesHelp);
                            return;
                    }
                }

                player.SendErrorMessage(AttributesHelp);
                return;
            }

            // If the first argument is not config, get the item id the second argument is referencing
            // Ex: /modpvp item daybreak, where item is the first argument and daybreak is the second argument
            if (section != StringConsts.Config) {
                if (input.Count < 2) {
                    player.SendErrorMessage(HelpModPvP);
                    return;
                }

                // Try to get the ID of an object if the user hasn't already inputted one
                if (!int.TryParse(input[1], out id)) {
                    var foundItems = TShock.Utils.GetIdFromInput(section, input[1]); 

                    if (foundItems.Count == 1) {
                        id = foundItems[0];
                    } else if (foundItems.Count > 1) {
                        player.SendErrorMessage("Found Multiple items");
                        foreach (int item in foundItems) {
                            player.SendMessage($"({item}) {MiscUtils.GetNameFromInput(section, item)}", Color.Yellow);
                        }
                        return;
                    } else {
                        player.SendErrorMessage(NothingFoundError);
                        return;
                    }
                }

                // Get the DbObject the ID was referencing
                dbObject = Cache.GetDbObject(section, id);
                if (dbObject == null) {
                    player.SendErrorMessage(NothingFoundError);
                    return;
                }
            }

            // Gives an error if the user has not put in any attributes to modify
            if (input.Count < (4 - (section == StringConsts.Config).ToInt())) {
                player.SendErrorMessage(InvalidSyntax + "Please enter value(s) for the attribute(s).");
                return;
            }

            // Splits the input into pairs in the form of string[][], where first dimension is the index of pairs and
            // the second dimension is the two pairs
            var pairedInputs = MiscUtils.SplitIntoPairs(input.Skip((section != StringConsts.Config).ToInt() + 1).ToArray());

            // Converts abbreviations into the actual attribute string name
            foreach (var pair in pairedInputs) {
                var attribute = "";
                if (section != StringConsts.Config)
                    StringConsts.TryGetDatabaseAttributeFromString(pair[0], out attribute);
                else
                    StringConsts.TryGetConfigValueFromString(pair[0], out attribute);

                pair[0] = attribute;
            }

            switch (section) {
                case DbTables.ItemTable:
                case DbTables.ProjectileTable:
                case DbTables.BuffTable:
                    // Displays the object being modified
                    player.SendMessage($"Modifying {MiscUtils.GetNameFromInput(section, id)} ({id})", Color.Green);
                    sb.AppendLine($"Modifying {MiscUtils.GetNameFromInput(section, id)} ({id})");

                    // For each pair, set the attribute to the value
                    foreach (var pair in pairedInputs) {
                        if (dbObject.TrySetValue(pair[0], pair[1])) {
                            player.SendMessage($"Set {pair[0]} to {pair[1]}", Color.YellowGreen);
                            sb.AppendLine($"Set {pair[0]} to {pair[1]}");
                        } else {
                            player.SendErrorMessage(InvalidValue(pair[0], section));
                        }
                    }

                    // If the item is an Item, update everyone's instance of that Item
                    if (dbObject is DbItem) {
                        foreach (var pvper in PvPUtils.ActivePlayers) {
                            if (!pvper.TPlayer.hostile) continue;

                            // If they have the item, remove that item and replace it with an updated one
                            int itemindex = pvper.TPlayer.FindItem(id);
                            if (itemindex != -1) {
                                // Fills everyone's inventory with a junk item to preserve the item slot,
                                // as dropped items go to the closest empty item slot
                                SSCUtils.FillInventoryToIndex(pvper, 0, Constants.JunkItem, itemindex);
                                var item = pvper.TPlayer.inventory[itemindex];
                                SSCUtils.SetItem(pvper, (byte)itemindex, 0);
                                pvper.GetInvTracker().AddItem(PvPUtils.GetCustomWeapon(pvper, id, item.prefix, (short)item.stack));
                            }
                            pvper.GetInvTracker().StartDroppingItems();
                        }
                    }

                    if (sb.Length > 0) {
                        PvPModifier.Config.LogChange($"({DateTime.Now}: {player.Name}) {sb.ToString()}");
                    }

                    break;

                // Sets the values to each config attribute
                case StringConsts.Config:
                    foreach (var pair in pairedInputs) {
                        if (MiscUtils.SetValueWithString(PvPModifier.Config, pair[0], pair[1])) {
                            player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                        } else {
                            player.SendErrorMessage(ConfigValueFail(pair[0], pair[1]));
                        }
                    }

                    // Writes the current changes to the .json file
                    PvPModifier.Config.Write(Config.ConfigPath);
                    break;

                default:
                    player.SendErrorMessage(HelpModPvP);
                    return;
            }
        }

        /// <summary>
        /// Dev command to run a SQL command.
        /// </summary>
        private static void SQLCommand(CommandArgs args) {
            string statement = string.Join(" ", args.Parameters);

            if (!Database.Query(statement))
                args.Player.SendErrorMessage("SQL statement failed.");
            else
                args.Player.SendSuccessMessage("SQL statement was successful.");
        }

        /// <summary>
        /// Attempts to change every weapon's dps to the same value.
        /// Formula: √(dps^2 * useanimation / 60)
        /// </summary>
        private static void DPSify(CommandArgs args) {
            List<string> queries = new List<string>();

            // Gives an error if the user hasn't entered a dps value
            if (args.Parameters.Count < 1 || !Double.TryParse(args.Parameters[0], out double dps)) {
                args.Player.SendErrorMessage("Invalid dps value.");
                return;
            }

            dps = dps * dps;

            // Goes through every item and calculates their new damage, given that it deals damage and is not ammo
            // Adds the list of potential damage changes to the queries list
            for (int x = 0; x < Terraria.Main.maxItemTypes; x++) {
                Item item = new Item();
                item.SetDefaults(x);

                var dbitem = (DbItem)Cache.GetDbObject(DbTables.ItemTable, x);
                int useanimation = dbitem.UseAnimation != -1 ?
                    dbitem.UseAnimation.Replace(0, 1) : item.useAnimation;

                if (item.damage > 0 && item.ammo == 0) {
                    queries.Add($"UPDATE {DbTables.ItemTable} SET {DbConsts.Damage} = {(int)Math.Sqrt(dps * useanimation / 60.0)} WHERE ID = {x}");
                }
            }

            // Updates the damage to every weapon and logs the action
            Database.PerformTransaction(queries.ToArray());
            Cache.Clear();
            string log = $"Set all weapon's pvp dps to be approx {Math.Sqrt(dps)}";
            args.Player.SendSuccessMessage(log);
            PvPModifier.Config.LogChange($"({DateTime.Now}) {log}");

            // Gives everyone the updated items
            foreach (var player in PvPUtils.ActivePlayers) {
                if (player.TPlayer.hostile)
                    PvPUtils.SendCustomItems(player);
            }
        }
    }
}