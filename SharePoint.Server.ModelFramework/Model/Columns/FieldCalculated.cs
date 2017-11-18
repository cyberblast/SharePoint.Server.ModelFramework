using Microsoft.SharePoint;
using System;

namespace cyberblast.SharePoint.Server.ModelFramework.Model
{
    public class FieldCalculated : SiteColumn
    {
        public FieldCalculated(ModelFactory parentSchema, string internalName)
            : base(parentSchema, internalName) 
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Calculated;            
        }
        public FieldCalculated(ModelFactory parentSchema, string internalName, string displayName)
            : base(parentSchema, internalName, displayName)
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Calculated;
        }

        public string Formula;

        public override void EnsureFieldConfiguration(ref SPWeb web, ref SPField field)
        {
            var typedField = field as SPFieldCalculated;
            typedField.Title = DisplayName;
            typedField.Group = ParentSchema.GroupName;
            typedField.DefaultValue = DefaultValue;
            typedField.Required = Required;

            typedField.Formula = Formula;
            typedField.Update();
        }
    }
}
