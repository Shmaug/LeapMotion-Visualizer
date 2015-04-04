using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Leap;

namespace LeapMotion_Visualization
{
    static class Util
    {
        public static Vector3 toWorld(Vector vector, bool slideDown=true)
        {
            return Vector3.Transform(new Vector3(vector.x, vector.y - (slideDown ? 200 : 0), vector.z) / 50f, Main.main.camera.rotationMatrix);
        }
        public static Vector3 toWorldNoTransform(Vector vector)
        {
            return new Vector3(vector.x, vector.y, vector.z) / 50f;
        }

        public static Vector3 QuaternionToEuler(Quaternion rotation)
        {
            float q0 = rotation.W;
            float q1 = rotation.Y;
            float q2 = rotation.X;
            float q3 = rotation.Z;
            Vector3 radAngles = new Vector3();
            radAngles.X = (float)Math.Atan2(2f * (q0 * q1 + q2 * q3), 1f - 2f * (Math.Pow(q1, 2) + Math.Pow(q2, 2)));
            radAngles.Y = (float)Math.Asin(2f * (q0 * q2 - q3 * q1));
            radAngles.Z = (float)Math.Atan2(2f * (q0 * q3 + q1 * q2), 1f - 2f * (Math.Pow(q2, 2) + Math.Pow(q3, 2)));
            Vector3 angles = new Vector3();
            angles.X = MathHelper.ToDegrees(radAngles.X);
            angles.Y = MathHelper.ToDegrees(radAngles.Y);
            angles.Z = MathHelper.ToDegrees(radAngles.Z);
            return angles;
        }

        public static VertexPositionColor[] visualizeHands(Frame frame)
        {
            List<VertexPositionColor> v = new List<VertexPositionColor>();
            foreach (Hand hand in frame.Hands)
            {
                Arm arm = hand.Arm;
                Vector3 elbow = Util.toWorldNoTransform(arm.ElbowPosition);
                Vector3 armwrist = Util.toWorldNoTransform(arm.WristPosition);
                Vector3 palm = Util.toWorldNoTransform(hand.PalmPosition);
                Vector3 wrist = Util.toWorldNoTransform(hand.WristPosition);

                v.Add(new VertexPositionColor(elbow, Color.Yellow));
                v.Add(new VertexPositionColor(armwrist, Color.Blue));

                v.Add(new VertexPositionColor(palm, Color.White));
                v.Add(new VertexPositionColor(wrist, Color.Violet));

                Vector3 lastmcp = default(Vector3);

                foreach (Finger finger in hand.Fingers)
                {
                    Vector3 mcp = Util.toWorldNoTransform(finger.JointPosition(Finger.FingerJoint.JOINT_MCP));
                    Vector3 pip = Util.toWorldNoTransform(finger.JointPosition(Finger.FingerJoint.JOINT_PIP));
                    Vector3 dip = Util.toWorldNoTransform(finger.JointPosition(Finger.FingerJoint.JOINT_DIP));
                    Vector3 tip = Util.toWorldNoTransform(finger.JointPosition(Finger.FingerJoint.JOINT_TIP));

                    VertexPositionColor p1 = new VertexPositionColor(mcp, Color.Red);
                    VertexPositionColor p2 = new VertexPositionColor(pip, Color.Blue);
                    VertexPositionColor p3 = new VertexPositionColor(dip, Color.Green);
                    VertexPositionColor p4 = new VertexPositionColor(tip, Color.Yellow);

                    if (lastmcp != Vector3.Zero)
                    {
                        VertexPositionColor p0 = new VertexPositionColor(lastmcp, Color.Red);
                        v.Add(p0);
                        v.Add(p1);
                    }

                    v.Add(p1);
                    v.Add(p2);

                    v.Add(p2);
                    v.Add(p3);

                    v.Add(p3);
                    v.Add(p4);

                    lastmcp = mcp;
                }
            }
            return v.ToArray();
        }
    }
}
