﻿using System;
using System.Collections.Generic;
using System.Linq;
using Composite.C1Console.Actions;
using Composite.C1Console.Events;
using Composite.Data;
using Composite.Data.DynamicTypes;
using Composite.Data.Foundation;
using Composite.Data.GeneratedTypes;
using Composite.Core.ResourceSystem;
using Composite.Core.Types;
using Composite.Data.Types;
using Composite.Data.Validation.ClientValidationRules;
using Composite.C1Console.Workflow;


namespace Composite.Plugins.Elements.ElementProviders.GeneratedDataTypesElementProvider
{
    [EntityTokenLock()]
    [AllowPersistingWorkflow(WorkflowPersistingType.Idle)]
    public sealed partial class EditInterfaceTypeWorkflow : Composite.C1Console.Workflow.Activities.FormsWorkflow
    {
        public EditInterfaceTypeWorkflow()
        {
            InitializeComponent();
        }


        private DataTypeDescriptor GetDataTypeDescriptor()
        {
            Type type = GetTypeFromEntityToken();

            Guid guid = type.GetImmutableTypeId();

            return DataMetaDataFacade.GetDataTypeDescriptor(guid);
        }


        private Type GetTypeFromEntityToken()
        {
            GeneratedDataTypesElementProviderTypeEntityToken entityToken = (GeneratedDataTypesElementProviderTypeEntityToken)this.EntityToken;
            return TypeManager.GetType(entityToken.SerializedTypeName);

        }


        private void initialStateCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            DataTypeDescriptor dataTypeDescriptor = GetDataTypeDescriptor();

            Dictionary<string, object> bindings = new Dictionary<string, object>();

            GeneratedTypesHelper helper = new GeneratedTypesHelper(dataTypeDescriptor);

            List<DataFieldDescriptor> fieldDescriptors = helper.EditableDataFieldDescriptors.ToList();

            bindings.Add("TypeName", dataTypeDescriptor.Name);
            bindings.Add("TypeNamespace", dataTypeDescriptor.Namespace);
            bindings.Add("TypeTitle", dataTypeDescriptor.Title);
            bindings.Add("LabelFieldName", dataTypeDescriptor.LabelFieldName);
            bindings.Add("DataFieldDescriptors", fieldDescriptors);
            bindings.Add("HasCaching", helper.IsCachable);
            bindings.Add("HasPublishing", helper.IsPublishControlled);
            bindings.Add("HasLocalization", helper.IsLocalizedControlled);
            bindings.Add("EditProcessControlledAllowed", helper.IsEditProcessControlledAllowed);
            bindings.Add("OldTypeName", dataTypeDescriptor.Name);
            bindings.Add("OldTypeNamespace", dataTypeDescriptor.Namespace);

            this.UpdateBindings(bindings);

            this.BindingsValidationRules.Add("TypeName", new List<ClientValidationRule> { new NotNullClientValidationRule() });
            this.BindingsValidationRules.Add("TypeNamespace", new List<ClientValidationRule> { new NotNullClientValidationRule() });
            this.BindingsValidationRules.Add("TypeTitle", new List<ClientValidationRule> { new NotNullClientValidationRule() });
        }


        private Type GetOldTypeFromBindings()
        {
            string typeFullName = GetBinding<string>("OldTypeNamespace") + "." + GetBinding<string>("OldTypeName");

            return TypeManager.GetType(typeFullName);
        }

        private void finalizeStateCodeActivity_ExecuteCode(object sender, EventArgs e)
        {
            Type oldType = GetOldTypeFromBindings();

            string typeName = this.GetBinding<string>("TypeName");
            string typeNamespace = this.GetBinding<string>("TypeNamespace");
            string typeTitle = this.GetBinding<string>("TypeTitle");
            bool hasCaching = this.GetBinding<bool>("HasCaching");
            bool hasPublishing = this.GetBinding<bool>("HasPublishing");
            bool hasLocalization = this.GetBinding<bool>("HasLocalization");
            string labelFieldName = this.GetBinding<string>("LabelFieldName");
            List<DataFieldDescriptor> dataFieldDescriptors = this.GetBinding<List<DataFieldDescriptor>>("DataFieldDescriptors");

            GeneratedTypesHelper helper = new GeneratedTypesHelper(oldType);

            string errorMessage;
            if (helper.ValidateNewTypeName(typeName, out errorMessage) == false)
            {
                this.ShowFieldMessage("TypeName", errorMessage);
                return;
            }

            if (helper.ValidateNewTypeNamespace(typeNamespace, out errorMessage) == false)
            {
                this.ShowFieldMessage("TypeNamespace", errorMessage);
                return;
            }

            if (helper.ValidateNewTypeFullName(typeName, typeNamespace, out errorMessage) == false)
            {
                this.ShowFieldMessage("TypeName", errorMessage);
                return;
            }

            if (helper.ValidateNewFieldDescriptors(dataFieldDescriptors, out errorMessage) == false)
            {
                this.ShowMessage(
                        DialogType.Warning,
                        StringResourceSystemFacade.GetString("Composite.Plugins.GeneratedDataTypesElementProvider", "EditInterfaceTypeStep1.ErrorTitle"),
                        errorMessage
                    );
                return;
            }


            helper.SetNewTypeFullName(typeName, typeNamespace);
            helper.SetNewTypeTitle(typeTitle);
            helper.SetNewFieldDescriptors(dataFieldDescriptors, labelFieldName);

            if (helper.IsEditProcessControlledAllowed == true)
            {
                helper.SetCachable(hasCaching);
                helper.SetPublishControlled(hasPublishing);
                helper.SetLocalizedControlled(hasLocalization);
            }

            bool originalTypeHasData = DataFacade.HasDataInAnyScope(oldType);

            if (helper.TryValidateUpdate(originalTypeHasData, out errorMessage) == false)
            {
                this.ShowMessage(
                        DialogType.Warning,
                        StringResourceSystemFacade.GetString("Composite.Plugins.GeneratedDataTypesElementProvider", "EditInterfaceTypeStep1.ErrorTitle"),
                        errorMessage
                    );
                return;
            }

            string newOldDataTypeFullName = typeNamespace + "." + typeName;
            
            string oldSerializedTypeName = GetSerializedTypeName(GetBinding<string>("OldTypeNamespace"), GetBinding<string>("OldTypeName"));
            string newSerializedTypeName = GetSerializedTypeName(typeNamespace, typeName);
            if (newSerializedTypeName != oldSerializedTypeName)
            {
                UpdateWhiteListedGeneratedTypes(oldSerializedTypeName, newSerializedTypeName);
            }

            helper.CreateType(originalTypeHasData);

            UpdateBinding("OldTypeName", typeName);
            UpdateBinding("OldTypeNamespace", typeNamespace);

            SetSaveStatus(true);

            GeneratedDataTypesElementProviderRootEntityToken rootEntityToken = new GeneratedDataTypesElementProviderRootEntityToken(this.EntityToken.Source, GeneratedDataTypesElementProviderRootEntityToken.GlobalDataTypeFolderId);
            SpecificTreeRefresher specificTreeRefresher = this.CreateSpecificTreeRefresher();
            specificTreeRefresher.PostRefreshMesseges(rootEntityToken);
        }

        private static void UpdateWhiteListedGeneratedTypes(string oldTypeName, string newTypeName)
        {
            var referencesToOldTypeName = DataFacade.GetData<IGeneratedTypeWhiteList>(item => item.TypeManagerTypeName == oldTypeName).ToList();
            if (!referencesToOldTypeName.Any())
            {
                return;
            }

            if(!DataFacade.GetData<IGeneratedTypeWhiteList>(item => item.TypeManagerTypeName == newTypeName).Any())
            {
                var newWhiteListItem = DataFacade.BuildNew<IGeneratedTypeWhiteList>();
                newWhiteListItem.TypeManagerTypeName = newTypeName;

                DataFacade.AddNew(newWhiteListItem);
            }

            DataFacade.Delete<IGeneratedTypeWhiteList>(referencesToOldTypeName);
        }

        private static string GetSerializedTypeName(string typeNamespace, string typeName)
        {
            return "DynamicType:" + (typeName.Length == 0 ? typeName : typeNamespace + "." + typeName);
        }
    }
}
