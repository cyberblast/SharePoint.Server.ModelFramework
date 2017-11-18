using System.Collections.Generic;
using Microsoft.SharePoint;
using cyberblast.SharePoint.Server.ModelFramework;
using cyberblast.SharePoint.Server.ModelFramework.Model;

namespace UsageExample.Model.Entities {
    public class FileCategories : ContentType {
        public FileCategories(ModelFactory schema, byte guidSuffix)
            : base(schema, guidSuffix) { }

        public override SPContentTypeId ParentContentTypeId {
            get {
                return SPBuiltInContentTypeId.Item;
            }
        }

        public override SPListTemplateType ListTemplateType {
            get {
                return SPListTemplateType.GenericList;
            }
        }

        public override SiteColumn[] Columns {
            get {
                return new SiteColumn[] {
                    new FieldText(ParentSchema, "abbr", "Abbreviation")
                };
            }
        }

        public override List<Dictionary<string, object>> InitialListContent {
            get {
                return new List<Dictionary<string, object>> {
                    new Dictionary<string, object> {
                        {"Title", "Confidential"},
                        {"Abbreviation", "C"}
                    },
                    new Dictionary<string, object> {
                        {"Title", "Secret"},
                        {"Abbreviation", "S"}
                    },
                    new Dictionary<string, object> {
                        {"Title", "Top Secret"},
                        {"Abbreviation", "TS"}
                    },
                    new Dictionary<string, object> {
                        {"Title", "Super Top Secret"},
                        {"Abbreviation", "STS"}
                    }
                };
            }
        }
    }
}
