using cyberblast.SharePoint.Server.ModelFramework;
using cyberblast.SharePoint.Server.ModelFramework.Model;
using Microsoft.SharePoint;

namespace UsageExample.Model.Entities {
    public class Files : ContentType {
        public Files(ModelFactory schema, byte guidSuffix)
            : base(schema, guidSuffix) {
            ListCreated += OnListCreated;
        }

        public override SPContentTypeId ParentContentTypeId {
            get {
                return SPBuiltInContentTypeId.Document;
            }
        }

        public override SPListTemplateType ListTemplateType {
            get {
                return SPListTemplateType.DocumentLibrary;
            }
        }

        public override SiteColumn[] Columns {
            get {
                return new SiteColumn[] {
                    new FieldLookup(ParentSchema, "Category") {
                        LookupListTitle = "FileCategories", 
                        LookupFieldInternalName = "Title"
                    },
                    new FieldNote(ParentSchema, "Comment") {
                        RichText = false
                    }
                };
            }
        }
        
        private void OnListCreated(ref SPList list) {
            SPFieldDateTime modified = list.Fields.GetFieldByInternalName("Modified") as SPFieldDateTime;
            modified.FriendlyDisplayFormat = SPDateTimeFieldFriendlyFormatType.Disabled;
            modified.Update();
            
            list.EnableVersioning = true;
            list.EnableMinorVersions = true;
            list.MajorVersionLimit = 10;
            list.MajorWithMinorVersionsLimit = 3;
            list.DraftVersionVisibility = DraftVisibilityType.Author;
            list.EnableModeration = false;
            list.ForceCheckout = true;
            
            list.Update();
        }
    }
}
