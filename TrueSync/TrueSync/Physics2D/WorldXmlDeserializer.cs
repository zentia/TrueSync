namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using TrueSync;

    internal static class WorldXmlDeserializer
    {
        internal static TrueSync.Physics2D.World Deserialize(Stream stream)
        {
            TrueSync.Physics2D.World world = new TrueSync.Physics2D.World(TSVector2.zero);
            Deserialize(world, stream);
            return world;
        }

        private static void Deserialize(TrueSync.Physics2D.World world, Stream stream)
        {
            uint num2;
            List<Body> list = new List<Body>();
            List<Fixture> list2 = new List<Fixture>();
            List<TrueSync.Physics2D.Joint> list3 = new List<TrueSync.Physics2D.Joint>();
            List<TrueSync.Physics2D.Shape> list4 = new List<TrueSync.Physics2D.Shape>();
            XMLFragmentElement element = XMLFragmentParser.LoadFromStream(stream);
            if (element.Name.ToLower() != "world")
            {
                throw new Exception();
            }
            foreach (XMLFragmentElement element2 in element.Elements)
            {
                if (element2.Name.ToLower() == "gravity")
                {
                    world.Gravity = ReadVector(element2);
                    break;
                }
            }
            foreach (XMLFragmentElement element3 in element.Elements)
            {
                if (element3.Name.ToLower() == "shapes")
                {
                    foreach (XMLFragmentElement element4 in element3.Elements)
                    {
                        if (element4.Name.ToLower() != "shape")
                        {
                            throw new Exception();
                        }
                        ShapeType type = (ShapeType) Enum.Parse(typeof(ShapeType), element4.Attributes[0].Value, true);
                        FP fp = float.Parse(element4.Attributes[1].Value);
                        switch (type)
                        {
                            case ShapeType.Circle:
                            {
                                CircleShape item = new CircleShape {
                                    _density = fp
                                };
                                foreach (XMLFragmentElement element5 in element4.Elements)
                                {
                                    string str = element5.Name.ToLower();
                                    if (!(str == "radius"))
                                    {
                                        if (str != "position")
                                        {
                                            throw new Exception();
                                        }
                                    }
                                    else
                                    {
                                        item.Radius = float.Parse(element5.Value);
                                        continue;
                                    }
                                    item.Position = ReadVector(element5);
                                }
                                list4.Add(item);
                                break;
                            }
                            case ShapeType.Edge:
                            {
                                EdgeShape shape3 = new EdgeShape {
                                    _density = fp
                                };
                                foreach (XMLFragmentElement element8 in element4.Elements)
                                {
                                    string str3 = element8.Name.ToLower();
                                    if (!(str3 == "hasvertex0"))
                                    {
                                        if (str3 != "hasvertex3")
                                        {
                                            if (str3 == "vertex0")
                                            {
                                                goto Label_03FC;
                                            }
                                            if (str3 == "vertex1")
                                            {
                                                goto Label_040D;
                                            }
                                            if (str3 == "vertex2")
                                            {
                                                goto Label_041E;
                                            }
                                            if (str3 != "vertex3")
                                            {
                                                throw new Exception();
                                            }
                                            goto Label_042F;
                                        }
                                    }
                                    else
                                    {
                                        shape3.HasVertex0 = bool.Parse(element8.Value);
                                        continue;
                                    }
                                    shape3.HasVertex0 = bool.Parse(element8.Value);
                                    continue;
                                Label_03FC:
                                    shape3.Vertex0 = ReadVector(element8);
                                    continue;
                                Label_040D:
                                    shape3.Vertex1 = ReadVector(element8);
                                    continue;
                                Label_041E:
                                    shape3.Vertex2 = ReadVector(element8);
                                    continue;
                                Label_042F:
                                    shape3.Vertex3 = ReadVector(element8);
                                }
                                list4.Add(shape3);
                                break;
                            }
                            case ShapeType.Polygon:
                            {
                                PolygonShape shape2 = new PolygonShape {
                                    _density = fp
                                };
                                foreach (XMLFragmentElement element6 in element4.Elements)
                                {
                                    string str2 = element6.Name.ToLower();
                                    if (!(str2 == "vertices"))
                                    {
                                        if (str2 == "centroid")
                                        {
                                            goto Label_02FC;
                                        }
                                    }
                                    else
                                    {
                                        List<TSVector2> vertices = new List<TSVector2>(element6.Elements.Count);
                                        foreach (XMLFragmentElement element7 in element6.Elements)
                                        {
                                            vertices.Add(ReadVector(element7));
                                        }
                                        shape2.Vertices = new Vertices(vertices);
                                    }
                                    continue;
                                Label_02FC:
                                    shape2.MassData.Centroid = ReadVector(element6);
                                }
                                list4.Add(shape2);
                                break;
                            }
                            case ShapeType.Chain:
                            {
                                ChainShape shape4 = new ChainShape {
                                    _density = fp
                                };
                                foreach (XMLFragmentElement element9 in element4.Elements)
                                {
                                    string str4 = element9.Name.ToLower();
                                    if (!(str4 == "vertices"))
                                    {
                                        if (str4 != "nextvertex")
                                        {
                                            if (str4 != "prevvertex")
                                            {
                                                throw new Exception();
                                            }
                                            goto Label_0554;
                                        }
                                    }
                                    else
                                    {
                                        List<TSVector2> list6 = new List<TSVector2>(element9.Elements.Count);
                                        foreach (XMLFragmentElement element10 in element9.Elements)
                                        {
                                            list6.Add(ReadVector(element10));
                                        }
                                        shape4.Vertices = new Vertices(list6);
                                        continue;
                                    }
                                    shape4.NextVertex = ReadVector(element9);
                                    continue;
                                Label_0554:
                                    shape4.PrevVertex = ReadVector(element9);
                                }
                                list4.Add(shape4);
                                break;
                            }
                        }
                    }
                }
            }
            foreach (XMLFragmentElement element11 in element.Elements)
            {
                if (element11.Name.ToLower() == "fixtures")
                {
                    foreach (XMLFragmentElement element12 in element11.Elements)
                    {
                        Fixture fixture = new Fixture();
                        if (element12.Name.ToLower() != "fixture")
                        {
                            throw new Exception();
                        }
                        fixture.FixtureId = int.Parse(element12.Attributes[0].Value);
                        foreach (XMLFragmentElement element13 in element12.Elements)
                        {
                            string str5 = element13.Name.ToLower();
                            if (!(str5 == "filterdata"))
                            {
                                if (str5 == "friction")
                                {
                                    goto Label_0809;
                                }
                                if (str5 == "issensor")
                                {
                                    goto Label_0824;
                                }
                                if (str5 == "restitution")
                                {
                                    goto Label_083A;
                                }
                                if (str5 == "userdata")
                                {
                                    goto Label_0855;
                                }
                            }
                            else
                            {
                                foreach (XMLFragmentElement element14 in element13.Elements)
                                {
                                    char[] chArray1;
                                    string str6 = element14.Name.ToLower();
                                    if (!(str6 == "categorybits"))
                                    {
                                        if (str6 == "maskbits")
                                        {
                                            goto Label_0771;
                                        }
                                        if (str6 == "groupindex")
                                        {
                                            goto Label_0786;
                                        }
                                        if (str6 == "CollisionIgnores")
                                        {
                                            goto Label_079B;
                                        }
                                    }
                                    else
                                    {
                                        fixture._collisionCategories = (Category) int.Parse(element14.Value);
                                    }
                                    continue;
                                Label_0771:
                                    fixture._collidesWith = (Category) int.Parse(element14.Value);
                                    continue;
                                Label_0786:
                                    fixture._collisionGroup = short.Parse(element14.Value);
                                    continue;
                                Label_079B:
                                    chArray1 = new char[] { '|' };
                                    string[] strArray = element14.Value.Split(chArray1);
                                    foreach (string str7 in strArray)
                                    {
                                        fixture._collisionIgnores.Add(int.Parse(str7));
                                    }
                                }
                            }
                            continue;
                        Label_0809:
                            fixture.Friction = float.Parse(element13.Value);
                            continue;
                        Label_0824:
                            fixture.IsSensor = bool.Parse(element13.Value);
                            continue;
                        Label_083A:
                            fixture.Restitution = float.Parse(element13.Value);
                            continue;
                        Label_0855:
                            fixture.UserData = ReadSimpleType(element13, null, false);
                        }
                        list2.Add(fixture);
                    }
                }
            }
            foreach (XMLFragmentElement element15 in element.Elements)
            {
                if (element15.Name.ToLower() == "bodies")
                {
                    foreach (XMLFragmentElement element16 in element15.Elements)
                    {
                        TSVector2? position = null;
                        Body body = new Body(world, position, 0, null);
                        if (element16.Name.ToLower() != "body")
                        {
                            throw new Exception();
                        }
                        body.BodyType = (BodyType) Enum.Parse(typeof(BodyType), element16.Attributes[0].Value, true);
                        foreach (XMLFragmentElement element17 in element16.Elements)
                        {
                            TSVector2 vector;
                            FP fp2;
                            string s = element17.Name.ToLower();
                            num2 = <PrivateImplementationDetails>.ComputeStringHash(s);
                            switch (num2)
                            {
                                case 0x7a04061b:
                                {
                                    if (s == "fixedrotation")
                                    {
                                        goto Label_0C8F;
                                    }
                                    continue;
                                }
                                case 0x934f4e0a:
                                {
                                    if (s == "position")
                                    {
                                        goto Label_0CDA;
                                    }
                                    continue;
                                }
                                case 0xac01f355:
                                {
                                    if (s == "allowsleep")
                                    {
                                        goto Label_0BDD;
                                    }
                                    continue;
                                }
                                case 0x1b1cc8c3:
                                {
                                    if (s == "bindings")
                                    {
                                        goto Label_0D15;
                                    }
                                    continue;
                                }
                                case 0x3c92b0b3:
                                {
                                    if (s == "linearvelocity")
                                    {
                                        goto Label_0CC6;
                                    }
                                    continue;
                                }
                                case 0x5ad2d9c6:
                                {
                                    if (s == "lineardamping")
                                    {
                                        goto Label_0CA8;
                                    }
                                    continue;
                                }
                                case 0xad544418:
                                {
                                    if (s == "angle")
                                    {
                                        goto Label_0BF6;
                                    }
                                    continue;
                                }
                                case 0xadbdac5e:
                                {
                                    if (s == "angularvelocity")
                                    {
                                        goto Label_0C3F;
                                    }
                                    continue;
                                }
                                case 0xcc12c421:
                                {
                                    if (s == "angulardamping")
                                    {
                                        goto Label_0C21;
                                    }
                                    continue;
                                }
                                case 0xdabe11a2:
                                {
                                    if (s == "userdata")
                                    {
                                        goto Label_0CFF;
                                    }
                                    continue;
                                }
                                case 0xe894a379:
                                {
                                    if (s == "bullet")
                                    {
                                        goto Label_0C76;
                                    }
                                    continue;
                                }
                                case 0xd71034dc:
                                {
                                    if (s == "awake")
                                    {
                                        goto Label_0C5D;
                                    }
                                    continue;
                                }
                                case 0xd975992f:
                                {
                                    if (s == "active")
                                    {
                                        break;
                                    }
                                    continue;
                                }
                                default:
                                {
                                    continue;
                                }
                            }
                            body._enabled = bool.Parse(element17.Value);
                            continue;
                        Label_0BDD:
                            body.SleepingAllowed = bool.Parse(element17.Value);
                            continue;
                        Label_0BF6:
                            vector = body.Position;
                            body.SetTransformIgnoreContacts(ref vector, float.Parse(element17.Value));
                            continue;
                        Label_0C21:
                            body.AngularDamping = float.Parse(element17.Value);
                            continue;
                        Label_0C3F:
                            body.AngularVelocity = float.Parse(element17.Value);
                            continue;
                        Label_0C5D:
                            body.Awake = bool.Parse(element17.Value);
                            continue;
                        Label_0C76:
                            body.IsBullet = bool.Parse(element17.Value);
                            continue;
                        Label_0C8F:
                            body.FixedRotation = bool.Parse(element17.Value);
                            continue;
                        Label_0CA8:
                            body.LinearDamping = float.Parse(element17.Value);
                            continue;
                        Label_0CC6:
                            body.LinearVelocity = ReadVector(element17);
                            continue;
                        Label_0CDA:
                            fp2 = body.Rotation;
                            TSVector2 vector2 = ReadVector(element17);
                            body.SetTransformIgnoreContacts(ref vector2, fp2);
                            continue;
                        Label_0CFF:
                            body.UserData = ReadSimpleType(element17, null, false);
                            continue;
                        Label_0D15:
                            foreach (XMLFragmentElement element18 in element17.Elements)
                            {
                                Fixture fixture2 = list2[int.Parse(element18.Attributes[0].Value)];
                                fixture2.Shape = list4[int.Parse(element18.Attributes[1].Value)].Clone();
                                fixture2.CloneOnto(body);
                            }
                        }
                        list.Add(body);
                    }
                }
            }
            foreach (XMLFragmentElement element19 in element.Elements)
            {
                if (element19.Name.ToLower() == "joints")
                {
                    foreach (XMLFragmentElement element20 in element19.Elements)
                    {
                        TrueSync.Physics2D.Joint joint;
                        if (element20.Name.ToLower() != "joint")
                        {
                            throw new Exception();
                        }
                        JointType type3 = (JointType) Enum.Parse(typeof(JointType), element20.Attributes[0].Value, true);
                        int num3 = -1;
                        int num4 = -1;
                        bool flag10 = false;
                        object obj2 = null;
                        foreach (XMLFragmentElement element21 in element20.Elements)
                        {
                            string str9 = element21.Name.ToLower();
                            if (!(str9 == "bodya"))
                            {
                                if (str9 == "bodyb")
                                {
                                    goto Label_0F26;
                                }
                                if (str9 == "collideconnected")
                                {
                                    goto Label_0F36;
                                }
                                if (str9 == "userdata")
                                {
                                    goto Label_0F46;
                                }
                            }
                            else
                            {
                                num3 = int.Parse(element21.Value);
                            }
                            continue;
                        Label_0F26:
                            num4 = int.Parse(element21.Value);
                            continue;
                        Label_0F36:
                            flag10 = bool.Parse(element21.Value);
                            continue;
                        Label_0F46:
                            obj2 = ReadSimpleType(element21, null, false);
                        }
                        Body body2 = list[num3];
                        Body body3 = list[num4];
                        switch (type3)
                        {
                            case JointType.Revolute:
                                joint = new RevoluteJoint();
                                break;

                            case JointType.Prismatic:
                                joint = new TrueSync.Physics2D.PrismaticJoint();
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
                        joint.CollideConnected = flag10;
                        joint.UserData = obj2;
                        joint.BodyA = body2;
                        joint.BodyB = body3;
                        list3.Add(joint);
                        world.AddJoint(joint);
                        foreach (XMLFragmentElement element22 in element20.Elements)
                        {
                            string str12;
                            string str13;
                            string str14;
                            string str15;
                            switch (type3)
                            {
                                case JointType.Revolute:
                                {
                                    str15 = element22.Name.ToLower();
                                    num2 = <PrivateImplementationDetails>.ComputeStringHash(str15);
                                    if (num2 > 0x8f0e4bb8)
                                    {
                                        goto Label_1A17;
                                    }
                                    if (num2 > 0x531a0974)
                                    {
                                        goto Label_19FB;
                                    }
                                    switch (num2)
                                    {
                                        case 0x2bf133f9:
                                            goto Label_1AE7;

                                        case 0x531a0974:
                                            goto Label_1ABB;
                                    }
                                    continue;
                                }
                                case JointType.Prismatic:
                                {
                                    str13 = element22.Name.ToLower();
                                    num2 = <PrivateImplementationDetails>.ComputeStringHash(str13);
                                    if (num2 > 0x920e5071)
                                    {
                                        goto Label_151D;
                                    }
                                    if (num2 > 0x6960cf59)
                                    {
                                        goto Label_14F0;
                                    }
                                    switch (num2)
                                    {
                                        case 0x3a463245:
                                            goto Label_15DA;

                                        case 0x6960cf59:
                                            goto Label_1632;
                                    }
                                    continue;
                                }
                                case JointType.Distance:
                                {
                                    switch (element22.Name.ToLower())
                                    {
                                        case "frequencyhz":
                                            goto Label_113E;

                                        case "length":
                                            goto Label_115E;

                                        case "localanchora":
                                            goto Label_117E;

                                        case "localanchorb":
                                            goto Label_1194;
                                    }
                                    continue;
                                }
                                case JointType.Pulley:
                                {
                                    str14 = element22.Name.ToLower();
                                    num2 = <PrivateImplementationDetails>.ComputeStringHash(str14);
                                    if (num2 > 0x920e5071)
                                    {
                                        goto Label_17DD;
                                    }
                                    if (num2 > 0x691ea25)
                                    {
                                        goto Label_17BE;
                                    }
                                    switch (num2)
                                    {
                                        case 0x2bc811:
                                            goto Label_181B;

                                        case 0x691ea25:
                                            goto Label_18B5;
                                    }
                                    continue;
                                }
                                case JointType.Gear:
                                    throw new Exception("Gear joint is unsupported");

                                case JointType.Wheel:
                                {
                                    str12 = element22.Name.ToLower();
                                    num2 = <PrivateImplementationDetails>.ComputeStringHash(str12);
                                    if (num2 > 0x920e5071)
                                    {
                                        goto Label_12CD;
                                    }
                                    if (num2 > 0x6d2badf4)
                                    {
                                        goto Label_12B4;
                                    }
                                    switch (num2)
                                    {
                                        case 0x531a0974:
                                            goto Label_1379;

                                        case 0x6d2badf4:
                                            goto Label_13A5;
                                    }
                                    continue;
                                }
                                case JointType.Weld:
                                {
                                    switch (element22.Name.ToLower())
                                    {
                                        case "localanchora":
                                            goto Label_1C6D;

                                        case "localanchorb":
                                            goto Label_1C83;
                                    }
                                    continue;
                                }
                                case JointType.Friction:
                                {
                                    switch (element22.Name.ToLower())
                                    {
                                        case "localanchora":
                                            goto Label_11F9;

                                        case "localanchorb":
                                            goto Label_120F;

                                        case "maxforce":
                                            goto Label_1225;

                                        case "maxtorque":
                                            goto Label_1245;
                                    }
                                    continue;
                                }
                                case JointType.Rope:
                                {
                                    switch (element22.Name.ToLower())
                                    {
                                        case "localanchora":
                                            goto Label_1CDA;

                                        case "localanchorb":
                                            goto Label_1CF0;

                                        case "maxlength":
                                            goto Label_1D06;
                                    }
                                    continue;
                                }
                                case JointType.Motor:
                                {
                                    switch (element22.Name.ToLower())
                                    {
                                        case "angularoffset":
                                            goto Label_1E62;

                                        case "linearoffset":
                                            goto Label_1E82;

                                        case "maxforce":
                                            goto Label_1E98;

                                        case "maxtorque":
                                            goto Label_1EB8;

                                        case "correctionfactor":
                                            goto Label_1ED8;
                                    }
                                    continue;
                                }
                                case JointType.Angle:
                                {
                                    string str18 = element22.Name.ToLower();
                                    if (!(str18 == "biasfactor"))
                                    {
                                        if (str18 == "maximpulse")
                                        {
                                            goto Label_1DA3;
                                        }
                                        if (str18 == "softness")
                                        {
                                            goto Label_1DC3;
                                        }
                                        if (str18 == "targetangle")
                                        {
                                            goto Label_1DE3;
                                        }
                                    }
                                    else
                                    {
                                        ((AngleJoint) joint).BiasFactor = float.Parse(element22.Value);
                                    }
                                    continue;
                                }
                                default:
                                {
                                    continue;
                                }
                            }
                            ((DistanceJoint) joint).DampingRatio = float.Parse(element22.Value);
                            continue;
                        Label_113E:
                            ((DistanceJoint) joint).Frequency = float.Parse(element22.Value);
                            continue;
                        Label_115E:
                            ((DistanceJoint) joint).Length = float.Parse(element22.Value);
                            continue;
                        Label_117E:
                            ((DistanceJoint) joint).LocalAnchorA = ReadVector(element22);
                            continue;
                        Label_1194:
                            ((DistanceJoint) joint).LocalAnchorB = ReadVector(element22);
                            continue;
                        Label_11F9:
                            ((FrictionJoint) joint).LocalAnchorA = ReadVector(element22);
                            continue;
                        Label_120F:
                            ((FrictionJoint) joint).LocalAnchorB = ReadVector(element22);
                            continue;
                        Label_1225:
                            ((FrictionJoint) joint).MaxForce = float.Parse(element22.Value);
                            continue;
                        Label_1245:
                            ((FrictionJoint) joint).MaxTorque = float.Parse(element22.Value);
                            continue;
                        Label_12B4:
                            switch (num2)
                            {
                                case 0x8f0e4bb8:
                                {
                                    if (str12 == "localanchora")
                                    {
                                        goto Label_13D9;
                                    }
                                    continue;
                                }
                                case 0x920e5071:
                                {
                                    if (str12 == "localanchorb")
                                    {
                                        goto Label_13F2;
                                    }
                                    continue;
                                }
                                default:
                                {
                                    continue;
                                }
                            }
                        Label_12CD:
                            if (num2 <= 0xcb33b6e5)
                            {
                                switch (num2)
                                {
                                    case 0xa0d28823:
                                        if (str12 == "frequencyhz")
                                        {
                                            goto Label_146B;
                                        }
                                        break;

                                    case 0xcb33b6e5:
                                        if (str12 == "motorspeed")
                                        {
                                            goto Label_140B;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                switch (num2)
                                {
                                    case 0xda43ff7f:
                                        if (str12 == "enablemotor")
                                        {
                                            goto Label_13BB;
                                        }
                                        break;

                                    case 0xfb1994b4:
                                        if (str12 == "dampingratio")
                                        {
                                            goto Label_142B;
                                        }
                                        break;
                                }
                            }
                            continue;
                        Label_1379:
                            if (str12 == "maxmotortorque")
                            {
                                goto Label_144B;
                            }
                            continue;
                        Label_13A5:
                            if (str12 == "axis")
                            {
                                goto Label_148B;
                            }
                            continue;
                        Label_13BB:
                            ((WheelJoint) joint).MotorEnabled = bool.Parse(element22.Value);
                            continue;
                        Label_13D9:
                            ((WheelJoint) joint).LocalAnchorA = ReadVector(element22);
                            continue;
                        Label_13F2:
                            ((WheelJoint) joint).LocalAnchorB = ReadVector(element22);
                            continue;
                        Label_140B:
                            ((WheelJoint) joint).MotorSpeed = float.Parse(element22.Value);
                            continue;
                        Label_142B:
                            ((WheelJoint) joint).DampingRatio = float.Parse(element22.Value);
                            continue;
                        Label_144B:
                            ((WheelJoint) joint).MaxMotorTorque = float.Parse(element22.Value);
                            continue;
                        Label_146B:
                            ((WheelJoint) joint).Frequency = float.Parse(element22.Value);
                            continue;
                        Label_148B:
                            ((WheelJoint) joint).Axis = ReadVector(element22);
                            continue;
                        Label_14F0:
                            switch (num2)
                            {
                                case 0x6d2badf4:
                                {
                                    if (str13 == "axis")
                                    {
                                        goto Label_16B6;
                                    }
                                    continue;
                                }
                                case 0x8f0e4bb8:
                                {
                                    if (str13 == "localanchora")
                                    {
                                        goto Label_1684;
                                    }
                                    continue;
                                }
                                case 0x920e5071:
                                {
                                    if (str13 == "localanchorb")
                                    {
                                        goto Label_169D;
                                    }
                                    continue;
                                }
                                default:
                                {
                                    continue;
                                }
                            }
                        Label_151D:
                            if (num2 <= 0xbdd63f85)
                            {
                                switch (num2)
                                {
                                    case 0xabbddefd:
                                        if (str13 == "enablelimit")
                                        {
                                            goto Label_1648;
                                        }
                                        break;

                                    case 0xbdd63f85:
                                        if (str13 == "lowertranslation")
                                        {
                                            goto Label_1712;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                switch (num2)
                                {
                                    case 0xcb33b6e5:
                                        if (str13 == "motorspeed")
                                        {
                                            goto Label_16F2;
                                        }
                                        break;

                                    case 0xcfa7201e:
                                        if (str13 == "uppertranslation")
                                        {
                                            goto Label_1732;
                                        }
                                        break;

                                    case 0xda43ff7f:
                                        if (str13 == "enablemotor")
                                        {
                                            goto Label_1666;
                                        }
                                        break;
                                }
                            }
                            continue;
                        Label_15DA:
                            if (str13 == "maxmotorforce")
                            {
                                goto Label_16CF;
                            }
                            continue;
                        Label_1632:
                            if (str13 == "referenceangle")
                            {
                                goto Label_1752;
                            }
                            continue;
                        Label_1648:
                            ((TrueSync.Physics2D.PrismaticJoint) joint).LimitEnabled = bool.Parse(element22.Value);
                            continue;
                        Label_1666:
                            ((TrueSync.Physics2D.PrismaticJoint) joint).MotorEnabled = bool.Parse(element22.Value);
                            continue;
                        Label_1684:
                            ((TrueSync.Physics2D.PrismaticJoint) joint).LocalAnchorA = ReadVector(element22);
                            continue;
                        Label_169D:
                            ((TrueSync.Physics2D.PrismaticJoint) joint).LocalAnchorB = ReadVector(element22);
                            continue;
                        Label_16B6:
                            ((TrueSync.Physics2D.PrismaticJoint) joint).Axis = ReadVector(element22);
                            continue;
                        Label_16CF:
                            ((TrueSync.Physics2D.PrismaticJoint) joint).MaxMotorForce = float.Parse(element22.Value);
                            continue;
                        Label_16F2:
                            ((TrueSync.Physics2D.PrismaticJoint) joint).MotorSpeed = float.Parse(element22.Value);
                            continue;
                        Label_1712:
                            ((TrueSync.Physics2D.PrismaticJoint) joint).LowerLimit = float.Parse(element22.Value);
                            continue;
                        Label_1732:
                            ((TrueSync.Physics2D.PrismaticJoint) joint).UpperLimit = float.Parse(element22.Value);
                            continue;
                        Label_1752:
                            ((TrueSync.Physics2D.PrismaticJoint) joint).ReferenceAngle = float.Parse(element22.Value);
                            continue;
                        Label_17BE:
                            switch (num2)
                            {
                                case 0x8f0e4bb8:
                                {
                                    if (str14 == "localanchora")
                                    {
                                        goto Label_1940;
                                    }
                                    continue;
                                }
                                case 0x920e5071:
                                {
                                    if (str14 == "localanchorb")
                                    {
                                        goto Label_1956;
                                    }
                                    continue;
                                }
                                default:
                                {
                                    continue;
                                }
                            }
                        Label_17DD:
                            if (num2 <= 0xf4c5b89c)
                            {
                                switch (num2)
                                {
                                    case 0xc1121e84:
                                        if (str14 == "ratio")
                                        {
                                            goto Label_196C;
                                        }
                                        break;

                                    case 0xf4c5b89c:
                                        if (str14 == "lengtha")
                                        {
                                            goto Label_18FD;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                switch (num2)
                                {
                                    case 0xf7c5bd55:
                                        if (str14 == "lengthb")
                                        {
                                            goto Label_1920;
                                        }
                                        break;

                                    case 0xfd2bc358:
                                        if (str14 == "worldanchorb")
                                        {
                                            goto Label_18E4;
                                        }
                                        break;
                                }
                            }
                            continue;
                        Label_181B:
                            if (str14 == "worldanchora")
                            {
                                goto Label_18CB;
                            }
                            continue;
                        Label_18B5:
                            if (str14 == "constant")
                            {
                                goto Label_198C;
                            }
                            continue;
                        Label_18CB:
                            ((PulleyJoint) joint).WorldAnchorA = ReadVector(element22);
                            continue;
                        Label_18E4:
                            ((PulleyJoint) joint).WorldAnchorB = ReadVector(element22);
                            continue;
                        Label_18FD:
                            ((PulleyJoint) joint).LengthA = float.Parse(element22.Value);
                            continue;
                        Label_1920:
                            ((PulleyJoint) joint).LengthB = float.Parse(element22.Value);
                            continue;
                        Label_1940:
                            ((PulleyJoint) joint).LocalAnchorA = ReadVector(element22);
                            continue;
                        Label_1956:
                            ((PulleyJoint) joint).LocalAnchorB = ReadVector(element22);
                            continue;
                        Label_196C:
                            ((PulleyJoint) joint).Ratio = float.Parse(element22.Value);
                            continue;
                        Label_198C:
                            ((PulleyJoint) joint).Constant = float.Parse(element22.Value);
                            continue;
                        Label_19FB:
                            switch (num2)
                            {
                                case 0x6960cf59:
                                {
                                    if (str15 == "referenceangle")
                                    {
                                        goto Label_1C1A;
                                    }
                                    continue;
                                }
                                case 0x8f0e4bb8:
                                {
                                    if (str15 == "localanchora")
                                    {
                                        goto Label_1B65;
                                    }
                                    continue;
                                }
                                default:
                                {
                                    continue;
                                }
                            }
                        Label_1A17:
                            if (num2 <= 0xabbddefd)
                            {
                                switch (num2)
                                {
                                    case 0x920e5071:
                                        if (str15 == "localanchorb")
                                        {
                                            goto Label_1B7E;
                                        }
                                        break;

                                    case 0xabbddefd:
                                        if (str15 == "enablelimit")
                                        {
                                            goto Label_1B29;
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                switch (num2)
                                {
                                    case 0xcb33b6e5:
                                        if (str15 == "motorspeed")
                                        {
                                            goto Label_1BBA;
                                        }
                                        break;

                                    case 0xda43ff7f:
                                        if (str15 == "enablemotor")
                                        {
                                            goto Label_1B47;
                                        }
                                        break;

                                    case 0xe5fffece:
                                        if (str15 == "upperangle")
                                        {
                                            goto Label_1BFA;
                                        }
                                        break;
                                }
                            }
                            continue;
                        Label_1ABB:
                            if (str15 == "maxmotortorque")
                            {
                                goto Label_1B97;
                            }
                            continue;
                        Label_1AE7:
                            if (str15 == "lowerangle")
                            {
                                goto Label_1BDA;
                            }
                            continue;
                        Label_1B29:
                            ((RevoluteJoint) joint).LimitEnabled = bool.Parse(element22.Value);
                            continue;
                        Label_1B47:
                            ((RevoluteJoint) joint).MotorEnabled = bool.Parse(element22.Value);
                            continue;
                        Label_1B65:
                            ((RevoluteJoint) joint).LocalAnchorA = ReadVector(element22);
                            continue;
                        Label_1B7E:
                            ((RevoluteJoint) joint).LocalAnchorB = ReadVector(element22);
                            continue;
                        Label_1B97:
                            ((RevoluteJoint) joint).MaxMotorTorque = float.Parse(element22.Value);
                            continue;
                        Label_1BBA:
                            ((RevoluteJoint) joint).MotorSpeed = float.Parse(element22.Value);
                            continue;
                        Label_1BDA:
                            ((RevoluteJoint) joint).LowerLimit = float.Parse(element22.Value);
                            continue;
                        Label_1BFA:
                            ((RevoluteJoint) joint).UpperLimit = float.Parse(element22.Value);
                            continue;
                        Label_1C1A:
                            ((RevoluteJoint) joint).ReferenceAngle = float.Parse(element22.Value);
                            continue;
                        Label_1C6D:
                            ((WeldJoint) joint).LocalAnchorA = ReadVector(element22);
                            continue;
                        Label_1C83:
                            ((WeldJoint) joint).LocalAnchorB = ReadVector(element22);
                            continue;
                        Label_1CDA:
                            ((RopeJoint) joint).LocalAnchorA = ReadVector(element22);
                            continue;
                        Label_1CF0:
                            ((RopeJoint) joint).LocalAnchorB = ReadVector(element22);
                            continue;
                        Label_1D06:
                            ((RopeJoint) joint).MaxLength = float.Parse(element22.Value);
                            continue;
                        Label_1DA3:
                            ((AngleJoint) joint).MaxImpulse = float.Parse(element22.Value);
                            continue;
                        Label_1DC3:
                            ((AngleJoint) joint).Softness = float.Parse(element22.Value);
                            continue;
                        Label_1DE3:
                            ((AngleJoint) joint).TargetAngle = float.Parse(element22.Value);
                            continue;
                        Label_1E62:
                            ((MotorJoint) joint).AngularOffset = float.Parse(element22.Value);
                            continue;
                        Label_1E82:
                            ((MotorJoint) joint).LinearOffset = ReadVector(element22);
                            continue;
                        Label_1E98:
                            ((MotorJoint) joint).MaxForce = float.Parse(element22.Value);
                            continue;
                        Label_1EB8:
                            ((MotorJoint) joint).MaxTorque = float.Parse(element22.Value);
                            continue;
                        Label_1ED8:
                            ((MotorJoint) joint).CorrectionFactor = float.Parse(element22.Value);
                        }
                    }
                }
            }
            world.ProcessChanges();
        }

        private static object ReadSimpleType(XMLFragmentElement node, Type type, bool outer)
        {
            if (type == null)
            {
                return ReadSimpleType(node.Elements[1], Type.GetType(node.Elements[0].Value), outer);
            }
            XmlSerializer serializer = new XmlSerializer(type);
            new XmlSerializerNamespaces().Add("", "");
            using (MemoryStream stream = new MemoryStream())
            {
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(outer ? node.OuterXml : node.InnerXml);
                writer.Flush();
                stream.Position = 0L;
                XmlReaderSettings settings = new XmlReaderSettings {
                    ConformanceLevel = ConformanceLevel.Fragment
                };
                return serializer.Deserialize(XmlReader.Create(stream, settings));
            }
        }

        private static TSVector2 ReadVector(XMLFragmentElement node)
        {
            char[] separator = new char[] { ' ' };
            string[] strArray = node.Value.Split(separator);
            return new TSVector2(float.Parse(strArray[0]), float.Parse(strArray[1]));
        }
    }
}

