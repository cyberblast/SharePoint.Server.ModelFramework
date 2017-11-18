using System;

namespace cyberblast.SharePoint.Server.ModelFramework
{
    public class FieldDependentLookup
    {
        ModelFactory ParentSchema;

        public FieldDependentLookup(ModelFactory parentSchema)
        {
            ParentSchema = parentSchema;
        }

        public string ParentFieldInternalName;
        public string Name;
        public string LookupField;

        private string _InternalName;
        public string InternalName
        {
            set
            {
                if (!value.StartsWith(string.Concat(ParentSchema.GroupName, ParentSchema.InternalFieldSchemaSeparator)))
                    _InternalName = string.Concat(ParentSchema.GroupName, ParentSchema.InternalFieldSchemaSeparator, value);
                else _InternalName = value;
            }
            get { return _InternalName; }
        }
    }
}
