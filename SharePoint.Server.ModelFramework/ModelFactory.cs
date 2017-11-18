using System;
using System.Collections.Generic;
using Microsoft.SharePoint;
using cyberblast.SharePoint.Server.ModelFramework.Model;

namespace cyberblast.SharePoint.Server.ModelFramework {
    public abstract class ModelFactory
    {
        public virtual string InternalFieldSchemaSeparator{ get { return "0"; } }
        public virtual string ContentTypeNameSchemaSeparator{ get { return "."; } }

        public abstract string GroupName { get; }
        public abstract List<ContentType> Entities { get; }
        public abstract string EntityGuidBase { get; }
        
        public virtual void Build(string webUrl)
        {
            foreach (ContentType iType in Entities)
            {
                SPContentTypeId myContentType = iType.EnsureContentType(webUrl);
                if (SPListTemplateType.NoListTemplate != iType.ListTemplateType)
                    iType.EnsureList(webUrl);
            }
        }
        
        public void Teardown(ref SPWeb web)
        {
            var types = Entities;

            //provision lists
            foreach (var iListDefinition in types)
            {
                try
                {
                    SPList list = web.Lists[iListDefinition.ListName];
                    list.Delete();
                    web.Update();
                }
                catch { }
                try
                {
                    SPContentType cType = web.ContentTypes[iListDefinition.Name];
                    if (cType != null)
                    {
                        cType.Delete();
                        web.Update();
                    }
                }
                catch { }
                try
                {
                    SPContentType cType = web.ContentTypes[iListDefinition.ContentTypeId];
                    if (cType != null)
                    {
                        cType.Delete();
                        web.Update();
                    }
                }
                catch { }
                foreach (var iColumn in iListDefinition.Columns)
                {
                    try
                    {
                        SPField field = web.Fields[iColumn.DisplayName];
                        field.Delete();
                    }
                    catch { }
                }
                web.Update();
            }

            List<SPField> deletables = new List<SPField>();
            foreach (SPField iField in web.Fields)
            {
                if (iField.Group == GroupName)
                    deletables.Add(iField);
            }
            foreach (var iField in deletables)
            {
                try
                {
                    iField.Delete();
                }
                catch { }
            }
            web.Update();
        }

    }
}
