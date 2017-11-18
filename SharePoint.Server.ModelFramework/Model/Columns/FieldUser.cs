using System;
using System.Collections.Generic;
using Microsoft.SharePoint;

namespace cyberblast.SharePoint.Server.ModelFramework.Model
{
    public class FieldUser : FieldLookup
    {
        public FieldUser(ModelFactory parentSchema, string internalName)
            : base(parentSchema, internalName) 
        {
            FieldType = Microsoft.SharePoint.SPFieldType.User;            
        }
        public FieldUser(ModelFactory parentSchema, string internalName, string displayName)
            : base(parentSchema, internalName, displayName)
        {
            FieldType = Microsoft.SharePoint.SPFieldType.User;
        }

        public SPFieldUserSelectionMode SelectionMode = SPFieldUserSelectionMode.PeopleOnly;

        public override SPField EnsureExists(ref SPFieldCollection fields)
        {
            SPField field = null;
            try
            {
                field = fields.TryGetFieldByStaticName(InternalName);
            }
            catch
            {
                // field doesn't exist
            }

            if (field == null || !(field.Group.ToLower().Equals(ParentSchema.GroupName.ToLower())))
            {
                InternalName = fields.Add(InternalName, FieldType, Required);
                field = fields.GetFieldByInternalName(InternalName);
            }

            return field;
        }

        public override void EnsureFieldConfiguration(ref SPWeb web, ref SPField field)
        {
            var typedField = field as SPFieldUser;
            typedField.Title = DisplayName;
            typedField.Group = ParentSchema.GroupName;
            typedField.DefaultValue = DefaultValue;
            typedField.Required = Required;

            typedField.SelectionMode = SelectionMode;
            typedField.AllowMultipleValues = AllowMultipleValues;
            typedField.Update();
        }
    }
}
