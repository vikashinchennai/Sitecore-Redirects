using Sitecore.Data;

namespace Sitecore.Feature.Redirects
{
    public class Templates
	{
		public Templates()
		{
		}

        public struct SiteLevelMap
        {
            public static ID ID;

            static SiteLevelMap()
            {
                ID = ID.Parse("{A929258B-A9B9-4DEB-8544-1E4F1FF568CF}");
            }
            public struct Fields
            {
                public readonly static ID RedirectToHttpsAlways;

                public readonly static ID AddWwwPrefix;

                static Fields()
                {
                    RedirectToHttpsAlways = new ID("{1340547C-8AEA-43B5-94DE-AABE5A60BDC4}");
                    AddWwwPrefix = new ID("{83C9E732-A308-4D42-A396-CFF4779CB8A7}");
                }
            }
        }
		public struct Redirect
		{
			public static ID ID;

			static Redirect()
			{
                ID = ID.Parse("{232D252D-7E17-489A-B692-9BFEABB2689F}");
			}

            public struct Fields
            {
                public readonly static ID RedirectUrl;

                public readonly static ID TargetItem;

                static Fields()
                {
                    RedirectUrl = new ID("{4B7AB173-C78C-46ED-B51F-5B583F986322}");
                    TargetItem = new ID("{627A5131-5F85-4037-A513-FAEE4C0FE169}");
                }
            }
        }

		public struct RedirectMap
		{
			public static ID ID;

			static RedirectMap()
			{
                ID = ID.Parse("{4F554D94-F449-429C-9DA0-187F316BC95E}");
			}

			public struct Fields
			{
				public readonly static ID RedirectType;

				public readonly static ID PreserveQueryString;

				public readonly static ID UrlMapping;

				static Fields()
				{
					RedirectType = new ID("{57A41BCA-DF6E-45CD-80B1-A840DF5CE724}");
					PreserveQueryString = new ID("{A15D77B0-F075-4B6E-8D9F-D406C69F1A8D}");
					UrlMapping = new ID("{3A32FF07-C588-4696-B512-3A553D1AD6A8}");
				}
			}
		}

		public struct RedirectMapGrouping
		{
			public static ID ID;

			static RedirectMapGrouping()
			{
                ID = ID.Parse("{57C61BB1-9B1E-4FCC-8CA0-CC580AF337F1}");
			}
		}
	}
}