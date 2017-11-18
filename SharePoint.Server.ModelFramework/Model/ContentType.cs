using Microsoft.SharePoint;
using Microsoft.SharePoint.Publishing.Fields;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace cyberblast.SharePoint.Server.ModelFramework.Model
{
    public abstract class ContentType
    {
        protected ModelFactory ParentSchema;

        public ContentType() { }
        
        public ContentType(ModelFactory schema, byte id)
        {
            ParentSchema = schema;
            _Guid = string.Concat(schema.EntityGuidBase, "00", id.ToString("X2"));
        }

        public struct AlternateListTemplateDefinition
        {
            public string listTargetUrlSuffix;
            public string FeatureId;
            public string DocumentTempateId;
            public int ListTemplateId;
        }

        #region Events

        public delegate void OnListCreatedHandler(ref SPList list);

        public event OnListCreatedHandler ListCreated;

        public void InvokeOnListCreated(ref SPList list)
        {
            if (ListCreated != null) ListCreated(ref list);
        }

        public delegate void OnContentTypeCreatedHandler(ref SPContentType type);

        public event OnContentTypeCreatedHandler ContentTypeCreated;

        public void InvokeOnContentTypeCreated(ref SPContentType type)
        {
            if (ContentTypeCreated != null) ContentTypeCreated(ref type);
        }

        #endregion

        #region Template Fields

        private string _Guid;
        public virtual string UniqueId
        {
            get { return _Guid; }
        }

        public virtual string Name
        {
            get
            {
                return string.Concat(ParentSchema.GroupName, ParentSchema.ContentTypeNameSchemaSeparator, this.GetType().Name);
            }
        }

        public virtual string ListName
        {
            get
            {
                return this.GetType().Name;
            }
        }

        public virtual SPContentTypeId ContentTypeId
        {
            get
            {
                if (UpdatedId != SPContentTypeId.Empty) return UpdatedId;

                var parent = ParentContentTypeId;
                var id = string.Format("{0}00{1}", parent.ToString(), UniqueId);

                return new SPContentTypeId(id);
            }
        }

        public virtual SPContentTypeId ParentContentTypeId
        {
            get
            {
                return SPBuiltInContentTypeId.Item;
            }
        }

        public virtual SPListTemplateType ListTemplateType
        {
            get
            {
                return SPListTemplateType.GenericList;
            }
        }

        /// <summary>
        /// Will be used, if ListTemplateType = InvalidType
        /// </summary>
        public virtual AlternateListTemplateDefinition AlternateListTemplate
        {
            get
            {
                return new AlternateListTemplateDefinition();
            }
        }

        public virtual SiteColumn[] Columns
        {
            get
            {
                return new SiteColumn[0];
            }
        }

        public virtual List<Dictionary<string, object>> InitialListContent
        {
            get { return null; }
        }

        public virtual bool RemoveDefaultContentTypesOnListCreation
        {
            get { return true; }
        }

        public virtual List<ListView> Views { get { return new List<ListView>(); } }

        public virtual bool OnQuickLaunch
        {
            get { return false; }
        }

        public virtual bool RecreateListOnUpgrade
        {
            get { return false; }
        }

        public bool EnableModeration = false;
        public bool EnableVersioning = true;
        public bool EnableMinorVersions = false;
        public int MajorVersionLimit = 5;
        public int MajorWithMinorVersionsLimit = 1;
        public bool ForceCheckout = false;
        public DraftVisibilityType DraftVersionVisibility = DraftVisibilityType.Author;

        #endregion

        #region Provisioning

        private SPContentTypeId UpdatedId = SPContentTypeId.Empty;
        public void SetContentTypeId(SPContentTypeId id)
        {
            UpdatedId = id;
        }

        private List<string> AddedInternalNames = new List<string>();

        #region ContentType

        public SPContentTypeId EnsureContentType(string webUrl)
        {
            return EnsureContentType(webUrl, false);
        }
        public SPContentTypeId EnsureContentType(string webUrl, bool removeExcessiveFields)
        {
            SPContentTypeId contentTypeId = SPContentTypeId.Empty;
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                SPSite site = new SPSite(webUrl);
                SPWeb web = site.OpenWeb();
                try
                {
                    SPContentType cType = EnsureContentTypeExists(ref web);
                    EnsureContentTypeFieldsExist(ref web, ref cType);
                    cType.Update(true, false);

                    if (removeExcessiveFields)
                    {
                        ContentTypeRemoveExcessiveFields(ref cType);
                        cType.Update(true, false);
                    }

                    InvokeOnContentTypeCreated(ref cType);
                    contentTypeId = cType.Id;
                    SetContentTypeId(contentTypeId);
                }
                catch { throw; }
                finally
                {
                    web.Dispose();
                    site.Dispose();
                }
            });
            return contentTypeId;
        }

        private SPContentType EnsureContentTypeExists(ref SPWeb web)
        {
            SPContentType cType = null;
            try
            {
                cType = web.ContentTypes[ContentTypeId];
            }
            catch { /* expected: not found */ }

            if (cType == null)
            {
                int ctCounter = 0;
                string ctId = ContentTypeId.ToString();
                string ctName = Name;
                while (web.AvailableContentTypes[new SPContentTypeId(ctId)] != null && ctCounter < 100)
                {
                    ctCounter++;
                    ctId = ContentTypeId.ToString() + ctCounter.ToString("00");
                    ctName = string.Concat(Name, ParentSchema.ContentTypeNameSchemaSeparator, ctCounter);
                }

                SPContentType.ValidateName(ctName);
                cType = new SPContentType(new SPContentTypeId(ctId), web.ContentTypes, ctName);
                cType.Group = ParentSchema.GroupName;
                cType.ReadOnly = false;
                web.ContentTypes.Add(cType);

                //myContentType.Update();
            }

            return cType;
        }

        private void EnsureContentTypeFieldsExist(ref SPWeb web, ref SPContentType cType)
        {
            AddedInternalNames = new List<string>();
            SiteColumn[] columns = Columns;
            foreach (var iColumn in columns)
            {
                if (!iColumn.CreateAfterListCreation)
                {
                    //EnsureInternalFieldNameSchema(ref iColumn.InternalName, iColumn.DisplayName);
                    SPFieldCollection siteColumns = web.Fields;
                    SPFieldCollection siteCollectionColumns = web.AvailableFields;
                    SPField field = null;
                    if (siteColumns.ContainsFieldWithStaticName(iColumn.InternalName) || !siteCollectionColumns.ContainsFieldWithStaticName(iColumn.InternalName))
                    {
                        // exists defined locally or does not exist at all
                        field = iColumn.EnsureExists(ref siteColumns);
                    }
                    else if (siteCollectionColumns.ContainsFieldWithStaticName(iColumn.InternalName) && iColumn is FieldLookup && !(iColumn is FieldUser))
                    {
                        // if field is lookup create a local instance
                        field = iColumn.EnsureExists(ref siteColumns);
                        if (field != null && cType.Fields.ContainsField(iColumn.DisplayName))
                        {
                            cType.FieldLinks.Delete(cType.Fields.GetField(iColumn.DisplayName).InternalName);
                            //cType.Update();
                        }
                    }

                    if (field != null)
                    {
                        iColumn.EnsureFieldConfiguration(ref web, ref field);
                        iColumn.CallOnColumnCreated(ref field);
                        if (!cType.Fields.ContainsFieldWithStaticName(iColumn.InternalName))
                        {
                            cType.FieldLinks.Add(new SPFieldLink(field));
                        }
                    }
                }
                AddedInternalNames.Add(iColumn.InternalName);
            }
        }

        private void ContentTypeRemoveExcessiveFields(ref SPContentType cType)
        {
            SPContentType parentContentType = cType.Parent;
            SPFieldCollection fields = cType.Fields;
            List<string> removeFirst = new List<string>();
            List<string> removeSecond = new List<string>();

            foreach (SPField iField in fields)
            {
                if (!AddedInternalNames.Contains(iField.InternalName) &&
                    !parentContentType.Fields.ContainsFieldWithStaticName(iField.InternalName))
                {
                    if (iField.Type == SPFieldType.Lookup && (iField as SPFieldLookup).IsDependentLookup)
                        removeFirst.Add(iField.InternalName);
                    else removeSecond.Add(iField.InternalName);
                }
            }
            foreach (string iExcess in removeFirst)
            {
                cType.FieldLinks.Delete(iExcess);
            }
            foreach (string iExcess in removeSecond)
            {
                cType.FieldLinks.Delete(iExcess);
            }
        }

        #endregion

        #region List

        public void EnsureList(string webUrl)
        {
            EnsureList(webUrl, false);
        }
        public void EnsureList(string webUrl, bool removeExcessiveFields)
        {
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                AddedInternalNames = new List<string>();
                SPSite site = new SPSite(webUrl);
                SPWeb web = site.OpenWeb();
                try
                {
                    SPContentType cType = web.ContentTypes[ContentTypeId];

                    if (cType != null)
                    {
                        Guid listId = EnsureListExists(ref web);
                        SPList list = web.Lists.GetList(listId, false);
                        EnsureListContentTypes(ref list, ref cType);
                        EnsureListConfiguration(ref list);

                        #region reload context

                        web.Update();

                        web.Dispose();
                        //try
                        //{
                        //    //list.ParentWeb.Dispose();
                        //}
                        //catch { }
                        site.Dispose();
                        list = null;

                        site = new SPSite(webUrl);
                        web = site.OpenWeb();
                        cType = web.ContentTypes[ContentTypeId];
                        list = web.Lists.TryGetList(ListName);

                        #endregion

                        EnsureListDependentLookupFields(ref list);
                        EnsureContentTypeDelayedFields(ref web, ref cType);

                        #region reload context

                        web.Update();

                        web.Dispose();
                        //try
                        //{
                        //    list.ParentWeb.Dispose();
                        //}
                        //catch { }
                        site.Dispose();
                        list = null;

                        site = new SPSite(webUrl);
                        web = site.OpenWeb();
                        cType = web.ContentTypes[ContentTypeId];
                        list = web.Lists.TryGetList(ListName);

                        #endregion

                        if (removeExcessiveFields)
                        {
                            ListRemoveExcessiveFields(ref list, ref cType);
                        }
                        InvokeOnListCreated(ref list);

                        #region reload context

                        web.Update();

                        web.Dispose();
                        //try
                        //{
                        //    list.ParentWeb.Dispose();
                        //}
                        //catch { }
                        site.Dispose();
                        list = null;

                        site = new SPSite(webUrl);
                        web = site.OpenWeb();
                        cType = web.ContentTypes[ContentTypeId];
                        list = web.Lists.TryGetList(ListName);

                        #endregion

                        ListPopulateContent(ref list, web.Url);
                        EnsureListViews(ref list, ref web);

                        web.Update();
                        //try
                        //{
                        //    list.ParentWeb.Dispose();
                        //}
                        //catch { }
                        list = null;
                    }
                }
                catch { throw; }
                finally
                {
                    web.Dispose();
                    site.Dispose();
                }
            });
        }

        private Guid EnsureListExists(ref SPWeb web)
        {
            SPList list = null;
            try
            {
                list = web.Lists.TryGetList(ListName);
            }
            catch { /* expected: list doesn't exist */ }

            if (RecreateListOnUpgrade && list != null)
            {
                list.Delete();
                web.Update();
                list = null;
            }

            if (list == null)
            {
                Guid listId;
                if (ListTemplateType != SPListTemplateType.InvalidType)
                {
                    listId = web.Lists.Add(ListName,
                        string.Empty,
                        ListTemplateType);
                }
                else
                {
                    listId = web.Lists.Add(ListName,
                        string.Empty,
                        AlternateListTemplate.listTargetUrlSuffix,
                        AlternateListTemplate.FeatureId,
                        AlternateListTemplate.ListTemplateId,
                        AlternateListTemplate.DocumentTempateId);
                }
                // Write the changes to database
                web.Update();

                return listId;
            }
            else return list.ID;
        }

        private void EnsureListContentTypes(ref SPList list, ref SPContentType cType)
        {
            #region Ensure List has Content Type

            SPContentType listContenType = null;
            try
            {
                listContenType = list.ContentTypes[list.GetDirectChildContentType(cType.Id)];
            }
            catch { /* expected: not yet contained */ }

            if (listContenType == null)
            {
                list.ContentTypesEnabled = true;
                listContenType = list.ContentTypes.Add(cType);
                list.Update();                
            }

            #endregion

            #region Remove unwanted Content Types

            if (RemoveDefaultContentTypesOnListCreation)
            {
                List<SPContentTypeId> deletionIds = new List<SPContentTypeId>();
                bool someLeft = false;
                string prefix = string.Concat(ParentSchema.GroupName, ParentSchema.ContentTypeNameSchemaSeparator);
                foreach (SPContentType iType in list.ContentTypes)
                {
                    if (!iType.Name.StartsWith(prefix) && iType.Name != listContenType.Name)
                    {
                        deletionIds.Add(iType.Id);
                    }
                    else someLeft = true;
                }

                if (someLeft)
                {
                    foreach (SPContentTypeId iId in deletionIds)
                        list.ContentTypes.Delete(iId);
                    list.Update();
                }
            }

            #endregion
        }

        private void EnsureListConfiguration(ref SPList list)
        {
            list.OnQuickLaunch = OnQuickLaunch;
            list.EnableModeration = EnableModeration;
            if(EnableModeration)
                list.DraftVersionVisibility = DraftVersionVisibility;
            list.EnableVersioning = EnableVersioning;
            if(EnableVersioning)
                list.MajorVersionLimit = MajorVersionLimit;

            if (this.ParentContentTypeId == SPBuiltInContentTypeId.Document || this.ParentContentTypeId.IsChildOf(SPBuiltInContentTypeId.Document))
            {
                list.EnableMinorVersions = EnableMinorVersions;
                if (EnableMinorVersions)
                    list.MajorWithMinorVersionsLimit = MajorWithMinorVersionsLimit;
                list.ForceCheckout = ForceCheckout;
            }
            list.Update();
        }

        private void EnsureListDependentLookupFields(ref SPList list)
        {
            SiteColumn[] columns = Columns;
            foreach (SiteColumn iColumn in columns)
            {
                if (iColumn is FieldLookup)
                {
                    FieldLookup lookup = iColumn as FieldLookup;
                    if (lookup.DependentLookups != null)
                    {
                        foreach (FieldDependentLookup iDependent in lookup.DependentLookups)
                        {
                            SPField parentField = list.Fields.TryGetFieldByStaticName(lookup.InternalName);
                            if (parentField == null) parentField = list.Fields.GetField(lookup.DisplayName);

                            if (parentField != null)
                            {
                                string internalName = lookup.EnsureDependentLookup(ref list, parentField.InternalName, iDependent.InternalName, iDependent.Name, iDependent.LookupField);
                                if (internalName != null)
                                    AddedInternalNames.Add(internalName);
                            }
                        }
                    }
                }
            }

            //foreach (FieldDependentLookup iDependent in DependentLookups)
            //{
            //    string createdInternalName = EnsureDependentLookup(ref list, iDependent.ParentFieldInternalName, iDependent.InternalName,
            //        iDependent.Name, iDependent.LookupField);
            //    if (createdInternalName != null)
            //        AddedInternalNames.Add(createdInternalName);
            //}
        }

        private void EnsureContentTypeDelayedFields(ref SPWeb web, ref SPContentType cType)
        {
            //eg for lookup on self
            bool updateCt = false;
            SiteColumn[] columns = Columns;
            foreach (var iColumn in columns)
            {
                if (iColumn.CreateAfterListCreation)
                {
                    SPFieldCollection siteColumns = web.Fields;
                    SPField field = iColumn.EnsureExists(ref siteColumns);
                    if (field != null)
                    {
                        iColumn.EnsureFieldConfiguration(ref web, ref field);
                        iColumn.CallOnColumnCreated(ref field);
                        if (!cType.Fields.ContainsFieldWithStaticName(iColumn.InternalName))
                        {
                            cType.FieldLinks.Add(new SPFieldLink(field));
                        }
                    }
                    updateCt = true;
                }
                AddedInternalNames.Add(iColumn.InternalName);
            }
            if (updateCt) 
                cType.Update(true, false);
        }

        private void ListRemoveExcessiveFields(ref SPList list, ref SPContentType cType)
        {
            SPFieldCollection fields = list.Fields;
            List<string> removeFirst = new List<string>();
            List<string> removeSecond = new List<string>();
            foreach (SPField iField in fields)
            {
                if (!iField.FromBaseType &&
                    iField.SourceId != "http://schemas.microsoft.com/sharepoint/v3" &&
                    !AddedInternalNames.Contains(iField.InternalName) &&
                    !cType.Fields.ContainsFieldWithStaticName(iField.StaticName))
                {
                    if (iField.Type == SPFieldType.Lookup && (iField as SPFieldLookup).IsDependentLookup)
                        removeFirst.Add(iField.InternalName);
                    else removeSecond.Add(iField.InternalName);
                }
            }

            foreach (string iExcess in removeFirst)
            {
                fields.Delete(iExcess);
            }
            if (removeFirst.Count > 0 ) list.Update();
            foreach (string iExcess in removeSecond)
            {
                fields.Delete(iExcess);
            }
            list.Update();
        }

        private void ListPopulateContent(ref SPList list, string webUrl)
        {
            //only if list empty
            if (InitialListContent != null &&
                InitialListContent.Count > 0 &&
                list.ItemCount == 0)
            {
                foreach (Dictionary<string, object> iRow in InitialListContent)
                {
                    SPListItem newItem = list.AddItem();
                    foreach (KeyValuePair<string, object> iField in iRow)
                    {
                        if (list.Fields.ContainsField(iField.Key) && list.Fields.GetField(iField.Key).Type == SPFieldType.Lookup)
                        {
                            SPFieldLookup lkp = (SPFieldLookup)list.Fields.GetField(iField.Key);
                            newItem[iField.Key] = GetLookupValue(webUrl, lkp, (string)iField.Value);
                        }
                        else 
                            newItem[iField.Key] = iField.Value;
                    }
                    newItem.Update();
                }
            }
        }

        private SPFieldLookupValue GetLookupValue(string siteUrl, SPFieldLookup field, string displayValue)
        {
            SPFieldLookupValue value = new SPFieldLookupValue();
            using(SPSite site = new SPSite(siteUrl))
            using(SPWeb web = site.OpenWeb(field.LookupWebId))
            {
                SPList lkpList = web.Lists.GetList(new Guid(field.LookupList), false);
                SPQuery query = new SPQuery();
                query.Query = string.Format("<Where><Eq><FieldRef Name=\"Title\" /><Value Type=\"Text\">{0}</Value></Eq></Where>", displayValue);
                SPListItemCollection items = lkpList.GetItems(query);

                if( items != null && items.Count > 0)
                {
                    value = new SPFieldLookupValue(items[items.Count-1].ID, displayValue);
                }
            }
            return value;
        }

        private void EnsureListViews(ref SPList list, ref SPWeb web)
        {
            if (Views != null && Views.Count > 0)
            {
                foreach (ListView iView in Views)
                {
                    SPView view = null;
                    try
                    {
                        if (iView.DefaultView)
                            view = list.DefaultView;
                        else
                            view = list.Views[iView.ViewName];
                    }
                    catch { view = null; /* as expected */ }

                    if (view == null)
                    {
                        #region Create configured View

                        view = list.Views.Add(iView.ViewName, iView.ViewFields, iView.Query, iView.RowLimit,
                            iView.Paged, iView.MakeDefaultView, iView.ViewType, false);

                        #endregion
                    }
                    else
                    {
                        #region Reconfigure View

                        view.ViewFields.DeleteAll();
                        foreach (var iField in iView.ViewFields)
                        {
                            view.ViewFields.Add(iField);
                        }

                        view.Query = iView.Query;
                        view.RowLimit = iView.RowLimit;
                        view.Paged = iView.Paged;
                        view.Title = iView.ViewName;
                        if( iView.MakeDefaultView )
                            view.DefaultView = iView.MakeDefaultView;

                        #endregion
                    }
                    if (iView.ViewStyle >= 0)
                        view.ApplyStyle(web.ViewStyles.StyleByID(iView.ViewStyle));
                    
                    // set additional flags
                    view.Hidden = iView.Hidden;
                    view.Scope = iView.AllItemsWithoutFolders ? SPViewScope.Recursive : SPViewScope.Default;
                    view.TabularView = iView.TabularView;

                    object[] setToolbarParam = new object[1];
                    if (iView.ToolbarType == ToolBarType.Summary)
                        setToolbarParam[0] = "Freeform";
                    else if (iView.ToolbarType == ToolBarType.Full)
                        setToolbarParam[0] = "Standard";
                    else setToolbarParam[0] = "None";
                    if (setToolbarParam[0] as string != view.ToolbarType)
                    {
                        Type[] toolbarMethodParamTypes = { Type.GetType("System.String") };
                        MethodInfo setToolbarTypeMethod = view.GetType().GetMethod("SetToolbarType", BindingFlags.Instance | BindingFlags.NonPublic, null, toolbarMethodParamTypes, null);
                        setToolbarTypeMethod.Invoke(view, setToolbarParam);
                    }

                    view.Update();
                }
                list.Update();
            }
        }

        #endregion

        #endregion

        #region EventReceivers

        public void RegisterItemEventHandler(ref SPList list, Type receiverClassType, SPEventReceiverType receiverType, int sequence, SPEventReceiverSynchronization sync)
        {
            SPEventReceiverDefinition def = GetEventReceiver(ref list, receiverClassType, receiverType);
            if (def == null)
            {
                def = list.EventReceivers.Add();

                def.Assembly = receiverClassType.Assembly.FullName;//"ERDefinition, Version=1.0.0.0, Culture=Neutral, PublicKeyToken=704f58d28567dc00";
                def.Class = receiverClassType.Name; // "ERDefinition.ItemEvents";
                def.Name = receiverClassType.Name + "_" + receiverType.ToString();//ItemAdded Event";
                def.Type = receiverType;
            }
            def.SequenceNumber = sequence;
            def.Synchronization = sync;
            def.Update();
        }

        private SPEventReceiverDefinition GetEventReceiver(ref SPList list, Type receiverClassType, SPEventReceiverType receiverType)
        {
            SPEventReceiverDefinition def = null;
            foreach (SPEventReceiverDefinition iDef in list.EventReceivers)
            {
                if (iDef.Assembly == receiverClassType.Assembly.FullName &&
                    iDef.Class == receiverClassType.Name &&
                    iDef.Type == receiverType)
                {
                    def = iDef;
                    break;
                }
            }
            return def;
        }
                
        #endregion
    }
}
