using System;
using System.Collections.Generic;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace Destrospean.OutfitAssignment
{
    public static class OutfitAssignmentUtils
    {
        [PersistableStatic(true)]
        public static List<OutfitAssignment> OutfitAssignments = new List<OutfitAssignment>();

        [PersistableStatic(true)]
        public static List<Outfit> PreviousOutfits = new List<Outfit>();

        [Persistable]
        public class Outfit
        {
            public OutfitCategories Category;

            public int Index;

            public SimDescription SimDescription;
        }

        [Persistable]
        public class OutfitAssignment
        {
            public InteractionInstanceTypeUtils.CallbackTypes EntryCallbackType, ExitCallbackType;

            public string InteractionInstanceType, SpecialOutfitKey;

            public SimDescription SimDescription;

            public OutfitAssignment()
            {
            }

            public OutfitAssignment(SimDescription simDescription, string specialOutfitKey, Type interactionInstanceType, InteractionInstanceTypeUtils.CallbackTypes entryCallbackType, InteractionInstanceTypeUtils.CallbackTypes exitCallbackType)
            {
                EntryCallbackType = entryCallbackType;
                ExitCallbackType = exitCallbackType;
                InteractionInstanceType = interactionInstanceType.FullName;
                SimDescription = simDescription;
                SpecialOutfitKey = specialOutfitKey;
            }
        }

        public static void AssignOutfitToInteraction(this SimDescription simDescription, string specialOutfitKey, Type interactionInstanceType, InteractionInstanceTypeUtils.CallbackTypes entryCallbackType, InteractionInstanceTypeUtils.CallbackTypes exitCallbackType)
        {
            UnassignOutfitToInteraction(simDescription, interactionInstanceType);
            OutfitAssignments.Add(new OutfitAssignment(simDescription, specialOutfitKey, interactionInstanceType, entryCallbackType, exitCallbackType));
        }

        public static OutfitAssignment[] GetAllOutfitAssignments(this SimDescription simDescription)
        {
            return OutfitAssignments.FindAll(x => x.SimDescription == simDescription).ToArray();
        }

        public static void RemoveAllOutfitAssignments(this SimDescription simDescription, bool removeSpecialOutfits = false)
        {
            foreach (OutfitAssignment outfitAssignment in new List<OutfitAssignment>(OutfitAssignments))
            {
                if (outfitAssignment.SimDescription == simDescription)
                {
                    OutfitAssignments.Remove(outfitAssignment);
                    if (removeSpecialOutfits)
                    {
                        simDescription.RemoveSpecialOutfit(outfitAssignment.InteractionInstanceType);
                    }
                }
            }
        }

        public static void SwitchToAssignedOutfit(this Sims3.Gameplay.Actors.Sim sim, OutfitAssignment outfitAssignment, bool spin = true)
        {
            if (sim.BuffManager.DisallowClothesChange() || sim.OccultManager.DisallowClothesChange())
            {
                return;
            }
            int specialOutfitIndex = sim.SimDescription.GetSpecialOutfitIndexFromKey(ResourceUtils.HashString32(outfitAssignment.SpecialOutfitKey));
            if (sim.CurrentOutfitCategory != OutfitCategories.Special || sim.CurrentOutfitIndex != specialOutfitIndex)
            {
                PreviousOutfits.RemoveAll(x => x.SimDescription == sim.SimDescription);
                PreviousOutfits.Insert(0, new Outfit
                    {
                        Category = sim.CurrentOutfitCategory,
                        Index = sim.CurrentOutfitIndex,
                        SimDescription = sim.SimDescription
                    });
            }
            if (spin)
            {
                sim.SwitchToOutfitWithSpin(OutfitCategories.Special, specialOutfitIndex);
            }
            else
            {
                sim.SwitchToOutfitWithoutSpin(OutfitCategories.Special, specialOutfitIndex);
            }
        }

        public static void SwitchToPreviousOutfit(this Sims3.Gameplay.Actors.Sim sim, bool spin = true)
        {
            int previousOutfitIndex = OutfitAssignmentUtils.PreviousOutfits.FindIndex(x => x.SimDescription == sim.SimDescription);
            if (previousOutfitIndex > -1)
            {
                if (!sim.BuffManager.DisallowClothesChange() && !sim.OccultManager.DisallowClothesChange())
                {
                    if (spin)
                    {
                        sim.SwitchToOutfitWithSpin(OutfitAssignmentUtils.PreviousOutfits[previousOutfitIndex].Category, OutfitAssignmentUtils.PreviousOutfits[previousOutfitIndex].Index);
                    }
                    else
                    {
                        sim.SwitchToOutfitWithoutSpin(OutfitAssignmentUtils.PreviousOutfits[previousOutfitIndex].Category, OutfitAssignmentUtils.PreviousOutfits[previousOutfitIndex].Index);
                    }
                }
                OutfitAssignmentUtils.PreviousOutfits.RemoveAt(previousOutfitIndex);
            }
        }

        public static bool TryGetOutfitAssignment(this SimDescription simDescription, Sims3.Gameplay.Interactions.InteractionInstance interactionInstance, out OutfitAssignment outfitAssignment)
        {
            return TryGetOutfitAssignment(simDescription, interactionInstance.GetType(), out outfitAssignment);
        }

        public static bool TryGetOutfitAssignment(this SimDescription simDescription, Type interactionInstanceType, out OutfitAssignment outfitAssignment)
        {
            List<OutfitAssignment> results = OutfitAssignments.FindAll(x => x.SimDescription == simDescription && Array.Find(InteractionInstanceTypeUtils.InteractionInstanceTypes, y => y.FullName == x.InteractionInstanceType).IsAssignableFrom(interactionInstanceType));
            if (results.Count == 0)
            {
                outfitAssignment = null;
                return false;
            }
            outfitAssignment = results[0];
            return true;
        }

        public static void UnassignOutfitToInteraction(this SimDescription simDescription, Type interactionInstanceType)
        {
            OutfitAssignment outfitAssignment;
            if (TryGetOutfitAssignment(simDescription, interactionInstanceType, out outfitAssignment))
            {
                OutfitAssignments.Remove(outfitAssignment);
            }
        }
    }
}
