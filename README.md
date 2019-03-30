# PvPModifier

## Table of Contents
* [Just the important things (tl;dr)](#Just-the-Important-Things)
* [About](#About)
* [Install](#Install)
* [Commands](#Commands)
* [Dev Notes](#Dev-Notes)

## Just the Important Things

Note: Everything will be typed with the parameter's abbreviations to keep the tl;dr shorter.

- To modify item damage, type **/mp i "item name" d [number]**.

You can chain attributes of an item into one command. For example: **/mp i "shadowflame hex doll" ut 0 d 999 ss 100**. This will make the shadowflame hex doll have 0 usetime, 999 base damage, and 100 shootspeed.

- To make projectiles home, type **/mp p "projectile name or id" hr [number] av [number]**. hr = homing radius, av = angular velocity

- To enable custom knockback, type **/mp c k true**

- To check the stats of a weapon, type **/cs "item/projectile/buff" "name"**

- If you mess up changing a stat of something, you can reset it by typing **/rpvp "item/projectile/buff" "name or id"**

- To reset the entire database, type **/rpvp d**. To reset config, type **/rpvp c**

- To stop the plugin from doing anything, type **/mp c p false" 

## About

Customize almost all aspect of pvp including:
- Item Stats
  - Damage 
  - Knockback 
  - UseAnimation 
  - UseTime 
  - Shoot 
  - ShootSpeed 
  - AmmoIdentifier 
  - UseAmmoIdentifier 
  - NotAmmo 
  - Inflict Buff ID/Duration 
  - Receive Buff ID/Duration 
- Projectile Stats
  - Shoot 
  - VelocityMultiplier 
  - Damage 
  - HomingRadius 
  - AngularVelocity 
  - Inflict Buff ID/Duration 
  - Receive Buff ID/Duration 
- Buff Stats
  - Inflict Buff ID/Duration 
  - Receive Buff ID/Duration 
- Reflection Damage (Turtle/Thorns)
  - Enable/Disable the effect
  - Change how much damage you reflect
- Armor Effects
  - Enable Nebula armor boosters
  - Enable Frost armor debuff
    - Set duration of the frost debuff
- Iframes (Invincibility frames)

Fixes some broken vanilla pvp mechanics as well, such as yoyo bag, armor set effects listed above, iframe time, and reflection damage. The server also has its own version of knockback, which knocks players back depending on the weapon knockback and where the attacker is relative to the target.

The server handles custom items by deleting the items that are modified from the player's inventory when they turn on pvp, placing Boring Bows in order to keep drops in the same place, and dropping modified items into the player's inventory. When the player has picked up all the modified items, the server will delete the placeholder Boring Bows from their inventory.

## Install

Put the .dll file into your ServerPlugins folder. That's it.

## Commands
All commands can be abbreviated by their syllables.

### /modpvp or /mp (Permission: "pvpmodifier")
Syntax:
- /modpvp [item/projectile/buff] [ID/name] [attribute] [value]
- /modpvp config [config value] [value]

To get help on what attributes are valid for some type (item/projectile/buff), type **/mp help [item/projectile/buff]** to get a list of attributes.

Examples: 
- **/mp i "crystal storm" ut 0 d 500** makes the crystal storm have a usetime of 0 and have a base damage of 500.<br>
- **/mp p 636 hr 10 av 5** makes the daybreak projectile home in when it finds a target within 10 blocks and turns at an angular velocity of 5 degrees per tick.<br>
- **/mp b 52 ibid 163 ibd 60** makes you inflict a person with Obstructed for 2 seconds when you have Tiki Spirit buff (which is a pet)<br>

<br>

### /resetpvp or /rpvp (Permission: "pvpmodifier")
Syntax: /resetpvp [config/database/item/projectile/buff] [optional: item name or id]
- config
  - Resets the config to its original values
- database
  - Resets the database to its original values
- item
  - Requires item name or ID
  - Resets an item's stats
- projectile
  - Requires projectile name or ID
  - Resets an projectile's stats
- buff
  - Requires buff name or ID
  - Resets an buff's stats
  
## Dev Notes

This plugin takes over the PlayerHurtV2 packet, so any plugins that use this packet may or may not work.<br>
CustomWeaponAPI was made by popstarfreas, which can be found [here](https://github.com/popstarfreas/CustomWeaponAPI).<br>
If there is an issue with the plugin, you can find me on the [Terraria PvP Lounge](https://discord.gg/gzeExdZ) discord.
