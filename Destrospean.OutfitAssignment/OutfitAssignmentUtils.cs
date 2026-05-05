using System;
using System.Collections.Generic;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using Tuning = Sims3.Gameplay.Destrospean.OutfitAssignment;

namespace Destrospean.OutfitAssignment
{
    public static class OutfitAssignmentUtils
    {
        static Dictionary<string, OutfitAssignment> sIndexedOutfitAssignments;

        public static Dictionary<string, AssignedOutfit> AssignedOutfits
        {
            get
            {
                return (Dictionary<string, AssignedOutfit>)typeof(OutfitAssignmentUtils).GetField("GlobalOutfits", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null);
            }
        }

        [Obsolete("Use AssignedOutfits instead", true), PersistableStatic(true)]
        public static Dictionary<string, AssignedOutfit> GlobalOutfits = new Dictionary<string, AssignedOutfit>();

        public static Dictionary<string, OutfitAssignment> IndexedOutfitAssignments
        {
            get
            {
                if (sIndexedOutfitAssignments == null)
                {
                    sIndexedOutfitAssignments = new Dictionary<string, OutfitAssignment>();
                    foreach (OutfitAssignment outfitAssignment in OutfitAssignments)
                    {
                        sIndexedOutfitAssignments[outfitAssignment.InteractionInstanceType + (outfitAssignment.SimDescription == null ? "" : ("_" + outfitAssignment.SimDescription.SimDescriptionId))] = outfitAssignment;
                    }
                }
                return sIndexedOutfitAssignments;
            }
        }

        public const string OutfitAssignmentCategoryPrefix = "OutfitAssignment_Category_";

        [PersistableStatic(true)]
        public static List<OutfitAssignment> OutfitAssignments = new List<OutfitAssignment>();

        public static readonly BodyTypes[] OverridableBodyTypes;

        [PersistableStatic(true)]
        public static List<Outfit> PreviousOutfits = new List<Outfit>();

        public static List<SimDescription> TimeToChangeBackList = new List<SimDescription>();

        [Persistable]
        public class AssignedOutfit
        {
            public List<BodyTypes> PartOverrides = new List<BodyTypes>(OverridableBodyTypes);

            public List<SavedPart> Parts;

            [Persistable]
            public class SavedPart
            {
                public CASPart Part;

                public string Preset;

                public SavedPart()
                {
                }

                public SavedPart(CASPart part, string preset)
                {
                    Part = part;
                    Preset = preset;
                }
            }
            
            public AssignedOutfit()
            {
            }

            public AssignedOutfit(AssignedOutfit outfit)
            {
                Parts = outfit.Parts.ConvertAll(x => new SavedPart(x.Part, x.Preset));
                PartOverrides = new List<BodyTypes>(outfit.PartOverrides);
            }

            public AssignedOutfit(SimOutfit outfit)
            {
                Parts = new List<CASPart>(outfit.Parts).ConvertAll(x => new SavedPart(x, outfit.GetPartPreset(x.Key)));
            }
        }

        public class BodyTypeColumn : Dialogs.ObjectPickerDialog.CommonHeaderInfo<BodyTypes>
        {
            readonly string mLocalizationPath;

            public BodyTypeColumn(string localizationPath) : base(localizationPath + "/Headers/PartType:Text", localizationPath + "/Headers/PartType:Tooltip", 400)
            {
                mLocalizationPath = localizationPath;
            }

            public override ObjectPicker.ColumnInfo GetValue(BodyTypes bodyType)
            {
                return new ObjectPicker.TextColumn(Responder.Instance.LocalizationModel.LocalizeString(mLocalizationPath + "/Options/PartType:" + bodyType));
            }
        }

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

        public class PartOverrideEnabledColumn : Dialogs.ObjectPickerDialog.CommonHeaderInfo<BodyTypes>
        {
            readonly string mLocalizationPath;

            readonly List<BodyTypes> mPartOverrides;

            public PartOverrideEnabledColumn(string localizationPath, BodyTypes[] partOverrides) : base(localizationPath + "/Headers/Enabled:Text", localizationPath + "/Headers/Enabled:Tooltip", 40)
            {
                mLocalizationPath = localizationPath;
                mPartOverrides = new List<BodyTypes>(partOverrides);
            }

            public override ObjectPicker.ColumnInfo GetValue(BodyTypes bodyType)
            {
                return new ObjectPicker.TextColumn(Responder.Instance.LocalizationModel.LocalizeString(mLocalizationPath + "/Options/Enabled:" + mPartOverrides.Contains(bodyType)));
            }
        }

        static OutfitAssignmentUtils()
        {
            List<BodyTypes> overridableBodyTypes = new List<BodyTypes>();
            foreach (BodyTypes bodyType in Enum.GetValues(typeof(BodyTypes)))
            {
                if (bodyType < BodyTypes.PetBody && !overridableBodyTypes.Contains(bodyType))
                {
                    switch (bodyType)
                    {
                        case BodyTypes.AgeWeathering:
                        case BodyTypes.BirthMark:
                        case BodyTypes.Dental:
                        case BodyTypes.EyeColor:
                        case BodyTypes.Face:
                        case BodyTypes.Freckles:
                        case BodyTypes.Moles:
                        case BodyTypes.None:
                        case BodyTypes.Scalp:
                        case BodyTypes.Tattoo:
                        case BodyTypes.TattooTemplate:
                            continue;
                    }
                    overridableBodyTypes.Add(bodyType);
                }
            }
            OverridableBodyTypes = overridableBodyTypes.ToArray();
        }

        public static bool AddAssignedOutfit(this Sim sim, string assignedSpecialOutfitKey, string simSpecialOutfitKey = null)
        {
            AssignedOutfit assignedOutfit;
            if (!AssignedOutfits.TryGetValue(assignedSpecialOutfitKey, out assignedOutfit))
            {
                return false;
            }
            simSpecialOutfitKey = simSpecialOutfitKey ?? assignedSpecialOutfitKey;
            if (sim.SimDescription.HasSpecialOutfit(simSpecialOutfitKey))
            {
                sim.SimDescription.RemoveSpecialOutfit(simSpecialOutfitKey);
            }
            using (SimBuilder simBuilder = new SimBuilder
                {
                    UseCompression = true
                })
            {
                simBuilder.PrepareForOutfit(sim.CurrentOutfit);
                foreach (BodyTypes bodyType in OverridableBodyTypes)
                {
                    if (!assignedOutfit.PartOverrides.Contains(bodyType))
                    {
                        continue;
                    }
                    int savedPartIndex = assignedOutfit.Parts.FindIndex(x => x.Part.BodyType == bodyType);
                    if (savedPartIndex == -1)
                    {
                        simBuilder.RemoveParts(bodyType);
                        continue;
                    }
                    AssignedOutfit.SavedPart savedPart = assignedOutfit.Parts[savedPartIndex];
                    switch (bodyType)
                    {
                        case BodyTypes.FullBody:
                            simBuilder.RemoveParts(BodyTypes.LowerBody, BodyTypes.UpperBody);
                            break;
                        case BodyTypes.LowerBody:
                        case BodyTypes.UpperBody:
                            simBuilder.RemoveParts(BodyTypes.FullBody);
                            break;
                    }
                    simBuilder.RemoveParts(bodyType);
                    simBuilder.AddPart(savedPart.Part);
                    if (!string.IsNullOrEmpty(savedPart.Preset))
                    {
                        if (CASUtils.ApplyPresetToPart(simBuilder, savedPart.Part, savedPart.Preset))
                        {
                            simBuilder.SetPartPreset(savedPart.Part.Key, null, savedPart.Preset);
                        }
                    }
                }
                return sim.SimDescription.AddSpecialOutfit(new SimOutfit(simBuilder.CacheOutfit(simSpecialOutfitKey + "_" + sim.SimDescription.SimDescriptionId)), simSpecialOutfitKey) > -1;
            }
        }

        public static void AssignOutfitToInteraction(this SimDescription simDescription, string specialOutfitKey, Type interactionInstanceType, InteractionInstanceTypeUtils.CallbackTypes entryCallbackType, InteractionInstanceTypeUtils.CallbackTypes exitCallbackType)
        {
            UnassignOutfitToInteraction(simDescription, interactionInstanceType);
            OutfitAssignments.Add(new OutfitAssignment(simDescription, specialOutfitKey, interactionInstanceType, entryCallbackType, exitCallbackType));
            sIndexedOutfitAssignments = null;
        }

        public static void CreateOutfitForCategoryIfNecessary(this SimDescription simDescription, OutfitCategories outfitCategory)
        {
            switch (outfitCategory)
            {
                case OutfitCategories.Singed:
                    BuffSinged.SetupSingedOutfit(simDescription.CreatedSim);
                    break;
                case OutfitCategories.SkinnyDippingTowel:
                    simDescription.RemoveOutfits(OutfitCategories.SkinnyDippingTowel, true);
                    if (simDescription.HasSpecialOutfit("SkinnyDipTowel"))
                    {
                        simDescription.AddOutfit(simDescription.GetSpecialOutfit("SkinnyDipTowel"), OutfitCategories.SkinnyDippingTowel, true);
                    }
                    else
                    {
                        OutfitUtils.CreateOutfitForSim(simDescription, ResourceKey.CreateOutfitKeyFromProductVersion(OutfitUtils.GetGenderPrefix(simDescription.Gender) + OutfitUtils.GetAgePrefix(simDescription.Age, true) + "_towel", ProductVersion.EP3), OutfitCategories.SkinnyDippingTowel, OutfitCategories.Swimwear, true);
                    }
                    break;
            }
        }

        public static OutfitAssignment[] GetAllOutfitAssignments(this SimDescription simDescription)
        {
            return OutfitAssignments.FindAll(x => x.SimDescription == simDescription).ToArray();
        }

        public static string GetGlobalAssignedOutfitPrefix(this Sim sim)
        {
            return "OutfitAssignment_Global_" + OutfitUtils.GetAgePrefix(sim.SimDescription.Age, true) + OutfitUtils.GetGenderPrefix(sim.SimDescription.Gender) + "_";
        }

        public static void RemoveAllOutfitAssignments(this SimDescription simDescription, bool removeSpecialOutfits = false)
        {
            foreach (OutfitAssignment outfitAssignment in new List<OutfitAssignment>(OutfitAssignments))
            {
                if (outfitAssignment.SimDescription == simDescription)
                {
                    OutfitAssignments.Remove(outfitAssignment);
                    if (AssignedOutfits.ContainsKey(outfitAssignment.SpecialOutfitKey))
                    {
                        AssignedOutfits.Remove(outfitAssignment.SpecialOutfitKey);
                    }
                    if (removeSpecialOutfits && simDescription.HasSpecialOutfit(outfitAssignment.SpecialOutfitKey))
                    {
                        simDescription.RemoveSpecialOutfit(outfitAssignment.SpecialOutfitKey);
                    }
                }
            }
            sIndexedOutfitAssignments = null;
        }

        public static bool ShowPartOverridesDialog(AssignedOutfit assignedOutfit, out BodyTypes[] partOverrides, BodyTypes[] preSelectedPartOverrides = null)
        {
            try
            {
                const string localizationPath = Common.kLocalizationPath + "/Dialogs/PartOverrideListDialog";
                List<BodyTypes> partOverrideList = new List<BodyTypes>(preSelectedPartOverrides ?? assignedOutfit.PartOverrides.ToArray());
                bool cancelled, confirmed;
                while (true)
                {
                    List<BodyTypes> selectedPartOverrides = Dialogs.ObjectPickerDialog.Show(Responder.Instance.LocalizationModel.LocalizeString(localizationPath + ":Title"), new List<ObjectPicker.TabInfo>
                        {
                            new ObjectPicker.TabInfo("shop_all_r2", Responder.Instance.LocalizationModel.LocalizeString("Ui/Caption/ObjectPicker:All"), new List<BodyTypes>(OverridableBodyTypes).ConvertAll(x => new ObjectPicker.RowInfo(x, new List<ObjectPicker.ColumnInfo>())))
                        }, new List<Dialogs.ObjectPickerDialog.CommonHeaderInfo<BodyTypes>>
                        {
                            new BodyTypeColumn(localizationPath),
                            new PartOverrideEnabledColumn(localizationPath, partOverrideList.ToArray())
                        }, 1, out confirmed, out cancelled, true);
                    if (cancelled)
                    {
                        partOverrides = null;
                        return false;
                    }
                    if (confirmed)
                    {
                        partOverrides = partOverrideList.ToArray();
                        return true;
                    }
                    if (partOverrideList.Contains(selectedPartOverrides[0]))
                    {
                        partOverrideList.Remove(selectedPartOverrides[0]);
                    }
                    else
                    {
                        partOverrideList.Add(selectedPartOverrides[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                ((IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                partOverrides = null;
                return false;
            }
        }

        public static void SwitchToAssignedOutfit(this Sim sim, OutfitAssignment outfitAssignment, bool spin = true)
        {
            if (sim.BuffManager.HasElement(BuffNames.Singed) || sim.BuffManager.HasElement(BuffNames.SingedElectricity) || sim.BuffManager.HasElement(BuffNames.EmbarrassedClothesHidden) || sim.BuffManager.DisallowClothesChange() || sim.OccultManager.DisallowClothesChange())
            {
                return;
            }
            OutfitCategories outfitCategory;
            int outfitIndex;
            if (outfitAssignment.SpecialOutfitKey.StartsWith(OutfitAssignmentCategoryPrefix))
            {
                outfitCategory = (OutfitCategories)Enum.Parse(typeof(OutfitCategories), outfitAssignment.SpecialOutfitKey.Substring(OutfitAssignmentCategoryPrefix.Length));
                if (outfitCategory == 0)
                {
                    return;
                }
                outfitIndex = Tuning.kPickRandomOutfitIndex ? Sims3.Gameplay.Core.RandomUtil.GetInt(sim.SimDescription.GetOutfitCount(outfitCategory) - 1) : 0;
            }
            else
            {
                if (AssignedOutfits.ContainsKey(outfitAssignment.SpecialOutfitKey))
                {
                    sim.AddAssignedOutfit(outfitAssignment.SpecialOutfitKey);
                }
                outfitCategory = OutfitCategories.Special;
                outfitIndex = sim.SimDescription.GetSpecialOutfitIndexFromKey(ResourceUtils.HashString32(outfitAssignment.SpecialOutfitKey));
            }
            sim.SimDescription.CreateOutfitForCategoryIfNecessary(outfitCategory);
            if (spin && !(sim.Posture is SittingPosture))
            {
                sim.SwitchToOutfitWithSpin(outfitCategory, outfitIndex);
            }
            else
            {
                sim.SwitchToOutfitWithoutSpin(outfitCategory, outfitIndex);
            }
        }

        public static void SwitchToPreviousOutfit(this Sim sim, bool spin = true)
        {
            TimeToChangeBackList.Add(sim.SimDescription);
            int previousOutfitIndex = PreviousOutfits.FindIndex(x => x.SimDescription == sim.SimDescription);
            if (previousOutfitIndex > -1)
            {
                if (!sim.BuffManager.HasElement(BuffNames.Singed) && !sim.BuffManager.HasElement(BuffNames.SingedElectricity) && !sim.BuffManager.HasElement(BuffNames.EmbarrassedClothesHidden) && !sim.BuffManager.DisallowClothesChange() && !sim.OccultManager.DisallowClothesChange())
                {
                    if (spin && !(sim.Posture is SittingPosture))
                    {
                        sim.SwitchToOutfitWithSpin(PreviousOutfits[previousOutfitIndex].Category, PreviousOutfits[previousOutfitIndex].Index);
                    }
                    else
                    {
                        sim.SwitchToOutfitWithoutSpin(PreviousOutfits[previousOutfitIndex].Category, PreviousOutfits[previousOutfitIndex].Index);
                    }
                }
                PreviousOutfits.RemoveAt(previousOutfitIndex);
            }
            TimeToChangeBackList.RemoveAll(x => x == sim.SimDescription);
        }

        public static bool TryGetOutfitAssignment(this SimDescription simDescription, Sims3.Gameplay.Interactions.InteractionInstance interactionInstance, out OutfitAssignment outfitAssignment)
        {
            return simDescription.TryGetOutfitAssignment(interactionInstance.GetType(), out outfitAssignment);
        }

        public static bool TryGetOutfitAssignment(this SimDescription simDescription, Type interactionInstanceType, out OutfitAssignment outfitAssignment)
        {
            return IndexedOutfitAssignments.TryGetValue(interactionInstanceType.FullName + (simDescription == null ? "" : ("_" + simDescription.SimDescriptionId)), out outfitAssignment);
        }

        public static void UnassignOutfitToInteraction(this SimDescription simDescription, Type interactionInstanceType)
        {
            OutfitAssignment outfitAssignment;
            if (TryGetOutfitAssignment(simDescription, interactionInstanceType, out outfitAssignment))
            {
                OutfitAssignments.Remove(outfitAssignment);
                sIndexedOutfitAssignments = null;
            }
        }
    }
}
