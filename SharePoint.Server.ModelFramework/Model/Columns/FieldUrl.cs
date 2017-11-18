using Microsoft.SharePoint;
using System;

namespace cyberblast.SharePoint.Server.ModelFramework.Model
{
    public class FieldUrl : SiteColumn
    {
        public FieldUrl(ModelFactory parentSchema, string internalName)
            : base(parentSchema, internalName) 
        {
            FieldType = Microsoft.SharePoint.SPFieldType.URL;            
        }
        public FieldUrl(ModelFactory parentSchema, string internalName, string displayName)
            : base(parentSchema, internalName, displayName)
        {
            FieldType = Microsoft.SharePoint.SPFieldType.URL;
        }

        public SPUrlFieldFormatType DisplayFormat = SPUrlFieldFormatType.Hyperlink;

        public override void EnsureFieldConfiguration(ref SPWeb web, ref SPField field)
        {
            SPFieldUrl typedField = field as SPFieldUrl;
            typedField.Title = DisplayName;
            typedField.Group = ParentSchema.GroupName;
            typedField.DefaultValue = DefaultValue;
            typedField.Required = Required;

            typedField.DisplayFormat = DisplayFormat;
            typedField.Update();
        }
    }
}
