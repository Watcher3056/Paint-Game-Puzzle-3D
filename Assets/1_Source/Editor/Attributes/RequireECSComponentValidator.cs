using System;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities;
using UnityEngine;
using Pixeye.Actors;

[assembly: RegisterValidator(typeof(TeamAlpha.Source.Editor.RequireECSComponentValidator))]
namespace TeamAlpha.Source.Editor
{

#pragma warning disable
    public class RequireECSComponentValidator : Validator
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
        public override void RunValueValidation(object value, UnityEngine.Object root, ref ValidationResult result)
        {
            if (result == null)
                result = new ValidationResult();

            result.Setup = new ValidationSetup()
            {
                Kind = ValidationKind.Value,
                Validator = this,
                Value = value,
                Root = root,
            };

            result.ResultValue = null;
            result.ResultType = ValidationResultType.Valid;
            result.Message = "";

            Actor actor = (root as Actor);
            if (actor == null)
            {
                result.ResultType = ValidationResultType.IgnoreResult;
                return;
            }
            Type[] requireTypes = value?.GetType().GetCustomAttribute<RequireECSComponentAttribute>()?.filter;
            if (requireTypes == null)
            {
                result.ResultType = ValidationResultType.IgnoreResult;
                return;
            }
            if (requireTypes.Length == 0)
            {
                result.ResultType = ValidationResultType.IgnoreResult;
                return;
            }
            FieldInfo[] fields = actor.GetType().GetFields();

            bool found = false;

            for (int i = 0; i < requireTypes.Length; i++)
            {
                for (int j = 0; j < fields.Length; j++)
                {
                    if (requireTypes[i] == fields[j].FieldType)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    result.Message += "Component: " + value.GetType().FullName + "\n" +
                        " Dependce on: " + requireTypes[i].FullName + "\n" +
                        " But not found in: " + actor.GetType().FullName;
                    result.ResultType = ValidationResultType.Error;
                }
            }
        }
    }
#pragma warning enable
}
