namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using TrueSync;

    internal static class WorldXmlSerializer
    {
        private static XmlWriter _writer;

        private static int FindIndex(List<Body> list, Body item)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == item)
                {
                    return i;
                }
            }
            return -1;
        }

        private static int FindIndex(List<Fixture> list, Fixture item)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].CompareTo(item))
                {
                    return i;
                }
            }
            return -1;
        }

        private static int FindIndex(List<TrueSync.Physics2D.Shape> list, TrueSync.Physics2D.Shape item)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].CompareTo(item))
                {
                    return i;
                }
            }
            return -1;
        }

        private static string Join<T>(string separator, IEnumerable<T> values)
        {
            using (IEnumerator<T> enumerator = values.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    return string.Empty;
                }
                StringBuilder builder = new StringBuilder();
                if (enumerator.Current > null)
                {
                    T current = enumerator.Current;
                    string str2 = current.ToString();
                    if (str2 > null)
                    {
                        builder.Append(str2);
                    }
                }
                while (enumerator.MoveNext())
                {
                    builder.Append(separator);
                    if (enumerator.Current > null)
                    {
                        string str3 = enumerator.Current.ToString();
                        if (str3 > null)
                        {
                            builder.Append(str3);
                        }
                    }
                }
                return builder.ToString();
            }
        }

        internal static void Serialize(TrueSync.Physics2D.World world, Stream stream)
        {
            List<Body> bodies = new List<Body>();
            List<Fixture> fixtures = new List<Fixture>();
            List<TrueSync.Physics2D.Shape> shapes = new List<TrueSync.Physics2D.Shape>();
            XmlWriterSettings settings = new XmlWriterSettings {
                Indent = true,
                NewLineOnAttributes = false,
                OmitXmlDeclaration = true
            };
            _writer = XmlWriter.Create(stream, settings);
            _writer.WriteStartElement("World");
            _writer.WriteAttributeString("Version", "3");
            WriteElement("Gravity", world.Gravity);
            _writer.WriteStartElement("Shapes");
            foreach (Body body in world.BodyList)
            {
                using (List<Fixture>.Enumerator enumerator2 = body.FixtureList.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        Fixture fixture = enumerator2.Current;
                        if (!Enumerable.Any<TrueSync.Physics2D.Shape>(shapes, (Func<TrueSync.Physics2D.Shape, bool>) (s2 => fixture.Shape.CompareTo(s2))))
                        {
                            SerializeShape(fixture.Shape);
                            shapes.Add(fixture.Shape);
                        }
                    }
                }
            }
            _writer.WriteEndElement();
            _writer.WriteStartElement("Fixtures");
            foreach (Body body2 in world.BodyList)
            {
                using (List<Fixture>.Enumerator enumerator4 = body2.FixtureList.GetEnumerator())
                {
                    while (enumerator4.MoveNext())
                    {
                        Fixture fixture = enumerator4.Current;
                        if (!Enumerable.Any<Fixture>(fixtures, (Func<Fixture, bool>) (f2 => fixture.CompareTo(f2))))
                        {
                            SerializeFixture(fixture);
                            fixtures.Add(fixture);
                        }
                    }
                }
            }
            _writer.WriteEndElement();
            _writer.WriteStartElement("Bodies");
            foreach (Body body3 in world.BodyList)
            {
                bodies.Add(body3);
                SerializeBody(fixtures, shapes, body3);
            }
            _writer.WriteEndElement();
            _writer.WriteStartElement("Joints");
            foreach (TrueSync.Physics2D.Joint joint in world.JointList)
            {
                SerializeJoint(bodies, joint);
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();
            _writer.Flush();
            _writer.Close();
        }

        private static void SerializeBody(List<Fixture> fixtures, List<TrueSync.Physics2D.Shape> shapes, Body body)
        {
            _writer.WriteStartElement("Body");
            _writer.WriteAttributeString("Type", body.BodyType.ToString());
            _writer.WriteElementString("Active", body.Enabled.ToString());
            _writer.WriteElementString("AllowSleep", body.SleepingAllowed.ToString());
            _writer.WriteElementString("Angle", body.Rotation.ToString());
            _writer.WriteElementString("AngularDamping", body.AngularDamping.ToString());
            _writer.WriteElementString("AngularVelocity", body.AngularVelocity.ToString());
            _writer.WriteElementString("Awake", body.Awake.ToString());
            _writer.WriteElementString("Bullet", body.IsBullet.ToString());
            _writer.WriteElementString("FixedRotation", body.FixedRotation.ToString());
            _writer.WriteElementString("LinearDamping", body.LinearDamping.ToString());
            WriteElement("LinearVelocity", body.LinearVelocity);
            WriteElement("Position", body.Position);
            if (body.UserData > null)
            {
                _writer.WriteStartElement("UserData");
                WriteDynamicType(body.UserData.GetType(), body.UserData);
                _writer.WriteEndElement();
            }
            _writer.WriteStartElement("Bindings");
            for (int i = 0; i < body.FixtureList.Count; i++)
            {
                _writer.WriteStartElement("Pair");
                _writer.WriteAttributeString("FixtureId", FindIndex(fixtures, body.FixtureList[i]).ToString());
                _writer.WriteAttributeString("ShapeId", FindIndex(shapes, body.FixtureList[i].Shape).ToString());
                _writer.WriteEndElement();
            }
            _writer.WriteEndElement();
            _writer.WriteEndElement();
        }

        private static void SerializeFixture(Fixture fixture)
        {
            _writer.WriteStartElement("Fixture");
            _writer.WriteAttributeString("Id", fixture.FixtureId.ToString());
            _writer.WriteStartElement("FilterData");
            _writer.WriteElementString("CategoryBits", ((int) fixture.CollisionCategories).ToString());
            _writer.WriteElementString("MaskBits", ((int) fixture.CollidesWith).ToString());
            _writer.WriteElementString("GroupIndex", fixture.CollisionGroup.ToString());
            _writer.WriteElementString("CollisionIgnores", Join<int>("|", fixture._collisionIgnores));
            _writer.WriteEndElement();
            _writer.WriteElementString("Friction", fixture.Friction.ToString());
            _writer.WriteElementString("IsSensor", fixture.IsSensor.ToString());
            _writer.WriteElementString("Restitution", fixture.Restitution.ToString());
            if (fixture.UserData > null)
            {
                _writer.WriteStartElement("UserData");
                WriteDynamicType(fixture.UserData.GetType(), fixture.UserData);
                _writer.WriteEndElement();
            }
            _writer.WriteEndElement();
        }

        private static void SerializeJoint(List<Body> bodies, TrueSync.Physics2D.Joint joint)
        {
            _writer.WriteStartElement("Joint");
            _writer.WriteAttributeString("Type", joint.JointType.ToString());
            WriteElement("BodyA", FindIndex(bodies, joint.BodyA));
            WriteElement("BodyB", FindIndex(bodies, joint.BodyB));
            WriteElement("CollideConnected", joint.CollideConnected);
            WriteElement("Breakpoint", joint.Breakpoint);
            if (joint.UserData > null)
            {
                _writer.WriteStartElement("UserData");
                WriteDynamicType(joint.UserData.GetType(), joint.UserData);
                _writer.WriteEndElement();
            }
            switch (joint.JointType)
            {
                case JointType.Revolute:
                {
                    RevoluteJoint joint7 = (RevoluteJoint) joint;
                    WriteElement("EnableLimit", joint7.LimitEnabled);
                    WriteElement("EnableMotor", joint7.MotorEnabled);
                    WriteElement("LocalAnchorA", joint7.LocalAnchorA);
                    WriteElement("LocalAnchorB", joint7.LocalAnchorB);
                    WriteElement("LowerAngle", joint7.LowerLimit);
                    WriteElement("MaxMotorTorque", joint7.MaxMotorTorque);
                    WriteElement("MotorSpeed", joint7.MotorSpeed);
                    WriteElement("ReferenceAngle", joint7.ReferenceAngle);
                    WriteElement("UpperAngle", joint7.UpperLimit);
                    break;
                }
                case JointType.Prismatic:
                {
                    TrueSync.Physics2D.PrismaticJoint joint5 = (TrueSync.Physics2D.PrismaticJoint) joint;
                    WriteElement("EnableLimit", joint5.LimitEnabled);
                    WriteElement("EnableMotor", joint5.MotorEnabled);
                    WriteElement("LocalAnchorA", joint5.LocalAnchorA);
                    WriteElement("LocalAnchorB", joint5.LocalAnchorB);
                    WriteElement("Axis", joint5.Axis);
                    WriteElement("LowerTranslation", joint5.LowerLimit);
                    WriteElement("UpperTranslation", joint5.UpperLimit);
                    WriteElement("MaxMotorForce", joint5.MaxMotorForce);
                    WriteElement("MotorSpeed", joint5.MotorSpeed);
                    break;
                }
                case JointType.Distance:
                {
                    DistanceJoint joint2 = (DistanceJoint) joint;
                    WriteElement("DampingRatio", joint2.DampingRatio);
                    WriteElement("FrequencyHz", joint2.Frequency);
                    WriteElement("Length", joint2.Length);
                    WriteElement("LocalAnchorA", joint2.LocalAnchorA);
                    WriteElement("LocalAnchorB", joint2.LocalAnchorB);
                    break;
                }
                case JointType.Pulley:
                {
                    PulleyJoint joint6 = (PulleyJoint) joint;
                    WriteElement("WorldAnchorA", joint6.WorldAnchorA);
                    WriteElement("WorldAnchorB", joint6.WorldAnchorB);
                    WriteElement("LengthA", joint6.LengthA);
                    WriteElement("LengthB", joint6.LengthB);
                    WriteElement("LocalAnchorA", joint6.LocalAnchorA);
                    WriteElement("LocalAnchorB", joint6.LocalAnchorB);
                    WriteElement("Ratio", joint6.Ratio);
                    WriteElement("Constant", joint6.Constant);
                    break;
                }
                case JointType.Gear:
                    throw new Exception("Gear joint not supported by serialization");

                case JointType.Wheel:
                {
                    WheelJoint joint4 = (WheelJoint) joint;
                    WriteElement("EnableMotor", joint4.MotorEnabled);
                    WriteElement("LocalAnchorA", joint4.LocalAnchorA);
                    WriteElement("LocalAnchorB", joint4.LocalAnchorB);
                    WriteElement("MotorSpeed", joint4.MotorSpeed);
                    WriteElement("DampingRatio", joint4.DampingRatio);
                    WriteElement("MaxMotorTorque", joint4.MaxMotorTorque);
                    WriteElement("FrequencyHz", joint4.Frequency);
                    WriteElement("Axis", joint4.Axis);
                    break;
                }
                case JointType.Weld:
                {
                    WeldJoint joint8 = (WeldJoint) joint;
                    WriteElement("LocalAnchorA", joint8.LocalAnchorA);
                    WriteElement("LocalAnchorB", joint8.LocalAnchorB);
                    break;
                }
                case JointType.Friction:
                {
                    FrictionJoint joint3 = (FrictionJoint) joint;
                    WriteElement("LocalAnchorA", joint3.LocalAnchorA);
                    WriteElement("LocalAnchorB", joint3.LocalAnchorB);
                    WriteElement("MaxForce", joint3.MaxForce);
                    WriteElement("MaxTorque", joint3.MaxTorque);
                    break;
                }
                case JointType.Rope:
                {
                    RopeJoint joint9 = (RopeJoint) joint;
                    WriteElement("LocalAnchorA", joint9.LocalAnchorA);
                    WriteElement("LocalAnchorB", joint9.LocalAnchorB);
                    WriteElement("MaxLength", joint9.MaxLength);
                    break;
                }
                case JointType.Motor:
                {
                    MotorJoint joint11 = (MotorJoint) joint;
                    WriteElement("AngularOffset", joint11.AngularOffset);
                    WriteElement("LinearOffset", joint11.LinearOffset);
                    WriteElement("MaxForce", joint11.MaxForce);
                    WriteElement("MaxTorque", joint11.MaxTorque);
                    WriteElement("CorrectionFactor", joint11.CorrectionFactor);
                    break;
                }
                case JointType.Angle:
                {
                    AngleJoint joint10 = (AngleJoint) joint;
                    WriteElement("BiasFactor", joint10.BiasFactor);
                    WriteElement("MaxImpulse", joint10.MaxImpulse);
                    WriteElement("Softness", joint10.Softness);
                    WriteElement("TargetAngle", joint10.TargetAngle);
                    break;
                }
                default:
                    throw new Exception("Joint not supported");
            }
            _writer.WriteEndElement();
        }

        private static void SerializeShape(TrueSync.Physics2D.Shape shape)
        {
            _writer.WriteStartElement("Shape");
            _writer.WriteAttributeString("Type", shape.ShapeType.ToString());
            _writer.WriteAttributeString("Density", shape.Density.ToString());
            switch (shape.ShapeType)
            {
                case ShapeType.Circle:
                {
                    CircleShape shape2 = (CircleShape) shape;
                    _writer.WriteElementString("Radius", shape2.Radius.ToString());
                    WriteElement("Position", shape2.Position);
                    break;
                }
                case ShapeType.Edge:
                {
                    EdgeShape shape4 = (EdgeShape) shape;
                    WriteElement("Vertex1", shape4.Vertex1);
                    WriteElement("Vertex2", shape4.Vertex2);
                    break;
                }
                case ShapeType.Polygon:
                {
                    PolygonShape shape3 = (PolygonShape) shape;
                    _writer.WriteStartElement("Vertices");
                    foreach (TSVector2 vector in shape3.Vertices)
                    {
                        WriteElement("Vertex", vector);
                    }
                    _writer.WriteEndElement();
                    WriteElement("Centroid", shape3.MassData.Centroid);
                    break;
                }
                case ShapeType.Chain:
                {
                    ChainShape shape5 = (ChainShape) shape;
                    _writer.WriteStartElement("Vertices");
                    foreach (TSVector2 vector2 in shape5.Vertices)
                    {
                        WriteElement("Vertex", vector2);
                    }
                    _writer.WriteEndElement();
                    WriteElement("NextVertex", shape5.NextVertex);
                    WriteElement("PrevVertex", shape5.PrevVertex);
                    break;
                }
                default:
                    throw new Exception();
            }
            _writer.WriteEndElement();
        }

        private static void WriteDynamicType(Type type, object val)
        {
            _writer.WriteElementString("Type", type.AssemblyQualifiedName);
            _writer.WriteStartElement("Value");
            XmlSerializer serializer = new XmlSerializer(type);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");
            serializer.Serialize(_writer, val, namespaces);
            _writer.WriteEndElement();
        }

        private static void WriteElement(string name, bool val)
        {
            _writer.WriteElementString(name, val.ToString());
        }

        private static void WriteElement(string name, int val)
        {
            _writer.WriteElementString(name, val.ToString());
        }

        private static void WriteElement(string name, FP val)
        {
            _writer.WriteElementString(name, val.ToString());
        }

        private static void WriteElement(string name, TSVector2 vec)
        {
            _writer.WriteElementString(name, vec.x + " " + vec.y);
        }
    }
}

