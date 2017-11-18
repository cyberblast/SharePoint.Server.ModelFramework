using System;
using System.Collections.Generic;
using Microsoft.SharePoint;

namespace cyberblast.SharePoint.Server.ModelFramework.Model
{
    public class FieldLookup : SiteColumn
    {
        public FieldLookup(ModelFactory parentSchema, string internalName)
            : base(parentSchema, internalName) 
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Lookup;
        }
        public FieldLookup(ModelFactory parentSchema, string internalName, string displayName)
            : base(parentSchema, internalName, displayName)
        {
            FieldType = Microsoft.SharePoint.SPFieldType.Lookup;
        }

        public bool AllowMultipleValues = false;
        public string LookupFieldInternalName = "Title";
        public string LookupListTitle;
        public SPRelationshipDeleteBehavior RelationshipDeleteBehavior = SPRelationshipDeleteBehavior.None;
        public List<FieldDependentLookup> DependentLookups = null;

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
                SPList lookupList = null;
                try
                {

                    lookupList = fields.Web.Lists[LookupListTitle];
                }
                catch (Exception e)
                {
                    //throw e;
                }
                if (lookupList != null)
                {
                    InternalName = fields.AddLookup(
                        InternalName,
                        lookupList.ID,
                        Required);
                }
                field = fields.GetFieldByInternalName(InternalName);
            }

            return field;
        }

        public override void EnsureFieldConfiguration(ref SPWeb web, ref SPField field)
        {
            SPList lookupList = null;
            try
            {
                lookupList = web.Lists[LookupListTitle];
            }
            catch (Exception e)
            {
                //throw e;
            }
            if (lookupList != null)
            {
                var typedField = field as SPFieldLookup;
                typedField.Title = DisplayName;
                typedField.Group = ParentSchema.GroupName;
                typedField.DefaultValue = DefaultValue;
                typedField.Required = Required;

                typedField.LookupField = LookupFieldInternalName;
                typedField.AllowMultipleValues = AllowMultipleValues;
                typedField.Update();

                if (string.IsNullOrEmpty(typedField.LookupList) || Guid.Parse(typedField.LookupList) != lookupList.ID)
                {
                    typedField.SchemaXml = ReplaceXmlAttributeValue(
                        ReplaceXmlAttributeValue(
                            typedField.SchemaXml,
                            "List",
                            lookupList.ID.ToString()),
                        "WebId",
                        web.ID.ToString());
                }
                typedField.Update(true);
                field = typedField;
            }
        }

        private static string ReplaceXmlAttributeValue(string xml, string attributeName, string value)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentNullException("xml");
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException("value");
            }


            int indexOfAttributeName = xml.IndexOf(attributeName, StringComparison.CurrentCultureIgnoreCase);
            if (indexOfAttributeName == -1)
            {
                throw new ArgumentOutOfRangeException("attributeName", string.Format("Attribute {0} not found in source xml", attributeName));
            }

            int indexOfAttibuteValueBegin = xml.IndexOf('"', indexOfAttributeName);
            int indexOfAttributeValueEnd = xml.IndexOf('"', indexOfAttibuteValueBegin + 1);

            return xml.Substring(0, indexOfAttibuteValueBegin + 1) + value + xml.Substring(indexOfAttributeValueEnd);
        }

        public string EnsureDependentLookup(ref SPList list, string parentFieldInternalName, string dependentFieldInternalName, string dependentFieldDisplayName, string lookupField, bool retry = true)
        {
            string createdInternalName = null;
            SPField parentField = list.Fields.TryGetFieldByStaticName(parentFieldInternalName);
            if (parentField != null)
            {
                //EnsureInternalFieldNameSchema(ref dependentFieldInternalName, dependentFieldDisplayName);
                try
                {
                    SPFieldLookup dependentField = (SPFieldLookup)list.Fields.TryGetFieldByStaticName(dependentFieldInternalName);
                    if (dependentField == null)
                    {
                        string depFieldInternalName = list.Fields.AddDependentLookup(dependentFieldInternalName, parentField.Id);
                        dependentField = (SPFieldLookup)list.Fields.GetFieldByInternalName(depFieldInternalName);
                    }
                    if (dependentField != null)
                    {
                        dependentField.Title = dependentFieldDisplayName;
                        dependentField.LookupField = lookupField;
                        dependentField.Update();
                        createdInternalName = dependentField.InternalName;
                    }
                }
                catch (System.IO.FileNotFoundException fnfEx)
                {
                    if (retry)
                    {
                        list.Update();
                        EnsureDependentLookup(ref list, parentFieldInternalName, dependentFieldInternalName, dependentFieldDisplayName, lookupField, false);
                    }
                    else throw fnfEx;
                }
            }
            return createdInternalName;
        }

    }
}
