using System;
using System.Collections.Generic;
using Microsoft.SharePoint;

namespace cyberblast.SharePoint.Server.ModelFramework.Model
{
    public class FieldNote : SiteColumn
    {
        public FieldNote(ModelFactory parentSchema, string internalName)
            : base(parentSchema, internalName) 
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Note;            
        }
        public FieldNote(ModelFactory parentSchema, string internalName, string displayName)
            : base(parentSchema, internalName, displayName)
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Note;
        }

        public bool RichText = false;
        public SPRichTextMode RichTextMode = SPRichTextMode.FullHtml;

        public override void EnsureFieldConfiguration(ref SPWeb web, ref SPField field)
        {
            var typedField = field as SPFieldMultiLineText;
            typedField.Title = DisplayName;
            typedField.Group = ParentSchema.GroupName;
            typedField.DefaultValue = DefaultValue;
            typedField.Required = Required;

            typedField.RichText = RichText;
            typedField.RichTextMode = RichTextMode;
            typedField.Update();
        }

    }
}
