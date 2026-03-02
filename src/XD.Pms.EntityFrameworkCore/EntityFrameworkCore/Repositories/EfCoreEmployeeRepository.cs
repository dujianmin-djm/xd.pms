using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using XD.Pms.BaseData.Employees;
using XD.Pms.Enums;

namespace XD.Pms.EntityFrameworkCore.Repositories;

public class EmployeeRepository : EfCoreRepository<PmsDbContext, Employee, Guid>, IEmployeeRepository
{
	public EmployeeRepository(IDbContextProvider<PmsDbContext> dbContextProvider)
		: base(dbContextProvider) 
	{ }

	public async Task<Employee?> FindByNumberAsync(string number)
	{
		return await (await GetDbSetAsync()).FirstOrDefaultAsync(e => e.Number == number);
	}

	public async Task<Employee?> GetWithDetailsAsync(Guid id)
	{
		return await (await GetDbSetAsync())
			.Include(e => e.Positions).ThenInclude(pa => pa.Department)
			.Include(e => e.Positions).ThenInclude(pa => pa.Position)
			.FirstOrDefaultAsync(e => e.Id == id);
	}

	public async Task<List<Employee>> GetListAsync(
		string? number, string? name, Gender? gender, string? phone,
		DocumentStatus? documentStatus,
		string? sorts,
		int skipCount = 0,
		int maxResultCount = int.MaxValue,
		CancellationToken cancellationToken = default)
	{
		return await (await GetDbSetAsync())
			.Include(e => e.Positions).ThenInclude(pa => pa.Department)
			.Include(e => e.Positions).ThenInclude(pa => pa.Position)
			.WhereIf(!number.IsNullOrWhiteSpace(), e => e.Number.ToLower().Contains(number!.ToLower()))
			.WhereIf(!name.IsNullOrWhiteSpace(), e => e.Name.Contains(name!))
			.WhereIf(gender.HasValue, e => e.Gender == gender)
			.WhereIf(!phone.IsNullOrWhiteSpace(), e => e.Phone != null && e.Phone.Contains(phone!))
			.WhereIf(documentStatus.HasValue, e => e.DocumentStatus == documentStatus)
			.OrderBy(sorts.IsNullOrWhiteSpace() ? $"{nameof(Employee.Number)} asc" : sorts)
			.PageBy(skipCount, maxResultCount)
			.ToListAsync(GetCancellationToken(cancellationToken));
	}

	public async Task<long> GetCountAsync(
		string? number, string? name, Gender? gender, string? phone,
		DocumentStatus? documentStatus, CancellationToken cancellationToken = default)
	{
		return await (await GetDbSetAsync())
			.WhereIf(!number.IsNullOrWhiteSpace(), e => e.Number.Contains(number!))
			.WhereIf(!name.IsNullOrWhiteSpace(), e => e.Name.Contains(name!))
			.WhereIf(gender.HasValue, e => e.Gender == gender)
			.WhereIf(!phone.IsNullOrWhiteSpace(), e => e.Phone != null && e.Phone.Contains(phone!))
			.WhereIf(documentStatus.HasValue, e => e.DocumentStatus == documentStatus)
			.LongCountAsync(GetCancellationToken(cancellationToken));
	}
}
