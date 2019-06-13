using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using PvPModifier.DataStorage;
using PvPModifier.Utilities;
using PvPModifier.Utilities.PvPConstants;
using PvPModifier.Variables;
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

            Commands.ChatCommands.Add(new Command("pvpmodifier.dev", SqlInject, "sqlinject") { HelpText = "Allows you to run a SQL command" });
        }

        private static void ResetPvP(CommandArgs args) {
            var player = args.Player;
            var input = args.Parameters;

            if (input.Count < 1 || !StringConsts.TryGetSectionFromString(input[0], out var section)) {
                player.SendErrorMessage(InvalidSyntax + ResetList);
                return;
            }

            switch (section) {
                case StringConsts.Database:
                    Database.InitDefaultTables();
                    Database.LoadDatabase();
                    foreach (var pvper in PvPUtils.ActivePlayers)
                        PvPUtils.RefreshInventory(pvper);
                    player.SendSuccessMessage("Reset database to default.");
                    return;

                case StringConsts.Config:
                    PvPModifier.Config.ResetConfigValues();
                    PvPModifier.Config.Write(Config.ConfigPath);
                    player.SendSuccessMessage("Reset config values to default.");
                    return;

                case DbTables.ItemTable:
                case DbTables.ProjectileTable:
                case DbTables.BuffTable:
                    if (input.Count < 2 || !int.TryParse(input[1], out int id)) {
                        var foundList = TShock.Utils.GetIdFromInput(section, input[1]);
                        if (foundList.Count == 0) {
                            player.SendErrorMessage("Please provide a valid id or name.");
                            return;
                        }

                        if (foundList.Count > 1) {
                            player.SendErrorMessage("Found multiple of input");
                            foreach (int items in foundList) {
                                player.SendMessage($"({items}) {MiscUtils.GetNameFromInput(section, items)}", Color.Yellow);
                            }
                            return;
                        }

                        id = foundList[0];
                    }

                    Database.DeleteRow(section, id);
                    Database.Query(Database.GetDefaultValueSqlString(section, id));

                    string log = "Reset the values of {0}".SFormat(MiscUtils.GetNameFromInput(section, id));
                    if (section == DbTables.ItemTable)
                        foreach (var pvper in PvPUtils.ActivePlayers)
                            PvPUtils.RefreshItem(pvper, id);
                    player.SendSuccessMessage(log);
                    Database.LoadDatabase();
                    PvPModifier.Config.LogChange($"[{player.Name} ({DateTime.Now})] {log}");
                    break;

                default:
                    player.SendErrorMessage("Invalid parameters. " + ResetList);
                    return;
            }
        }

        private static void CheckStat(CommandArgs args) {
            var player = args.Player;
            var input = args.Parameters;

            if (input.Count != 2 || !StringConsts.TryGetSectionFromString(input[0], out string section)) {
                player.SendErrorMessage(InvalidCheckStat);
                return;
            }

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

            switch (section) {
                case DbTables.ItemTable:
                    if (id > 0 && id < Terraria.Main.maxItemTypes) {
                        player.SendMessage(Cache.Items[id].ToString(), Color.YellowGreen);
                        player.SendMessage(ScrollUp + "\n" + NegativeExplanation, Color.Yellow);
                    }
                    return;
                case DbTables.ProjectileTable:
                    if (id > 0 && id < Terraria.Main.maxProjectileTypes) {
                        player.SendMessage(Cache.Projectiles[id].ToString(), Color.YellowGreen);
                        player.SendMessage(ScrollUp + "\n" + NegativeExplanation, Color.Yellow);
                    }
                    return;
                case DbTables.BuffTable:
                    if (id > 0 && id < Terraria.Main.maxBuffTypes) {
                        player.SendMessage(Cache.Buffs[id].ToString(), Color.YellowGreen);
                        player.SendMessage(ScrollUp + "\n" + NegativeExplanation, Color.Yellow);
                    }
                    return;
                case StringConsts.Config:
                    player.SendErrorMessage(InvalidCheckStat);
                    return;
            }

            player.SendErrorMessage(OverflowID);
        }

        private static void ModPvP(CommandArgs args) {
            var player = args.Player;
            var input = args.Parameters;
            int id = -1;
            DbObject dbObject = null;

            if (input.Count < 1 || !StringConsts.TryGetSectionFromString(input[0], out var section)) {
                player.SendErrorMessage(HelpModPvP);
                return;
            }

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

            if (section != StringConsts.Config) {
                if (input.Count < 3) {
                    player.SendErrorMessage(HelpModPvP);
                    return;
                }

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

                dbObject = Cache.GetSection(section, id);
                if (dbObject == null) {
                    player.SendErrorMessage(NothingFoundError);
                    return;
                }

                if (input.Count < 4) {
                    player.SendErrorMessage(InvalidSyntax + "Please enter value(s) for the attribute(s).");
                    return;
                }
            } else {
                if (input.Count < 3) {
                    player.SendErrorMessage(HelpModPvP);
                    return;
                }
            }

            var pairedInputs = MiscUtils.SplitIntoPairs(input.Skip((section != StringConsts.Config).ToInt() + 1).ToArray());

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
                    player.SendMessage($"Modifying {MiscUtils.GetNameFromInput(section, id)} ({id})", Color.Green);

                    foreach (var pair in pairedInputs) {
                        if (dbObject.TrySetValue(pair[0], pair[1])) {
                            player.SendMessage($"Set {pair[0]} to {pair[1]}", Color.YellowGreen);
                        } else {
                            player.SendErrorMessage(InvalidValue(pair[0], section));
                        }
                    }

                    if (dbObject is DbItem) {
                        foreach (var pvper in PvPUtils.ActivePlayers) {
                            if (!pvper.TPlayer.hostile) continue;

                            int itemindex = pvper.TPlayer.FindItem(id);
                            if (itemindex != -1) {
                                SSCUtils.FillInventoryToIndex(pvper, 0, Constants.JunkItem, itemindex);
                                var item = pvper.TPlayer.inventory[itemindex];
                                SSCUtils.SetItem(pvper, (byte)itemindex, 0);
                                pvper.GetInvTracker().AddItem(PvPUtils.GetCustomWeapon(pvper, id, item.prefix, (short)item.stack));
                            }
                            pvper.GetInvTracker().DropModifiedItems();
                        }
                    }

                    break;

                case StringConsts.Config:
                    foreach (var pair in pairedInputs) {
                        if (MiscUtils.SetValueWithString(PvPModifier.Config, pair[0], pair[1])) {
                            player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                        } else {
                            player.SendErrorMessage(ConfigValueFail(pair[0], pair[1]));
                        }
                    }

                    PvPModifier.Config.Write(Config.ConfigPath);
                    break;

                default:
                    player.SendErrorMessage(HelpModPvP);
                    return;
            }
        }

        private static void SqlInject(CommandArgs args) {
            string statement = string.Join(" ", args.Parameters);

            if (!Database.Query(statement))
                args.Player.SendErrorMessage("SQL statement failed.");
            else
                args.Player.SendSuccessMessage("SQL statement was successful.");
        }

        private static void DPSify(CommandArgs args) {
            List<string> queries = new List<string>();

            if (args.Parameters.Count < 1 || !Double.TryParse(args.Parameters[0], out double dps)) {
                args.Player.SendErrorMessage("Invalid dps value.");
                return;
            }

            dps = dps * dps;

            for (int x = 0; x < Terraria.Main.maxItemTypes; x++) {
                Item item = new Item();
                item.SetDefaults(x);

                int useanimation = Cache.Items[x].UseAnimation != -1 ? Cache.Items[x].UseAnimation.Replace(0, 1) : item.useAnimation;

                if (item.damage > 0 && item.ammo == 0) {
                    queries.Add($"UPDATE {DbTables.ItemTable} SET {DbConsts.Damage} = {(int)Math.Sqrt(dps * useanimation / 60.0)} WHERE ID = {x}");
                }
            }

            Database.PerformTransaction(queries.ToArray());
            Database.LoadDatabase();
            string log = $"Set all weapon's pvp dps to be approx {Math.Sqrt(dps)}";
            args.Player.SendSuccessMessage(log);
            PvPModifier.Config.LogChange($"({DateTime.Now}) {log}");

            foreach (var player in PvPUtils.ActivePlayers) {
                if (player.TPlayer.hostile)
                    PvPUtils.SendCustomItemsAsync(player);
            }
        }
    }
}