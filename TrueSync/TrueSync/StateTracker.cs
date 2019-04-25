namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class StateTracker
    {
        internal static StateTracker instance;
        private static ResourcePoolStateTrackerState resourcePool = new ResourcePoolStateTrackerState();
        private GenericBufferWindow<List<State>> states;
        private List<TrackedInfo> trackedInfos = new List<TrackedInfo>();
        private HashSet<string> trackedInfosAdded = new HashSet<string>();

        public static void AddTracking(object obj)
        {
            if (instance != null)
            {
                foreach (MemberInfo info in Utils.GetMembersInfo(obj.GetType()))
                {
                    object[] customAttributes = info.GetCustomAttributes(true);
                    if (customAttributes != null)
                    {
                        foreach (object obj2 in customAttributes)
                        {
                            if (obj2 is AddTracking)
                            {
                                AddTracking(obj, info.Name);
                            }
                        }
                    }
                }
            }
        }

        public static void AddTracking(object obj, string path)
        {
            if (instance != null)
            {
                string item = string.Format("{0}_{1}_{2}", obj.GetType().FullName, obj.GetHashCode(), path);
                if (!instance.trackedInfosAdded.Contains(item))
                {
                    instance.trackedInfos.Add(GetTrackedInfo(obj, path));
                    instance.trackedInfosAdded.Add(item);
                }
            }
        }

        private static TrackedInfo GetTrackedInfo(object obj, string name)
        {
            char[] separator = new char[] { '.' };
            string[] strArray = name.Split(separator);
            int index = 0;
            int length = strArray.Length;
            while (index < length)
            {
                string str = strArray[index];
                if (obj == null)
                {
                    return null;
                }
                Type type = obj.GetType();
                MemberInfo memberInfo = type.GetProperty(str, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (memberInfo == null)
                    memberInfo = type.GetField(str, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (memberInfo == null)
                {
                    return null;
                }
                if (index == (length - 1))
                {
                    return new TrackedInfo { relatedObj = obj, propInfo = memberInfo };
                }
                obj = memberInfo.GetValue(obj);
                index++;
            }
            return null;
        }

        public static void Init()
        {
            instance = new StateTracker();
            TSRandom.Init();
        }

        internal static void Init(int stateWindow)
        {
            instance.states = new GenericBufferWindow<List<State>>(stateWindow);
        }

        internal void MoveNextState()
        {
            states.MoveNext();
        }

        internal void RestoreState()
        {
            List<State> list = states.Current();
            int num = 0;
            int count = list.Count;
            while (num < count)
            {
                list[num].RestoreValue();
                num++;
            }
        }

        internal void SaveState()
        {
            List<State> list = this.states.Current();
            int num = 0;
            int count = list.Count;
            while (num < count)
            {
                resourcePool.GiveBack(list[num]);
                num++;
            }
            list.Clear();
            int num3 = 0;
            int num4 = this.trackedInfos.Count;
            while (num3 < num4)
            {
                State item = resourcePool.GetNew();
                item.SetInfo(this.trackedInfos[num3]);
                list.Add(item);
                num3++;
            }
            MoveNextState();
        }

        internal class State
        {
            private TrackedInfo trackedInfo;
            private object value;

            public void RestoreValue()
            {
                if (trackedInfo.relatedObj != null)
                {
                    if (value is Array)
                    {
                        Array destinationArray = Array.CreateInstance(value.GetType().GetElementType(), ((Array) this.value).Length);
                        Array.Copy((Array) value, destinationArray, ((Array) this.value).Length);
                        this.trackedInfo.propInfo.SetValue(trackedInfo.relatedObj, destinationArray);
                    }
                    else
                    {
                        trackedInfo.propInfo.SetValue(this.trackedInfo.relatedObj, this.value);
                    }
                }
            }

            public void SaveValue()
            {
                object obj2 = this.trackedInfo.propInfo.GetValue(this.trackedInfo.relatedObj);
                if (obj2 != null)
                {
                    if (obj2.GetType().IsArray)
                    {
                        this.value = Array.CreateInstance(obj2.GetType().GetElementType(), ((Array) obj2).Length);
                        Array.Copy((Array) obj2, (Array) this.value, ((Array) obj2).Length);
                    }
                    else
                    {
                        this.value = obj2;
                    }
                }
                else
                {
                    this.value = null;
                }
            }

            public void SetInfo(StateTracker.TrackedInfo trackedInfo)
            {
                this.trackedInfo = trackedInfo;
                this.SaveValue();
            }
        }

        internal class TrackedInfo
        {
            public MemberInfo propInfo;
            public object relatedObj;
        }
    }
}

