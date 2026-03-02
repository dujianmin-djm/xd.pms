using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using XD.Pms.Enums;

namespace XD.Pms.Common;

public abstract class PmsAuditedAggregateRoot : AuditedAggregateRoot<Guid>
{
	public DocumentStatus DocumentStatus { get; protected set; } = DocumentStatus.Created;
	public Guid? ApproverId { get; protected set; }
	public DateTime? ApprovalTime { get; protected set; }

	protected PmsAuditedAggregateRoot() { }

	protected void CheckEditable()
	{
		if (DocumentStatus != DocumentStatus.Created)
			throw new BusinessException(PmsDomainErrorCodes.DocumentNotEditable);
	}

	protected void CheckDeletable()
	{
		if (DocumentStatus != DocumentStatus.Created)
			throw new BusinessException(PmsDomainErrorCodes.DocumentCannotDelete);
	}

	public virtual void Submit()
	{
		if (DocumentStatus != DocumentStatus.Created)
			throw new BusinessException(PmsDomainErrorCodes.DocumentCannotSubmit);
		DocumentStatus = DocumentStatus.Submitted;
	}

	public virtual void Audit(Guid auditorId, DateTime auditTime)
	{
		if (DocumentStatus != DocumentStatus.Submitted)
			throw new BusinessException(PmsDomainErrorCodes.DocumentCannotAudit);
		DocumentStatus = DocumentStatus.Approved;
		ApproverId = auditorId;
		ApprovalTime = auditTime;
	}

	public virtual void UnAudit()
	{
		if (DocumentStatus != DocumentStatus.Approved)
			throw new BusinessException(PmsDomainErrorCodes.DocumentCannotUnAudit);
		DocumentStatus = DocumentStatus.Created;
		ApproverId = null;
		ApprovalTime = null;
	}
}
