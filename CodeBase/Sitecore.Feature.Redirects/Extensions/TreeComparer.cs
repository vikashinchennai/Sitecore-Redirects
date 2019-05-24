using Sitecore.Data;
using Sitecore.Data.Comparers;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
namespace Sitecore.Feature.Redirects.Extensions
{
    public class TreeComparer : IComparer<Item>, IEqualityComparer<Item>
    {
        protected readonly Dictionary<ID, IComparer<Item>> Comparers = new Dictionary<ID, IComparer<Item>>();

        public TreeComparer()
        {
        }

        public virtual int Compare(Item x, Item y)
        {
            int num;
            if (x.ID == y.ID)
            {
                return 0;
            }
            using (SecurityDisabler securityDisabler = new SecurityDisabler())
            {
                Item parent = x.Parent;
                if (parent.ID != y.ParentID)
                {
                    string longID = x.Paths.LongID;
                    string str = y.Paths.LongID;
                    if (longID.StartsWith(str))
                    {
                        num = -1;
                    }
                    else if (!str.StartsWith(longID))
                    {
                        string[] strArrays = longID.Split(new char[] { '/' });
                        string[] strArrays1 = str.Split(new char[] { '/' });
                        int num1 = Math.Min((int)strArrays.Length, (int)strArrays1.Length) - 1;
                        while (strArrays[num1] != strArrays1[num1])
                        {
                            num1--;
                        }
                        ID d = ID.Parse(strArrays[num1]);
                        Item item = x.Database.GetItem(ID.Parse(strArrays[num1 + 1]));
                        Item item1 = x.Database.GetItem(ID.Parse(strArrays1[num1 + 1]));
                        num = this.GetComparer(d, x.Database, null).Compare(item, item1);
                    }
                    else
                    {
                        num = 1;
                    }
                }
                else
                {
                    num = this.GetComparer(parent.ID, x.Database, parent).Compare(x, y);
                }
            }
            return num;
        }

        public bool Equals(Item x, Item y)
        {
            return x.ID == y.ID;
        }

        protected virtual IComparer<Item> GetComparer(ID id, Database database, Item item = null)
        {
            if (!this.Comparers.ContainsKey(id))
            {
                this.Comparers[id] = ComparerFactory.GetComparer(item ?? database.GetItem(id));
            }
            return this.Comparers[id];
        }

        public int GetHashCode(Item item)
        {
            return item.ID.GetHashCode();
        }
    }
}
