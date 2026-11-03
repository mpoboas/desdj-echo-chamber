using System.Collections.Generic;
using UnityEngine;

namespace EchoChamber
{
    /// <summary>
    /// Procedural pose for a Kenney character rig (arms, legs, neck), ported from the prototype.
    /// Rotations are applied around axes expressed in the model root space.
    /// </summary>
    public class RigPose
    {
        private class BoneState
        {
            public Transform Bone;
            public Quaternion Base;
            public Vector3 AxisX;
            public Vector3 AxisZ;
        }

        private static readonly string[] BoneNames = { "LeftArm", "RightArm", "LeftUpLeg", "RightUpLeg", "Neck" };
        private readonly Dictionary<string, BoneState> bones = new Dictionary<string, BoneState>();

        /// <summary>Flip these if a rotation looks mirrored after the glTF X mirroring.</summary>
        public float SignX = 1f;
        public float SignZ = -1f;

        public RigPose(Transform root)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (System.Array.IndexOf(BoneNames, t.name) < 0 || bones.ContainsKey(t.name)) continue;
                Quaternion parentRel = Quaternion.Inverse(root.rotation) * t.parent.rotation;
                Quaternion inv = Quaternion.Inverse(parentRel);
                bones[t.name] = new BoneState
                {
                    Bone = t,
                    Base = t.localRotation,
                    AxisX = inv * Vector3.right,
                    AxisZ = inv * Vector3.forward
                };
            }
        }

        public bool HasBones => bones.Count > 0;

        public void Reset()
        {
            foreach (var b in bones.Values) b.Bone.localRotation = b.Base;
        }

        private void Turn(string name, bool zAxis, float angle)
        {
            if (!bones.TryGetValue(name, out var b) || Mathf.Approximately(angle, 0f)) return;
            Vector3 axis = zAxis ? b.AxisZ : b.AxisX;
            float sign = zAxis ? SignZ : SignX;
            b.Bone.localRotation = Quaternion.AngleAxis(angle * sign * Mathf.Rad2Deg, axis) * b.Bone.localRotation;
        }

        public enum Mode
        {
            Idle,
            Walk,
            Protest,
            Party,
            Phone,
            Tense
        }

        public void Apply(Mode mode, float time, float bob, bool fast)
        {
            Reset();
            float arm = 1.2f + Mathf.Sin(time / 0.9f + bob) * 0.04f;
            float swing = 0f, leg = 0f, neck = 0f;
            bool phone = false;
            float t = time * (fast ? 12f : 8.5f) + bob;
            switch (mode)
            {
                case Mode.Walk:
                    leg = Mathf.Sin(t) * 0.5f;
                    swing = Mathf.Sin(t) * 0.35f;
                    break;
                case Mode.Protest:
                    arm = -1.0f + Mathf.Sin(time / 0.17f + bob) * 0.3f;
                    break;
                case Mode.Party:
                    arm = -0.7f + Mathf.Sin(time / 0.14f + bob) * 0.45f;
                    break;
                case Mode.Phone:
                    phone = true;
                    neck = 0.45f;
                    break;
                case Mode.Tense:
                    arm = 1.35f;
                    neck = 0.25f;
                    break;
            }
            Turn("LeftArm", true, -arm);
            Turn("RightArm", true, arm);
            Turn("LeftArm", false, -swing);
            Turn("RightArm", false, swing);
            if (phone)
            {
                Turn("RightArm", false, -1.15f);
                Turn("LeftArm", false, -0.9f);
            }
            Turn("LeftUpLeg", false, leg);
            Turn("RightUpLeg", false, -leg);
            Turn("Neck", false, neck);
        }
    }
}
