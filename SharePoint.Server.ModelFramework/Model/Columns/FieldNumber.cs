using System;
using System.Collections.Generic;
using Microsoft.SharePoint;

namespace cyberblast.SharePoint.Server.ModelFramework.Model
{
    public class FieldNumber : SiteColumn
    {
        public FieldNumber(ModelFactory parentSchema, string internalName)
            : base(parentSchema, internalName) 
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Number;            
        }
        public FieldNumber(ModelFactory parentSchema, string internalName, string displayName)
            : base(parentSchema, internalName, displayName)
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Number;
        }

        public SPNumberFormatTypes DisplayFormat = SPNumberFormatTypes.Automatic;

        public override void EnsureFieldConfiguration(ref SPWeb web, ref SPField field)
        {
            var typedField = field as SPFieldNumber;
            typedField.Title = DisplayName;
            typedField.Group = ParentSchema.GroupName;
            typedField.DefaultValue = DefaultValue;
            typedField.Required = Required;

            typedField.DisplayFormat = DisplayFormat;
            typedField.Update();
        }
    }
}
