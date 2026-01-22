using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using XD.Pms.ApiResponse;
using XD.Pms.Books;

namespace XD.Pms.Controllers;

[Route("papi/book")]
public class BookController : PmsControllerBase
{
	private readonly IBookAppService _bookAppService;

	public BookController(IBookAppService bookAppService)
	{
		_bookAppService = bookAppService;
	}

	[HttpGet("query")]
	public async Task<ActionResult<ApiResponse<PagedResultDto<BookDto>>>> GetListAsync([FromQuery] PagedAndSortedResultRequestDto input)
	{
		var result = await _bookAppService.GetListAsync(input);
		return Ok(ApiResponse<PagedResultDto<BookDto>>.Succeed(true, result, "查询成功"));
	}

	[HttpGet("query2")]
	[Authorize]
	public async Task<ApiResponse<PagedResultDto<BookDto>>> GetListAsync2([FromQuery] PagedAndSortedResultRequestDto input)
	{
		var result = await _bookAppService.GetListAsync(input);
		return ApiResponse<PagedResultDto<BookDto>>.Succeed(true, result, "查询成功");
	}
}
