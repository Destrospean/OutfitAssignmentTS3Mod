using Sims3.Gameplay.CAS;
using Sims3.SimIFace.CAS;
using Sims3.UI;

namespace Destrospean.OutfitAssignment.UI.Columns
{
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

    public class PartOverrideEnabledColumn : Dialogs.ObjectPickerDialog.CommonHeaderInfo<BodyTypes>
    {
        readonly string mLocalizationPath;

        readonly System.Collections.Generic.List<BodyTypes> mPartOverrides;

        public PartOverrideEnabledColumn(string localizationPath, BodyTypes[] partOverrides) : base(localizationPath + "/Headers/Enabled:Text", localizationPath + "/Headers/Enabled:Tooltip", 40)
        {
            mLocalizationPath = localizationPath;
            mPartOverrides = new System.Collections.Generic.List<BodyTypes>(partOverrides);
        }

        public override ObjectPicker.ColumnInfo GetValue(BodyTypes bodyType)
        {
            return new ObjectPicker.TextColumn(Responder.Instance.LocalizationModel.LocalizeString(mLocalizationPath + "/Options/Enabled:" + mPartOverrides.Contains(bodyType)));
        }
    }

    public class TextColumn : Dialogs.ObjectPickerDialog.CommonHeaderInfo<string>
    {
        public TextColumn(string localizationPath) : base(localizationPath + "/Header:Text", localizationPath + "/Header:Tooltip", 440)
        {
        }

        public override ObjectPicker.ColumnInfo GetValue(string text)
        {
            return new ObjectPicker.TextColumn(text ?? "");
        }
    }

    public class TypeColumn : Dialogs.ObjectPickerDialog.CommonHeaderInfo<System.Type>
    {
        public TypeColumn(string localizationPath) : base(localizationPath + "/Header:Text", localizationPath + "/Header:Tooltip", 440)
        {
        }

        public override ObjectPicker.ColumnInfo GetValue(System.Type type)
        {
            return new ObjectPicker.TextColumn(type == null ? "" : type.FullName);
        }
    }
}
