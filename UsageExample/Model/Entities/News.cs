using cyberblast.SharePoint.Server.ModelFramework;
using cyberblast.SharePoint.Server.ModelFramework.Model;
using Microsoft.SharePoint;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace UsageExample.Model.Entities {
    public class News : ContentType {
        public News(ModelFactory schema, byte guidSuffix) 
            : base(schema, guidSuffix) {
            ListCreated += OnListCreated;
        }

        public override SPContentTypeId ParentContentTypeId {
            get {
                return SPBuiltInContentTypeId.Announcement;
            }
        }

        public override SPListTemplateType ListTemplateType {
            get {
                return SPListTemplateType.Announcements;
            }
        }

        private void OnListCreated(ref SPList list) {
            SPFieldDateTime modified = list.Fields.GetFieldByInternalName("Modified") as SPFieldDateTime;
            modified.FriendlyDisplayFormat = SPDateTimeFieldFriendlyFormatType.Disabled;
            modified.Update();
            SPFieldDateTime created = list.Fields.GetFieldByInternalName("Created") as SPFieldDateTime;
            created.FriendlyDisplayFormat = SPDateTimeFieldFriendlyFormatType.Disabled;
            created.Update();
        }

        public override List<ListView> Views {
            get {
                return new List<ListView> {
                    new ListView {
                        ViewName = "Overview",
                        MakeDefaultView = false,
                        Paged = true,
                        Query = "<OrderBy><FieldRef Name=\"Created\" Ascending=\"FALSE\" /></OrderBy><Where><Or><IsNull><FieldRef Name=\"Expires\" /></IsNull><Geq><FieldRef Name=\"Expires\" /><Value Type=\"DateTime\"><Today /></Value></Geq></Or></Where>",
                        RowLimit = 30,
                        ViewFields = new StringCollection {
                            "Created",
                            "LinkTitleNoMenu"
                        },
                        AllItemsWithoutFolders = true
                    }
                };
            }
        }
    }
}
