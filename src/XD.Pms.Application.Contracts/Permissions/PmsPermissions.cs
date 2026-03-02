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
			public const string ManageRoles = Default + ".ManageRoles";
			public const string ResetPassword = Default + ".ResetPassword";
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

		public static class Departments
		{
			public const string Default = GroupName + ".Departments";
			public const string Create = Default + ".Create";
			public const string Update = Default + ".Update";
			public const string Delete = Default + ".Delete";
			public const string Submit = Default + ".Submit";
			public const string Cancel = Default + ".Cancel";
			public const string Audit = Default + ".Audit";
			public const string UnAudit = Default + ".UnAudit";
		}

		public static class Positions
		{
			public const string Default = GroupName + ".Positions";
			public const string Create = Default + ".Create";
			public const string Update = Default + ".Update";
			public const string Delete = Default + ".Delete";
			public const string Submit = Default + ".Submit";
			public const string Cancel = Default + ".Cancel";
			public const string Audit = Default + ".Audit";
			public const string UnAudit = Default + ".UnAudit";
		}

		public static class Employees
		{
			public const string Default = GroupName + ".Employees";
			public const string Create = Default + ".Create";
			public const string Update = Default + ".Update";
			public const string Delete = Default + ".Delete";
			public const string Submit = Default + ".Submit";
			public const string Cancel = Default + ".Cancel";
			public const string Audit = Default + ".Audit";
			public const string UnAudit = Default + ".UnAudit";
		}
	}

	public static class Business
	{
		public const string GroupName = RootName + ".Business";
	}
}