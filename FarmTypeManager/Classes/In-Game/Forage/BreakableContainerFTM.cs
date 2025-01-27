﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

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
                this.NetFields.AddFields(Items, HitsToBreak, (INetSerializable)this.debris, (INetSerializable)this.breakDebrisSource, (INetSerializable)this.breakDebrisSource2);
            }

            public BreakableContainerFTM()
            {
            }

            /// <summary>Create a new breakable container with the specified item contents.</summary>
            /// <param name="tile">The tile location of the container.</param>
            /// <param name="items">A set of items the container will drop when broken. Null or empty lists are valid.</param>
            /// <param name="isBarrel">If true, the container will use the "barrel" sprite. If false, it will use the "crate" sprite.</param>
            public BreakableContainerFTM(Vector2 tile, IEnumerable<Item> items, bool isBarrel = true)
                : base(tile, barrel, false)
            {
                Items.AddRange(items);

                if (!isBarrel) //uses a parameter instead of a 50% chance
                    ParentSheetIndex = crate;

                HitsToBreak.Value = 3;
                debris.Value = 12;
                breakDebrisSource.Value = new Rectangle(598, 1275, 13, 4);
                breakDebrisSource2.Value = new Rectangle(611, 1275, 10, 4);
            }

            public override bool performToolAction(Tool t, GameLocation location)
            {
                if (t != null && t.isHeavyHitter())
                {
                    Multiplayer multiplayer = Utility.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue(); //reflect to access SDV's multiplayer field

                    --HitsToBreak.Value;
                    if (t is MeleeWeapon weapon && weapon.type.Value == 2)
                        --HitsToBreak.Value;
                    if (HitsToBreak.Value <= 0)
                    {
                        location.playSound(BreakSound, NetAudio.SoundContext.Default); //this no longer checks whether "breakSound" is assigned
                        releaseContents(location); //this now passes the provided location, rather than the tool owner and their location
                        t.getLastFarmerToUse().currentLocation.objects.Remove(tileLocation.Value);
                        int num = Game1.random.Next(4, 12);
                        //removed the code that determines color based on parent sheet index
                        for (int index = 0; index < num; ++index)
                            multiplayer.broadcastSprites(t.getLastFarmerToUse().currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", Game1.random.NextDouble() < 0.5 ? breakDebrisSource.Value : breakDebrisSource2.Value, 999f, 1, 0, tileLocation.Value * 64f + new Vector2(32f, 32f), false, Game1.random.NextDouble() < 0.5, (float)(((double)tileLocation.Y * 64.0 + 32.0) / 10000.0), 0.01f, color, 4f, 0.0f, (float)((double)Game1.random.Next(-5, 6) * 3.14159274101257 / 8.0), (float)((double)Game1.random.Next(-5, 6) * 3.14159274101257 / 64.0), false)
                            {
                                motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, (float)Game1.random.Next(-10, -7)),
                                acceleration = new Vector2(0.0f, 0.3f)
                            });
                    }
                    else //this no longer checks whether "hitSound" is assigned
                    {
                        this.shakeTimer = 300;
                        location.playSound(HitSound, NetAudio.SoundContext.Default);
                        Game1.createRadialDebris(t.getLastFarmerToUse().currentLocation, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 7), false, -1, false, ParentSheetIndex == 120 ? 10000 : -1); //this now reads the container's parent sheet index instead of "containerType"
                    }
                }
                return false;
            }

            public override bool onExplosion(Farmer who, GameLocation location)
            {
                Multiplayer multiplayer = Utility.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue(); //reflect to access SDV's multiplayer field

                if (who == null)
                    who = Game1.player;
                releaseContents(location); //this no longer passes the farmer
                int num = Game1.random.Next(4, 12);
                //removed the code that determines color based on parent sheet index
                for (int index = 0; index < num; ++index)
                    multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", Game1.random.NextDouble() < 0.5 ? breakDebrisSource.Value : breakDebrisSource2.Value, 999f, 1, 0, tileLocation.Value * 64f + new Vector2(32f, 32f), false, Game1.random.NextDouble() < 0.5, (float)(((double)tileLocation.Y * 64.0 + 32.0) / 10000.0), 0.01f, color, 4f, 0.0f, (float)((double)Game1.random.Next(-5, 6) * 3.14159274101257 / 8.0), (float)((double)Game1.random.Next(-5, 6) * 3.14159274101257 / 64.0), false)
                    {
                        motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, (float)Game1.random.Next(-10, -7)),
                        acceleration = new Vector2(0.0f, 0.3f)
                    });
                return true;
            }

            /// <summary>Drops the items from this container's "items" list.</summary>
            /// <param name="location">The location of the container.</param>
            /// <remarks>This replaces the method's original behavior and no longer takes Farmer as an argument.</remarks>
            public void releaseContents(GameLocation location)
            {
                if (Items == null || Items.Count < 1) { return; } //if there are no items listed, do nothing

                Vector2 itemPosition = new Vector2(boundingBox.Center.X, boundingBox.Center.Y); //get the "pixel" location where these items should spawn

                foreach (Item item in Items) //for each item in this container's item list
                {
                    Game1.createItemDebris(item, itemPosition, Utility.RNG.Next(4), location); //spawn the item as "debris" at this location
                }
            }

            public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
            {
                if (this.shakeTimer <= 0)
                    return;
                this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
            }

            public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
            {
                Vector2 vector2 = this.getScale() * 4f;
                Vector2 local = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64)));
                Rectangle destinationRectangle = new Rectangle((int)((double)local.X - (double)vector2.X / 2.0), (int)((double)local.Y - (double)vector2.Y / 2.0), (int)(64.0 + (double)vector2.X), (int)(128.0 + (double)vector2.Y / 2.0));
                if (this.shakeTimer > 0)
                {
                    int num = this.shakeTimer / 100 + 1;
                    destinationRectangle.X += Game1.random.Next(-num, num + 1);
                    destinationRectangle.Y += Game1.random.Next(-num, num + 1);
                }
                spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destinationRectangle, new Rectangle?(StardewValley.Object.getSourceRectForBigCraftable(showNextIndex.Value ? ParentSheetIndex + 1 : ParentSheetIndex)), Color.White * alpha, 0.0f, Vector2.Zero, SpriteEffects.None, Math.Max(0.0f, (float)((y + 1) * 64 - 1) / 10000f) + (ParentSheetIndex == 105 ? 0.0015f : 0.0f));
            }
        }
    }
}
