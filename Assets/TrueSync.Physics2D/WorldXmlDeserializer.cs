using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TrueSync.Physics2D
{
	internal static class WorldXmlDeserializer
	{
		internal static World Deserialize(Stream stream)
		{
			World world = new World(TSVector2.zero);
			WorldXmlDeserializer.Deserialize(world, stream);
			return world;
		}

		private static void Deserialize(World world, Stream stream)
		{
			List<Body> list = new List<Body>();
			List<Fixture> list2 = new List<Fixture>();
			List<Joint> list3 = new List<Joint>();
			List<Shape> list4 = new List<Shape>();
			XMLFragmentElement xMLFragmentElement = XMLFragmentParser.LoadFromStream(stream);
			bool flag = xMLFragmentElement.Name.ToLower() != "world";
			if (flag)
			{
				throw new Exception();
			}
			foreach (XMLFragmentElement current in xMLFragmentElement.Elements)
			{
				bool flag2 = current.Name.ToLower() == "gravity";
				if (flag2)
				{
					world.Gravity = WorldXmlDeserializer.ReadVector(current);
					break;
				}
			}
			foreach (XMLFragmentElement current2 in xMLFragmentElement.Elements)
			{
				bool flag3 = current2.Name.ToLower() == "shapes";
				if (flag3)
				{
					foreach (XMLFragmentElement current3 in current2.Elements)
					{
						bool flag4 = current3.Name.ToLower() != "shape";
						if (flag4)
						{
							throw new Exception();
						}
						ShapeType shapeType = (ShapeType)Enum.Parse(typeof(ShapeType), current3.Attributes[0].Value, true);
						FP density = float.Parse(current3.Attributes[1].Value);
						switch (shapeType)
						{
						case ShapeType.Circle:
						{
							CircleShape circleShape = new CircleShape();
							circleShape._density = density;
							foreach (XMLFragmentElement current4 in current3.Elements)
							{
								string a = current4.Name.ToLower();
								if (!(a == "radius"))
								{
									if (!(a == "position"))
									{
										throw new Exception();
									}
									circleShape.Position = WorldXmlDeserializer.ReadVector(current4);
								}
								else
								{
									circleShape.Radius = float.Parse(current4.Value);
								}
							}
							list4.Add(circleShape);
							break;
						}
						case ShapeType.Edge:
						{
							EdgeShape edgeShape = new EdgeShape();
							edgeShape._density = density;
							foreach (XMLFragmentElement current5 in current3.Elements)
							{
								string a2 = current5.Name.ToLower();
								if (!(a2 == "hasvertex0"))
								{
									if (!(a2 == "hasvertex3"))
									{
										if (!(a2 == "vertex0"))
										{
											if (!(a2 == "vertex1"))
											{
												if (!(a2 == "vertex2"))
												{
													if (!(a2 == "vertex3"))
													{
														throw new Exception();
													}
													edgeShape.Vertex3 = WorldXmlDeserializer.ReadVector(current5);
												}
												else
												{
													edgeShape.Vertex2 = WorldXmlDeserializer.ReadVector(current5);
												}
											}
											else
											{
												edgeShape.Vertex1 = WorldXmlDeserializer.ReadVector(current5);
											}
										}
										else
										{
											edgeShape.Vertex0 = WorldXmlDeserializer.ReadVector(current5);
										}
									}
									else
									{
										edgeShape.HasVertex0 = bool.Parse(current5.Value);
									}
								}
								else
								{
									edgeShape.HasVertex0 = bool.Parse(current5.Value);
								}
							}
							list4.Add(edgeShape);
							break;
						}
						case ShapeType.Polygon:
						{
							PolygonShape polygonShape = new PolygonShape();
							polygonShape._density = density;
							foreach (XMLFragmentElement current6 in current3.Elements)
							{
								string a3 = current6.Name.ToLower();
								if (!(a3 == "vertices"))
								{
									if (a3 == "centroid")
									{
										polygonShape.MassData.Centroid = WorldXmlDeserializer.ReadVector(current6);
									}
								}
								else
								{
									List<TSVector2> list5 = new List<TSVector2>(current6.Elements.Count);
									foreach (XMLFragmentElement current7 in current6.Elements)
									{
										list5.Add(WorldXmlDeserializer.ReadVector(current7));
									}
									polygonShape.Vertices = new Vertices(list5);
								}
							}
							list4.Add(polygonShape);
							break;
						}
						case ShapeType.Chain:
						{
							ChainShape chainShape = new ChainShape();
							chainShape._density = density;
							foreach (XMLFragmentElement current8 in current3.Elements)
							{
								string a4 = current8.Name.ToLower();
								if (!(a4 == "vertices"))
								{
									if (!(a4 == "nextvertex"))
									{
										if (!(a4 == "prevvertex"))
										{
											throw new Exception();
										}
										chainShape.PrevVertex = WorldXmlDeserializer.ReadVector(current8);
									}
									else
									{
										chainShape.NextVertex = WorldXmlDeserializer.ReadVector(current8);
									}
								}
								else
								{
									List<TSVector2> list6 = new List<TSVector2>(current8.Elements.Count);
									foreach (XMLFragmentElement current9 in current8.Elements)
									{
										list6.Add(WorldXmlDeserializer.ReadVector(current9));
									}
									chainShape.Vertices = new Vertices(list6);
								}
							}
							list4.Add(chainShape);
							break;
						}
						}
					}
				}
			}
			foreach (XMLFragmentElement current10 in xMLFragmentElement.Elements)
			{
				bool flag5 = current10.Name.ToLower() == "fixtures";
				if (flag5)
				{
					foreach (XMLFragmentElement current11 in current10.Elements)
					{
						Fixture fixture = new Fixture();
						bool flag6 = current11.Name.ToLower() != "fixture";
						if (flag6)
						{
							throw new Exception();
						}
						fixture.FixtureId = int.Parse(current11.Attributes[0].Value);
						foreach (XMLFragmentElement current12 in current11.Elements)
						{
							string a5 = current12.Name.ToLower();
							if (!(a5 == "filterdata"))
							{
								if (!(a5 == "friction"))
								{
									if (!(a5 == "issensor"))
									{
										if (!(a5 == "restitution"))
										{
											if (a5 == "userdata")
											{
												fixture.UserData = WorldXmlDeserializer.ReadSimpleType(current12, null, false);
											}
										}
										else
										{
											fixture.Restitution = float.Parse(current12.Value);
										}
									}
									else
									{
										fixture.IsSensor = bool.Parse(current12.Value);
									}
								}
								else
								{
									fixture.Friction = float.Parse(current12.Value);
								}
							}
							else
							{
								foreach (XMLFragmentElement current13 in current12.Elements)
								{
									string a6 = current13.Name.ToLower();
									if (!(a6 == "categorybits"))
									{
										if (!(a6 == "maskbits"))
										{
											if (!(a6 == "groupindex"))
											{
												if (a6 == "CollisionIgnores")
												{
													string[] array = current13.Value.Split(new char[]
													{
														'|'
													});
													string[] array2 = array;
													for (int i = 0; i < array2.Length; i++)
													{
														string s = array2[i];
														fixture._collisionIgnores.Add(int.Parse(s));
													}
												}
											}
											else
											{
												fixture._collisionGroup = short.Parse(current13.Value);
											}
										}
										else
										{
											fixture._collidesWith = (Category)int.Parse(current13.Value);
										}
									}
									else
									{
										fixture._collisionCategories = (Category)int.Parse(current13.Value);
									}
								}
							}
						}
						list2.Add(fixture);
					}
				}
			}
			foreach (XMLFragmentElement current14 in xMLFragmentElement.Elements)
			{
				bool flag7 = current14.Name.ToLower() == "bodies";
				if (flag7)
				{
					foreach (XMLFragmentElement current15 in current14.Elements)
					{
						Body body = new Body(world, null, 0, null);
						bool flag8 = current15.Name.ToLower() != "body";
						if (flag8)
						{
							throw new Exception();
						}
						body.BodyType = (BodyType)Enum.Parse(typeof(BodyType), current15.Attributes[0].Value, true);
						foreach (XMLFragmentElement current16 in current15.Elements)
						{
							string text = current16.Name.ToLower();
							uint num = <PrivateImplementationDetails>.ComputeStringHash(text);
							if (num <= 2885808981u)
							{
								if (num <= 1523767750u)
								{
									if (num != 454871235u)
									{
										if (num != 1016246451u)
										{
											if (num == 1523767750u)
											{
												if (text == "lineardamping")
												{
													body.LinearDamping = float.Parse(current16.Value);
												}
											}
										}
										else if (text == "linearvelocity")
										{
											body.LinearVelocity = WorldXmlDeserializer.ReadVector(current16);
										}
									}
									else if (text == "bindings")
									{
										foreach (XMLFragmentElement current17 in current16.Elements)
										{
											Fixture fixture2 = list2[int.Parse(current17.Attributes[0].Value)];
											fixture2.Shape = list4[int.Parse(current17.Attributes[1].Value)].Clone();
											fixture2.CloneOnto(body);
										}
									}
								}
								else if (num != 2047084059u)
								{
									if (num != 2471448074u)
									{
										if (num == 2885808981u)
										{
											if (text == "allowsleep")
											{
												body.SleepingAllowed = bool.Parse(current16.Value);
											}
										}
									}
									else if (text == "position")
									{
										FP rotation = body.Rotation;
										TSVector2 tSVector = WorldXmlDeserializer.ReadVector(current16);
										body.SetTransformIgnoreContacts(ref tSVector, rotation);
									}
								}
								else if (text == "fixedrotation")
								{
									body.FixedRotation = bool.Parse(current16.Value);
								}
							}
							else if (num <= 3423781921u)
							{
								if (num != 2907980824u)
								{
									if (num != 2914888798u)
									{
										if (num == 3423781921u)
										{
											if (text == "angulardamping")
											{
												body.AngularDamping = float.Parse(current16.Value);
											}
										}
									}
									else if (text == "angularvelocity")
									{
										body.AngularVelocity = float.Parse(current16.Value);
									}
								}
								else if (text == "angle")
								{
									TSVector2 position = body.Position;
									body.SetTransformIgnoreContacts(ref position, float.Parse(current16.Value));
								}
							}
							else if (num <= 3648362799u)
							{
								if (num != 3608163548u)
								{
									if (num == 3648362799u)
									{
										if (text == "active")
										{
											body._enabled = bool.Parse(current16.Value);
										}
									}
								}
								else if (text == "awake")
								{
									body.Awake = bool.Parse(current16.Value);
								}
							}
							else if (num != 3669889442u)
							{
								if (num == 3902055289u)
								{
									if (text == "bullet")
									{
										body.IsBullet = bool.Parse(current16.Value);
									}
								}
							}
							else if (text == "userdata")
							{
								body.UserData = WorldXmlDeserializer.ReadSimpleType(current16, null, false);
							}
						}
						list.Add(body);
					}
				}
			}
			foreach (XMLFragmentElement current18 in xMLFragmentElement.Elements)
			{
				bool flag9 = current18.Name.ToLower() == "joints";
				if (flag9)
				{
					foreach (XMLFragmentElement current19 in current18.Elements)
					{
						bool flag10 = current19.Name.ToLower() != "joint";
						if (flag10)
						{
							throw new Exception();
						}
						JointType jointType = (JointType)Enum.Parse(typeof(JointType), current19.Attributes[0].Value, true);
						int index = -1;
						int index2 = -1;
						bool collideConnected = false;
						object userData = null;
						foreach (XMLFragmentElement current20 in current19.Elements)
						{
							string a7 = current20.Name.ToLower();
							if (!(a7 == "bodya"))
							{
								if (!(a7 == "bodyb"))
								{
									if (!(a7 == "collideconnected"))
									{
										if (a7 == "userdata")
										{
											userData = WorldXmlDeserializer.ReadSimpleType(current20, null, false);
										}
									}
									else
									{
										collideConnected = bool.Parse(current20.Value);
									}
								}
								else
								{
									index2 = int.Parse(current20.Value);
								}
							}
							else
							{
								index = int.Parse(current20.Value);
							}
						}
						Body bodyA = list[index];
						Body bodyB = list[index2];
						Joint joint;
						switch (jointType)
						{
						case JointType.Revolute:
							joint = new RevoluteJoint();
							break;
						case JointType.Prismatic:
							joint = new PrismaticJoint();
							break;
						case JointType.Distance:
							joint = new DistanceJoint();
							break;
						case JointType.Pulley:
							joint = new PulleyJoint();
							break;
						case JointType.Gear:
							throw new Exception("GearJoint is not supported.");
						case JointType.Wheel:
							joint = new WheelJoint();
							break;
						case JointType.Weld:
							joint = new WeldJoint();
							break;
						case JointType.Friction:
							joint = new FrictionJoint();
							break;
						case JointType.Rope:
							joint = new RopeJoint();
							break;
						case JointType.Motor:
							joint = new MotorJoint();
							break;
						case JointType.Angle:
							joint = new AngleJoint();
							break;
						default:
							throw new Exception("Invalid or unsupported joint.");
						}
						joint.CollideConnected = collideConnected;
						joint.UserData = userData;
						joint.BodyA = bodyA;
						joint.BodyB = bodyB;
						list3.Add(joint);
						world.AddJoint(joint);
						foreach (XMLFragmentElement current21 in current19.Elements)
						{
							switch (jointType)
							{
							case JointType.Revolute:
							{
								string text2 = current21.Name.ToLower();
								uint num = <PrivateImplementationDetails>.ComputeStringHash(text2);
								if (num <= 2400078776u)
								{
									if (num <= 1394215284u)
									{
										if (num != 737227769u)
										{
											if (num == 1394215284u)
											{
												if (text2 == "maxmotortorque")
												{
													((RevoluteJoint)joint).MaxMotorTorque = float.Parse(current21.Value);
												}
											}
										}
										else if (text2 == "lowerangle")
										{
											((RevoluteJoint)joint).LowerLimit = float.Parse(current21.Value);
										}
									}
									else if (num != 1767952217u)
									{
										if (num == 2400078776u)
										{
											if (text2 == "localanchora")
											{
												((RevoluteJoint)joint).LocalAnchorA = WorldXmlDeserializer.ReadVector(current21);
											}
										}
									}
									else if (text2 == "referenceangle")
									{
										((RevoluteJoint)joint).ReferenceAngle = float.Parse(current21.Value);
									}
								}
								else if (num <= 2881347325u)
								{
									if (num != 2450411633u)
									{
										if (num == 2881347325u)
										{
											if (text2 == "enablelimit")
											{
												((RevoluteJoint)joint).LimitEnabled = bool.Parse(current21.Value);
											}
										}
									}
									else if (text2 == "localanchorb")
									{
										((RevoluteJoint)joint).LocalAnchorB = WorldXmlDeserializer.ReadVector(current21);
									}
								}
								else if (num != 3409164005u)
								{
									if (num != 3661889407u)
									{
										if (num == 3858759374u)
										{
											if (text2 == "upperangle")
											{
												((RevoluteJoint)joint).UpperLimit = float.Parse(current21.Value);
											}
										}
									}
									else if (text2 == "enablemotor")
									{
										((RevoluteJoint)joint).MotorEnabled = bool.Parse(current21.Value);
									}
								}
								else if (text2 == "motorspeed")
								{
									((RevoluteJoint)joint).MotorSpeed = float.Parse(current21.Value);
								}
								break;
							}
							case JointType.Prismatic:
							{
								string text3 = current21.Name.ToLower();
								uint num = <PrivateImplementationDetails>.ComputeStringHash(text3);
								if (num <= 2450411633u)
								{
									if (num <= 1767952217u)
									{
										if (num != 977678917u)
										{
											if (num == 1767952217u)
											{
												if (text3 == "referenceangle")
												{
													((PrismaticJoint)joint).ReferenceAngle = float.Parse(current21.Value);
												}
											}
										}
										else if (text3 == "maxmotorforce")
										{
											((PrismaticJoint)joint).MaxMotorForce = float.Parse(current21.Value);
										}
									}
									else if (num != 1831579124u)
									{
										if (num != 2400078776u)
										{
											if (num == 2450411633u)
											{
												if (text3 == "localanchorb")
												{
													((PrismaticJoint)joint).LocalAnchorB = WorldXmlDeserializer.ReadVector(current21);
												}
											}
										}
										else if (text3 == "localanchora")
										{
											((PrismaticJoint)joint).LocalAnchorA = WorldXmlDeserializer.ReadVector(current21);
										}
									}
									else if (text3 == "axis")
									{
										((PrismaticJoint)joint).Axis = WorldXmlDeserializer.ReadVector(current21);
									}
								}
								else if (num <= 3184934789u)
								{
									if (num != 2881347325u)
									{
										if (num == 3184934789u)
										{
											if (text3 == "lowertranslation")
											{
												((PrismaticJoint)joint).LowerLimit = float.Parse(current21.Value);
											}
										}
									}
									else if (text3 == "enablelimit")
									{
										((PrismaticJoint)joint).LimitEnabled = bool.Parse(current21.Value);
									}
								}
								else if (num != 3409164005u)
								{
									if (num != 3483836446u)
									{
										if (num == 3661889407u)
										{
											if (text3 == "enablemotor")
											{
												((PrismaticJoint)joint).MotorEnabled = bool.Parse(current21.Value);
											}
										}
									}
									else if (text3 == "uppertranslation")
									{
										((PrismaticJoint)joint).UpperLimit = float.Parse(current21.Value);
									}
								}
								else if (text3 == "motorspeed")
								{
									((PrismaticJoint)joint).MotorSpeed = float.Parse(current21.Value);
								}
								break;
							}
							case JointType.Distance:
							{
								string a8 = current21.Name.ToLower();
								if (!(a8 == "dampingratio"))
								{
									if (!(a8 == "frequencyhz"))
									{
										if (!(a8 == "length"))
										{
											if (!(a8 == "localanchora"))
											{
												if (a8 == "localanchorb")
												{
													((DistanceJoint)joint).LocalAnchorB = WorldXmlDeserializer.ReadVector(current21);
												}
											}
											else
											{
												((DistanceJoint)joint).LocalAnchorA = WorldXmlDeserializer.ReadVector(current21);
											}
										}
										else
										{
											((DistanceJoint)joint).Length = float.Parse(current21.Value);
										}
									}
									else
									{
										((DistanceJoint)joint).Frequency = float.Parse(current21.Value);
									}
								}
								else
								{
									((DistanceJoint)joint).DampingRatio = float.Parse(current21.Value);
								}
								break;
							}
							case JointType.Pulley:
							{
								string text4 = current21.Name.ToLower();
								uint num = <PrivateImplementationDetails>.ComputeStringHash(text4);
								if (num <= 2450411633u)
								{
									if (num <= 110225957u)
									{
										if (num != 2869265u)
										{
											if (num == 110225957u)
											{
												if (text4 == "constant")
												{
													((PulleyJoint)joint).Constant = float.Parse(current21.Value);
												}
											}
										}
										else if (text4 == "worldanchora")
										{
											((PulleyJoint)joint).WorldAnchorA = WorldXmlDeserializer.ReadVector(current21);
										}
									}
									else if (num != 2400078776u)
									{
										if (num == 2450411633u)
										{
											if (text4 == "localanchorb")
											{
												((PulleyJoint)joint).LocalAnchorB = WorldXmlDeserializer.ReadVector(current21);
											}
										}
									}
									else if (text4 == "localanchora")
									{
										((PulleyJoint)joint).LocalAnchorA = WorldXmlDeserializer.ReadVector(current21);
									}
								}
								else if (num <= 4106598556u)
								{
									if (num != 3239190148u)
									{
										if (num == 4106598556u)
										{
											if (text4 == "lengtha")
											{
												((PulleyJoint)joint).LengthA = float.Parse(current21.Value);
											}
										}
									}
									else if (text4 == "ratio")
									{
										((PulleyJoint)joint).Ratio = float.Parse(current21.Value);
									}
								}
								else if (num != 4156931413u)
								{
									if (num == 4247503704u)
									{
										if (text4 == "worldanchorb")
										{
											((PulleyJoint)joint).WorldAnchorB = WorldXmlDeserializer.ReadVector(current21);
										}
									}
								}
								else if (text4 == "lengthb")
								{
									((PulleyJoint)joint).LengthB = float.Parse(current21.Value);
								}
								break;
							}
							case JointType.Gear:
								throw new Exception("Gear joint is unsupported");
							case JointType.Wheel:
							{
								string text5 = current21.Name.ToLower();
								uint num = <PrivateImplementationDetails>.ComputeStringHash(text5);
								if (num <= 2450411633u)
								{
									if (num <= 1831579124u)
									{
										if (num != 1394215284u)
										{
											if (num == 1831579124u)
											{
												if (text5 == "axis")
												{
													((WheelJoint)joint).Axis = WorldXmlDeserializer.ReadVector(current21);
												}
											}
										}
										else if (text5 == "maxmotortorque")
										{
											((WheelJoint)joint).MaxMotorTorque = float.Parse(current21.Value);
										}
									}
									else if (num != 2400078776u)
									{
										if (num == 2450411633u)
										{
											if (text5 == "localanchorb")
											{
												((WheelJoint)joint).LocalAnchorB = WorldXmlDeserializer.ReadVector(current21);
											}
										}
									}
									else if (text5 == "localanchora")
									{
										((WheelJoint)joint).LocalAnchorA = WorldXmlDeserializer.ReadVector(current21);
									}
								}
								else if (num <= 3409164005u)
								{
									if (num != 2698151971u)
									{
										if (num == 3409164005u)
										{
											if (text5 == "motorspeed")
											{
												((WheelJoint)joint).MotorSpeed = float.Parse(current21.Value);
											}
										}
									}
									else if (text5 == "frequencyhz")
									{
										((WheelJoint)joint).Frequency = float.Parse(current21.Value);
									}
								}
								else if (num != 3661889407u)
								{
									if (num == 4212757684u)
									{
										if (text5 == "dampingratio")
										{
											((WheelJoint)joint).DampingRatio = float.Parse(current21.Value);
										}
									}
								}
								else if (text5 == "enablemotor")
								{
									((WheelJoint)joint).MotorEnabled = bool.Parse(current21.Value);
								}
								break;
							}
							case JointType.Weld:
							{
								string a9 = current21.Name.ToLower();
								if (!(a9 == "localanchora"))
								{
									if (a9 == "localanchorb")
									{
										((WeldJoint)joint).LocalAnchorB = WorldXmlDeserializer.ReadVector(current21);
									}
								}
								else
								{
									((WeldJoint)joint).LocalAnchorA = WorldXmlDeserializer.ReadVector(current21);
								}
								break;
							}
							case JointType.Friction:
							{
								string a10 = current21.Name.ToLower();
								if (!(a10 == "localanchora"))
								{
									if (!(a10 == "localanchorb"))
									{
										if (!(a10 == "maxforce"))
										{
											if (a10 == "maxtorque")
											{
												((FrictionJoint)joint).MaxTorque = float.Parse(current21.Value);
											}
										}
										else
										{
											((FrictionJoint)joint).MaxForce = float.Parse(current21.Value);
										}
									}
									else
									{
										((FrictionJoint)joint).LocalAnchorB = WorldXmlDeserializer.ReadVector(current21);
									}
								}
								else
								{
									((FrictionJoint)joint).LocalAnchorA = WorldXmlDeserializer.ReadVector(current21);
								}
								break;
							}
							case JointType.Rope:
							{
								string a11 = current21.Name.ToLower();
								if (!(a11 == "localanchora"))
								{
									if (!(a11 == "localanchorb"))
									{
										if (a11 == "maxlength")
										{
											((RopeJoint)joint).MaxLength = float.Parse(current21.Value);
										}
									}
									else
									{
										((RopeJoint)joint).LocalAnchorB = WorldXmlDeserializer.ReadVector(current21);
									}
								}
								else
								{
									((RopeJoint)joint).LocalAnchorA = WorldXmlDeserializer.ReadVector(current21);
								}
								break;
							}
							case JointType.Motor:
							{
								string a12 = current21.Name.ToLower();
								if (!(a12 == "angularoffset"))
								{
									if (!(a12 == "linearoffset"))
									{
										if (!(a12 == "maxforce"))
										{
											if (!(a12 == "maxtorque"))
											{
												if (a12 == "correctionfactor")
												{
													((MotorJoint)joint).CorrectionFactor = float.Parse(current21.Value);
												}
											}
											else
											{
												((MotorJoint)joint).MaxTorque = float.Parse(current21.Value);
											}
										}
										else
										{
											((MotorJoint)joint).MaxForce = float.Parse(current21.Value);
										}
									}
									else
									{
										((MotorJoint)joint).LinearOffset = WorldXmlDeserializer.ReadVector(current21);
									}
								}
								else
								{
									((MotorJoint)joint).AngularOffset = float.Parse(current21.Value);
								}
								break;
							}
							case JointType.Angle:
							{
								string a13 = current21.Name.ToLower();
								if (!(a13 == "biasfactor"))
								{
									if (!(a13 == "maximpulse"))
									{
										if (!(a13 == "softness"))
										{
											if (a13 == "targetangle")
											{
												((AngleJoint)joint).TargetAngle = float.Parse(current21.Value);
											}
										}
										else
										{
											((AngleJoint)joint).Softness = float.Parse(current21.Value);
										}
									}
									else
									{
										((AngleJoint)joint).MaxImpulse = float.Parse(current21.Value);
									}
								}
								else
								{
									((AngleJoint)joint).BiasFactor = float.Parse(current21.Value);
								}
								break;
							}
							}
						}
					}
				}
			}
			world.ProcessChanges();
		}

		private static TSVector2 ReadVector(XMLFragmentElement node)
		{
			string[] array = node.Value.Split(new char[]
			{
				' '
			});
			return new TSVector2(float.Parse(array[0]), float.Parse(array[1]));
		}

		private static object ReadSimpleType(XMLFragmentElement node, Type type, bool outer)
		{
			bool flag = type == null;
			object result;
			if (flag)
			{
				result = WorldXmlDeserializer.ReadSimpleType(node.Elements[1], Type.GetType(node.Elements[0].Value), outer);
			}
			else
			{
				XmlSerializer xmlSerializer = new XmlSerializer(type);
				XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
				xmlSerializerNamespaces.Add("", "");
				using (MemoryStream memoryStream = new MemoryStream())
				{
					StreamWriter streamWriter = new StreamWriter(memoryStream);
					streamWriter.Write(outer ? node.OuterXml : node.InnerXml);
					streamWriter.Flush();
					memoryStream.Position = 0L;
					result = xmlSerializer.Deserialize(XmlReader.Create(memoryStream, new XmlReaderSettings
					{
						ConformanceLevel = ConformanceLevel.Fragment
					}));
				}
			}
			return result;
		}
	}
}
