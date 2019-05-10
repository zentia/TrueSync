using System;
using System.Collections.Generic;
using System.Reflection;

namespace TrueSync
{
	public class StateTracker
	{
		internal class State
		{
			private StateTracker.TrackedInfo trackedInfo;

			private object value;

			public void SetInfo(StateTracker.TrackedInfo trackedInfo)
			{
				this.trackedInfo = trackedInfo;
				this.SaveValue();
			}

			public void SaveValue()
			{
				object obj = this.trackedInfo.propInfo.GetValue(this.trackedInfo.relatedObj);
				bool flag = obj != null;
				if (flag)
				{
					bool isArray = obj.GetType().IsArray;
					if (isArray)
					{
						this.value = Array.CreateInstance(obj.GetType().GetElementType(), ((Array)obj).Length);
						Array.Copy((Array)obj, (Array)this.value, ((Array)obj).Length);
					}
					else
					{
						this.value = obj;
					}
				}
				else
				{
					this.value = null;
				}
			}

			public void RestoreValue()
			{
				bool flag = this.trackedInfo.relatedObj != null;
				if (flag)
				{
					bool flag2 = this.value is Array;
					if (flag2)
					{
						Array destinationArray = Array.CreateInstance(this.value.GetType().GetElementType(), ((Array)this.value).Length);
						Array.Copy((Array)this.value, destinationArray, ((Array)this.value).Length);
						this.trackedInfo.propInfo.SetValue(this.trackedInfo.relatedObj, destinationArray);
					}
					else
					{
						this.trackedInfo.propInfo.SetValue(this.trackedInfo.relatedObj, this.value);
					}
				}
			}
		}

		internal class TrackedInfo
		{
			public object relatedObj;

			public MemberInfo propInfo;
		}

		private static ResourcePoolStateTrackerState resourcePool = new ResourcePoolStateTrackerState();

		private HashSet<string> trackedInfosAdded = new HashSet<string>();

		private List<StateTracker.TrackedInfo> trackedInfos = new List<StateTracker.TrackedInfo>();

		private GenericBufferWindow<List<StateTracker.State>> states;

		internal static StateTracker instance;

		public static void Init()
		{
			StateTracker.instance = new StateTracker();
			TSRandom.Init();
		}

		internal static void Init(int stateWindow)
		{
			StateTracker.instance.states = new GenericBufferWindow<List<StateTracker.State>>(stateWindow);
		}

		public static void AddTracking(object obj, string path)
		{
			bool flag = StateTracker.instance != null;
			if (flag)
			{
				string item = string.Format("{0}_{1}_{2}", obj.GetType().FullName, obj.GetHashCode(), path);
				bool flag2 = !StateTracker.instance.trackedInfosAdded.Contains(item);
				if (flag2)
				{
					StateTracker.instance.trackedInfos.Add(StateTracker.GetTrackedInfo(obj, path));
					StateTracker.instance.trackedInfosAdded.Add(item);
				}
			}
		}

		public static void AddTracking(object obj)
		{
			bool flag = StateTracker.instance != null;
			if (flag)
			{
				foreach (MemberInfo current in Utils.GetMembersInfo(obj.GetType()))
				{
					object[] customAttributes = current.GetCustomAttributes(true);
					bool flag2 = customAttributes != null;
					if (flag2)
					{
						object[] array = customAttributes;
						for (int i = 0; i < array.Length; i++)
						{
							object obj2 = array[i];
							bool flag3 = obj2 is AddTracking;
							if (flag3)
							{
								StateTracker.AddTracking(obj, current.Name);
							}
						}
					}
				}
			}
		}

		internal void SaveState()
		{
			List<StateTracker.State> list = this.states.Current();
			int i = 0;
			int count = list.Count;
			while (i < count)
			{
				StateTracker.resourcePool.GiveBack(list[i]);
				i++;
			}
			list.Clear();
			int j = 0;
			int count2 = this.trackedInfos.Count;
			while (j < count2)
			{
				StateTracker.State @new = StateTracker.resourcePool.GetNew();
				@new.SetInfo(this.trackedInfos[j]);
				list.Add(@new);
				j++;
			}
			this.MoveNextState();
		}

		internal void RestoreState()
		{
			List<StateTracker.State> list = this.states.Current();
			int i = 0;
			int count = list.Count;
			while (i < count)
			{
				list[i].RestoreValue();
				i++;
			}
		}

		internal void MoveNextState()
		{
			this.states.MoveNext();
		}

		private static StateTracker.TrackedInfo GetTrackedInfo(object obj, string name)
		{
			string[] array = name.Split(new char[]
			{
				'.'
			});
			int i = 0;
			int num = array.Length;
			StateTracker.TrackedInfo result;
			while (i < num)
			{
				string name2 = array[i];
				bool flag = obj == null;
				if (flag)
				{
					result = null;
				}
				else
				{
					Type type = obj.GetType();
					MemberInfo memberInfo = type.GetProperty(name2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? type.GetField(name2, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					bool flag2 = memberInfo == null;
					if (flag2)
					{
						result = null;
					}
					else
					{
						bool flag3 = i == num - 1;
						if (!flag3)
						{
							obj = memberInfo.GetValue(obj);
							i++;
							continue;
						}
						result = new StateTracker.TrackedInfo
						{
							relatedObj = obj,
							propInfo = memberInfo
						};
					}
				}
				return result;
			}
			result = null;
			return result;
		}
	}
}
