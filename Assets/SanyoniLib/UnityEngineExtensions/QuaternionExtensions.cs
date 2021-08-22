using UnityEngine;


namespace SanyoniLib.UnityEngineExtensions
{

    public static class QuaternionExtensions
    {

        public static Vector3 GetForwardVector(this Quaternion me)
        {
            return (me * Vector3.forward).normalized;
        }

        public static Vector3 GetRightVector(this Quaternion me)
        {
            return (me * Quaternion.AngleAxis(90f, Vector3.up) * Vector3.forward).normalized;
        }

        public static Vector3 GetUpVector(this Quaternion me)
        {
            return (me * Quaternion.AngleAxis(-90f, Vector3.right) * Vector3.forward).normalized;
        }

        public static Vector3 GetForwardVectorXZ(this Quaternion me)
        {
            return new Vector3(me.eulerAngles.x, 0f, me.eulerAngles.z).normalized;
        }

        public static Vector3 GetRightVectorXZ(this Quaternion me)
        {
            Vector3 rightVector = me.GetRightVector();
            return new Vector3(rightVector.x, 0f, rightVector.z).normalized;
        }

    }

}