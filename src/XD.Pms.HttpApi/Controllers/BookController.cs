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
	public async Task<ActionResult<PagedResultDto<BookDto>>> GetListAsync([FromQuery] PagedAndSortedResultRequestDto input)
	{
		var result = await _bookAppService.GetListAsync(input);
		return Ok(result);
	}

	[HttpGet("query2")]
	[Authorize]
	public async Task<PagedResultDto<BookDto>> GetListAsync2([FromQuery] PagedAndSortedResultRequestDto input)
	{
		return await _bookAppService.GetListAsync(input);
	}
}
