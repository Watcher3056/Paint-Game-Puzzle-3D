using System;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities;
using UnityEngine;
using UnityEditor;

[assembly: RegisterValidator(typeof(TeamAlpha.Source.Editor.ComponentGUIDValidator))]
namespace TeamAlpha.Source.Editor
{

#pragma warning disable
    public class ComponentGUIDValidator : Validator
    {
        public override RevalidationCriteria RevalidationCriteria => RevalidationCriteria.OnValueChange;
        public override bool CanValidateValues()
        {
            return true;
        }
        public override bool CanValidateMembers()
        {
            return false;
        }
        public override bool CanValidateMember(MemberInfo member, Type memberValueType)
        {
            return false;
        }
        public override bool CanValidateProperty(InspectorProperty property)
        {
            return true;
        }
        public override void RunValidation(ref ValidationResult result)
        {
            if (result == null)
                result = new ValidationResult();

            result.Setup = new ValidationSetup()
            {
                Kind = ValidationKind.Value,
                Validator = this,
                Value = this.Property.ValueEntry.WeakSmartValue,
                Root = Property.SerializationRoot.ValueEntry.WeakSmartValue,
            };

            result.ResultType = ValidationResultType.Valid;
            result.Message = "";

            ComponentGUID cGUID = this.Property.ValueEntry.WeakSmartValue as ComponentGUID;
            if (cGUID == null)
            {
                result.ResultType = ValidationResultType.IgnoreResult;
                return;
            }
            if (cGUID.sourceType == ComponentGUID.IdSourceType.Random)
            {
                cGUID.UpdateGUID();
                result.ResultType = ValidationResultType.Valid;
                return;
            }

            UnityEngine.Object _obj = Property.SerializationRoot.ValueEntry.WeakSmartValue as UnityEngine.Object;
            string assetPath = _obj.GetAssetPath();
            if (assetPath == null || assetPath == string.Empty)
            {
                result.ResultType = ValidationResultType.IgnoreResult;
                return;
            }
            else
            {
                string assetGUID = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);


                if (!assetGUID.Equals(cGUID.id))
                {
                    cGUID.prefabId = assetGUID;
                    if (cGUID.sourceType == ComponentGUID.IdSourceType.Prefab)
                        cGUID.id = assetGUID;

                    EditorUtility.SetDirty(_obj);
                }
                result.ResultType = ValidationResultType.Valid;
            }
        }
    }
#pragma warning enable
}
