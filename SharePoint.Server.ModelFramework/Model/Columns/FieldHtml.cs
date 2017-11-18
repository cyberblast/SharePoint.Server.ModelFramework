using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing.Fields;

namespace cyberblast.SharePoint.Server.ModelFramework.Model
{
    public class FieldHtml : SiteColumn
    {
        public FieldHtml(ModelFactory parentSchema, string internalName)
            : base(parentSchema, internalName) 
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Invalid;            
        }
        public FieldHtml(ModelFactory parentSchema, string internalName, string displayName)
            : base(parentSchema, internalName, displayName)
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Invalid;
        }

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
                HtmlField htmlField = new HtmlField(fields, "HTML", InternalName);
                InternalName = fields.Add(htmlField);

                field = fields.GetFieldByInternalName(InternalName);
            }

            return field;
        }
    }
}
