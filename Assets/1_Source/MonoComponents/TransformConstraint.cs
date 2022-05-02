using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TeamAlpha.Source
{
    [ExecuteAlways]
    public class TransformConstraint : MonoBehaviour
    {
        [Serializable]
        public class Constraint
        {
            public enum Type { PositionX, PositionY, PositionZ, RotationX, RotationY, RotationZ, ScaleX, ScaleY, ScaleZ }

            public Type fromTargetValue;
            public Type toOwnerValue;

            public float multiplier;
            [HideInInspector]
            public float prevValueFrom;
        }
        [Required]
        public Transform target;

        public List<Constraint> constraints;
        public void LateUpdate()
        {
            if (target == null)
                return;
            foreach (Constraint c in constraints)
            {
                float valueFrom = 0f;
                if (c.fromTargetValue == Constraint.Type.PositionX)
                    valueFrom = target.transform.localPosition.x;
                else if (c.fromTargetValue == Constraint.Type.PositionY)
                    valueFrom = target.transform.localPosition.y;
                else if (c.fromTargetValue == Constraint.Type.PositionZ)
                    valueFrom = target.transform.localPosition.z;
                else if (c.fromTargetValue == Constraint.Type.RotationX)
                    valueFrom = target.transform.localEulerAngles.x;
                else if (c.fromTargetValue == Constraint.Type.RotationY)
                    valueFrom = target.transform.localEulerAngles.y;
                else if (c.fromTargetValue == Constraint.Type.RotationZ)
                    valueFrom = target.transform.localEulerAngles.z;
                else if (c.fromTargetValue == Constraint.Type.ScaleX)
                    valueFrom = target.transform.localScale.x;
                else if (c.fromTargetValue == Constraint.Type.ScaleY)
                    valueFrom = target.transform.localScale.y;
                else if (c.fromTargetValue == Constraint.Type.ScaleZ)
                    valueFrom = target.transform.localScale.z;

                if (c.prevValueFrom == valueFrom)
                    continue;
                float result = (valueFrom - c.prevValueFrom) * c.multiplier;
                c.prevValueFrom = valueFrom;

                if (c.toOwnerValue == Constraint.Type.PositionX)
                    transform.localPosition = new Vector3(transform.localPosition.x + result, transform.localPosition.y, transform.localPosition.z);
                else if (c.toOwnerValue == Constraint.Type.PositionY)
                    transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + result, transform.localPosition.z);
                else if (c.toOwnerValue == Constraint.Type.PositionZ)
                    transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z + result);
                else if (c.toOwnerValue == Constraint.Type.RotationX)
                    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x + result, transform.localEulerAngles.y, transform.localEulerAngles.z);
                else if (c.toOwnerValue == Constraint.Type.RotationY)
                    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y + result, transform.localEulerAngles.z);
                else if (c.toOwnerValue == Constraint.Type.RotationZ)
                    transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z + result);
                else if (c.toOwnerValue == Constraint.Type.ScaleX)
                    transform.localScale = new Vector3(transform.localScale.x + result, transform.localScale.y, transform.localScale.z);
                else if (c.toOwnerValue == Constraint.Type.ScaleY)
                    transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y + result, transform.localScale.z);
                else if (c.toOwnerValue == Constraint.Type.ScaleZ)
                    transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z + result);
            }
        }
    }
}
