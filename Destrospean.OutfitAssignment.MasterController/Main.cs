using System;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Interactions;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace Destrospean.OutfitAssignment.MasterController
{
    public class Main
    {
        [Tunable]
        protected static bool kInstantiator;

        public class AssignOutfitToInteraction : Interactions.AssignOutfitToInteraction
        {
            public class DefinitionModified : ImmediateInteractionDefinition<Sim, GameObject, AssignOutfitToInteraction>
            {
                Definition mDefinitionBase = new Definition();

                public override string GetInteractionName(Sim actor, GameObject target, Sims3.Gameplay.Autonomy.InteractionObjectPair iop)
                {
                    return mDefinitionBase.GetInteractionName(actor, target, iop);
                }

                public override string[] GetPath(bool isFemale)
                {
                    return mDefinitionBase.GetPath(isFemale);
                }

                public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return mDefinitionBase.Test(actor, target, isAutonomous, ref greyedOutTooltipCallback);
                }
            }

            public override bool Run()
            {
                Sim targetSim = Target as Sim;
                Type[] selectedInteractionInstanceTypes;
                InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.FindAll(InteractionInstanceTypeUtils.InteractionInstanceTypes, x => !OutfitAssignmentUtils.OutfitAssignments.Exists(y => y.SimDescription == targetSim.GetSimDescription() && (targetSim != null || y.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && y.InteractionInstanceType == x.FullName))) && TryGetEntryCallbackType(targetSim, selectedInteractionInstanceTypes[0], out entryCallbackType) && TryGetExitCallbackType(targetSim, selectedInteractionInstanceTypes[0], out exitCallbackType))
                {
                    string specialOutfitKey = "";
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                        if (targetSim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out outfitAssignment) && specialOutfitKey != outfitAssignment.SpecialOutfitKey)
                        {
                            if (!string.IsNullOrEmpty(specialOutfitKey))
                            {
                                Common.Notify(Common.Localize(targetSim == null || targetSim.IsFemale, sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", targetSim ?? Actor), targetSim.GetSimDescription(), Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                                return true;
                            }
                            specialOutfitKey = outfitAssignment.SpecialOutfitKey;
                        }
                    }
                    specialOutfitKey = string.IsNullOrEmpty(specialOutfitKey) ? (targetSim == null ? Actor.GetGlobalAssignedOutfitPrefix() : "OutfitAssignment_") + Sims3.SimIFace.CustomContent.DownloadContent.GenerateGUID() : specialOutfitKey;
                    bool outfitIsPreexisting = targetSim != null && targetSim.SimDescription.HasSpecialOutfit(specialOutfitKey);
                    if (targetSim == null && Actor.SimDescription.HasSpecialOutfit(specialOutfitKey))
                    {
                        Actor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                    }
                    if (targetSim == null && OutfitAssignmentUtils.AssignedOutfits.ContainsKey(specialOutfitKey))
                    {
                        Actor.AddAssignedOutfit(specialOutfitKey);
                    }
                    if (EditSpecialOutfit(targetSim ?? Actor, specialOutfitKey))
                    {
                        foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                        {
                            targetSim.GetSimDescription().AssignOutfitToInteraction(specialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                        }
                        if (targetSim == null)
                        {
                            BodyTypes[] partOverrides;
                            if (OutfitAssignmentUtils.ShowPartOverridesDialog(OutfitAssignmentUtils.AssignedOutfits[specialOutfitKey] = new OutfitAssignmentUtils.AssignedOutfit(Actor.SimDescription.GetSpecialOutfit(specialOutfitKey)), out partOverrides))
                            {
                                OutfitAssignmentUtils.AssignedOutfits[specialOutfitKey].PartOverrides = new System.Collections.Generic.List<BodyTypes>(partOverrides);
                            }
                            if (Actor.SimDescription.HasSpecialOutfit(specialOutfitKey))
                            {
                                Actor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                            }
                        }
                    }
                    else if (!outfitIsPreexisting)
                    {
                        (targetSim ?? Actor).SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                    }
                }
                return true;
            }
        }

        public class EditAssignedOutfit : Interactions.EditAssignedOutfit
        {
            public class DefinitionModified : ImmediateInteractionDefinition<Sim, GameObject, EditAssignedOutfit>
            {
                Definition mDefinitionBase = new Definition();

                public override string GetInteractionName(Sim actor, GameObject target, Sims3.Gameplay.Autonomy.InteractionObjectPair iop)
                {
                    return mDefinitionBase.GetInteractionName(actor, target, iop);
                }

                public override string[] GetPath(bool isFemale)
                {
                    return mDefinitionBase.GetPath(isFemale);
                }

                public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return mDefinitionBase.Test(actor, target, isAutonomous, ref greyedOutTooltipCallback);
                }
            }

            public override bool Run()
            {
                Sim targetSim = Target as Sim;
                Type[] selectedInteractionInstanceTypes;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.ConvertAll(Array.FindAll(targetSim.GetSimDescription().GetAllOutfitAssignments(), x => (targetSim != null || x.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && !x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix)), x => Array.Find(InteractionInstanceTypeUtils.InteractionInstanceTypes, y => y.FullName == x.InteractionInstanceType))))
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment = null;
                    string specialOutfitKey = "";
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.OutfitAssignment tempOutfitAssignment;
                        if (targetSim.GetSimDescription().TryGetOutfitAssignment(interactionInstanceType, out tempOutfitAssignment) && specialOutfitKey != tempOutfitAssignment.SpecialOutfitKey)
                        {
                            if (!string.IsNullOrEmpty(specialOutfitKey))
                            {
                                Common.Notify(Common.Localize(targetSim != null && targetSim.IsFemale, AssignOutfitToInteraction.sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", targetSim ?? Actor), targetSim.GetSimDescription(), Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                                return true;
                            }
                            outfitAssignment = tempOutfitAssignment;
                            specialOutfitKey = tempOutfitAssignment.SpecialOutfitKey;
                        }
                    }
                    if (targetSim == null && Actor.SimDescription.HasSpecialOutfit(specialOutfitKey))
                    {
                        Actor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                    }
                    if (targetSim == null && OutfitAssignmentUtils.AssignedOutfits.ContainsKey(specialOutfitKey))
                    {
                        Actor.AddAssignedOutfit(specialOutfitKey);
                    }
                    if (EditSpecialOutfit(targetSim ?? Actor, specialOutfitKey))
                    {
                        foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                        {
                            targetSim.GetSimDescription().AssignOutfitToInteraction(specialOutfitKey, interactionInstanceType, outfitAssignment.EntryCallbackType, outfitAssignment.ExitCallbackType);
                        }
                        if (targetSim == null)
                        {
                            OutfitAssignmentUtils.AssignedOutfits[specialOutfitKey] = new OutfitAssignmentUtils.AssignedOutfit(Actor.SimDescription.GetSpecialOutfit(specialOutfitKey))
                                {
                                    PartOverrides = OutfitAssignmentUtils.AssignedOutfits[specialOutfitKey].PartOverrides
                                };
                        }
                    }
                    if (targetSim == null)
                    {
                        if (Actor.SimDescription.HasSpecialOutfit(specialOutfitKey))
                        {
                            Actor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                        }
                    }
                }
                return true;
            }
        }

        static Main()
        {
            LoadSaveManager.ObjectGroupsPreLoad += () =>
                {
                    Interactions.AssignOutfitToInteraction.Singleton = new AssignOutfitToInteraction.DefinitionModified();
                    Interactions.EditAssignedOutfit.Singleton = new EditAssignedOutfit.DefinitionModified();
                };
        }

        static bool EditSpecialOutfit(Sim sim, string specialOutfitKey)
        {
            SimDescription simDescription = sim.SimDescription;
            if (!simDescription.HasSpecialOutfit(specialOutfitKey))
            {
                simDescription.AddSpecialOutfit(simDescription.GetOutfit(OutfitCategories.Everyday, 0), specialOutfitKey);
            }
            OutfitCategories previousOutfitCategory = sim.CurrentOutfitCategory;
            int previousOutfitIndex = sim.CurrentOutfitIndex;
            simDescription.AddOutfit(simDescription.GetSpecialOutfit(specialOutfitKey), OutfitCategories.Everyday, 0);
            simDescription.RemoveSpecialOutfit(specialOutfitKey);
            sim.SwitchToOutfitWithoutSpin(OutfitCategories.Everyday, 0);
            CASLogic casLogic = CASLogic.GetSingleton();
            new NRaas.MasterControllerSpace.Sims.Stylist().Perform(new NRaas.CommonSpace.Options.GameHitParameters<Sims3.Gameplay.Abstracts.GameObject>(sim, sim, GameObjectHit.NoHit));
            casLogic.ShowUI += OutfitExtensions.OnShowUI;
            while (GameStates.NextInWorldStateId != 0)
            {
                NRaas.SpeedTrap.Sleep();
            }
            casLogic.ShowUI -= OutfitExtensions.OnShowUI;
            simDescription.AddSpecialOutfit(simDescription.GetOutfit(OutfitCategories.Everyday, 0), specialOutfitKey);
            simDescription.RemoveOutfit(OutfitCategories.Everyday, 0, true);
            sim.SwitchToOutfitWithoutSpin(previousOutfitCategory, previousOutfitIndex);
            return !CASChangeReporter.Instance.CasCancelled;
        }
    }
}
