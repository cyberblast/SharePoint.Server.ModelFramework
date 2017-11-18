using System.Collections.Generic;
using Microsoft.SharePoint;

namespace cyberblast.SharePoint.Server.ModelFramework.Model
{
    public abstract class SiteColumn
    {
        public SiteColumn(ModelFactory parentSchema, string internalName)
        {
            ParentSchema = parentSchema;
            DisplayName = internalName;
            InternalName = internalName;
        }
        public SiteColumn(ModelFactory parentSchema, string internalName, string displayName)
        {
            ParentSchema = parentSchema;
            DisplayName = displayName;
            InternalName = internalName;
        }
        protected ModelFactory ParentSchema;
        public string DisplayName;
        private string _InternalName;
        public string InternalName
        {
            set 
            {
                if (!value.StartsWith(string.Concat(ParentSchema.GroupName, ParentSchema.InternalFieldSchemaSeparator)))
                    _InternalName = string.Concat(ParentSchema.GroupName, ParentSchema.InternalFieldSchemaSeparator, value);
                else _InternalName = value; 
            }
            get { return _InternalName;  }
        }
        protected SPFieldType FieldType = SPFieldType.Error;
        public bool CreateAfterListCreation = false; // e.g. for LookUp Column referencing same list
        public bool Required = false;
        public string DefaultValue = null;
        public bool Hidden = false;
        
        #region Events

        public delegate void ColumnCreatedHandler(ref SPField newField);
        public event ColumnCreatedHandler _OnColumnCreated;

        public ColumnCreatedHandler OnColumnCreated
        {
            set { _OnColumnCreated = value; }
        }

        public void CallOnColumnCreated(ref SPField field)
        {
            if (_OnColumnCreated != null)
                _OnColumnCreated(ref field);
        }

        #endregion

        #region Provisioning

        public virtual SPField EnsureExists(ref SPFieldCollection fields)
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

        public virtual void EnsureFieldConfiguration(ref SPWeb web, ref SPField field)
        {
            EnsureCommonFieldConfiguration(ref field);
            field.Update();
        }

        protected void EnsureCommonFieldConfiguration(ref SPField field)
        {
            field.Title = DisplayName;
            field.Group = ParentSchema.GroupName;
            field.DefaultValue = DefaultValue;
            field.Required = Required;
        }

        #endregion

    }
}
