using cyberblast.SharePoint.Server.ModelFramework;
using cyberblast.SharePoint.Server.ModelFramework.Model;
using Microsoft.SharePoint;

namespace UsageExample.Model.Entities {
    public class WorkItems : ContentType {
        public WorkItems(ModelFactory schema, byte guidSuffix)
            : base(schema, guidSuffix) {
            ListCreated += OnListCreated;
        }

        public override SPContentTypeId ParentContentTypeId {
            get {
                return SPBuiltInContentTypeId.Task;
            }
        }

        public override SPListTemplateType ListTemplateType {
            get {
                return SPListTemplateType.TasksWithTimelineAndHierarchy;
            }
        }

        private void OnListCreated(ref SPList list) {
            SPFieldDateTime modified = list.Fields.GetFieldByInternalName("Modified") as SPFieldDateTime;
            modified.FriendlyDisplayFormat = SPDateTimeFieldFriendlyFormatType.Disabled;
            modified.Update();
        }
    }
}
