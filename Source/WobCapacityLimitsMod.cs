using System;
using System.Linq;
using Verse;
using RimWorld;
using HugsLib;
using HugsLib.Settings;
using System.Collections.Generic;

namespace WobCapacityLimits {
    public class WobCapacityLimitsMod : ModBase {
        public override string ModIdentifier {
            get {
                return "WobCapacityLimits";
            }
        }

        // Settings for basic capacity values
        private SettingHandle<float> pawnMassCapacity;
        private SettingHandle<float> podMassCapacity;
        private SettingHandle<float> podFuelPerTile;

        // Settings for stack maximum size multipliers
        private SettingHandle<float> mealsStack;
        private SettingHandle<float> foodStack;
        private SettingHandle<float> animalfeedStack;
        private SettingHandle<float> textilesStack;
        private SettingHandle<float> medicineStack;
        private SettingHandle<float> drugsStack;
        private SettingHandle<float> manufacturedStack;
        private SettingHandle<float> silverStack;
        private SettingHandle<float> resourcesStack;
        private SettingHandle<float> chunksStack;
        private SettingHandle<float> artifactsStack;
        private SettingHandle<float> bodypartsStack;
        private SettingHandle<float> miscStack;

        // Setting for verbose logging for debugging
        private SettingHandle<bool> debugMode;

        // Record of stack sizes before we edit them, for if setiings are changed
        private Dictionary<string, int> stackSizes;
        
        public override void DefsLoaded() {
            // No need to do anything if the mod is disabled
            if( !ModIsActive )
                return;

            // Basic capacity values
            pawnMassCapacity = Settings.GetHandle<float>( "pawnMassCapacity", "WobCapacityLimits.PawnMassCapacity.Title".Translate(), "WobCapacityLimits.PawnMassCapacity.Desc".Translate(), 35f, Validators.FloatRangeValidator( 1, float.MaxValue ) );
            podMassCapacity = Settings.GetHandle<float>( "podMassCapacity", "WobCapacityLimits.PodMassCapacity.Title".Translate(), "WobCapacityLimits.PodMassCapacity.Desc".Translate(), 150f, Validators.FloatRangeValidator( 1, float.MaxValue ) );
            podFuelPerTile = Settings.GetHandle<float>( "podFuelPerTile", "WobCapacityLimits.PodFuelPerTile.Title".Translate(), "WobCapacityLimits.PodFuelPerTile.Desc".Translate(), 2.25f, Validators.FloatRangeValidator( float.Epsilon, float.MaxValue ) );

            // Stack maximum size multipliers
            mealsStack = Settings.GetHandle<float>( "mealsStack", "WobCapacityLimits.Stack.Meals.Title".Translate(), "WobCapacityLimits.Stack.Meals.Desc".Translate(), 1.0f, Validators.FloatRangeValidator( float.Epsilon, float.MaxValue ) );
            foodStack = Settings.GetHandle<float>( "foodStack", "WobCapacityLimits.Stack.Food.Title".Translate(), "WobCapacityLimits.Stack.Food.Desc".Translate(), 1.0f, Validators.FloatRangeValidator( float.Epsilon, float.MaxValue ) );
            animalfeedStack = Settings.GetHandle<float>( "animalfeedStack", "WobCapacityLimits.Stack.AnimalFeed.Title".Translate(), "WobCapacityLimits.Stack.AnimalFeed.Desc".Translate(), 1.0f, Validators.FloatRangeValidator( float.Epsilon, float.MaxValue ) );
            textilesStack = Settings.GetHandle<float>( "textilesStack", "WobCapacityLimits.Stack.Textiles.Title".Translate(), "WobCapacityLimits.Stack.Textiles.Desc".Translate(), 1.0f, Validators.FloatRangeValidator( float.Epsilon, float.MaxValue ) );
            medicineStack = Settings.GetHandle<float>( "medicineStack", "WobCapacityLimits.Stack.Medicine.Title".Translate(), "WobCapacityLimits.Stack.Medicine.Desc".Translate(), 1.0f, Validators.FloatRangeValidator( float.Epsilon, float.MaxValue ) );
            drugsStack = Settings.GetHandle<float>( "drugsStack", "WobCapacityLimits.Stack.Drugs.Title".Translate(), "WobCapacityLimits.Stack.Drugs.Desc".Translate(), 1.0f, Validators.FloatRangeValidator( float.Epsilon, float.MaxValue ) );
            manufacturedStack = Settings.GetHandle<float>( "manufacturedStack", "WobCapacityLimits.Stack.Manufactured.Title".Translate(), "WobCapacityLimits.Stack.Manufactured.Desc".Translate(), 1.0f, Validators.FloatRangeValidator( float.Epsilon, float.MaxValue ) );
            silverStack = Settings.GetHandle<float>( "silverStack", "WobCapacityLimits.Stack.Silver.Title".Translate(), "WobCapacityLimits.Stack.Silver.Desc".Translate(), 1.0f, Validators.FloatRangeValidator( float.Epsilon, float.MaxValue ) );
            resourcesStack = Settings.GetHandle<float>( "resourcesStack", "WobCapacityLimits.Stack.Resources.Title".Translate(), "WobCapacityLimits.Stack.Resources.Desc".Translate(), 1.0f, Validators.FloatRangeValidator( float.Epsilon, float.MaxValue ) );
            chunksStack = Settings.GetHandle<float>( "chunksStack", "WobCapacityLimits.Stack.Chunks.Title".Translate(), "WobCapacityLimits.Stack.Chunks.Desc".Translate(), 1.0f, Validators.FloatRangeValidator( float.Epsilon, float.MaxValue ) );
            artifactsStack = Settings.GetHandle<float>( "artifactsStack", "WobCapacityLimits.Stack.Artifacts.Title".Translate(), "WobCapacityLimits.Stack.Artifacts.Desc".Translate(), 1.0f, Validators.FloatRangeValidator( float.Epsilon, float.MaxValue ) );
            bodypartsStack = Settings.GetHandle<float>( "bodypartsStack", "WobCapacityLimits.Stack.BodyParts.Title".Translate(), "WobCapacityLimits.Stack.BodyParts.Desc".Translate(), 1.0f, Validators.FloatRangeValidator( float.Epsilon, float.MaxValue ) );
            miscStack = Settings.GetHandle<float>( "miscStack", "WobCapacityLimits.Stack.Misc.Title".Translate(), "WobCapacityLimits.Stack.Misc.Desc".Translate(), 1.0f, Validators.FloatRangeValidator( float.Epsilon, float.MaxValue ) );

            // Verbose logging for debugging
            debugMode = Settings.GetHandle<bool>( "debugMode", "WobCapacityLimits.DebugMode.Title".Translate(), "WobCapacityLimits.DebugMode.Desc".Translate(), false );

            stackSizes = new Dictionary<string, int>();

            // Set static values used in harmony patches
            SettingsContainer.pawnMassCapacityValue = pawnMassCapacity.Value;
            SettingsContainer.podFuelPerTileValue = podFuelPerTile.Value;
            // Edit the pod capacity property
            EditPodMassCapacity( podMassCapacity.Value );
            // Modify stack sizes, and return the maximum stack size
            int maxStack = EditStackLimits( true );
            // Edit the pawn carry capacity to the maximum stack size
            EditPawnStackCapacity( maxStack );

            // All initialised, note on log
            Logger.Message( "Loaded" );
        }

        // Method called when the mod settings are closed
        public override void SettingsChanged() {
            base.SettingsChanged();
            // Update static values used in harmony patches
            SettingsContainer.pawnMassCapacityValue = pawnMassCapacity.Value;
            SettingsContainer.podFuelPerTileValue = podFuelPerTile.Value;
            // Update the pod capacity property
            EditPodMassCapacity( podMassCapacity.Value );
            // Update stack sizes, and return the maximum stack size
            int maxStack = EditStackLimits( false );
            // Update the pawn carry capacity to the maximum stack size
            EditPawnStackCapacity( maxStack );
        }

        // Method to find and edit the mass capacity property of transport pods
        private void EditPodMassCapacity( float newCapacity ) {
            // Get the definition of the pod and go through each of the comps
            foreach( CompProperties comp in DefDatabase<ThingDef>.GetNamed( "TransportPod" ).comps ) {
                // Find the transportr comp
                if( comp.GetType() == typeof( CompProperties_Transporter ) ) {
                    // Edit the capacity
                    ( (CompProperties_Transporter)comp ).massCapacity = newCapacity;
                }
            }
        }

        // Method to edit the maximum stack size that a pawn can carry in their hands
        private void EditPawnStackCapacity( int newCapacity ) {
            DefDatabase<StatDef>.GetNamed( "CarryingCapacity" ).defaultBaseValue = newCapacity;
        }

        // Method to filter which things that can have their stack limit safely edited
        private static bool CanEditStack( ThingDef thing ) {
            // We only care about items, and they must be categorised properly
            if( thing.thingCategories.NullOrEmpty() || ( thing.category != ThingCategory.Item ) ) {
                return false;
            }

            // Everything under the following categories, or anything else that is already stacked
            return (
                IsInCategory( thing, ThingCategoryDefOf.Foods ) ||
                IsInCategory( thing, ThingCategoryDefOf.Manufactured ) ||
                IsInCategory( thing, ThingCategoryDefOf.ResourcesRaw ) ||
                IsInCategory( thing, ThingCategoryDefOf.Chunks ) ||
                IsInCategory( thing, ThingCategoryDefOf.BodyParts ) ||
                IsInCategory( thing, ThingCategoryDefOfExt.Artifacts ) ||
                ( thing.stackLimit > 1 )
                );
        }

        // Debug function to highlight things not currently edited, to check that the filter is correct
        private static bool MaybeStack( ThingDef thing ) {
            // We still only care about items, and they must be categorised properly
            if( thing.thingCategories.NullOrEmpty() || ( thing.category != ThingCategory.Item ) )
                return false;

            // The following categories are not stackable, so don't bother showing them
            return !(
                IsInCategory( thing, ThingCategoryDefOfExt.Unfinished ) ||
                IsInCategory( thing, ThingCategoryDefOf.Weapons ) ||
                IsInCategory( thing, ThingCategoryDefOf.Apparel ) ||
                IsInCategory( thing, ThingCategoryDefOf.Buildings ) ||
                IsInCategory( thing, ThingCategoryDefOf.Corpses )
                );
        }

        // Send text to the dev console if debug mode is on
        private void LogText( string text ) {
            if( !debugMode ) {
                return;
            }
            Logger.Message( text );
        }

        // Send detailed text to the dev console if debug mode is on
        private void LogCatText( ThingDef thing, string text ) {
            if( !debugMode ) {
                return;
            }
            // Log the item name, main category, stack size, then the text
            Logger.Message( thing.defName + " (" + thing.thingCategories[0] + ", " + thing.stackLimit + ")" + ": " + text );
        }

        // Method to calculate the new stack size and write it to the definition
        private static void EditStackLimit( ThingDef thing, int oldStackLimit, float multiplier ) {
            thing.stackLimit = Math.Max( 1, (int)Math.Round( oldStackLimit * multiplier ) );
        }

        // Method to check if an item is directly in a category, or the category is a parent of the item's category
        private static bool IsInCategory( ThingDef thing, ThingCategoryDef category ) {
            return !thing.thingCategories.NullOrEmpty() && ( thing.thingCategories[0] == category || thing.thingCategories[0].Parents.Contains( category ) );
        }

        // Method to go through each item definition, decide if its stack size can be edited, then edit using the correct multiplier
        private int EditStackLimits( bool firstPass ) {
            LogText( "Editing stack sizes, firstPass is " + firstPass );
            // Variable to hold the maximum stack size found. This will be the return value.
            int maxStackSize = 0;
            // Step through every thing definition
            foreach( ThingDef thing in DefDatabase<ThingDef>.AllDefs ) {
                // Check if the thing is suitable for editing
                if( CanEditStack( thing ) ) {
                    // If this run is when the mod is first loading, remember the existing stack limits, to be used if settings are changed
                    if( firstPass ) {
                        try {
                            stackSizes.Add( thing.defName, thing.stackLimit );
                        } catch( ArgumentException ) {
                            LogText( "Error adding " + thing.defName );
                        }
                    }
                    // Silver
                    if( thing == ThingDefOf.Silver ) {
                        // Log it to the console if debug mode is on
                        LogCatText( thing, "silver" );
                        // Edit the stack size with the appropriate multiplier from the settings
                        EditStackLimit( thing, stackSizes[thing.defName], silverStack.Value );
                    }
                    // Meals
                    else if( IsInCategory( thing, ThingCategoryDefOf.FoodMeals ) ) {
                        LogCatText( thing, "meals" );
                        EditStackLimit( thing, stackSizes[thing.defName], mealsStack.Value );
                    }
                    // Animal Feed
                    else if( IsInCategory( thing, ThingCategoryDefOf.Foods ) &&
                            ( thing.ingestible.preferability == FoodPreferability.DesperateOnlyForHumanlikes ||
                            thing.ingestible.optimalityOffsetHumanlikes < thing.ingestible.optimalityOffsetFeedingAnimals ) ) {
                        LogCatText( thing, "animal feed" );
                        EditStackLimit( thing, stackSizes[thing.defName], animalfeedStack.Value );
                    }
                    // Other Foods
                    else if( IsInCategory( thing, ThingCategoryDefOf.Foods ) ) {
                        LogCatText( thing, "foods" );
                        EditStackLimit( thing, stackSizes[thing.defName], foodStack.Value );
                    }
                    // Textiles
                    else if( IsInCategory( thing, ThingCategoryDefOfExt.Textiles ) ) {
                        LogCatText( thing, "textiles" );
                        EditStackLimit( thing, stackSizes[thing.defName], textilesStack.Value );
                    }
                    // Medicine
                    else if( IsInCategory( thing, ThingCategoryDefOf.Medicine ) ) {
                        LogCatText( thing, "medicine" );
                        EditStackLimit( thing, stackSizes[thing.defName], medicineStack.Value );
                    }
                    // Drugs
                    else if( IsInCategory( thing, ThingCategoryDefOf.Drugs ) ) {
                        LogCatText( thing, "drugs" );
                        EditStackLimit( thing, stackSizes[thing.defName], drugsStack.Value );
                    }
                    // Other Manufactured
                    else if( IsInCategory( thing, ThingCategoryDefOf.Manufactured ) ) {
                        LogCatText( thing, "manufactured" );
                        EditStackLimit( thing, stackSizes[thing.defName], manufacturedStack.Value );
                    }
                    // Resources
                    else if( IsInCategory( thing, ThingCategoryDefOf.ResourcesRaw ) ) {
                        LogCatText( thing, "resources" );
                        EditStackLimit( thing, stackSizes[thing.defName], resourcesStack.Value );
                    }
                    // Chunks
                    else if( IsInCategory( thing, ThingCategoryDefOf.Chunks ) ) {
                        LogCatText( thing, "chunks" );
                        EditStackLimit( thing, stackSizes[thing.defName], chunksStack.Value );
                    }
                    // Artifacts
                    else if( IsInCategory( thing, ThingCategoryDefOfExt.Artifacts ) ) {
                        LogCatText( thing, "artifacts" );
                        EditStackLimit( thing, stackSizes[thing.defName], artifactsStack.Value );
                    }
                    // Body Parts
                    else if( IsInCategory( thing, ThingCategoryDefOf.BodyParts ) ) {
                        LogCatText( thing, "body parts" );
                        EditStackLimit( thing, stackSizes[thing.defName], bodypartsStack.Value );
                    }
                    // Misc - anything that isn't in the above categories
                    else {
                        LogCatText( thing, "misc" );
                        EditStackLimit( thing, stackSizes[thing.defName], miscStack.Value );
                    }
                    // If it is stackable after our changes...
                    if ( thing.stackLimit > 1 ) {
                        // Make sure the game will show how many are in each stack
                        thing.drawGUIOverlay = true;
                        // Only standable items will stack (by default chunks are 'PassThroughOnly')
                        thing.passability = Traversability.Standable;
                    }
                    // Check if the current thing has the largest stack, accounting for small volume items (silver/gold)
                    maxStackSize = Math.Max( maxStackSize, ( thing.smallVolume ? thing.stackLimit / 10 : thing.stackLimit ) );
                } else {
                    // Since this only prints to console, only do it if we are in debug mode
                    if( debugMode ) {
                        // Find stacks that are currently excluded, but potentially could be edited
                        if( MaybeStack( thing ) ) {
                            // Note the item details in the dev console
                            LogCatText( thing, "EXCLUDED <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<" );
                        }
                    }
                }
            }
            // Note the largest stack size on the dev console
            LogText( "Max stack: " + maxStackSize );
            // Return the largest stack size, to be used to set pawn carry capacity
            return maxStackSize;
        }

    }

    // This class simply fills in all of the default categories that are not defined in the base game's ThingCategoryDefOf class
    [DefOf]
    public static class ThingCategoryDefOfExt {
        //public static ThingCategoryDef Root;
        //public static ThingCategoryDef Foods;
        //public static ThingCategoryDef FoodMeals;
        public static ThingCategoryDef FoodRaw;
        //public static ThingCategoryDef MeatRaw;
        //public static ThingCategoryDef PlantFoodRaw;
        public static ThingCategoryDef AnimalProductRaw;
        public static ThingCategoryDef EggsUnfertilized;
        public static ThingCategoryDef EggsFertilized;
        //public static ThingCategoryDef Manufactured;
        public static ThingCategoryDef Textiles;
        //public static ThingCategoryDef Leathers;
        //public static ThingCategoryDef Medicine;
        //public static ThingCategoryDef Drugs;
        public static ThingCategoryDef MortarShells;
        //public static ThingCategoryDef ResourcesRaw;
        //public static ThingCategoryDef PlantMatter;
        //public static ThingCategoryDef StoneBlocks;
        //public static ThingCategoryDef Items;
        public static ThingCategoryDef Unfinished;
        public static ThingCategoryDef Artifacts;
        //public static ThingCategoryDef BodyParts;
        public static ThingCategoryDef BodyPartsNatural;
        public static ThingCategoryDef BodyPartsArtificial;
        //public static ThingCategoryDef Weapons;
        public static ThingCategoryDef WeaponsMelee;
        public static ThingCategoryDef WeaponsRanged;
        public static ThingCategoryDef Grenades;
        //public static ThingCategoryDef Apparel;
        public static ThingCategoryDef Headgear;
        //public static ThingCategoryDef Buildings;
        //public static ThingCategoryDef BuildingsArt;
        public static ThingCategoryDef BuildingsProduction;
        public static ThingCategoryDef BuildingsFurniture;
        public static ThingCategoryDef BuildingsPower;
        public static ThingCategoryDef BuildingsSecurity;
        public static ThingCategoryDef BuildingsMisc;
        public static ThingCategoryDef BuildingsJoy;
        public static ThingCategoryDef BuildingsTemperature;
        public static ThingCategoryDef BuildingsSpecial;
        //public static ThingCategoryDef Chunks;
        //public static ThingCategoryDef StoneChunks;
        //public static ThingCategoryDef Corpses;
        //public static ThingCategoryDef CorpsesHumanlike;
        //public static ThingCategoryDef CorpsesAnimal;
        //public static ThingCategoryDef CorpsesInsect;
        //public static ThingCategoryDef CorpsesMechanoid;
    }
}
