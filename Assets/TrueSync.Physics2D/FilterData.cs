using System;

namespace TrueSync.Physics2D
{
	public abstract class FilterData
	{
		public Category DisabledOnCategories = Category.None;

		public int DisabledOnGroup;

		public Category EnabledOnCategories = Category.All;

		public int EnabledOnGroup;

		public virtual bool IsActiveOn(Body body)
		{
			bool flag = body == null || !body.Enabled || body.IsStatic;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = body.FixtureList == null;
				if (flag2)
				{
					result = false;
				}
				else
				{
					foreach (Fixture current in body.FixtureList)
					{
						bool flag3 = (int)current.CollisionGroup == this.DisabledOnGroup && current.CollisionGroup != 0 && this.DisabledOnGroup != 0;
						if (flag3)
						{
							result = false;
							return result;
						}
						bool flag4 = (current.CollisionCategories & this.DisabledOnCategories) > Category.None;
						if (flag4)
						{
							result = false;
							return result;
						}
						bool flag5 = this.EnabledOnGroup != 0 || this.EnabledOnCategories != Category.All;
						if (!flag5)
						{
							result = true;
							return result;
						}
						bool flag6 = (int)current.CollisionGroup == this.EnabledOnGroup && current.CollisionGroup != 0 && this.EnabledOnGroup != 0;
						if (flag6)
						{
							result = true;
							return result;
						}
						bool flag7 = (current.CollisionCategories & this.EnabledOnCategories) != Category.None && this.EnabledOnCategories != Category.All;
						if (flag7)
						{
							result = true;
							return result;
						}
					}
					result = false;
				}
			}
			return result;
		}

		public void AddDisabledCategory(Category category)
		{
			this.DisabledOnCategories |= category;
		}

		public void RemoveDisabledCategory(Category category)
		{
			this.DisabledOnCategories &= ~category;
		}

		public bool IsInDisabledCategory(Category category)
		{
			return (this.DisabledOnCategories & category) == category;
		}

		public void AddEnabledCategory(Category category)
		{
			this.EnabledOnCategories |= category;
		}

		public void RemoveEnabledCategory(Category category)
		{
			this.EnabledOnCategories &= ~category;
		}

		public bool IsInEnabledInCategory(Category category)
		{
			return (this.EnabledOnCategories & category) == category;
		}
	}
}
