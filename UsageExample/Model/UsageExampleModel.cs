using cyberblast.SharePoint.Server.ModelFramework;
using cyberblast.SharePoint.Server.ModelFramework.Model;
using System.Collections.Generic;
using UsageExample.Model.Entities;

namespace UsageExample.Model {
    public class UsageExampleModel : ModelFactory {
        public override string GroupName {
            get { return "Sample"; }
        }
        public override string EntityGuidBase {
            get {
                return "AAAA0000AAAA0000AAAA00000000";
            }
        }

        public override List<ContentType> Entities {
            get {
                return new List<ContentType> {
                    new FileCategories(this, 0x01),
                    new Files(this, 0x02),
                    new News(this, 0x03),
                    new Dates(this, 0x03),
                    new WorkItems(this, 0x03)
                };
            }
        }
    }
}
