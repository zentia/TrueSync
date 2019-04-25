namespace TrueSync.Physics2D
{
    using System;

    public abstract class FilterData
    {
        public Category DisabledOnCategories = Category.None;
        public int DisabledOnGroup;
        public Category EnabledOnCategories = Category.All;
        public int EnabledOnGroup;

        protected FilterData()
        {
        }

        public void AddDisabledCategory(Category category)
        {
            this.DisabledOnCategories |= category;
        }

        public void AddEnabledCategory(Category category)
        {
            this.EnabledOnCategories |= category;
        }

        public virtual bool IsActiveOn(Body body)
        {
            if (((body != null) && body.Enabled) && !body.IsStatic)
            {
                if (body.FixtureList == null)
                {
                    return false;
                }
                foreach (Fixture fixture in body.FixtureList)
                {
                    if (((fixture.CollisionGroup == this.DisabledOnGroup) && (fixture.CollisionGroup != 0)) && (this.DisabledOnGroup > 0))
                    {
                        return false;
                    }
                    if ((fixture.CollisionCategories & this.DisabledOnCategories) > Category.None)
                    {
                        return false;
                    }
                    if ((this.EnabledOnGroup != 0) || (this.EnabledOnCategories != Category.All))
                    {
                        if (((fixture.CollisionGroup == this.EnabledOnGroup) && (fixture.CollisionGroup != 0)) && (this.EnabledOnGroup > 0))
                        {
                            return true;
                        }
                        if (((fixture.CollisionCategories & this.EnabledOnCategories) != Category.None) && (this.EnabledOnCategories != Category.All))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsInDisabledCategory(Category category)
        {
            return ((this.DisabledOnCategories & category) == category);
        }

        public bool IsInEnabledInCategory(Category category)
        {
            return ((this.EnabledOnCategories & category) == category);
        }

        public void RemoveDisabledCategory(Category category)
        {
            this.DisabledOnCategories &= ~category;
        }

        public void RemoveEnabledCategory(Category category)
        {
            this.EnabledOnCategories &= ~category;
        }
    }
}

