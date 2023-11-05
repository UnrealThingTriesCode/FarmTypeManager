﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Network;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace FarmTypeManager
{
    public partial class ModEntry : Mod
    {
        /// <summary>A breakable container with customized item contents.</summary>
        /// <remarks>This class is not currently designed to be saved by the game's native processes. All instances should be removed from the game before saving (i.e. end of day).
        /// Except where commented, this code copies or imitates SDV 1.4.3's BreakableContainer class.</remarks>
        public class BreakableContainerFTM : StardewValley.Object
        {
            /// <summary>The list of items dropped by this container when it breaks.</summary>
            /// <remarks>Replaces the predetermined objects generated by the original class.</remarks>
            [XmlElement("Items")]
            public readonly NetObjectList<Item> Items = new NetObjectList<Item>();
            /// <summary>The number of times this container needs to be hit before breaking.</summary>
            /// <remarks>Renames the original class's "health" field to avoid confusion with StardewValley.Object.health.</remarks>
            [XmlElement("HitsToBreak")]
            public readonly NetInt HitsToBreak = new NetInt();

            [XmlElement("debris")]
            private readonly NetInt debris = new NetInt();
            [XmlElement("breakDebrisSource")]
            private readonly NetRectangle breakDebrisSource = new NetRectangle();
            [XmlElement("breakDebrisSource2")]
            private readonly NetRectangle breakDebrisSource2 = new NetRectangle();

            //convert previously variable fields to const/readonly values
            protected const string HitSound = "woodWhack";
            protected const string BreakSound = "barrelBreak";
            protected readonly Color color = new Color(130, 80, 30);

            //replace or remove unused constants
            public const int barrel = 118;
            public const int crate = 119;

            private new int shakeTimer;

            protected override void initNetFields()
            {
                base.initNetFields();
                //use this class's modified set of net fields
                base.NetFields
                    .AddField(Items, "Items")
                    .AddField(HitsToBreak, "HitsToBreak")
                    .AddField(debris, "debris")
                    .AddField(breakDebrisSource, "breakDebrisSource")
                    .AddField(breakDebrisSource2, "breakDebrisSource2");
            }

            public BreakableContainerFTM()
            {
            }

            /// <summary>Create a new breakable container with the specified item contents.</summary>
            /// <param name="tile">The tile location of the container.</param>
            /// <param name="items">A set of items the container will drop when broken. Null or empty lists are valid.</param>
            /// <param name="isBarrel">If true, the container will use the "barrel" sprite. If false, it will use the "crate" sprite.</param>
            public BreakableContainerFTM(Vector2 tile, IEnumerable<Item> items, bool isBarrel = true)
                : base(tile, barrel.ToString(), false)
            {
                Items.AddRange(items);

                if (!isBarrel) //uses a parameter instead of a 50% chance
                    ParentSheetIndex = crate;

                HitsToBreak.Value = 3;
                debris.Value = 12;
                breakDebrisSource.Value = new Rectangle(598, 1275, 13, 4);
                breakDebrisSource2.Value = new Rectangle(611, 1275, 10, 4);
            }

            public override bool performToolAction(Tool t)
            {
                if (Location == null)
                    return false;

                if (t != null && t.isHeavyHitter())
                {
                    Multiplayer multiplayer = Utility.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue(); //reflect to access SDV's multiplayer field

                    --HitsToBreak.Value;
                    if (t is MeleeWeapon weapon && weapon.type.Value == 2)
                        --HitsToBreak.Value;
                    if (HitsToBreak.Value <= 0)
                    {
                        playNearbySoundAll("barrelBreak"); //this no longer checks whether "breakSound" is assigned
                        releaseContents();
                        Location.objects.Remove(TileLocation);
                        int numDebris = Game1.random.Next(4, 12);
                        //removed the code that determines color based on parent sheet index
                        for (int i = 0; i < numDebris; ++i)
                            multiplayer.broadcastSprites(Location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", Game1.random.NextBool() ? breakDebrisSource.Value : breakDebrisSource2.Value, 999f, 1, 0, tileLocation.Value * 64f + new Vector2(32f, 32f), flicker: false, Game1.random.NextBool(), (float)(((double)tileLocation.Y * 64.0 + 32.0) / 10000.0), 0.01f, color, 4f, 0f, (float)((double)Game1.random.Next(-5, 6) * Math.PI / 8.0), (float)((double)Game1.random.Next(-5, 6) * Math.PI / 64.0))
                            {
                                motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, (float)Game1.random.Next(-10, -7)),
                                acceleration = new Vector2(0.0f, 0.3f)
                            });
                    }
                    else //this no longer checks whether "hitSound" is assigned
                    {
                        shakeTimer = 300;
                        playNearbySoundAll("woodWhack");
                        Game1.createRadialDebris(Location, debris.Value, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 7), resource: false, -1, item: false, color);
                    }
                }
                return false;
            }

            public override bool onExplosion(Farmer who)
            {
                if (Location == null)
                    return true;
                if (who == null)
                    who = Game1.player;

                Multiplayer multiplayer = Utility.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue(); //reflect to access SDV's multiplayer field

                releaseContents();
                int numDebris = Game1.random.Next(4, 12);
                for (int i = 0; i < numDebris; i++)
                {
                    multiplayer.broadcastSprites(Location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", Game1.random.NextBool() ? this.breakDebrisSource.Value : this.breakDebrisSource2.Value, 999f, 1, 0, TileLocation * 64f + new Vector2(32f, 32f), flicker: false, Game1.random.NextBool(), (TileLocation.Y * 64f + 32f) / 10000f, 0.01f, color, 4f, 0f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 8f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 64f)
                    {
                        motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-10, -7)),
                        acceleration = new Vector2(0f, 0.3f)
                    });
                }
                return true;
            }

            /// <summary>Drops the items from this container's "items" list.</summary>
            /// <remarks>This replaces the method's original behavior and no longer takes Farmer as an argument.</remarks>
            public void releaseContents()
            {
                if (Items == null || Items.Count < 1 || Location == null) { return; } //if there are no items listed, do nothing

                Vector2 itemPosition = new Vector2(boundingBox.Center.X, boundingBox.Center.Y); //get the "pixel" location where these items should spawn

                foreach (Item item in Items) //for each item in this container's item list
                {
                    Game1.createItemDebris(item, itemPosition, Utility.RNG.Next(4), Location); //spawn the item as "debris" at this location
                }
            }

            public override void updateWhenCurrentLocation(GameTime time)
            {
                if (shakeTimer > 0)
                {
                    shakeTimer -= time.ElapsedGameTime.Milliseconds;
                }
            }

            public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
            {
                Vector2 scaleFactor = this.getScale();
                scaleFactor *= 4f;
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
                Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f), (int)(position.Y - scaleFactor.Y / 2f), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                if (this.shakeTimer > 0)
                {
                    int intensity = this.shakeTimer / 100 + 1;
                    destination.X += Game1.random.Next(-intensity, intensity + 1);
                    destination.Y += Game1.random.Next(-intensity, intensity + 1);
                }
                ParsedItemData data = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
                spriteBatch.Draw(data.GetTexture(), destination, data.GetSourceRect(showNextIndex.Value ? 1 : 0), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
            }
        }
    }
}
