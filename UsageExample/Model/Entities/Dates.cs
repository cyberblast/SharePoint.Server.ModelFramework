using cyberblast.SharePoint.Server.ModelFramework;
using cyberblast.SharePoint.Server.ModelFramework.Model;
using Microsoft.SharePoint;

namespace UsageExample.Model.Entities {
    public class Dates : ContentType {
        public Dates(ModelFactory schema, byte guidSuffix)
            : base(schema, guidSuffix) {
            ListCreated += OnListCreated;
        }

        public override SPContentTypeId ParentContentTypeId {
            get { return SPBuiltInContentTypeId.Event; }
        }

        public override SPListTemplateType ListTemplateType {
            get { return SPListTemplateType.Events; }
        }

        private void OnListCreated(ref SPList list) {
            SPFieldDateTime modified = list.Fields.GetFieldByInternalName("Modified") as SPFieldDateTime;
            modified.FriendlyDisplayFormat = SPDateTimeFieldFriendlyFormatType.Disabled;
            modified.Update();
        }
    }
}
