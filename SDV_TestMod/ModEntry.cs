using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace SDV_TestMod
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        //Warp location the remember totem is placed at, will need to be saved?
        private Warp placedTotemWarp;
        // If the player is using the remember totem
        private bool use;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            placedTotemWarp = null;
            use = false;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.World.ObjectListChanged += World_ObjectListChanged;
            helper.Events.Player.InventoryChanged += Player_InventoryChanged;
        }

        private void Player_InventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            // Handles when the last of a stackable item is removed from inventory
            foreach (Item item in e.Removed)
            {
                HandleUseRememberTotem(item);
            }

            // Handles when any item is removed from inventory
            foreach (ItemStackSizeChange item in e.QuantityChanged)
            {
                HandleUseRememberTotem(item.Item);
            }
        }

        /**
         * Determines if item is of type Remember Totem and if the player is using 
         * it vs. placing it.
         */
        private void HandleUseRememberTotem(Item item)
        {
            if (item.Name == "Remember Totem")
            {
                // If player is using the remember totem
                if (use)
                {
                    // Warp if the player has placed a remember totem in the world
                    if (placedTotemWarp != null)
                    {
                        this.Monitor.Log("Warping farmer to remember totem");
                        Game1.player.warpFarmer(placedTotemWarp);
                        //TODO: play warp anim & sound
                    } else // Display an error message if totem has not be placed
                    {
                        this.Monitor.Log("A Remember Totem has not been placed.");
                        Game1.addHUDMessage(new HUDMessage("A Remember Totem has not been placed."));
                    }
                }
            }
            else
                use = false;
        }

        /**
         * Handles setting the warp location and removing the used Remember totem.
         */
        private void World_ObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            foreach (KeyValuePair<Vector2, StardewValley.Object> pair in e.Added)
            {
                if (pair.Value.name == "Remember Totem")
                {
                    int x = (int)pair.Key.X;
                    int y = (int)pair.Key.Y;
                    //if remember totem is being used (not placed)
                    if (use && placedTotemWarp != null)
                    {
                        Game1.currentLocation.removeEverythingFromThisTile(x, y);
                    }
                    //if remember totem is being placed (not used)
                    else {
                        //TODO: possibly pop up prompt to warn player about their previous totem being removed.
                        //remove previously placed remember totem
                        string loc = placedTotemWarp.TargetName;
                        int xx = placedTotemWarp.TargetX;
                        int yy = placedTotemWarp.TargetY;
                        Game1.getLocationFromName(loc).removeEverythingFromThisTile(xx, yy);
                                       
                        //assign newly placed totem to field
                        placedTotemWarp = new Warp(x, y, Game1.player.currentLocation.Name, x, y, false);
                        this.Monitor.Log("Remember totem placed");
                    }
                }
            }
        }


        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // Determine whether the player is using the remember totem or placing it. TODO: more elegant method
            if (e.Button.Equals(SButton.MouseRight))
                use = true;
            if (e.Button.Equals(SButton.MouseLeft))
                use = false;
        }

//        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
//        {
//            //HandleLowHealthIndicatiors(e);
//        }
//        
//        //will display a message that the players health is low
//        private void HandleLowHealthIndicatiors(UpdateTickedEventArgs e)
//        {
//            float healthRatio = Game1.player.health / Game1.player.maxHealth;
//
//            this.Monitor.Log("Health at: " + 100 * healthRatio + "%");
//
//            //display a message at increasingly faster intervals as the players health decreases
//            //if (healthRatio < 0.5f && healthRatio > 0)
//                if (e.IsMultipleOf(30))
//                {
//                    Game1.addHUDMessage(new HUDMessage("Health at: " + 100 * healthRatio + "%", 5, false, Color.Red));
//                }
//        }
    }
}
