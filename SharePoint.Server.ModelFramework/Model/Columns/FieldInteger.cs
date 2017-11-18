using System;

namespace cyberblast.SharePoint.Server.ModelFramework.Model
{
    public class FieldInteger : SiteColumn
    {
        public FieldInteger(ModelFactory parentSchema, string internalName)
            : base(parentSchema, internalName) 
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Integer;            
        }
        public FieldInteger(ModelFactory parentSchema, string internalName, string displayName)
            : base(parentSchema, internalName, displayName)
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Integer;
        }
    }
}
