namespace XD.Pms;

public static class PmsDomainErrorCodes
{
	/* ── 通用 ── */
	public const string DocumentNotEditable = "Pms:DocumentNotEditable";
	public const string DocumentCannotDelete = "Pms:DocumentCannotDelete";
	public const string DocumentCannotSubmit = "Pms:DocumentCannotSubmit";
	public const string DocumentCannotAudit = "Pms:DocumentCannotAudit";
	public const string DocumentCannotUnAudit = "Pms:DocumentCannotUnAudit";
	public const string DuplicateNumber = "Pms:DuplicateNumber";

	/* ── 部门 ── */
	public const string DepartmentParentNotFound = "Pms:DepartmentParentNotFound";
	public const string DepartmentCannotDeleteHasChildren = "Pms:DepartmentCannotDeleteHasChildren";
	public const string DepartmentCannotDeleteReferenced = "Pms:DepartmentCannotDeleteReferenced";
	public const string DepartmentCannotSetSelfAsParent = "Pms:DepartmentCannotSetSelfAsParent";

	/* ── 岗位 ── */
	public const string PositionDepartmentNotFound = "Pms:PositionDepartmentNotFound";

	/* ── 员工 ── */
	public const string EmployeePositionNotFound = "Pms:EmployeePositionNotFound";
	public const string EmployeeMultiplePrimary = "Pms:EmployeeMultiplePrimary";
	public const string EmployeeNoPrimary = "Pms:EmployeeNoPrimary";
}
