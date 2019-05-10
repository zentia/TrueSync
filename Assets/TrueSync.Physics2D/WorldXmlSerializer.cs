using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace TrueSync.Physics2D
{
	internal static class WorldXmlSerializer
	{
		private static XmlWriter _writer;

		private static void SerializeShape(Shape shape)
		{
			WorldXmlSerializer._writer.WriteStartElement("Shape");
			WorldXmlSerializer._writer.WriteAttributeString("Type", shape.ShapeType.ToString());
			WorldXmlSerializer._writer.WriteAttributeString("Density", shape.Density.ToString());
			switch (shape.ShapeType)
			{
			case ShapeType.Circle:
			{
				CircleShape circleShape = (CircleShape)shape;
				WorldXmlSerializer._writer.WriteElementString("Radius", circleShape.Radius.ToString());
				WorldXmlSerializer.WriteElement("Position", circleShape.Position);
				break;
			}
			case ShapeType.Edge:
			{
				EdgeShape edgeShape = (EdgeShape)shape;
				WorldXmlSerializer.WriteElement("Vertex1", edgeShape.Vertex1);
				WorldXmlSerializer.WriteElement("Vertex2", edgeShape.Vertex2);
				break;
			}
			case ShapeType.Polygon:
			{
				PolygonShape polygonShape = (PolygonShape)shape;
				WorldXmlSerializer._writer.WriteStartElement("Vertices");
				foreach (TSVector2 current in polygonShape.Vertices)
				{
					WorldXmlSerializer.WriteElement("Vertex", current);
				}
				WorldXmlSerializer._writer.WriteEndElement();
				WorldXmlSerializer.WriteElement("Centroid", polygonShape.MassData.Centroid);
				break;
			}
			case ShapeType.Chain:
			{
				ChainShape chainShape = (ChainShape)shape;
				WorldXmlSerializer._writer.WriteStartElement("Vertices");
				foreach (TSVector2 current2 in chainShape.Vertices)
				{
					WorldXmlSerializer.WriteElement("Vertex", current2);
				}
				WorldXmlSerializer._writer.WriteEndElement();
				WorldXmlSerializer.WriteElement("NextVertex", chainShape.NextVertex);
				WorldXmlSerializer.WriteElement("PrevVertex", chainShape.PrevVertex);
				break;
			}
			default:
				throw new Exception();
			}
			WorldXmlSerializer._writer.WriteEndElement();
		}

		private static void SerializeFixture(Fixture fixture)
		{
			WorldXmlSerializer._writer.WriteStartElement("Fixture");
			WorldXmlSerializer._writer.WriteAttributeString("Id", fixture.FixtureId.ToString());
			WorldXmlSerializer._writer.WriteStartElement("FilterData");
			WorldXmlSerializer._writer.WriteElementString("CategoryBits", ((int)fixture.CollisionCategories).ToString());
			WorldXmlSerializer._writer.WriteElementString("MaskBits", ((int)fixture.CollidesWith).ToString());
			WorldXmlSerializer._writer.WriteElementString("GroupIndex", fixture.CollisionGroup.ToString());
			WorldXmlSerializer._writer.WriteElementString("CollisionIgnores", WorldXmlSerializer.Join<int>("|", fixture._collisionIgnores));
			WorldXmlSerializer._writer.WriteEndElement();
			WorldXmlSerializer._writer.WriteElementString("Friction", fixture.Friction.ToString());
			WorldXmlSerializer._writer.WriteElementString("IsSensor", fixture.IsSensor.ToString());
			WorldXmlSerializer._writer.WriteElementString("Restitution", fixture.Restitution.ToString());
			bool flag = fixture.UserData != null;
			if (flag)
			{
				WorldXmlSerializer._writer.WriteStartElement("UserData");
				WorldXmlSerializer.WriteDynamicType(fixture.UserData.GetType(), fixture.UserData);
				WorldXmlSerializer._writer.WriteEndElement();
			}
			WorldXmlSerializer._writer.WriteEndElement();
		}

		private static void SerializeBody(List<Fixture> fixtures, List<Shape> shapes, Body body)
		{
			WorldXmlSerializer._writer.WriteStartElement("Body");
			WorldXmlSerializer._writer.WriteAttributeString("Type", body.BodyType.ToString());
			WorldXmlSerializer._writer.WriteElementString("Active", body.Enabled.ToString());
			WorldXmlSerializer._writer.WriteElementString("AllowSleep", body.SleepingAllowed.ToString());
			WorldXmlSerializer._writer.WriteElementString("Angle", body.Rotation.ToString());
			WorldXmlSerializer._writer.WriteElementString("AngularDamping", body.AngularDamping.ToString());
			WorldXmlSerializer._writer.WriteElementString("AngularVelocity", body.AngularVelocity.ToString());
			WorldXmlSerializer._writer.WriteElementString("Awake", body.Awake.ToString());
			WorldXmlSerializer._writer.WriteElementString("Bullet", body.IsBullet.ToString());
			WorldXmlSerializer._writer.WriteElementString("FixedRotation", body.FixedRotation.ToString());
			WorldXmlSerializer._writer.WriteElementString("LinearDamping", body.LinearDamping.ToString());
			WorldXmlSerializer.WriteElement("LinearVelocity", body.LinearVelocity);
			WorldXmlSerializer.WriteElement("Position", body.Position);
			bool flag = body.UserData != null;
			if (flag)
			{
				WorldXmlSerializer._writer.WriteStartElement("UserData");
				WorldXmlSerializer.WriteDynamicType(body.UserData.GetType(), body.UserData);
				WorldXmlSerializer._writer.WriteEndElement();
			}
			WorldXmlSerializer._writer.WriteStartElement("Bindings");
			for (int i = 0; i < body.FixtureList.Count; i++)
			{
				WorldXmlSerializer._writer.WriteStartElement("Pair");
				WorldXmlSerializer._writer.WriteAttributeString("FixtureId", WorldXmlSerializer.FindIndex(fixtures, body.FixtureList[i]).ToString());
				WorldXmlSerializer._writer.WriteAttributeString("ShapeId", WorldXmlSerializer.FindIndex(shapes, body.FixtureList[i].Shape).ToString());
				WorldXmlSerializer._writer.WriteEndElement();
			}
			WorldXmlSerializer._writer.WriteEndElement();
			WorldXmlSerializer._writer.WriteEndElement();
		}

		private static void SerializeJoint(List<Body> bodies, Joint joint)
		{
			WorldXmlSerializer._writer.WriteStartElement("Joint");
			WorldXmlSerializer._writer.WriteAttributeString("Type", joint.JointType.ToString());
			WorldXmlSerializer.WriteElement("BodyA", WorldXmlSerializer.FindIndex(bodies, joint.BodyA));
			WorldXmlSerializer.WriteElement("BodyB", WorldXmlSerializer.FindIndex(bodies, joint.BodyB));
			WorldXmlSerializer.WriteElement("CollideConnected", joint.CollideConnected);
			WorldXmlSerializer.WriteElement("Breakpoint", joint.Breakpoint);
			bool flag = joint.UserData != null;
			if (flag)
			{
				WorldXmlSerializer._writer.WriteStartElement("UserData");
				WorldXmlSerializer.WriteDynamicType(joint.UserData.GetType(), joint.UserData);
				WorldXmlSerializer._writer.WriteEndElement();
			}
			switch (joint.JointType)
			{
			case JointType.Revolute:
			{
				RevoluteJoint revoluteJoint = (RevoluteJoint)joint;
				WorldXmlSerializer.WriteElement("EnableLimit", revoluteJoint.LimitEnabled);
				WorldXmlSerializer.WriteElement("EnableMotor", revoluteJoint.MotorEnabled);
				WorldXmlSerializer.WriteElement("LocalAnchorA", revoluteJoint.LocalAnchorA);
				WorldXmlSerializer.WriteElement("LocalAnchorB", revoluteJoint.LocalAnchorB);
				WorldXmlSerializer.WriteElement("LowerAngle", revoluteJoint.LowerLimit);
				WorldXmlSerializer.WriteElement("MaxMotorTorque", revoluteJoint.MaxMotorTorque);
				WorldXmlSerializer.WriteElement("MotorSpeed", revoluteJoint.MotorSpeed);
				WorldXmlSerializer.WriteElement("ReferenceAngle", revoluteJoint.ReferenceAngle);
				WorldXmlSerializer.WriteElement("UpperAngle", revoluteJoint.UpperLimit);
				break;
			}
			case JointType.Prismatic:
			{
				PrismaticJoint prismaticJoint = (PrismaticJoint)joint;
				WorldXmlSerializer.WriteElement("EnableLimit", prismaticJoint.LimitEnabled);
				WorldXmlSerializer.WriteElement("EnableMotor", prismaticJoint.MotorEnabled);
				WorldXmlSerializer.WriteElement("LocalAnchorA", prismaticJoint.LocalAnchorA);
				WorldXmlSerializer.WriteElement("LocalAnchorB", prismaticJoint.LocalAnchorB);
				WorldXmlSerializer.WriteElement("Axis", prismaticJoint.Axis);
				WorldXmlSerializer.WriteElement("LowerTranslation", prismaticJoint.LowerLimit);
				WorldXmlSerializer.WriteElement("UpperTranslation", prismaticJoint.UpperLimit);
				WorldXmlSerializer.WriteElement("MaxMotorForce", prismaticJoint.MaxMotorForce);
				WorldXmlSerializer.WriteElement("MotorSpeed", prismaticJoint.MotorSpeed);
				break;
			}
			case JointType.Distance:
			{
				DistanceJoint distanceJoint = (DistanceJoint)joint;
				WorldXmlSerializer.WriteElement("DampingRatio", distanceJoint.DampingRatio);
				WorldXmlSerializer.WriteElement("FrequencyHz", distanceJoint.Frequency);
				WorldXmlSerializer.WriteElement("Length", distanceJoint.Length);
				WorldXmlSerializer.WriteElement("LocalAnchorA", distanceJoint.LocalAnchorA);
				WorldXmlSerializer.WriteElement("LocalAnchorB", distanceJoint.LocalAnchorB);
				break;
			}
			case JointType.Pulley:
			{
				PulleyJoint pulleyJoint = (PulleyJoint)joint;
				WorldXmlSerializer.WriteElement("WorldAnchorA", pulleyJoint.WorldAnchorA);
				WorldXmlSerializer.WriteElement("WorldAnchorB", pulleyJoint.WorldAnchorB);
				WorldXmlSerializer.WriteElement("LengthA", pulleyJoint.LengthA);
				WorldXmlSerializer.WriteElement("LengthB", pulleyJoint.LengthB);
				WorldXmlSerializer.WriteElement("LocalAnchorA", pulleyJoint.LocalAnchorA);
				WorldXmlSerializer.WriteElement("LocalAnchorB", pulleyJoint.LocalAnchorB);
				WorldXmlSerializer.WriteElement("Ratio", pulleyJoint.Ratio);
				WorldXmlSerializer.WriteElement("Constant", pulleyJoint.Constant);
				break;
			}
			case JointType.Gear:
				throw new Exception("Gear joint not supported by serialization");
			case JointType.Wheel:
			{
				WheelJoint wheelJoint = (WheelJoint)joint;
				WorldXmlSerializer.WriteElement("EnableMotor", wheelJoint.MotorEnabled);
				WorldXmlSerializer.WriteElement("LocalAnchorA", wheelJoint.LocalAnchorA);
				WorldXmlSerializer.WriteElement("LocalAnchorB", wheelJoint.LocalAnchorB);
				WorldXmlSerializer.WriteElement("MotorSpeed", wheelJoint.MotorSpeed);
				WorldXmlSerializer.WriteElement("DampingRatio", wheelJoint.DampingRatio);
				WorldXmlSerializer.WriteElement("MaxMotorTorque", wheelJoint.MaxMotorTorque);
				WorldXmlSerializer.WriteElement("FrequencyHz", wheelJoint.Frequency);
				WorldXmlSerializer.WriteElement("Axis", wheelJoint.Axis);
				break;
			}
			case JointType.Weld:
			{
				WeldJoint weldJoint = (WeldJoint)joint;
				WorldXmlSerializer.WriteElement("LocalAnchorA", weldJoint.LocalAnchorA);
				WorldXmlSerializer.WriteElement("LocalAnchorB", weldJoint.LocalAnchorB);
				break;
			}
			case JointType.Friction:
			{
				FrictionJoint frictionJoint = (FrictionJoint)joint;
				WorldXmlSerializer.WriteElement("LocalAnchorA", frictionJoint.LocalAnchorA);
				WorldXmlSerializer.WriteElement("LocalAnchorB", frictionJoint.LocalAnchorB);
				WorldXmlSerializer.WriteElement("MaxForce", frictionJoint.MaxForce);
				WorldXmlSerializer.WriteElement("MaxTorque", frictionJoint.MaxTorque);
				break;
			}
			case JointType.Rope:
			{
				RopeJoint ropeJoint = (RopeJoint)joint;
				WorldXmlSerializer.WriteElement("LocalAnchorA", ropeJoint.LocalAnchorA);
				WorldXmlSerializer.WriteElement("LocalAnchorB", ropeJoint.LocalAnchorB);
				WorldXmlSerializer.WriteElement("MaxLength", ropeJoint.MaxLength);
				break;
			}
			case JointType.Motor:
			{
				MotorJoint motorJoint = (MotorJoint)joint;
				WorldXmlSerializer.WriteElement("AngularOffset", motorJoint.AngularOffset);
				WorldXmlSerializer.WriteElement("LinearOffset", motorJoint.LinearOffset);
				WorldXmlSerializer.WriteElement("MaxForce", motorJoint.MaxForce);
				WorldXmlSerializer.WriteElement("MaxTorque", motorJoint.MaxTorque);
				WorldXmlSerializer.WriteElement("CorrectionFactor", motorJoint.CorrectionFactor);
				break;
			}
			case JointType.Angle:
			{
				AngleJoint angleJoint = (AngleJoint)joint;
				WorldXmlSerializer.WriteElement("BiasFactor", angleJoint.BiasFactor);
				WorldXmlSerializer.WriteElement("MaxImpulse", angleJoint.MaxImpulse);
				WorldXmlSerializer.WriteElement("Softness", angleJoint.Softness);
				WorldXmlSerializer.WriteElement("TargetAngle", angleJoint.TargetAngle);
				break;
			}
			default:
				throw new Exception("Joint not supported");
			}
			WorldXmlSerializer._writer.WriteEndElement();
		}

		private static void WriteDynamicType(Type type, object val)
		{
			WorldXmlSerializer._writer.WriteElementString("Type", type.AssemblyQualifiedName);
			WorldXmlSerializer._writer.WriteStartElement("Value");
			XmlSerializer xmlSerializer = new XmlSerializer(type);
			XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
			xmlSerializerNamespaces.Add("", "");
			xmlSerializer.Serialize(WorldXmlSerializer._writer, val, xmlSerializerNamespaces);
			WorldXmlSerializer._writer.WriteEndElement();
		}

		private static void WriteElement(string name, TSVector2 vec)
		{
			WorldXmlSerializer._writer.WriteElementString(name, vec.x + " " + vec.y);
		}

		private static void WriteElement(string name, int val)
		{
			WorldXmlSerializer._writer.WriteElementString(name, val.ToString());
		}

		private static void WriteElement(string name, bool val)
		{
			WorldXmlSerializer._writer.WriteElementString(name, val.ToString());
		}

		private static void WriteElement(string name, FP val)
		{
			WorldXmlSerializer._writer.WriteElementString(name, val.ToString());
		}

		private static int FindIndex(List<Body> list, Body item)
		{
			int result;
			for (int i = 0; i < list.Count; i++)
			{
				bool flag = list[i] == item;
				if (flag)
				{
					result = i;
					return result;
				}
			}
			result = -1;
			return result;
		}

		private static int FindIndex(List<Fixture> list, Fixture item)
		{
			int result;
			for (int i = 0; i < list.Count; i++)
			{
				bool flag = list[i].CompareTo(item);
				if (flag)
				{
					result = i;
					return result;
				}
			}
			result = -1;
			return result;
		}

		private static int FindIndex(List<Shape> list, Shape item)
		{
			int result;
			for (int i = 0; i < list.Count; i++)
			{
				bool flag = list[i].CompareTo(item);
				if (flag)
				{
					result = i;
					return result;
				}
			}
			result = -1;
			return result;
		}

		private static string Join<T>(string separator, IEnumerable<T> values)
		{
			string result;
			using (IEnumerator<T> enumerator = values.GetEnumerator())
			{
				bool flag = !enumerator.MoveNext();
				if (flag)
				{
					result = string.Empty;
				}
				else
				{
					StringBuilder stringBuilder = new StringBuilder();
					bool flag2 = enumerator.Current != null;
					if (flag2)
					{
						T current = enumerator.Current;
						string text = current.ToString();
						bool flag3 = text != null;
						if (flag3)
						{
							stringBuilder.Append(text);
						}
					}
					while (enumerator.MoveNext())
					{
						stringBuilder.Append(separator);
						bool flag4 = enumerator.Current != null;
						if (flag4)
						{
							T current = enumerator.Current;
							string text2 = current.ToString();
							bool flag5 = text2 != null;
							if (flag5)
							{
								stringBuilder.Append(text2);
							}
						}
					}
					result = stringBuilder.ToString();
				}
			}
			return result;
		}

		internal static void Serialize(World world, Stream stream)
		{
			List<Body> list = new List<Body>();
			List<Fixture> list2 = new List<Fixture>();
			List<Shape> list3 = new List<Shape>();
			WorldXmlSerializer._writer = XmlWriter.Create(stream, new XmlWriterSettings
			{
				Indent = true,
				NewLineOnAttributes = false,
				OmitXmlDeclaration = true
			});
			WorldXmlSerializer._writer.WriteStartElement("World");
			WorldXmlSerializer._writer.WriteAttributeString("Version", "3");
			WorldXmlSerializer.WriteElement("Gravity", world.Gravity);
			WorldXmlSerializer._writer.WriteStartElement("Shapes");
			foreach (Body current in world.BodyList)
			{
				using (List<Fixture>.Enumerator enumerator2 = current.FixtureList.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Fixture fixture = enumerator2.Current;
						bool flag = !list3.Any((Shape s2) => fixture.Shape.CompareTo(s2));
						if (flag)
						{
							WorldXmlSerializer.SerializeShape(fixture.Shape);
							list3.Add(fixture.Shape);
						}
					}
				}
			}
			WorldXmlSerializer._writer.WriteEndElement();
			WorldXmlSerializer._writer.WriteStartElement("Fixtures");
			foreach (Body current2 in world.BodyList)
			{
				using (List<Fixture>.Enumerator enumerator4 = current2.FixtureList.GetEnumerator())
				{
					while (enumerator4.MoveNext())
					{
						Fixture fixture = enumerator4.Current;
						bool flag2 = !list2.Any((Fixture f2) => fixture.CompareTo(f2));
						if (flag2)
						{
							WorldXmlSerializer.SerializeFixture(fixture);
							list2.Add(fixture);
						}
					}
				}
			}
			WorldXmlSerializer._writer.WriteEndElement();
			WorldXmlSerializer._writer.WriteStartElement("Bodies");
			foreach (Body current3 in world.BodyList)
			{
				list.Add(current3);
				WorldXmlSerializer.SerializeBody(list2, list3, current3);
			}
			WorldXmlSerializer._writer.WriteEndElement();
			WorldXmlSerializer._writer.WriteStartElement("Joints");
			foreach (Joint current4 in world.JointList)
			{
				WorldXmlSerializer.SerializeJoint(list, current4);
			}
			WorldXmlSerializer._writer.WriteEndElement();
			WorldXmlSerializer._writer.WriteEndElement();
			WorldXmlSerializer._writer.Flush();
			WorldXmlSerializer._writer.Close();
		}
	}
}
