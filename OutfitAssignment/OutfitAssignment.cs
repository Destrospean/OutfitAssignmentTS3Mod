using System;
using System.Collections.Generic;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace.CAS;

namespace Destrospean.OutfitAssignment
{
    [Sims3.SimIFace.Persistable]
    public class OutfitAssignment
    {
        public readonly InteractionInstanceTypeUtils.CallbackTypes EntryCallbackType, ExitCallbackType;

        public readonly string InteractionInstanceType, SpecialOutfitKey;

        [Sims3.SimIFace.PersistableStatic(true)]
        public static readonly List<OutfitAssignment> OutfitAssignments = new List<OutfitAssignment>();

        public OutfitCategories PreviousOutfitCategory = OutfitCategories.Everyday;

        public int PreviousOutfitIndex = 0;

        public readonly SimDescription SimDescription;

        public OutfitAssignment(SimDescription simDescription, string specialOutfitKey, Type interactionInstanceType, InteractionInstanceTypeUtils.CallbackTypes entryCallbackType, InteractionInstanceTypeUtils.CallbackTypes exitCallbackType)
        {
            EntryCallbackType = entryCallbackType;
            ExitCallbackType = exitCallbackType;
            InteractionInstanceType = interactionInstanceType.FullName;
            SimDescription = simDescription;
            SpecialOutfitKey = specialOutfitKey;
        }

        public static void AssignOutfitToInteraction(SimDescription simDescription, string specialOutfitKey, Type interactionInstanceType, InteractionInstanceTypeUtils.CallbackTypes entryCallbackType, InteractionInstanceTypeUtils.CallbackTypes exitCallbackType)
        {
            UnassignOutfitToInteraction(simDescription, interactionInstanceType);
            OutfitAssignments.Add(new OutfitAssignment(simDescription, specialOutfitKey, interactionInstanceType, entryCallbackType, exitCallbackType));
        }

        public static OutfitAssignment[] GetAllOutfitAssignments(SimDescription simDescription)
        {
            return OutfitAssignments.FindAll(x => x.SimDescription == simDescription).ToArray();
        }

        public static void RemoveAllOutfitAssignments(SimDescription simDescription, bool removeSpecialOutfits = false)
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

        public static void SwitchSimToAssignedOutfit(Sims3.Gameplay.Actors.Sim sim, OutfitAssignment outfitAssignment)
        {
            int specialOutfitIndex = outfitAssignment.SimDescription.GetSpecialOutfitIndexFromKey(Sims3.SimIFace.ResourceUtils.HashString32(outfitAssignment.SpecialOutfitKey));
            if (sim.CurrentOutfitCategory != OutfitCategories.Special || sim.CurrentOutfitIndex != specialOutfitIndex)
            {
                outfitAssignment.PreviousOutfitCategory = sim.CurrentOutfitCategory;
                outfitAssignment.PreviousOutfitIndex = sim.CurrentOutfitIndex;
            }
            sim.SwitchToOutfitWithSpin(OutfitCategories.Special, specialOutfitIndex);
        }

        public static bool TryGetOutfitAssignment(SimDescription simDescription, Sims3.Gameplay.Interactions.InteractionInstance interactionInstance, out OutfitAssignment outfitAssignment)
        {
            return TryGetOutfitAssignment(simDescription, interactionInstance.GetType(), out outfitAssignment);
        }

        public static bool TryGetOutfitAssignment(SimDescription simDescription, Type interactionInstanceType, out OutfitAssignment outfitAssignment)
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

        public static void UnassignOutfitToInteraction(SimDescription simDescription, Type interactionInstanceType)
        {
            OutfitAssignment outfitAssignment;
            if (TryGetOutfitAssignment(simDescription, interactionInstanceType, out outfitAssignment))
            {
                OutfitAssignments.Remove(outfitAssignment);
            }
        }
    }
}
