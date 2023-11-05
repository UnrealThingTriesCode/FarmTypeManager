﻿using StardewModdingAPI;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Pants;
using StardewValley.GameData.Shirts;
using StardewValley.GameData.Tools;
using StardewValley.GameData.Weapons;
using StardewValley;
using System;
using System.Collections.Generic;

namespace FarmTypeManager
{
    public partial class ModEntry : Mod
    {
        /// <summary>Methods used repeatedly by other sections of this mod, e.g. to locate tiles.</summary>
        private static partial class Utility
        {
            /// <summary>Returns the ID of an item with the given name and category, or null if no such item could be found.</summary>
            /// <param name="category">The name of the item category to search (e.g. "furniture" or "weapon").</param>
            /// <param name="idOrName">The item's ID or internal name.</param>
            /// <returns>The ID of the item. Null if the item was not found.</returns>
            public static string GetItemID(string category, string idOrName)
            {
                //depending on the category name, load a specific data asset and compare the item ID/name to its entries
                switch (category.ToLower()) 
                {
                    case "(bc)":
                    case "big craftable":
                    case "bigcraftables":
                    case "bc":
                    case "bigcraftable":
                    case "big craftables":
                        if (Game1.bigCraftableData.ContainsKey(idOrName))
                            return idOrName;
                        foreach (var entry in Game1.bigCraftableData)
                        {
                            if (entry.Value.Name.Equals(idOrName, StringComparison.OrdinalIgnoreCase))
                                return entry.Key;
                        }
                        break;
                    case "boot":
                    case "b":
                    case "(b)":
                    case "boots":
                        var bootsData = Game1.content.Load<Dictionary<string, string>>("Data\\Boots");
                        if (bootsData.ContainsKey(idOrName))
                            return idOrName;
                        foreach (var entry in bootsData)
                        {
                            if (entry.Value.Split('/')[0].Equals(idOrName, StringComparison.OrdinalIgnoreCase))
                                return entry.Key;
                        }
                        break;
                    case "f":
                    case "(f)":
                    case "furniture":
                        var furnitureData = Game1.content.Load<Dictionary<string, string>>("Data\\Furniture");
                        if (furnitureData.ContainsKey(idOrName))
                            return idOrName;
                        foreach (var entry in furnitureData)
                        {
                            if (entry.Value.Split('/')[0].Equals(idOrName, StringComparison.OrdinalIgnoreCase))
                                return entry.Key;
                        }
                        break;
                    case "hats":
                    case "h":
                    case "(h)":
                    case "hat":
                        var hatsData = Game1.content.Load<Dictionary<string, string>>("Data\\hats");
                        if (hatsData.ContainsKey(idOrName))
                            return idOrName;
                        foreach (var entry in hatsData)
                        {
                            if (entry.Value.Split('/')[0].Equals(idOrName, StringComparison.OrdinalIgnoreCase))
                                return entry.Key;
                        }
                        break;
                    case "item":
                    case "o":
                    case "(o)":
                    case "items":
                    case "object":
                    case "objects":
                        if (Game1.objectData.ContainsKey(idOrName))
                            return idOrName;
                        foreach (var entry in Game1.objectData)
                        {
                            if (entry.Value.Name.Equals(idOrName, StringComparison.OrdinalIgnoreCase))
                                return entry.Key;
                        }
                        break;
                    case "pant":
                    case "p":
                    case "(p)":
                    case "pants":
                        if (Game1.pantsData.ContainsKey(idOrName))
                            return idOrName;
                        foreach (var entry in Game1.pantsData)
                        {
                            if (entry.Value.Name.Equals(idOrName, StringComparison.OrdinalIgnoreCase))
                                return entry.Key;
                        }
                        break;
                    case "ring":
                    case "rings":
                        //Note: Ring data is stored with basic object data, e.g. in the "Data/Objects" asset. Rings can be identified by the Type value "Ring", the context tag "ring_item", etc.
                        if (Game1.objectData.TryGetValue(idOrName, out var ringEntry) && ringEntry.Type.Equals("Ring", StringComparison.OrdinalIgnoreCase)) //if this matches a ring's ID
                            return idOrName;
                        foreach (var entry in Game1.objectData)
                        {
                            if (entry.Value.Type.Equals("Ring", StringComparison.OrdinalIgnoreCase) && entry.Value.Name.Equals(idOrName, StringComparison.OrdinalIgnoreCase)) //if this matches a ring's name
                                return entry.Key;
                        }
                        break;
                    case "s":
                    case "(s)":
                    case "shirt":
                    case "shirts":
                        if (Game1.shirtData.ContainsKey(idOrName))
                            return idOrName;
                        foreach (var entry in Game1.shirtData)
                        {
                            if (entry.Value.Name.Equals(idOrName, StringComparison.OrdinalIgnoreCase))
                                return entry.Key;
                        }
                        break;
                    case "tool":
                    case "t":
                    case "(t)":
                    case "tools":
                        if (Game1.toolData.ContainsKey(idOrName))
                            return idOrName;
                        foreach (var entry in Game1.toolData)
                        {
                            if (entry.Value.Name.Equals(idOrName, StringComparison.OrdinalIgnoreCase))
                                return entry.Key;
                        }
                        break;
                    case "w":
                    case "(w)":
                    case "weapon":
                    case "weapons":
                        if (Game1.weaponData.ContainsKey(idOrName))
                            return idOrName;
                        foreach (var entry in Game1.weaponData)
                        {
                            if (entry.Value.Name.Equals(idOrName, StringComparison.OrdinalIgnoreCase))
                                return entry.Key;
                        }
                        break;
                }

                return null; //no item could be found with this category and name
            }
        }
    }
}