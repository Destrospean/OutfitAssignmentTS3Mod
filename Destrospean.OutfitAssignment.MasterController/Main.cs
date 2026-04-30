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
                Sim sim = Target as Sim;
                Type[] selectedInteractionInstanceTypes;
                InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.FindAll(InteractionInstanceTypeUtils.InteractionInstanceTypes, x => !OutfitAssignmentUtils.OutfitAssignments.Exists(y => y.SimDescription == (sim == null ? null : sim.SimDescription) && (sim != null || y.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && y.InteractionInstanceType == x.FullName))) && TryGetEntryCallbackType(sim ?? Actor, out entryCallbackType) && TryGetExitCallbackType(sim ?? Actor, out exitCallbackType))
                {
                    string specialOutfitKey = "";
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                        if (OutfitAssignmentUtils.TryGetOutfitAssignment(sim == null ? null : sim.SimDescription, interactionInstanceType, out outfitAssignment) && specialOutfitKey != outfitAssignment.SpecialOutfitKey)
                        {
                            if (!string.IsNullOrEmpty(specialOutfitKey))
                            {
                                Common.Notify(Common.Localize(sim == null || sim.IsFemale, sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", sim ?? Actor), sim == null ? null : sim.SimDescription, Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                                return true;
                            }
                            specialOutfitKey = outfitAssignment.SpecialOutfitKey;
                        }
                    }
                    specialOutfitKey = string.IsNullOrEmpty(specialOutfitKey) ? (sim == null ? Actor.GetGlobalAssignedOutfitPrefix() : "OutfitAssignment_") + Sims3.SimIFace.CustomContent.DownloadContent.GenerateGUID() : specialOutfitKey;
                    bool outfitIsPreexisting = sim != null && sim.SimDescription.HasSpecialOutfit(specialOutfitKey);
                    if (sim == null && Actor.SimDescription.HasSpecialOutfit(specialOutfitKey))
                    {
                        Actor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                    }
                    if (sim == null && OutfitAssignmentUtils.GlobalAssignedOutfits.ContainsKey(specialOutfitKey))
                    {
                        SimOutfit outfit;
                        if (Sims3.Gameplay.CAS.OutfitUtils.TryApplyUniformToOutfit(Actor.CurrentOutfit, OutfitAssignmentUtils.GlobalAssignedOutfits[specialOutfitKey], Actor.SimDescription, specialOutfitKey, out outfit))
                        {
                            Actor.SimDescription.AddSpecialOutfit(outfit, specialOutfitKey);
                        }
                    }
                    if (EditSpecialOutfit(sim ?? Actor, specialOutfitKey))
                    {
                        foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                        {
                            OutfitAssignmentUtils.AssignOutfitToInteraction(sim == null ? null : sim.SimDescription, specialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                        }
                    }
                    else if (!outfitIsPreexisting)
                    {
                        (sim ?? Actor).SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                    }
                    if (sim == null)
                    {
                        SimBuilder simBuilder = new SimBuilder();
                        simBuilder.PrepareForOutfit(Actor.SimDescription.GetSpecialOutfit(specialOutfitKey));
                        simBuilder.RemoveParts(BodyTypes.AgeWeathering, BodyTypes.BodyHairCalves, BodyTypes.BodyHairFeet, BodyTypes.BodyHairForearms, BodyTypes.BodyHairFullBack, BodyTypes.BodyHairLowerBack, BodyTypes.BodyHairStomach, BodyTypes.BodyHairUpperBack, BodyTypes.BodyHairUpperChest, BodyTypes.Face, BodyTypes.Freckles, BodyTypes.Moles, BodyTypes.Scalp, BodyTypes.Tattoo, BodyTypes.TattooTemplate);
                        OutfitAssignmentUtils.GlobalAssignedOutfits[specialOutfitKey] = new SimOutfit(simBuilder.CacheOutfit(specialOutfitKey));
                        if (Actor.SimDescription.HasSpecialOutfit(specialOutfitKey))
                        {
                            Actor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                        }
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
                Sim sim = Target as Sim;
                Type[] selectedInteractionInstanceTypes;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.ConvertAll(Array.FindAll(OutfitAssignmentUtils.GetAllOutfitAssignments(sim == null ? null : sim.SimDescription), x => (sim != null || x.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && !x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix)), x => Array.Find(InteractionInstanceTypeUtils.InteractionInstanceTypes, y => y.FullName == x.InteractionInstanceType))))
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment = null;
                    string specialOutfitKey = "";
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.OutfitAssignment tempOutfitAssignment;
                        if (OutfitAssignmentUtils.TryGetOutfitAssignment(sim == null ? null : sim.SimDescription, interactionInstanceType, out tempOutfitAssignment) && specialOutfitKey != tempOutfitAssignment.SpecialOutfitKey)
                        {
                            if (!string.IsNullOrEmpty(specialOutfitKey))
                            {
                                Common.Notify(Common.Localize(sim != null && sim.IsFemale, AssignOutfitToInteraction.sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", sim ?? Actor), sim == null ? null : sim.SimDescription, Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                                return true;
                            }
                            outfitAssignment = tempOutfitAssignment;
                            specialOutfitKey = tempOutfitAssignment.SpecialOutfitKey;
                        }
                    }
                    if (sim == null && Actor.SimDescription.HasSpecialOutfit(specialOutfitKey))
                    {
                        Actor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                    }
                    if (sim == null && OutfitAssignmentUtils.GlobalAssignedOutfits.ContainsKey(specialOutfitKey))
                    {
                        Actor.SimDescription.AddSpecialOutfit(OutfitAssignmentUtils.GlobalAssignedOutfits[specialOutfitKey], specialOutfitKey);
                    }
                    if (EditSpecialOutfit(sim ?? Actor, specialOutfitKey))
                    {
                        foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                        {
                            OutfitAssignmentUtils.AssignOutfitToInteraction(sim == null ? null : sim.SimDescription, specialOutfitKey, interactionInstanceType, outfitAssignment.EntryCallbackType, outfitAssignment.ExitCallbackType);
                        }
                    }
                    if (sim == null)
                    {
                        SimBuilder simBuilder = new SimBuilder();
                        simBuilder.PrepareForOutfit(Actor.SimDescription.GetSpecialOutfit(specialOutfitKey));
                        simBuilder.RemoveParts(BodyTypes.AgeWeathering, BodyTypes.BodyHairCalves, BodyTypes.BodyHairFeet, BodyTypes.BodyHairForearms, BodyTypes.BodyHairFullBack, BodyTypes.BodyHairLowerBack, BodyTypes.BodyHairStomach, BodyTypes.BodyHairUpperBack, BodyTypes.BodyHairUpperChest, BodyTypes.Face, BodyTypes.Freckles, BodyTypes.Moles, BodyTypes.Scalp, BodyTypes.Tattoo, BodyTypes.TattooTemplate);
                        OutfitAssignmentUtils.GlobalAssignedOutfits[specialOutfitKey] = new SimOutfit(simBuilder.CacheOutfit(specialOutfitKey));
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
