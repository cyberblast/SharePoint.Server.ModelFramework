using System;
using System.Collections.Generic;
using Microsoft.SharePoint;

namespace cyberblast.SharePoint.Server.ModelFramework.Model
{
    public class FieldChoice : SiteColumn
    {
        public FieldChoice(ModelFactory parentSchema, string internalName)
            : base(parentSchema, internalName) 
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Choice;
        }
        public FieldChoice(ModelFactory parentSchema, string internalName, string displayName)
            : base(parentSchema, internalName, displayName)
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Choice;
        }

        public List<string> Choices;
        public SPChoiceFormatType EditFormat = SPChoiceFormatType.Dropdown;

        public override void EnsureFieldConfiguration(ref SPWeb web, ref SPField field)
        {
            var typedField = field as SPFieldChoice;
            typedField.Title = DisplayName;
            typedField.Group = ParentSchema.GroupName;
            typedField.DefaultValue = DefaultValue;
            typedField.Required = Required;

            typedField.Choices.Clear();
            typedField.Choices.AddRange(Choices.ToArray());
            typedField.EditFormat = EditFormat;
            typedField.Update();
        }
    }
}
