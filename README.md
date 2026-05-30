# Loot In Vicinity

**Loot In Vicinity** adds a **Nearby Items** column when you open your **inventory in a raid**. It lists loose loot
around you so you can drag items, use quick moves, or see what is close without having to pin point that Veritas
guitar pin everytime.

**Website:** [softwyx.com](https://softwyx.com)

---

## What you need

- A working **SPT** already set up.
- This mod.
- The mod is turned **on** in its config (see below).

If other inventory or loot mods are installed, list them when you report a problem; they sometimes interact in odd ways.

---

## Install

1. Download the release ZIP -- the filename looks like **`Softwyx-LootInVicinity-2.14.40.zip`** (the version number
   changes each release).
2. **Extract** it so that your game folder contains:
    - `BepInEx\plugins\Softwyx-LootInVicinity\Softwyx-LootInVicinity.dll`
3. Start the game.

**First launch** may create a config file:

- `BepInEx\config\com.softwyx.lootinvicinity.cfg`

You can edit that file when the game is **closed**, or use **Configuration Manager** in-game.

---

## How to use in-game

1. Enter a **raid**.
2. Open **inventory** (usually **Tab**) **without** opening a world container (body, crate, etc.).
3. On the **right**, you should see the **Nearby Items** panel (title may differ if you changed it in config).
4. **Drag** or **quick move** loot like any other stash.

Which items appear is set under **3. Scanning** in the config (see below).

---

## Settings

Open **Configuration Manager** in-game, or edit `BepInEx\config\com.softwyx.lootinvicinity.cfg` while the game is *
*closed**.

**1. General**

- **Enabled** -- master switch.

**2. Inventory panel**

- **Panel title**, **Hide panel when empty**, **Append item count to title** -- how the right column looks.

**3. Scanning**

- **Scan radius (m)** -- how far around you the list reaches (3 to 6 m). There is always a **1 m** wide zone at your
  feet where items can show up even when they are under other loot or even under the ground.
- **Main scan requires line of sight** -- when on, loot farther out in that radius must be in clear view (walls still
  block). Other loot piles and bodies on the ground do **not** count as blocking. The **1 m** zone at your feet ignores
  this setting.
- **Max items in panel** -- cap on how many rows show each time you open Tab inventory.

**4. Quest items**

- **Show quest items in vicinity** -- whether quest loot appears in the nearby list.
- **Route quest items to task stash** -- when on, moving quest loot from the panel sends it to the quest stash instead
  of your pockets.

Delete the config file with the game **closed** to reset all defaults (or reset manually in-game).

---

## Known issues / quirks

These are behaviours players have reported; they may vary with game version or other mods.

1. **Quest items in the Nearby list** -- Drag previews (green/red outlines) sometimes **do not match** where a quest
   item can actually go (for example armour vs backpack). **Quick move** (e.g. **Ctrl + click**, depending on your
   setup) is often more reliable than drag for those items.

2. **Mods that change inventory, stash, quick move, or loot** -- Compatibility is **best effort**. If something breaks
   after adding another mod, try with only Loot In Vicinity + SPT essentials to narrow it down.

The mod is updated when issues are reproduced and fixed; the list above may shrink or grow in newer versions.

---

## How to report a bug or ask for help

You get faster help if you include detail. **Copy/paste this checklist** and fill what you can:

1. **Mod version** -- From BepInEx `LogOutput.log`: the line where **Loot In Vicinity** loads (it shows **v...**).

2. **Game / SPT** -- Roughly what you use (example: "SPT 4.0.13, EFT client build ..."). Exact numbers help.

3. **What happened** -- One short sentence (e.g. "Tab in raid, panel empty" / "the game froze after I ...").

4. **Steps** -- Numbered list: what you clicked, map, single player or group (FIKA), etc.

5. **Other mods** -- Names of other BepInEx plugins that touch **inventory, loot, UI, or progression**.

6. **Logs** -- With the game **closed**:
    - `BepInEx\LogOutput.log` (and `FullLogOutput.log` if someone asks for it)

**Where to send it** -- Use the **comments section, or issue tracker of the repo** which should be listed where you
**downloaded** the mod. Never email me about mods.

Do **not** send passwords, full Windows usernames, or unrelated personal files. I will try my best to remove any
personal data whenever I see it but my availability is not guaranteed!

---

## Thanks

People across the **SPT** and **modding** scenes shared ideas, repros, and "what if you tried..." moments that shaped
this plugin. That help mattered.

Bits of implementation are the usual mix: inspiration from forum threads, other public mods, and long sessions with
dnSpy. If something looks **suspiciously familiar**, assume I **borrowed the good parts** on purpose, thank you for
putting it out there.

---

## Support the author

Maintenance and compatibility work takes time. If this mod saves you frustration in raids, consider
**[supporting on GitHub Sponsors](https://github.com/sponsors/FallegaHQ)**. There is no paywall--everything stays free
under the licence below.

---

## Licence

This project is licensed under the **Mozilla Public License 2.0** (MPL-2.0). See the [`LICENSE`](LICENSE) file for the
full text.

SPDX: `MPL-2.0`

---

## Disclaimer

*Escape from Tarkov* is a trademark of Battlestate Games. This mod is an independent, community-made add-on. Use at your
own risk. The author is not responsible for lost progress, banned accounts on other services, or damage from third-party
tools.
