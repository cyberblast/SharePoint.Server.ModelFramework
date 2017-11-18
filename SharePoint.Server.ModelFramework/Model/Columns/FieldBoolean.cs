using Microsoft.SharePoint;
using System;

namespace cyberblast.SharePoint.Server.ModelFramework.Model
{
    public class FieldBoolean : SiteColumn
    {
        public FieldBoolean(ModelFactory parentSchema, string internalName)
            : base(parentSchema, internalName) 
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Boolean;            
        }
        public FieldBoolean(ModelFactory parentSchema, string internalName, string displayName)
            : base(parentSchema, internalName, displayName)
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Boolean;
        }        
    }
}
