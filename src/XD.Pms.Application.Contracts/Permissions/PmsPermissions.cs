namespace XD.Pms.Permissions;

public static class PmsPermissions
{
	private const string RootName = "Pms";

	public static class System
	{
		public const string GroupName = RootName + ".System";

		public static class Roles
		{
			public const string Default = GroupName + ".Roles";
			public const string Create = Default + ".Create";
			public const string Update = Default + ".Update";
			public const string Delete = Default + ".Delete";
			public const string ManagePermissions = Default + ".ManagePermissions";
		}

		public static class Users
		{
			public const string Default = GroupName + ".Users";
			public const string Create = Default + ".Create";
			public const string Update = Default + ".Update";
			public const string Delete = Default + ".Delete";
			public const string ManagePermissions = Default + ".ManagePermissions";
			public const string ManageRoles = Update + ".ManageRoles";
		}

		public static class ApiKeys
		{
			public const string Default = GroupName + ".ApiKeys";
			public const string Create = Default + ".Create";
			public const string Update = Default + ".Update";
			public const string Delete = Default + ".Delete";
		}
	}

	public static class BaseData
	{
		public const string GroupName = RootName + ".BaseData";

		public static class Books
		{
			public const string Default = GroupName + ".Books";
			public const string Create = Default + ".Create";
			public const string Update = Default + ".Update";
			public const string Delete = Default + ".Delete";
		}
	}

	public static class Business
	{
		public const string GroupName = RootName + ".Business";
	}
}