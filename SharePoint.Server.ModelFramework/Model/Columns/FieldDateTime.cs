using System;
using Microsoft.SharePoint;

namespace cyberblast.SharePoint.Server.ModelFramework.Model
{
    public class FieldDateTime : SiteColumn
    {
        public FieldDateTime(ModelFactory parentSchema, string internalName)
            : base(parentSchema, internalName) 
        {
            FieldType = Microsoft.SharePoint.SPFieldType.DateTime;            
        }
        public FieldDateTime(ModelFactory parentSchema, string internalName, string displayName)
            : base(parentSchema, internalName, displayName)
        {
            FieldType = Microsoft.SharePoint.SPFieldType.DateTime;
        }

        public SPDateTimeFieldFormatType DisplayFormat = SPDateTimeFieldFormatType.DateOnly;
        public SPDateTimeFieldFriendlyFormatType FriendlyDisplayFormatType = SPDateTimeFieldFriendlyFormatType.Disabled;

        public override void EnsureFieldConfiguration(ref SPWeb web, ref SPField field)
        {
            var typedField = field as SPFieldDateTime;
            typedField.Title = DisplayName;
            typedField.Group = ParentSchema.GroupName;
            typedField.DefaultValue = DefaultValue;
            typedField.Required = Required;

            typedField.DisplayFormat = DisplayFormat;
            typedField.FriendlyDisplayFormat = FriendlyDisplayFormatType;
            typedField.Update();
        }
    }
}
