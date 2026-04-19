using System;
using System.Collections.Generic;
using Sims3.UI;
using Tuning = Sims3.Gameplay.Destrospean.OutfitAssignment;

namespace Destrospean.OutfitAssignment
{
    public static class InteractionInstanceTypeUtils
    {
        static Type[] sInteractionInstanceTypes;

        const string kDialogLocalizationPath = Common.kLocalizationPath + "/Dialogs/InteractionListDialog";

        public static Type[] InteractionInstanceTypes
        {
            get
            {
                if (sInteractionInstanceTypes == null)
                {
                    InitInteractionInstanceTypes();
                }
                return sInteractionInstanceTypes;
            }
        }

        public enum CallbackTypes
        {
            Never,
            InteractionStarted,
            InteractionEnded,
            StandardEntry,
            StandardExit
        }

        class TextColumn : Dialogs.ObjectPickerDialog.CommonHeaderInfo<string>
        {
            public TextColumn() : base(kDialogLocalizationPath + "/Header:Text", kDialogLocalizationPath + "/Header:Tooltip", 40)
            {
            }

            public override ObjectPicker.ColumnInfo GetValue(string text)
            {
                return new ObjectPicker.TextColumn(text ?? "");
            }
        }

        class TypeColumn : Dialogs.ObjectPickerDialog.CommonHeaderInfo<Type>
        {
            public TypeColumn() : base(kDialogLocalizationPath + "/Header:Text", kDialogLocalizationPath + "/Header:Tooltip", 40)
            {
            }

            public override ObjectPicker.ColumnInfo GetValue(Type type)
            {
                return new ObjectPicker.TextColumn(type == null ? "" : type.FullName);
            }
        }

        public static void InitInteractionInstanceTypes()
        {
            List<Type> interactionInstanceTypes = new List<Type>();
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                interactionInstanceTypes.AddRange(Array.FindAll(assembly.GetTypes(), x => typeof(Sims3.Gameplay.Interactions.InteractionInstance).IsAssignableFrom(x) && x.IsClass && !x.IsAbstract));
            }
            sInteractionInstanceTypes = interactionInstanceTypes.ToArray();
        }

        public static bool TryGetSelectedInteractionInstanceTypes(out Type[] selectedInteractionInstanceTypes, Type[] allInteractionInstanceTypes = null)
        {
            try
            {
                allInteractionInstanceTypes = allInteractionInstanceTypes ?? InteractionInstanceTypes;
                Array.Sort(allInteractionInstanceTypes, (a, b) => a.FullName.CompareTo(b.FullName));
                int remainder = allInteractionInstanceTypes.Length;
                string[] pageLabels = new string[(int)Math.Ceiling((double)remainder / Tuning.kInteractionInstanceTypesPerPage)];
                for (int i = 0; i < pageLabels.Length; i++)
                {
                    pageLabels[i] = "[" + allInteractionInstanceTypes[i * Tuning.kInteractionInstanceTypesPerPage].FullName + "..." + allInteractionInstanceTypes[i * Tuning.kInteractionInstanceTypesPerPage + (remainder < Tuning.kInteractionInstanceTypesPerPage ? remainder : Tuning.kInteractionInstanceTypesPerPage) - 1].FullName + "]";
                    remainder -= Tuning.kInteractionInstanceTypesPerPage;
                }
                bool confirmed;
                List<string> selectedPageLabels = Dialogs.ObjectPickerDialog.Show(Responder.Instance.LocalizationModel.LocalizeString(kDialogLocalizationPath + ":Title"), new List<ObjectPicker.TabInfo>
                    {
                        new ObjectPicker.TabInfo("shop_all_r2", Responder.Instance.LocalizationModel.LocalizeString("Ui/Caption/ObjectPicker:All"), new List<string>(pageLabels).ConvertAll(x => new ObjectPicker.RowInfo(x, new List<ObjectPicker.ColumnInfo>())))
                    }, new List<Dialogs.ObjectPickerDialog.CommonHeaderInfo<string>>
                    {
                        new TextColumn()
                    }, 1, out confirmed);
                if (!confirmed || selectedPageLabels == null || selectedPageLabels.Count == 0)
                {
                    selectedInteractionInstanceTypes = null;
                    return false;
                }
                int lowerBound = Array.FindIndex(allInteractionInstanceTypes, x => x.FullName == selectedPageLabels[0].Remove(selectedPageLabels[0].IndexOf("...")).Replace("[", "")),
                upperBound = Array.FindIndex(allInteractionInstanceTypes, x => x.FullName == selectedPageLabels[0].Substring(selectedPageLabels[0].IndexOf("...") + 3).Replace("]", ""));
                selectedInteractionInstanceTypes = (Dialogs.ObjectPickerDialog.Show(Responder.Instance.LocalizationModel.LocalizeString(kDialogLocalizationPath + ":Title"), new List<ObjectPicker.TabInfo>
                    {
                        new ObjectPicker.TabInfo("shop_all_r2", "[" + lowerBound + "..." + upperBound + "]", new List<Type>(allInteractionInstanceTypes).GetRange(lowerBound, upperBound - lowerBound + 1).ConvertAll(x => new ObjectPicker.RowInfo(x, new List<ObjectPicker.ColumnInfo>())))
                    }, new List<Dialogs.ObjectPickerDialog.CommonHeaderInfo<Type>>
                    {
                        new TypeColumn()
                    }, int.MaxValue, out confirmed) ?? new List<Type>()).ToArray();
                if (!confirmed || selectedInteractionInstanceTypes.Length == 0)
                {
                    return TryGetSelectedInteractionInstanceTypes(out selectedInteractionInstanceTypes, allInteractionInstanceTypes);
                }
                return true;
            }
            catch (Exception ex)
            {
                ((Sims3.SimIFace.IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                selectedInteractionInstanceTypes = null;
                return false;
            }
        }
    }
}
