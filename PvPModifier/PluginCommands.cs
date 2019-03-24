using System;
using System.Linq;
using Microsoft.Xna.Framework;
using PvPModifier.DataStorage;
using PvPModifier.Utilities;
using Terraria;
using TShockAPI;
using static PvPModifier.Utilities.DbConsts;

namespace PvPModifier {
    public class PluginCommands {
        private static string InvalidSyntax = "Invalid Syntax. ";

        private static string HelpModPvP = InvalidSyntax +
                                           "/modpvp [item/projectile/buff] [ID/Name] [attribute] [values]\n" +
                                           "or /modpvp config [ConfigValue] [Values]";
        private const string ResetList = "Parameters: [config/database/item/projectile/buff]";

        private static string NothingFoundError = InvalidSyntax + "Failed to find something with the given ID or Name.";
        private static string OverflowID = InvalidSyntax + "ID is out of bounds.";

        private static string Attributes = string.Join(", ", Damage, Knockback, UseTime,
            UseAnimation, Shoot, ShootSpeed, VelocityMultiplier,
            AmmoIdentifier, UseAmmoIdentifier, NotAmmo,
            InflictBuffID, InflictBuffDuration, ReceiveBuffID, ReceiveBuffDuration);

        private static string InvalidAttributes = "Possible attributes are: " + Attributes;
        private static string InvalidValue(string attribute) => "Invalid " + attribute + " value.";

        private static string InvalidCheckStat = InvalidSyntax + "/checkstat [item/projectile/buff] [id]";
        private static string ScrollUp = "Press the Up/Down arrow to scroll chat.";
        private static string NegativeExplanation = "-1 means the value was not modified.";

        public static void RegisterCommands() {
            Commands.ChatCommands.Add(new Command("pvpmodifier", ModPvP, "modpvp", "mp") {HelpText = "Allows you to mod pvp values"});
            Commands.ChatCommands.Add(new Command("pvpmodifier", ResetPvP, "resetpvp", "rpvp") { HelpText = "Allows you to reset pvp values" });
            Commands.ChatCommands.Add(new Command(CheckStat, "checkstat", "cs") { HelpText = "Checks the stat of an item" });

            Commands.ChatCommands.Add(new Command("pvpmodifier.dev", SqlInject, "sqlinject") {HelpText = "Allows you to run a SQL command"});
            Commands.ChatCommands.Add(new Command("pvpmodifier.dev", Reload, "reload") {HelpText = "Reloads pvp data"});
        }

        private static void ResetPvP(CommandArgs args) {
            var player = PvPModifier.PvPers[args.Player.Index];
            var input = args.Parameters;

            if (input.Count < 1 || !StringConsts.TryGetSectionFromString(input[0], out var section)) {
                player.SendErrorMessage(InvalidSyntax + ResetList);
                return;
            }

            switch (section) {
                case StringConsts.Database:
                    Database.InitDefaultTables();
                    Database.LoadDatabase();
                    foreach (var pvper in PvPModifier.PvPers.Where(c => c != null))
                        PvPUtils.RefreshInventory(pvper);
                    player.SendSuccessMessage("Reset database to default.");
                    return;

                case StringConsts.Config:
                    PvPModifier.Config.ResetConfigValues();
                    PvPModifier.Config.Write(Config.ConfigPath);
                    player.SendSuccessMessage("Reset config values to default.");
                    return;

                case DbConsts.ItemTable:
                case DbConsts.ProjectileTable:
                case DbConsts.BuffTable:
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
                    if (section == DbConsts.ItemTable)
                        PvPUtils.RefreshItem(player, id);
                    player.SendSuccessMessage(log);
                    Database.LoadDatabase();
                    PvPModifier.Config.LogChange($"({DateTime.Now}) {log}");
                    break;

                default:
                    player.SendErrorMessage("Invalid parameters. " + ResetList);
                    return;
            }
        }

        private static void CheckStat(CommandArgs args) {
            var player = args.Player;
            var input = args.Parameters;

            if (input.Count != 2 || !StringConsts.TryGetSectionFromString(input[0], out var table)
                || !int.TryParse(input[1], out int id)) {
                player.SendErrorMessage(InvalidCheckStat);
                return;
            }

            switch (table) {
                case ItemTable:
                    if (id > 0 && id < Terraria.Main.maxItemTypes) {
                        player.SendMessage(Cache.Items[id].ToString(), Color.YellowGreen);
                        player.SendMessage(ScrollUp + "\n" + NegativeExplanation, Color.Yellow);
                    }
                    return;
                case ProjectileTable:
                    if (id > 0 && id < Terraria.Main.maxProjectileTypes) {
                        player.SendMessage(Cache.Projectiles[id].ToString(), Color.YellowGreen);
                        player.SendMessage(ScrollUp + "\n" + NegativeExplanation, Color.Yellow);
                    }
                    return;
                case BuffTable:
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
            string attribute = "";
            int id = -1;

            if (input.Count < 1 || !StringConsts.TryGetSectionFromString(input[0], out var section)) {
                player.SendErrorMessage(HelpModPvP);
                return;
            }

            if (section != StringConsts.Config) {
                if (input.Count < 3) {
                    player.SendErrorMessage(HelpModPvP);
                    player.SendErrorMessage(InvalidAttributes);
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
                    } else {
                        player.SendErrorMessage(NothingFoundError);
                        return;
                    }
                }

                if (id < 0) {
                    player.SendErrorMessage(NothingFoundError);
                    return;
                }

                if (input.Count < 4) {
                    player.SendErrorMessage(InvalidSyntax + "Please enter values for the attribute.");
                    return;
                }
            } else {
                if (input.Count < 3) {
                    player.SendErrorMessage(HelpModPvP);
                    return;
                }
            }

            var pairedInputs = MiscUtils.SplitIntoPairs(input.Skip(2).ToArray());

            foreach (var pair in pairedInputs) {
                StringConsts.TryGetAttributeFromString(pair[0], out attribute);
                pair[0] = attribute;
            }

            switch (section) {
                case ItemTable:
                    if (id > Terraria.Main.maxItemTypes) {
                        player.SendErrorMessage(ItemTable + ": " + OverflowID);
                        return;
                    }
                    
                    foreach (var pair in pairedInputs) {
                        switch (pair[0]) {
                            case Damage:
                                if (int.TryParse(pair[1], out int damage)) {
                                    Cache.Items[id].Damage = damage;
                                    Database.Update(section, id, Damage, damage);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(Damage));
                                }

                                break;

                            case Knockback:
                                if (float.TryParse(pair[1], out float knockback)) {
                                    Cache.Items[id].Knockback = knockback;
                                    Database.Update(section, id, Knockback, knockback);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(Knockback));
                                }

                                break;

                            case UseAnimation:
                                if (int.TryParse(pair[1], out int useanimation)) {
                                    Cache.Items[id].UseAnimation = useanimation;
                                    Database.Update(section, id, UseAnimation, useanimation);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(UseAnimation));
                                }

                                break;

                            case UseTime:
                                if (int.TryParse(pair[1], out int usetime)) {
                                    Cache.Items[id].UseTime = usetime;
                                    Database.Update(section, id, UseTime, usetime);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(UseTime));
                                }

                                break;

                            case Shoot:
                                if (int.TryParse(pair[1], out int shoot)) {
                                    Cache.Items[id].Shoot = shoot;
                                    Database.Update(section, id, Shoot, shoot);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(Shoot));
                                }

                                break;

                            case ShootSpeed:
                                if (float.TryParse(pair[1], out float shootspeed)) {
                                    Cache.Items[id].ShootSpeed = shootspeed;
                                    Database.Update(section, id, ShootSpeed, shootspeed);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(ShootSpeed));
                                }

                                break;

                            case AmmoIdentifier:
                                if (int.TryParse(pair[1], out int ammoidentifier)) {
                                    Cache.Items[id].AmmoIdentifier = ammoidentifier;
                                    Database.Update(section, id, AmmoIdentifier, ammoidentifier);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(AmmoIdentifier));
                                }

                                break;

                            case UseAmmoIdentifier:
                                if (int.TryParse(pair[1], out int useammoidentifier)) {
                                    Cache.Items[id].UseAmmoIdentifier = useammoidentifier;
                                    Database.Update(section, id, UseAmmoIdentifier, useammoidentifier);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(UseAmmoIdentifier));
                                }

                                break;

                            case NotAmmo:
                                if (bool.TryParse(pair[1], out bool notammo)) {
                                    Cache.Items[id].NotAmmo = notammo;
                                    Database.Update(section, id, NotAmmo, notammo.ToInt());
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(NotAmmo));
                                }

                                break;

                            case InflictBuffID:
                                if (int.TryParse(pair[1], out int inflictbuffid)) {
                                    Cache.Items[id].InflictBuff.BuffId = inflictbuffid;
                                    Database.Update(section, id, InflictBuffID, inflictbuffid);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(InflictBuffID));
                                }

                                break;

                            case InflictBuffDuration:
                                if (int.TryParse(pair[1], out int inflictbuffduration)) {
                                    Cache.Items[id].InflictBuff.BuffDuration = inflictbuffduration;
                                    Database.Update(section, id, InflictBuffDuration, inflictbuffduration);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(InflictBuffDuration));
                                }

                                break;

                            case ReceiveBuffID:
                                if (int.TryParse(pair[1], out int receivebuffid)) {
                                    Cache.Items[id].ReceiveBuff.BuffId = receivebuffid;
                                    Database.Update(section, id, InflictBuffID, receivebuffid);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(ReceiveBuffID));
                                }

                                break;

                            case ReceiveBuffDuration:
                                if (int.TryParse(pair[1], out int receivebuffduration)) {
                                    Cache.Items[id].ReceiveBuff.BuffDuration = receivebuffduration;
                                    Database.Update(section, id, InflictBuffDuration, receivebuffduration);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(ReceiveBuffDuration));
                                }

                                break;

                            default:
                                player.SendErrorMessage(attribute + " does not exist for Items.");
                                return;
                        }
                    }

                    foreach (var pvper in PvPModifier.PvPers.Where(c => c != null && c.TPlayer.hostile)) {
                        int itemindex = pvper.TPlayer.FindItem(id);
                        if (itemindex != -1) {
                            SSCUtils.FillInventoryToIndex(pvper, 0, Constants.JunkItem, itemindex);
                            var item = pvper.TPlayer.inventory[itemindex];
                            SSCUtils.SetItem(pvper, (byte)itemindex, 0);
                            pvper.InvTracker.AddItem(PvPUtils.GetCustomWeapon(pvper, id, item.prefix, (short)item.stack));
                        }
                        pvper.InvTracker.StartDroppingItems();
                    }
                    break;

                case ProjectileTable:
                    if (id > Terraria.Main.maxProjectileTypes) {
                        player.SendErrorMessage(ProjectileTable + ": " + OverflowID);
                        return;
                    }

                    foreach (var pair in pairedInputs) {
                        switch (attribute) {
                            case Shoot:
                                if (int.TryParse(pair[1], out int shoot)) {
                                    Cache.Projectiles[id].Shoot = shoot;
                                    Database.Update(section, id, Shoot, shoot);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(Shoot));
                                }

                                break;

                            case Damage:
                                if (int.TryParse(pair[1], out int damage)) {
                                    Cache.Projectiles[id].Damage = damage;
                                    Database.Update(section, id, Damage, damage);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(Damage));
                                }

                                break;

                            case VelocityMultiplier:
                                if (float.TryParse(pair[1], out float velocitymultiplier)) {
                                    Cache.Projectiles[id].VelocityMultiplier = velocitymultiplier;
                                    Database.Update(section, id, VelocityMultiplier, velocitymultiplier);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(VelocityMultiplier));
                                }

                                break;

                            case InflictBuffID:
                                if (int.TryParse(pair[1], out int inflictbuffid)) {
                                    Cache.Projectiles[id].InflictBuff.BuffId = inflictbuffid;
                                    Database.Update(section, id, InflictBuffID, inflictbuffid);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(InflictBuffID));
                                }

                                break;

                            case InflictBuffDuration:
                                if (int.TryParse(pair[1], out int inflictbuffduration)) {
                                    Cache.Projectiles[id].InflictBuff.BuffDuration = inflictbuffduration;
                                    Database.Update(section, id, InflictBuffDuration, inflictbuffduration);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(InflictBuffDuration));
                                }

                                break;

                            case ReceiveBuffID:
                                if (int.TryParse(pair[1], out int receivebuffid)) {
                                    Cache.Projectiles[id].ReceiveBuff.BuffId = receivebuffid;
                                    Database.Update(section, id, InflictBuffID, receivebuffid);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(ReceiveBuffID));
                                }

                                break;

                            case ReceiveBuffDuration:
                                if (int.TryParse(pair[1], out int receivebuffduration)) {
                                    Cache.Projectiles[id].ReceiveBuff.BuffDuration = receivebuffduration;
                                    Database.Update(section, id, InflictBuffDuration, receivebuffduration);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(ReceiveBuffDuration));
                                }

                                break;

                            default:
                                player.SendErrorMessage(attribute + " does not exist for Projectiles.");
                                return;
                        }
                    }
                    

                    break;

                case BuffTable:
                    if (id > Terraria.Main.maxBuffTypes) {
                        player.SendErrorMessage(BuffTable + ": " + OverflowID);
                        return;
                    }

                    foreach (var pair in pairedInputs) {
                        switch (attribute) {
                            case InflictBuffID:
                                if (int.TryParse(pair[1], out int inflictbuffid)) {
                                    Cache.Buffs[id].InflictBuff.BuffId = inflictbuffid;
                                    Database.Update(section, id, InflictBuffID, inflictbuffid);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(InflictBuffID));
                                }

                                break;

                            case InflictBuffDuration:
                                if (int.TryParse(pair[1], out int inflictbuffduration)) {
                                    Cache.Buffs[id].InflictBuff.BuffDuration = inflictbuffduration;
                                    Database.Update(section, id, InflictBuffDuration, inflictbuffduration);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(InflictBuffDuration));
                                }

                                break;

                            case ReceiveBuffID:
                                if (int.TryParse(pair[1], out int receivebuffid)) {
                                    Cache.Buffs[id].ReceiveBuff.BuffId = receivebuffid;
                                    Database.Update(section, id, InflictBuffID, receivebuffid);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(ReceiveBuffID));
                                }

                                break;

                            case ReceiveBuffDuration:
                                if (int.TryParse(pair[1], out int receivebuffduration)) {
                                    Cache.Buffs[id].ReceiveBuff.BuffDuration = receivebuffduration;
                                    Database.Update(section, id, InflictBuffDuration, receivebuffduration);
                                    player.SendSuccessMessage($"Set {pair[0]} to {pair[1]}");
                                } else {
                                    player.SendErrorMessage(InvalidValue(ReceiveBuffDuration));
                                }

                                break;

                            default:
                                player.SendErrorMessage(attribute + " does not exist for Buffs.");
                                return;
                        }
                    }
                    
                    break;

                case StringConsts.Config:
                    switch (input[1].ToLower()) {
                        case "enable":
                        case "e":
                            switch (input[2].ToLower()) {
                                case "plugin":
                                case "p":
                                    PvPModifier.Config.EnablePlugin = !PvPModifier.Config.EnablePlugin;
                                    player.SendSuccessMessage("Plugin: " + PvPModifier.Config.EnablePlugin);
                                    break;

                                case "knockback":
                                case "kb":
                                case "k":
                                    PvPModifier.Config.EnableKnockback = !PvPModifier.Config.EnableKnockback;
                                    player.SendSuccessMessage("Knockback: " + PvPModifier.Config.EnableKnockback);
                                    break;

                                case "turtle":
                                    PvPModifier.Config.EnableTurtle = !PvPModifier.Config.EnableTurtle;
                                    player.SendSuccessMessage("Turtle: " + PvPModifier.Config.EnableTurtle);
                                    break;

                                case "thorns":
                                    PvPModifier.Config.EnableThorns = !PvPModifier.Config.EnableThorns;
                                    player.SendSuccessMessage("Thorns: " + PvPModifier.Config.EnableThorns);
                                    break;

                                case "nebula":
                                case "n":
                                    PvPModifier.Config.EnableNebula = !PvPModifier.Config.EnableNebula;
                                    player.SendSuccessMessage("Nebula: " + PvPModifier.Config.EnableNebula);
                                    break;

                                case "buffs":
                                case "b":
                                    PvPModifier.Config.EnableBuffs = !PvPModifier.Config.EnableBuffs;
                                    player.SendSuccessMessage("Buffs: " + PvPModifier.Config.EnableBuffs);
                                    break;

                                case "frost":
                                case "f":
                                    PvPModifier.Config.EnableFrost = !PvPModifier.Config.EnableFrost;
                                    player.SendSuccessMessage("Frost: " + PvPModifier.Config.EnableFrost);
                                    break;
                            }
                            break;

                        case "nebula":
                        case "n":
                            if (input.Count > 4) {
                                if (double.TryParse(input[2], out double tier1) &&
                                    double.TryParse(input[3], out double tier2) &&
                                    double.TryParse(input[4], out double tier3)) {
                                    PvPModifier.Config.NebulaTier1Duration = tier1;
                                    PvPModifier.Config.NebulaTier2Duration = tier2;
                                    PvPModifier.Config.NebulaTier3Duration = tier3;

                                    player.SendSuccessMessage($"Tier durations set to 1:{tier1}s, 2:{tier2}s, 3:{tier3}s");
                                }
                            } else {
                                player.SendErrorMessage(InvalidSyntax + "/modpvp config nebula [tier1] [tier2] [tier3]");
                            }
                            break;

                        case "frost":
                        case "f":
                            if (double.TryParse(input[2], out double frost)) {
                                PvPModifier.Config.FrostDuration = frost;
                                player.SendSuccessMessage($"Frost armor debuff duration set to {frost}s");
                            } else {
                                player.SendErrorMessage(InvalidSyntax + "/modpvp config frost [seconds]");
                            }
                            break;

                        case "turtle":
                            if (double.TryParse(input[2], out double turtle)) {
                                PvPModifier.Config.TurtleMultiplier = turtle;
                                player.SendSuccessMessage($"Turtle reflect multiplier set to {turtle}");
                            } else {
                                player.SendErrorMessage(InvalidSyntax + "/modpvp config turtle [multiplier]");
                            }
                            break;

                        case "thorns":
                            if (double.TryParse(input[2], out double thorns)) {
                                PvPModifier.Config.ThornMultiplier = thorns;
                                player.SendSuccessMessage($"Turtle reflect multiplier set to {thorns}");
                            } else {
                                player.SendErrorMessage(InvalidSyntax + "/modpvp config thorns [multiplier]");
                            }
                            break;

                        case "iframetime":
                        case "iframe":
                        case "ift":
                            if (double.TryParse(input[2], out double ift)) {
                                PvPModifier.Config.IframeTime = ift;
                                player.SendSuccessMessage($"Iframe time set to {ift}s");
                            } else {
                                player.SendErrorMessage(InvalidSyntax + "/modpvp config iframetime [seconds]");
                            }
                            break;

                        default:
                            player.SendErrorMessage("This value does not exist in the config.");
                            return;
                    }

                    PvPModifier.Config.Write(Config.ConfigPath);
                    break;

                default:
                    player.SendErrorMessage(HelpModPvP);
                    return;
            }
        }

        private static void Reload(CommandArgs args) {
            PvPModifier.Config = Config.Read(Config.ConfigPath);
            Database.LoadDatabase();
            args.Player.SendSuccessMessage("PvPModifier reloaded.");
        }

        private static void SqlInject(CommandArgs args) {
            string statement = string.Join(" ", args.Parameters);

            if (!Database.Query(statement))
                args.Player.SendErrorMessage("SQL statement failed.");
            else
                args.Player.SendSuccessMessage("SQL statement was successful.");
        }
    }
}
