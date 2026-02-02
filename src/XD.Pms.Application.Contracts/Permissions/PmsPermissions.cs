namespace XD.Pms.Permissions;

public static class PmsPermissions
{
    public const string GroupName = "Pms";

    public static class Books
    {
        public const string Default = GroupName + ".Books";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

	public static class ApiKeys
	{
		public const string Default = GroupName + ".ApiKeys";
		public const string Create = Default + ".Create";
		public const string Edit = Default + ".Edit";
		public const string Delete = Default + ".Delete";
	}

}