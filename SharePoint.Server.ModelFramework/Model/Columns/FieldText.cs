using Microsoft.SharePoint;
using System;

namespace cyberblast.SharePoint.Server.ModelFramework.Model
{
    public class FieldText : SiteColumn
    {
        public FieldText(ModelFactory parentSchema, string internalName)
            : base(parentSchema, internalName) 
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Text;            
        }
        public FieldText(ModelFactory parentSchema, string internalName, string displayName)
            : base(parentSchema, internalName, displayName)
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Text;
        }

        public int MaxLength = 255;

        public override void EnsureFieldConfiguration(ref SPWeb web, ref SPField field)
        {
            var typedField = field as SPFieldText;
            typedField.Title = DisplayName;
            typedField.Group = ParentSchema.GroupName;
            typedField.DefaultValue = DefaultValue;
            typedField.Required = Required;

            typedField.MaxLength = MaxLength;
            typedField.Update();
        }
    }
}
