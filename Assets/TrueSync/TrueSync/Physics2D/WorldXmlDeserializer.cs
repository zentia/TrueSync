// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.WorldXmlDeserializer
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

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
            List<Body> bodyList = new List<Body>();
            List<Fixture> fixtureList = new List<Fixture>();
            List<Joint> jointList = new List<Joint>();
            List<Shape> shapeList = new List<Shape>();
            XMLFragmentElement xmlFragmentElement = XMLFragmentParser.LoadFromStream(stream);
            if (xmlFragmentElement.Name.ToLower() != nameof(world))
                throw new Exception();
            foreach (XMLFragmentElement element in (IEnumerable<XMLFragmentElement>)xmlFragmentElement.Elements)
            {
                if (element.Name.ToLower() == "gravity")
                {
                    world.Gravity = WorldXmlDeserializer.ReadVector(element);
                    break;
                }
            }
            foreach (XMLFragmentElement element1 in (IEnumerable<XMLFragmentElement>)xmlFragmentElement.Elements)
            {
                if (element1.Name.ToLower() == "shapes")
                {
                    foreach (XMLFragmentElement element2 in (IEnumerable<XMLFragmentElement>)element1.Elements)
                    {
                        if (element2.Name.ToLower() != "shape")
                            throw new Exception();
                        ShapeType shapeType = (ShapeType)Enum.Parse(typeof(ShapeType), element2.Attributes[0].Value, true);
                        FP fp = (FP)float.Parse(element2.Attributes[1].Value);
                        switch (shapeType)
                        {
                            case ShapeType.Circle:
                                CircleShape circleShape = new CircleShape();
                                circleShape._density = fp;
                                foreach (XMLFragmentElement element3 in (IEnumerable<XMLFragmentElement>)element2.Elements)
                                {
                                    string lower = element3.Name.ToLower();
                                    if (!(lower == "radius"))
                                    {
                                        if (!(lower == "position"))
                                            throw new Exception();
                                        circleShape.Position = WorldXmlDeserializer.ReadVector(element3);
                                    }
                                    else
                                        circleShape.Radius = (FP)float.Parse(element3.Value);
                                }
                                shapeList.Add((Shape)circleShape);
                                break;
                            case ShapeType.Edge:
                                EdgeShape edgeShape = new EdgeShape();
                                edgeShape._density = fp;
                                foreach (XMLFragmentElement element3 in (IEnumerable<XMLFragmentElement>)element2.Elements)
                                {
                                    string lower = element3.Name.ToLower();
                                    if (!(lower == "hasvertex0"))
                                    {
                                        if (!(lower == "hasvertex3"))
                                        {
                                            if (!(lower == "vertex0"))
                                            {
                                                if (!(lower == "vertex1"))
                                                {
                                                    if (!(lower == "vertex2"))
                                                    {
                                                        if (!(lower == "vertex3"))
                                                            throw new Exception();
                                                        edgeShape.Vertex3 = WorldXmlDeserializer.ReadVector(element3);
                                                    }
                                                    else
                                                        edgeShape.Vertex2 = WorldXmlDeserializer.ReadVector(element3);
                                                }
                                                else
                                                    edgeShape.Vertex1 = WorldXmlDeserializer.ReadVector(element3);
                                            }
                                            else
                                                edgeShape.Vertex0 = WorldXmlDeserializer.ReadVector(element3);
                                        }
                                        else
                                            edgeShape.HasVertex0 = bool.Parse(element3.Value);
                                    }
                                    else
                                        edgeShape.HasVertex0 = bool.Parse(element3.Value);
                                }
                                shapeList.Add((Shape)edgeShape);
                                break;
                            case ShapeType.Polygon:
                                PolygonShape polygonShape = new PolygonShape();
                                polygonShape._density = fp;
                                foreach (XMLFragmentElement element3 in (IEnumerable<XMLFragmentElement>)element2.Elements)
                                {
                                    string lower = element3.Name.ToLower();
                                    if (!(lower == "vertices"))
                                    {
                                        if (lower == "centroid")
                                            polygonShape.MassData.Centroid = WorldXmlDeserializer.ReadVector(element3);
                                    }
                                    else
                                    {
                                        List<TSVector2> tsVector2List = new List<TSVector2>(element3.Elements.Count);
                                        foreach (XMLFragmentElement element4 in (IEnumerable<XMLFragmentElement>)element3.Elements)
                                            tsVector2List.Add(WorldXmlDeserializer.ReadVector(element4));
                                        polygonShape.Vertices = new Vertices((IEnumerable<TSVector2>)tsVector2List);
                                    }
                                }
                                shapeList.Add((Shape)polygonShape);
                                break;
                            case ShapeType.Chain:
                                ChainShape chainShape = new ChainShape();
                                chainShape._density = fp;
                                foreach (XMLFragmentElement element3 in (IEnumerable<XMLFragmentElement>)element2.Elements)
                                {
                                    string lower = element3.Name.ToLower();
                                    if (!(lower == "vertices"))
                                    {
                                        if (!(lower == "nextvertex"))
                                        {
                                            if (!(lower == "prevvertex"))
                                                throw new Exception();
                                            chainShape.PrevVertex = WorldXmlDeserializer.ReadVector(element3);
                                        }
                                        else
                                            chainShape.NextVertex = WorldXmlDeserializer.ReadVector(element3);
                                    }
                                    else
                                    {
                                        List<TSVector2> tsVector2List = new List<TSVector2>(element3.Elements.Count);
                                        foreach (XMLFragmentElement element4 in (IEnumerable<XMLFragmentElement>)element3.Elements)
                                            tsVector2List.Add(WorldXmlDeserializer.ReadVector(element4));
                                        chainShape.Vertices = new Vertices((IEnumerable<TSVector2>)tsVector2List);
                                    }
                                }
                                shapeList.Add((Shape)chainShape);
                                break;
                        }
                    }
                }
            }
            foreach (XMLFragmentElement element1 in (IEnumerable<XMLFragmentElement>)xmlFragmentElement.Elements)
            {
                if (element1.Name.ToLower() == "fixtures")
                {
                    foreach (XMLFragmentElement element2 in (IEnumerable<XMLFragmentElement>)element1.Elements)
                    {
                        Fixture fixture = new Fixture();
                        if (element2.Name.ToLower() != "fixture")
                            throw new Exception();
                        fixture.FixtureId = int.Parse(element2.Attributes[0].Value);
                        foreach (XMLFragmentElement element3 in (IEnumerable<XMLFragmentElement>)element2.Elements)
                        {
                            string lower1 = element3.Name.ToLower();
                            if (!(lower1 == "filterdata"))
                            {
                                if (!(lower1 == "friction"))
                                {
                                    if (!(lower1 == "issensor"))
                                    {
                                        if (!(lower1 == "restitution"))
                                        {
                                            if (lower1 == "userdata")
                                                fixture.UserData = WorldXmlDeserializer.ReadSimpleType(element3, (Type)null, false);
                                        }
                                        else
                                            fixture.Restitution = (FP)float.Parse(element3.Value);
                                    }
                                    else
                                        fixture.IsSensor = bool.Parse(element3.Value);
                                }
                                else
                                    fixture.Friction = (FP)float.Parse(element3.Value);
                            }
                            else
                            {
                                foreach (XMLFragmentElement element4 in (IEnumerable<XMLFragmentElement>)element3.Elements)
                                {
                                    string lower2 = element4.Name.ToLower();
                                    if (!(lower2 == "categorybits"))
                                    {
                                        if (!(lower2 == "maskbits"))
                                        {
                                            if (!(lower2 == "groupindex"))
                                            {
                                                if (lower2 == "CollisionIgnores")
                                                {
                                                    string str = element4.Value;
                                                    char[] chArray = new char[1]
                                                    {
                            '|'
                                                    };
                                                    foreach (string s in str.Split(chArray))
                                                        fixture._collisionIgnores.Add(int.Parse(s));
                                                }
                                            }
                                            else
                                                fixture._collisionGroup = short.Parse(element4.Value);
                                        }
                                        else
                                            fixture._collidesWith = (Category)int.Parse(element4.Value);
                                    }
                                    else
                                        fixture._collisionCategories = (Category)int.Parse(element4.Value);
                                }
                            }
                        }
                        fixtureList.Add(fixture);
                    }
                }
            }
            foreach (XMLFragmentElement element1 in (IEnumerable<XMLFragmentElement>)xmlFragmentElement.Elements)
            {
                if (element1.Name.ToLower() == "bodies")
                {
                    foreach (XMLFragmentElement element2 in (IEnumerable<XMLFragmentElement>)element1.Elements)
                    {
                        Body body = new Body(world, new TSVector2?(), (FP)0, (object)null);
                        if (element2.Name.ToLower() != "body")
                            throw new Exception();
                        body.BodyType = (BodyType)Enum.Parse(typeof(BodyType), element2.Attributes[0].Value, true);
                        foreach (XMLFragmentElement element3 in (IEnumerable<XMLFragmentElement>)element2.Elements)
                        {
                            string lower = element3.Name.ToLower();
                            // ISSUE: reference to a compiler-generated method
                            uint stringHash = \u003CPrivateImplementationDetails\u003E.ComputeStringHash(lower);
                            if (stringHash <= 2885808981U)
                            {
                                if (stringHash <= 1523767750U)
                                {
                                    if ((int)stringHash != 454871235)
                                    {
                                        if ((int)stringHash != 1016246451)
                                        {
                                            if ((int)stringHash == 1523767750 && lower == "lineardamping")
                                                body.LinearDamping = (FP)float.Parse(element3.Value);
                                        }
                                        else if (lower == "linearvelocity")
                                            body.LinearVelocity = WorldXmlDeserializer.ReadVector(element3);
                                    }
                                    else if (lower == "bindings")
                                    {
                                        foreach (XMLFragmentElement element4 in (IEnumerable<XMLFragmentElement>)element3.Elements)
                                        {
                                            Fixture fixture = fixtureList[int.Parse(element4.Attributes[0].Value)];
                                            fixture.Shape = shapeList[int.Parse(element4.Attributes[1].Value)].Clone();
                                            fixture.CloneOnto(body);
                                        }
                                    }
                                }
                                else if ((int)stringHash != 2047084059)
                                {
                                    if ((int)stringHash != -1823519222)
                                    {
                                        if ((int)stringHash == -1409158315 && lower == "allowsleep")
                                            body.SleepingAllowed = bool.Parse(element3.Value);
                                    }
                                    else if (lower == "position")
                                    {
                                        FP rotation = body.Rotation;
                                        TSVector2 position = WorldXmlDeserializer.ReadVector(element3);
                                        body.SetTransformIgnoreContacts(ref position, rotation);
                                    }
                                }
                                else if (lower == "fixedrotation")
                                    body.FixedRotation = bool.Parse(element3.Value);
                            }
                            else if (stringHash <= 3423781921U)
                            {
                                if ((int)stringHash != -1386986472)
                                {
                                    if ((int)stringHash != -1380078498)
                                    {
                                        if ((int)stringHash == -871185375 && lower == "angulardamping")
                                            body.AngularDamping = (FP)float.Parse(element3.Value);
                                    }
                                    else if (lower == "angularvelocity")
                                        body.AngularVelocity = (FP)float.Parse(element3.Value);
                                }
                                else if (lower == "angle")
                                {
                                    TSVector2 position = body.Position;
                                    body.SetTransformIgnoreContacts(ref position, (FP)float.Parse(element3.Value));
                                }
                            }
                            else if (stringHash <= 3648362799U)
                            {
                                if ((int)stringHash != -686803748)
                                {
                                    if ((int)stringHash == -646604497 && lower == "active")
                                        body._enabled = bool.Parse(element3.Value);
                                }
                                else if (lower == "awake")
                                    body.Awake = bool.Parse(element3.Value);
                            }
                            else if ((int)stringHash != -625077854)
                            {
                                if ((int)stringHash == -392912007 && lower == "bullet")
                                    body.IsBullet = bool.Parse(element3.Value);
                            }
                            else if (lower == "userdata")
                                body.UserData = WorldXmlDeserializer.ReadSimpleType(element3, (Type)null, false);
                        }
                        bodyList.Add(body);
                    }
                }
            }
            foreach (XMLFragmentElement element1 in (IEnumerable<XMLFragmentElement>)xmlFragmentElement.Elements)
            {
                if (element1.Name.ToLower() == "joints")
                {
                    foreach (XMLFragmentElement element2 in (IEnumerable<XMLFragmentElement>)element1.Elements)
                    {
                        if (element2.Name.ToLower() != "joint")
                            throw new Exception();
                        JointType jointType = (JointType)Enum.Parse(typeof(JointType), element2.Attributes[0].Value, true);
                        int index1 = -1;
                        int index2 = -1;
                        bool flag = false;
                        object obj = (object)null;
                        foreach (XMLFragmentElement element3 in (IEnumerable<XMLFragmentElement>)element2.Elements)
                        {
                            string lower = element3.Name.ToLower();
                            if (!(lower == "bodya"))
                            {
                                if (!(lower == "bodyb"))
                                {
                                    if (!(lower == "collideconnected"))
                                    {
                                        if (lower == "userdata")
                                            obj = WorldXmlDeserializer.ReadSimpleType(element3, (Type)null, false);
                                    }
                                    else
                                        flag = bool.Parse(element3.Value);
                                }
                                else
                                    index2 = int.Parse(element3.Value);
                            }
                            else
                                index1 = int.Parse(element3.Value);
                        }
                        Body body1 = bodyList[index1];
                        Body body2 = bodyList[index2];
                        Joint joint;
                        switch (jointType)
                        {
                            case JointType.Revolute:
                                joint = (Joint)new RevoluteJoint();
                                break;
                            case JointType.Prismatic:
                                joint = (Joint)new PrismaticJoint();
                                break;
                            case JointType.Distance:
                                joint = (Joint)new DistanceJoint();
                                break;
                            case JointType.Pulley:
                                joint = (Joint)new PulleyJoint();
                                break;
                            case JointType.Gear:
                                throw new Exception("GearJoint is not supported.");
                            case JointType.Wheel:
                                joint = (Joint)new WheelJoint();
                                break;
                            case JointType.Weld:
                                joint = (Joint)new WeldJoint();
                                break;
                            case JointType.Friction:
                                joint = (Joint)new FrictionJoint();
                                break;
                            case JointType.Rope:
                                joint = (Joint)new RopeJoint();
                                break;
                            case JointType.Motor:
                                joint = (Joint)new MotorJoint();
                                break;
                            case JointType.Angle:
                                joint = (Joint)new AngleJoint();
                                break;
                            default:
                                throw new Exception("Invalid or unsupported joint.");
                        }
                        joint.CollideConnected = flag;
                        joint.UserData = obj;
                        joint.BodyA = body1;
                        joint.BodyB = body2;
                        jointList.Add(joint);
                        world.AddJoint(joint);
                        foreach (XMLFragmentElement element3 in (IEnumerable<XMLFragmentElement>)element2.Elements)
                        {
                            switch (jointType)
                            {
                                case JointType.Revolute:
                                    string lower1 = element3.Name.ToLower();
                                    // ISSUE: reference to a compiler-generated method
                                    uint stringHash1 = \u003CPrivateImplementationDetails\u003E.ComputeStringHash(lower1);
                                    if (stringHash1 <= 2400078776U)
                                    {
                                        if (stringHash1 <= 1394215284U)
                                        {
                                            if ((int)stringHash1 != 737227769)
                                            {
                                                if ((int)stringHash1 == 1394215284 && lower1 == "maxmotortorque")
                                                {
                                                    ((RevoluteJoint)joint).MaxMotorTorque = (FP)float.Parse(element3.Value);
                                                    break;
                                                }
                                                break;
                                            }
                                            if (lower1 == "lowerangle")
                                            {
                                                ((RevoluteJoint)joint).LowerLimit = (FP)float.Parse(element3.Value);
                                                break;
                                            }
                                            break;
                                        }
                                        if ((int)stringHash1 != 1767952217)
                                        {
                                            if ((int)stringHash1 == -1894888520 && lower1 == "localanchora")
                                            {
                                                ((RevoluteJoint)joint).LocalAnchorA = WorldXmlDeserializer.ReadVector(element3);
                                                break;
                                            }
                                            break;
                                        }
                                        if (lower1 == "referenceangle")
                                        {
                                            ((RevoluteJoint)joint).ReferenceAngle = (FP)float.Parse(element3.Value);
                                            break;
                                        }
                                        break;
                                    }
                                    if (stringHash1 <= 2881347325U)
                                    {
                                        if ((int)stringHash1 != -1844555663)
                                        {
                                            if ((int)stringHash1 == -1413619971 && lower1 == "enablelimit")
                                            {
                                                ((RevoluteJoint)joint).LimitEnabled = bool.Parse(element3.Value);
                                                break;
                                            }
                                            break;
                                        }
                                        if (lower1 == "localanchorb")
                                        {
                                            ((RevoluteJoint)joint).LocalAnchorB = WorldXmlDeserializer.ReadVector(element3);
                                            break;
                                        }
                                        break;
                                    }
                                    if ((int)stringHash1 != -885803291)
                                    {
                                        if ((int)stringHash1 != -633077889)
                                        {
                                            if ((int)stringHash1 == -436207922 && lower1 == "upperangle")
                                            {
                                                ((RevoluteJoint)joint).UpperLimit = (FP)float.Parse(element3.Value);
                                                break;
                                            }
                                            break;
                                        }
                                        if (lower1 == "enablemotor")
                                        {
                                            ((RevoluteJoint)joint).MotorEnabled = bool.Parse(element3.Value);
                                            break;
                                        }
                                        break;
                                    }
                                    if (lower1 == "motorspeed")
                                    {
                                        ((RevoluteJoint)joint).MotorSpeed = (FP)float.Parse(element3.Value);
                                        break;
                                    }
                                    break;
                                case JointType.Prismatic:
                                    string lower2 = element3.Name.ToLower();
                                    // ISSUE: reference to a compiler-generated method
                                    uint stringHash2 = \u003CPrivateImplementationDetails\u003E.ComputeStringHash(lower2);
                                    if (stringHash2 <= 2450411633U)
                                    {
                                        if (stringHash2 <= 1767952217U)
                                        {
                                            if ((int)stringHash2 != 977678917)
                                            {
                                                if ((int)stringHash2 == 1767952217 && lower2 == "referenceangle")
                                                {
                                                    ((PrismaticJoint)joint).ReferenceAngle = (FP)float.Parse(element3.Value);
                                                    break;
                                                }
                                                break;
                                            }
                                            if (lower2 == "maxmotorforce")
                                            {
                                                ((PrismaticJoint)joint).MaxMotorForce = (FP)float.Parse(element3.Value);
                                                break;
                                            }
                                            break;
                                        }
                                        if ((int)stringHash2 != 1831579124)
                                        {
                                            if ((int)stringHash2 != -1894888520)
                                            {
                                                if ((int)stringHash2 == -1844555663 && lower2 == "localanchorb")
                                                {
                                                    ((PrismaticJoint)joint).LocalAnchorB = WorldXmlDeserializer.ReadVector(element3);
                                                    break;
                                                }
                                                break;
                                            }
                                            if (lower2 == "localanchora")
                                            {
                                                ((PrismaticJoint)joint).LocalAnchorA = WorldXmlDeserializer.ReadVector(element3);
                                                break;
                                            }
                                            break;
                                        }
                                        if (lower2 == "axis")
                                        {
                                            ((PrismaticJoint)joint).Axis = WorldXmlDeserializer.ReadVector(element3);
                                            break;
                                        }
                                        break;
                                    }
                                    if (stringHash2 <= 3184934789U)
                                    {
                                        if ((int)stringHash2 != -1413619971)
                                        {
                                            if ((int)stringHash2 == -1110032507 && lower2 == "lowertranslation")
                                            {
                                                ((PrismaticJoint)joint).LowerLimit = (FP)float.Parse(element3.Value);
                                                break;
                                            }
                                            break;
                                        }
                                        if (lower2 == "enablelimit")
                                        {
                                            ((PrismaticJoint)joint).LimitEnabled = bool.Parse(element3.Value);
                                            break;
                                        }
                                        break;
                                    }
                                    if ((int)stringHash2 != -885803291)
                                    {
                                        if ((int)stringHash2 != -811130850)
                                        {
                                            if ((int)stringHash2 == -633077889 && lower2 == "enablemotor")
                                            {
                                                ((PrismaticJoint)joint).MotorEnabled = bool.Parse(element3.Value);
                                                break;
                                            }
                                            break;
                                        }
                                        if (lower2 == "uppertranslation")
                                        {
                                            ((PrismaticJoint)joint).UpperLimit = (FP)float.Parse(element3.Value);
                                            break;
                                        }
                                        break;
                                    }
                                    if (lower2 == "motorspeed")
                                    {
                                        ((PrismaticJoint)joint).MotorSpeed = (FP)float.Parse(element3.Value);
                                        break;
                                    }
                                    break;
                                case JointType.Distance:
                                    string lower3 = element3.Name.ToLower();
                                    if (!(lower3 == "dampingratio"))
                                    {
                                        if (!(lower3 == "frequencyhz"))
                                        {
                                            if (!(lower3 == "length"))
                                            {
                                                if (!(lower3 == "localanchora"))
                                                {
                                                    if (lower3 == "localanchorb")
                                                    {
                                                        ((DistanceJoint)joint).LocalAnchorB = WorldXmlDeserializer.ReadVector(element3);
                                                        break;
                                                    }
                                                    break;
                                                }
                                              ((DistanceJoint)joint).LocalAnchorA = WorldXmlDeserializer.ReadVector(element3);
                                                break;
                                            }
                                          ((DistanceJoint)joint).Length = (FP)float.Parse(element3.Value);
                                            break;
                                        }
                                      ((DistanceJoint)joint).Frequency = (FP)float.Parse(element3.Value);
                                        break;
                                    }
                                ((DistanceJoint)joint).DampingRatio = (FP)float.Parse(element3.Value);
                                    break;
                                case JointType.Pulley:
                                    string lower4 = element3.Name.ToLower();
                                    // ISSUE: reference to a compiler-generated method
                                    uint stringHash3 = \u003CPrivateImplementationDetails\u003E.ComputeStringHash(lower4);
                                    if (stringHash3 <= 2450411633U)
                                    {
                                        if (stringHash3 <= 110225957U)
                                        {
                                            if ((int)stringHash3 != 2869265)
                                            {
                                                if ((int)stringHash3 == 110225957 && lower4 == "constant")
                                                {
                                                    ((PulleyJoint)joint).Constant = (FP)float.Parse(element3.Value);
                                                    break;
                                                }
                                                break;
                                            }
                                            if (lower4 == "worldanchora")
                                            {
                                                joint.WorldAnchorA = WorldXmlDeserializer.ReadVector(element3);
                                                break;
                                            }
                                            break;
                                        }
                                        if ((int)stringHash3 != -1894888520)
                                        {
                                            if ((int)stringHash3 == -1844555663 && lower4 == "localanchorb")
                                            {
                                                ((PulleyJoint)joint).LocalAnchorB = WorldXmlDeserializer.ReadVector(element3);
                                                break;
                                            }
                                            break;
                                        }
                                        if (lower4 == "localanchora")
                                        {
                                            ((PulleyJoint)joint).LocalAnchorA = WorldXmlDeserializer.ReadVector(element3);
                                            break;
                                        }
                                        break;
                                    }
                                    if (stringHash3 <= 4106598556U)
                                    {
                                        if ((int)stringHash3 != -1055777148)
                                        {
                                            if ((int)stringHash3 == -188368740 && lower4 == "lengtha")
                                            {
                                                ((PulleyJoint)joint).LengthA = (FP)float.Parse(element3.Value);
                                                break;
                                            }
                                            break;
                                        }
                                        if (lower4 == "ratio")
                                        {
                                            ((PulleyJoint)joint).Ratio = (FP)float.Parse(element3.Value);
                                            break;
                                        }
                                        break;
                                    }
                                    if ((int)stringHash3 != -138035883)
                                    {
                                        if ((int)stringHash3 == -47463592 && lower4 == "worldanchorb")
                                        {
                                            joint.WorldAnchorB = WorldXmlDeserializer.ReadVector(element3);
                                            break;
                                        }
                                        break;
                                    }
                                    if (lower4 == "lengthb")
                                    {
                                        ((PulleyJoint)joint).LengthB = (FP)float.Parse(element3.Value);
                                        break;
                                    }
                                    break;
                                case JointType.Gear:
                                    throw new Exception("Gear joint is unsupported");
                                case JointType.Wheel:
                                    string lower5 = element3.Name.ToLower();
                                    // ISSUE: reference to a compiler-generated method
                                    uint stringHash4 = \u003CPrivateImplementationDetails\u003E.ComputeStringHash(lower5);
                                    if (stringHash4 <= 2450411633U)
                                    {
                                        if (stringHash4 <= 1831579124U)
                                        {
                                            if ((int)stringHash4 != 1394215284)
                                            {
                                                if ((int)stringHash4 == 1831579124 && lower5 == "axis")
                                                {
                                                    ((WheelJoint)joint).Axis = WorldXmlDeserializer.ReadVector(element3);
                                                    break;
                                                }
                                                break;
                                            }
                                            if (lower5 == "maxmotortorque")
                                            {
                                                ((WheelJoint)joint).MaxMotorTorque = (FP)float.Parse(element3.Value);
                                                break;
                                            }
                                            break;
                                        }
                                        if ((int)stringHash4 != -1894888520)
                                        {
                                            if ((int)stringHash4 == -1844555663 && lower5 == "localanchorb")
                                            {
                                                ((WheelJoint)joint).LocalAnchorB = WorldXmlDeserializer.ReadVector(element3);
                                                break;
                                            }
                                            break;
                                        }
                                        if (lower5 == "localanchora")
                                        {
                                            ((WheelJoint)joint).LocalAnchorA = WorldXmlDeserializer.ReadVector(element3);
                                            break;
                                        }
                                        break;
                                    }
                                    if (stringHash4 <= 3409164005U)
                                    {
                                        if ((int)stringHash4 != -1596815325)
                                        {
                                            if ((int)stringHash4 == -885803291 && lower5 == "motorspeed")
                                            {
                                                ((WheelJoint)joint).MotorSpeed = (FP)float.Parse(element3.Value);
                                                break;
                                            }
                                            break;
                                        }
                                        if (lower5 == "frequencyhz")
                                        {
                                            ((WheelJoint)joint).Frequency = (FP)float.Parse(element3.Value);
                                            break;
                                        }
                                        break;
                                    }
                                    if ((int)stringHash4 != -633077889)
                                    {
                                        if ((int)stringHash4 == -82209612 && lower5 == "dampingratio")
                                        {
                                            ((WheelJoint)joint).DampingRatio = (FP)float.Parse(element3.Value);
                                            break;
                                        }
                                        break;
                                    }
                                    if (lower5 == "enablemotor")
                                    {
                                        ((WheelJoint)joint).MotorEnabled = bool.Parse(element3.Value);
                                        break;
                                    }
                                    break;
                                case JointType.Weld:
                                    string lower6 = element3.Name.ToLower();
                                    if (!(lower6 == "localanchora"))
                                    {
                                        if (lower6 == "localanchorb")
                                        {
                                            ((WeldJoint)joint).LocalAnchorB = WorldXmlDeserializer.ReadVector(element3);
                                            break;
                                        }
                                        break;
                                    }
                                ((WeldJoint)joint).LocalAnchorA = WorldXmlDeserializer.ReadVector(element3);
                                    break;
                                case JointType.Friction:
                                    string lower7 = element3.Name.ToLower();
                                    if (!(lower7 == "localanchora"))
                                    {
                                        if (!(lower7 == "localanchorb"))
                                        {
                                            if (!(lower7 == "maxforce"))
                                            {
                                                if (lower7 == "maxtorque")
                                                {
                                                    ((FrictionJoint)joint).MaxTorque = (FP)float.Parse(element3.Value);
                                                    break;
                                                }
                                                break;
                                            }
                                          ((FrictionJoint)joint).MaxForce = (FP)float.Parse(element3.Value);
                                            break;
                                        }
                                      ((FrictionJoint)joint).LocalAnchorB = WorldXmlDeserializer.ReadVector(element3);
                                        break;
                                    }
                                ((FrictionJoint)joint).LocalAnchorA = WorldXmlDeserializer.ReadVector(element3);
                                    break;
                                case JointType.Rope:
                                    string lower8 = element3.Name.ToLower();
                                    if (!(lower8 == "localanchora"))
                                    {
                                        if (!(lower8 == "localanchorb"))
                                        {
                                            if (lower8 == "maxlength")
                                            {
                                                ((RopeJoint)joint).MaxLength = (FP)float.Parse(element3.Value);
                                                break;
                                            }
                                            break;
                                        }
                                      ((RopeJoint)joint).LocalAnchorB = WorldXmlDeserializer.ReadVector(element3);
                                        break;
                                    }
                                ((RopeJoint)joint).LocalAnchorA = WorldXmlDeserializer.ReadVector(element3);
                                    break;
                                case JointType.Motor:
                                    string lower9 = element3.Name.ToLower();
                                    if (!(lower9 == "angularoffset"))
                                    {
                                        if (!(lower9 == "linearoffset"))
                                        {
                                            if (!(lower9 == "maxforce"))
                                            {
                                                if (!(lower9 == "maxtorque"))
                                                {
                                                    if (lower9 == "correctionfactor")
                                                    {
                                                        ((MotorJoint)joint).CorrectionFactor = (FP)float.Parse(element3.Value);
                                                        break;
                                                    }
                                                    break;
                                                }
                                              ((MotorJoint)joint).MaxTorque = (FP)float.Parse(element3.Value);
                                                break;
                                            }
                                          ((MotorJoint)joint).MaxForce = (FP)float.Parse(element3.Value);
                                            break;
                                        }
                                      ((MotorJoint)joint).LinearOffset = WorldXmlDeserializer.ReadVector(element3);
                                        break;
                                    }
                                ((MotorJoint)joint).AngularOffset = (FP)float.Parse(element3.Value);
                                    break;
                                case JointType.Angle:
                                    string lower10 = element3.Name.ToLower();
                                    if (!(lower10 == "biasfactor"))
                                    {
                                        if (!(lower10 == "maximpulse"))
                                        {
                                            if (!(lower10 == "softness"))
                                            {
                                                if (lower10 == "targetangle")
                                                {
                                                    ((AngleJoint)joint).TargetAngle = (FP)float.Parse(element3.Value);
                                                    break;
                                                }
                                                break;
                                            }
                                          ((AngleJoint)joint).Softness = (FP)float.Parse(element3.Value);
                                            break;
                                        }
                                      ((AngleJoint)joint).MaxImpulse = (FP)float.Parse(element3.Value);
                                        break;
                                    }
                                ((AngleJoint)joint).BiasFactor = (FP)float.Parse(element3.Value);
                                    break;
                            }
                        }
                    }
                }
            }
            world.ProcessChanges();
        }

        private static TSVector2 ReadVector(XMLFragmentElement node)
        {
            string[] strArray = node.Value.Split(' ');
            return new TSVector2((FP)float.Parse(strArray[0]), (FP)float.Parse(strArray[1]));
        }

        private static object ReadSimpleType(XMLFragmentElement node, Type type, bool outer)
        {
            if (type == null)
                return WorldXmlDeserializer.ReadSimpleType(node.Elements[1], Type.GetType(node.Elements[0].Value), outer);
            XmlSerializer xmlSerializer = new XmlSerializer(type);
            new XmlSerializerNamespaces().Add("", "");
            using (MemoryStream memoryStream = new MemoryStream())
            {
                StreamWriter streamWriter = new StreamWriter((Stream)memoryStream);
                streamWriter.Write(outer ? node.OuterXml : node.InnerXml);
                streamWriter.Flush();
                memoryStream.Position = 0L;
                return xmlSerializer.Deserialize(XmlReader.Create((Stream)memoryStream, new XmlReaderSettings()
                {
                    ConformanceLevel = ConformanceLevel.Fragment
                }));
            }
        }
    }
}
