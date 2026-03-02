using System;
using Volo.Abp.Application.Dtos;
using XD.Pms.Enums;

namespace XD.Pms.Services.Dtos;

public abstract class PmsAuditedEntityDto : AuditedEntityDto<Guid>
{
	public DocumentStatus DocumentStatus { get; set; }
	public string? CreatorName { get; set; }
	public string? LastModifierName { get; set; }
	public Guid? ApproverId { get; set; }
	public string? ApproverName { get; set; }
	public DateTime? ApprovalTime { get; set; }
	public string? ConcurrencyStamp { get; set; }
}