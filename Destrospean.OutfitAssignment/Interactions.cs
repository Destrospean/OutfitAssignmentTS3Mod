using System;
using System.Collections.Generic;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace Destrospean.OutfitAssignment
{
    public class Interactions
    {
        public class AssignOutfitCategoryToInteraction : ImmediateInteraction<Sim, GameObject>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/AssignOutfitCategoryToInteraction";

            public class Definition : ImmediateInteractionDefinition<Sim, GameObject, AssignOutfitCategoryToInteraction>
            {
                public override string GetInteractionName(Sim actor, GameObject target, InteractionObjectPair iop)
                {
                    Sim sim = target as Sim;
                    return Common.Localize(sim != null && sim.IsFemale, sLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Common.Localize(isFemale, sLocalizationKey + ":Path")
                    };
                }

                public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    Sim sim = target as Sim;
                    return sim == null || sim.IsHuman;
                }
            }

            public override bool Run()
            {
                Sim sim = Target as Sim;
                Type[] selectedInteractionInstanceTypes;
                InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
                OutfitCategories outfitCategory;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes) && AssignOutfitToInteraction.TryGetEntryCallbackType(sim ?? Actor, out entryCallbackType) && AssignOutfitToInteraction.TryGetExitCallbackType(sim ?? Actor, out exitCallbackType) && TryGetOutfitCategory(sim ?? Actor, out outfitCategory))
                {
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.AssignOutfitToInteraction(sim == null ? null : sim.SimDescription, OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix + outfitCategory, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                    }
                }
                return true;
            }

            public static bool TryGetOutfitCategory(Sim target, out OutfitCategories outfitCategory)
            {
                string localizationKey = "/Dialogs/OutfitCategoryDialog",
                text = Dialogs.ComboSelectionDialog.Show(Common.Localize(target.IsFemale, localizationKey + ":Title"), new SortedDictionary<string, object>(new AssignOutfitToInteraction.DummyComparer())
                    {
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:Everyday"),
                            OutfitCategories.Everyday.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:Formalwear"),
                            OutfitCategories.Formalwear.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:Sleepwear"),
                            OutfitCategories.Sleepwear.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:Swimwear"),
                            OutfitCategories.Swimwear.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:Athletic"),
                            OutfitCategories.Athletic.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:Career"),
                            OutfitCategories.Career.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:Outerwear"),
                            OutfitCategories.Outerwear.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:MartialArts"),
                            OutfitCategories.MartialArts.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:Naked"),
                            OutfitCategories.Naked.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:Singed"),
                            OutfitCategories.Singed.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:SkinnyDippingTowel"),
                            OutfitCategories.SkinnyDippingTowel.ToString()
                        }
                    }, OutfitCategories.Everyday.ToString()) as string;
                if (text == null)
                {
                    outfitCategory = OutfitCategories.None;
                    return false;
                }
                outfitCategory = (OutfitCategories)Enum.Parse(typeof(OutfitCategories), text);
                return true;
            }
        }

        public class AssignOutfitToInteraction : ImmediateInteraction<Sim, GameObject>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/AssignOutfitToInteraction";

            public class Definition : ImmediateInteractionDefinition<Sim, GameObject, AssignOutfitToInteraction>
            {
                public override string GetInteractionName(Sim actor, GameObject target, InteractionObjectPair iop)
                {
                    Sim sim = target as Sim;
                    return Common.Localize(sim != null && sim.IsFemale, sLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Common.Localize(isFemale, sLocalizationKey + ":Path")
                    };
                }

                public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    Sim sim = target as Sim;
                    return sim == null || sim.IsHuman;
                }
            }

            public class DummyComparer : IComparer<string>
            {
                public int Compare(string a, string b)
                {
                    return 1;
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
                    if ((sim ?? Actor).EditSpecialOutfit(specialOutfitKey))
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
                        simBuilder.ClearBlends();
                        OutfitAssignmentUtils.GlobalAssignedOutfits[specialOutfitKey] = new SimOutfit(simBuilder.CacheOutfit(specialOutfitKey));
                        if (Actor.SimDescription.HasSpecialOutfit(specialOutfitKey))
                        {
                            Actor.SimDescription.RemoveSpecialOutfit(specialOutfitKey);
                        }
                    }
                }
                return true;
            }

            public static bool TryGetEntryCallbackType(Sim target, out InteractionInstanceTypeUtils.CallbackTypes? callbackType)
            {
                string localizationKey = "/Dialogs/EntryCallbackTypeDialog",
                text = Dialogs.ComboSelectionDialog.Show(Common.Localize(target.IsFemale, localizationKey + ":Title"), new SortedDictionary<string, object>(new DummyComparer())
                    {
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:InteractionStarted"),
                            InteractionInstanceTypeUtils.CallbackTypes.InteractionStarted.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:StandardEntry"),
                            InteractionInstanceTypeUtils.CallbackTypes.StandardEntry.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:SyncLevelRouted"),
                            InteractionInstanceTypeUtils.CallbackTypes.SyncLevelRouted.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:SyncLevelCommitted"),
                            InteractionInstanceTypeUtils.CallbackTypes.SyncLevelCommitted.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:OutfitChanged"),
                            InteractionInstanceTypeUtils.CallbackTypes.OutfitChanged.ToString()
                        }
                    }, InteractionInstanceTypeUtils.CallbackTypes.InteractionStarted.ToString()) as string;
                if (text == null)
                {
                    callbackType = null;
                    return false;
                }
                callbackType = (InteractionInstanceTypeUtils.CallbackTypes)Enum.Parse(typeof(InteractionInstanceTypeUtils.CallbackTypes), text);
                return true;
            }

            public static bool TryGetExitCallbackType(Sim target, out InteractionInstanceTypeUtils.CallbackTypes? callbackType)
            {
                string localizationKey = "/Dialogs/ExitCallbackTypeDialog",
                text = Dialogs.ComboSelectionDialog.Show(Common.Localize(target.IsFemale, localizationKey + ":Title"), new SortedDictionary<string, object>(new DummyComparer())
                    {
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:InteractionEnded"),
                            InteractionInstanceTypeUtils.CallbackTypes.InteractionEnded.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:StandardExit"),
                            InteractionInstanceTypeUtils.CallbackTypes.StandardExit.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:SyncLevelCompleted"),
                            InteractionInstanceTypeUtils.CallbackTypes.SyncLevelCompleted.ToString()
                        },
                        {
                            Common.Localize(target.IsFemale, localizationKey + "/Options:Never"),
                            InteractionInstanceTypeUtils.CallbackTypes.Never.ToString()
                        }
                    }, InteractionInstanceTypeUtils.CallbackTypes.InteractionEnded.ToString()) as string;
                if (text == null)
                {
                    callbackType = null;
                    return false;
                }
                callbackType = (InteractionInstanceTypeUtils.CallbackTypes)Enum.Parse(typeof(InteractionInstanceTypeUtils.CallbackTypes), text);
                return true;
            }
        }

        public class ConfigureOutfitAssignment : ImmediateInteraction<Sim, GameObject>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/ConfigureOutfitAssignment";

            public class Definition : ImmediateInteractionDefinition<Sim, GameObject, ConfigureOutfitAssignment>
            {
                public override string GetInteractionName(Sim actor, GameObject target, InteractionObjectPair iop)
                {
                    Sim sim = target as Sim;
                    return Common.Localize(sim != null && sim.IsFemale, sLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Common.Localize(isFemale, sLocalizationKey + ":Path")
                    };
                }

                public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    Sim sim = target as Sim;
                    return Array.FindAll(OutfitAssignmentUtils.GetAllOutfitAssignments(sim == null ? null : sim.SimDescription), x => sim != null || x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix) || x.SpecialOutfitKey.StartsWith(actor.GetGlobalAssignedOutfitPrefix())).Length > 0;
                }
            }

            public override bool Run()
            {
                Sim sim = Target as Sim;
                Type[] selectedInteractionInstanceTypes;
                InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.FindAll(InteractionInstanceTypeUtils.InteractionInstanceTypes, x => OutfitAssignmentUtils.OutfitAssignments.Exists(y => y.SimDescription == (sim == null ? null : sim.SimDescription) && (sim != null || y.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && y.InteractionInstanceType == x.FullName))) && AssignOutfitToInteraction.TryGetEntryCallbackType(sim ?? Actor, out entryCallbackType) && AssignOutfitToInteraction.TryGetExitCallbackType(sim ?? Actor, out exitCallbackType))
                {
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                        if (OutfitAssignmentUtils.TryGetOutfitAssignment(sim == null ? null : sim.SimDescription, interactionInstanceType, out outfitAssignment))
                        {
                            OutfitAssignmentUtils.AssignOutfitToInteraction(sim == null ? null : sim.SimDescription, outfitAssignment.SpecialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                        }
                    }
                }
                return true;
            }
        }

        public class CopyAssignedOutfitToInteraction : ImmediateInteraction<Sim, GameObject>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/CopyAssignedOutfitToInteraction";

            public class Definition : ImmediateInteractionDefinition<Sim, GameObject, CopyAssignedOutfitToInteraction>
            {
                public override string GetInteractionName(Sim actor, GameObject target, InteractionObjectPair iop)
                {
                    Sim sim = target as Sim;
                    return Common.Localize(sim != null && sim.IsFemale, sLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Common.Localize(isFemale, sLocalizationKey + ":Path")
                    };
                }

                public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    Sim sim = target as Sim;
                    return Array.FindAll(OutfitAssignmentUtils.GetAllOutfitAssignments(sim == null ? null : sim.SimDescription), x => !x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix)).Length > 0;
                }
            }

            public override bool Run()
            {
                Sim sim = Target as Sim;
                Type[] sourceInteractionInstanceTypes, destinationInteractionInstanceTypes;
                string sourceSpecialOutfitKey = "";
                if (!InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out sourceInteractionInstanceTypes, Array.ConvertAll(Array.FindAll(OutfitAssignmentUtils.GetAllOutfitAssignments(sim == null ? null : sim.SimDescription), x => (sim != null || x.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && !x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix)), x => Array.Find(InteractionInstanceTypeUtils.InteractionInstanceTypes, y => y.FullName == x.InteractionInstanceType)), Common.Localize(sLocalizationKey + "/Miscellaneous:SelectSourceNamespace"), Common.Localize(sLocalizationKey + "/Miscellaneous:SelectSourceInteraction")))
                {
                    return true;
                }
                foreach (Type interactionInstanceType in sourceInteractionInstanceTypes)
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (OutfitAssignmentUtils.TryGetOutfitAssignment(sim == null ? null : sim.SimDescription, interactionInstanceType, out outfitAssignment) && sourceSpecialOutfitKey != outfitAssignment.SpecialOutfitKey)
                    {
                        if (!string.IsNullOrEmpty(sourceSpecialOutfitKey))
                        {
                            Common.Notify(Common.Localize(sim != null && sim.IsFemale, AssignOutfitToInteraction.sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", sim ?? Actor), sim == null ? null : sim.SimDescription, Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                            return true;
                        }
                        sourceSpecialOutfitKey = outfitAssignment.SpecialOutfitKey;
                    }
                }
                InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out destinationInteractionInstanceTypes, null, Common.Localize(sLocalizationKey + "/Miscellaneous:SelectDestinationNamespace"), Common.Localize(sLocalizationKey + "/Miscellaneous:SelectDestinationInteraction")) && AssignOutfitToInteraction.TryGetEntryCallbackType(sim ?? Actor, out entryCallbackType) && AssignOutfitToInteraction.TryGetExitCallbackType(sim ?? Actor, out exitCallbackType))
                {
                    string destinationSpecialOutfitKey = (sim == null ? Actor.GetGlobalAssignedOutfitPrefix() : "OutfitAssignment_") + Sims3.SimIFace.CustomContent.DownloadContent.GenerateGUID();
                    if (sim == null)
                    {
                        OutfitAssignmentUtils.GlobalAssignedOutfits[destinationSpecialOutfitKey] = new SimOutfit(sim.SimDescription.GetSpecialOutfit(sourceSpecialOutfitKey).Key);
                    }
                    else
                    {
                        SimOutfit outfit;
                        if (Sims3.Gameplay.CAS.OutfitUtils.TryApplyUniformToOutfit(Actor.CurrentOutfit, OutfitAssignmentUtils.GlobalAssignedOutfits[destinationSpecialOutfitKey], Actor.SimDescription, destinationSpecialOutfitKey, out outfit))
                        {
                            Actor.SimDescription.AddSpecialOutfit(outfit, destinationSpecialOutfitKey);
                        }
                    }
                    foreach (Type interactionInstanceType in destinationInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.AssignOutfitToInteraction(sim == null ? null : sim.SimDescription, destinationSpecialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                    }
                }
                return true;
            }
        }

        public class EditAssignedOutfit : ImmediateInteraction<Sim, GameObject>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/EditAssignedOutfit";

            public class Definition : ImmediateInteractionDefinition<Sim, GameObject, EditAssignedOutfit>
            {
                public override string GetInteractionName(Sim actor, GameObject target, InteractionObjectPair iop)
                {
                    Sim sim = target as Sim;
                    return Common.Localize(sim != null && sim.IsFemale, sLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Common.Localize(isFemale, sLocalizationKey + ":Path")
                    };
                }

                public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    Sim sim = target as Sim;
                    return Array.FindAll(OutfitAssignmentUtils.GetAllOutfitAssignments(sim == null ? null : sim.SimDescription), x => !x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix)).Length > 0;
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
                    if ((sim ?? Actor).EditSpecialOutfit(specialOutfitKey))
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

        public class ExtendAssignedOutfitToInteraction : ImmediateInteraction<Sim, GameObject>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/ExtendAssignedOutfitToInteraction";

            public class Definition : ImmediateInteractionDefinition<Sim, GameObject, ExtendAssignedOutfitToInteraction>
            {
                public override string GetInteractionName(Sim actor, GameObject target, InteractionObjectPair iop)
                {
                    Sim sim = target as Sim;
                    return Common.Localize(sim != null && sim.IsFemale, sLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Common.Localize(isFemale, sLocalizationKey + ":Path")
                    };
                }

                public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    Sim sim = target as Sim;
                    return Array.FindAll(OutfitAssignmentUtils.GetAllOutfitAssignments(sim == null ? null : sim.SimDescription), x => sim != null || x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix) || x.SpecialOutfitKey.StartsWith(actor.GetGlobalAssignedOutfitPrefix())).Length > 0;

                }
            }

            public override bool Run()
            {
                Sim sim = Target as Sim;
                Type[] sourceInteractionInstanceTypes, destinationInteractionInstanceTypes;
                if (!InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out sourceInteractionInstanceTypes, Array.FindAll(InteractionInstanceTypeUtils.InteractionInstanceTypes, x => OutfitAssignmentUtils.OutfitAssignments.Exists(y => y.SimDescription == (sim == null ? null : sim.SimDescription) && (sim != null || y.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && y.InteractionInstanceType == x.FullName)), Common.Localize(CopyAssignedOutfitToInteraction.sLocalizationKey + "/Miscellaneous:SelectSourceNamespace"), Common.Localize(CopyAssignedOutfitToInteraction.sLocalizationKey + "/Miscellaneous:SelectSourceInteraction")))
                {
                    return true;
                }
                string specialOutfitKey = "";
                foreach (Type interactionInstanceType in sourceInteractionInstanceTypes)
                {
                    OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                    if (OutfitAssignmentUtils.TryGetOutfitAssignment(sim == null ? null : sim.SimDescription, interactionInstanceType, out outfitAssignment) && specialOutfitKey != outfitAssignment.SpecialOutfitKey)
                    {
                        if (!string.IsNullOrEmpty(specialOutfitKey))
                        {
                            Common.Notify(Common.Localize(sim != null && sim.IsFemale, AssignOutfitToInteraction.sLocalizationKey + "/Messages:MultipleOutfitsForInteractionsFound", sim ?? Actor), sim == null ? null : sim.SimDescription, Sims3.UI.StyledNotification.NotificationStyle.kSystemMessage);
                            return true;
                        }
                        specialOutfitKey = outfitAssignment.SpecialOutfitKey;
                    }
                }
                InteractionInstanceTypeUtils.CallbackTypes? entryCallbackType, exitCallbackType;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out destinationInteractionInstanceTypes, null, Common.Localize(CopyAssignedOutfitToInteraction.sLocalizationKey + "/Miscellaneous:SelectDestinationNamespace"), Common.Localize(CopyAssignedOutfitToInteraction.sLocalizationKey + "/Miscellaneous:SelectDestinationInteraction")) && AssignOutfitToInteraction.TryGetEntryCallbackType(sim ?? Actor, out entryCallbackType) && AssignOutfitToInteraction.TryGetExitCallbackType(sim ?? Actor, out exitCallbackType))
                {
                    foreach (Type interactionInstanceType in destinationInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.AssignOutfitToInteraction(sim == null ? null : sim.SimDescription, specialOutfitKey, interactionInstanceType, entryCallbackType.Value, exitCallbackType.Value);
                    }
                }
                return true;
            }
        }

        public class UnassignOutfitToInteraction : ImmediateInteraction<Sim, GameObject>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = "/Interactions/UnassignOutfitToInteraction";

            public class Definition : ImmediateInteractionDefinition<Sim, GameObject, UnassignOutfitToInteraction>
            {
                public override string GetInteractionName(Sim actor, GameObject target, InteractionObjectPair iop)
                {
                    Sim sim = target as Sim;
                    return Common.Localize(sim != null && sim.IsFemale, sLocalizationKey + ":Name");
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Common.Localize(isFemale, sLocalizationKey + ":Path")
                    };
                }

                public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    Sim sim = target as Sim;
                    return Array.FindAll(OutfitAssignmentUtils.GetAllOutfitAssignments(sim == null ? null : sim.SimDescription), x => sim != null || x.SpecialOutfitKey.StartsWith(OutfitAssignmentUtils.OutfitAssignmentCategoryPrefix) || x.SpecialOutfitKey.StartsWith(actor.GetGlobalAssignedOutfitPrefix())).Length > 0;

                }
            }

            public override bool Run()
            {
                Sim sim = Target as Sim;
                Type[] selectedInteractionInstanceTypes;
                if (InteractionInstanceTypeUtils.TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, Array.FindAll(InteractionInstanceTypeUtils.InteractionInstanceTypes, x => OutfitAssignmentUtils.OutfitAssignments.Exists(y => y.SimDescription == (sim == null ? null : sim.SimDescription) && (sim != null || y.SpecialOutfitKey.StartsWith(Actor.GetGlobalAssignedOutfitPrefix())) && y.InteractionInstanceType == x.FullName))))
                {
                    foreach (Type interactionInstanceType in selectedInteractionInstanceTypes)
                    {
                        OutfitAssignmentUtils.OutfitAssignment outfitAssignment;
                        if (OutfitAssignmentUtils.TryGetOutfitAssignment(sim == null ? null : sim.SimDescription, interactionInstanceType, out outfitAssignment) && Array.FindAll(OutfitAssignmentUtils.GetAllOutfitAssignments(sim == null ? null : sim.SimDescription), x => x.SpecialOutfitKey == outfitAssignment.SpecialOutfitKey).Length == 1)
                        {
                            if (sim == null)
                            {
                                foreach (Sim tempSim in Sims3.Gameplay.Queries.GetObjects<Sim>())
                                {
                                    if (tempSim.SimDescription != null && tempSim.SimDescription.HasSpecialOutfit(outfitAssignment.SpecialOutfitKey))
                                    {
                                        tempSim.SimDescription.RemoveSpecialOutfit(outfitAssignment.SpecialOutfitKey);
                                    }
                                }
                                OutfitAssignmentUtils.GlobalAssignedOutfits.Remove(outfitAssignment.SpecialOutfitKey);
                            }
                            else
                            {
                                sim.SimDescription.RemoveSpecialOutfit(outfitAssignment.SpecialOutfitKey);
                            }
                        }
                        OutfitAssignmentUtils.UnassignOutfitToInteraction(sim == null ? null : sim.SimDescription, interactionInstanceType);
                    }
                }
                return true;
            }
        }
    }
}
