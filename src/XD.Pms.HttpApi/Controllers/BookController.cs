using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using XD.Pms.Books;
using XD.Pms.Services.Dtos;

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
	public async Task<ActionResult<PagedResponseDto<BookDto>>> GetListAsync([FromQuery] PagedRequestDto input)
	{
		var result = await _bookAppService.GetListAsync(input);
		return Ok(result);
	}

	[HttpGet("query2")]
	[Authorize]
	public async Task<PagedResponseDto<BookDto>> GetListAsync2([FromQuery] PagedRequestDto input)
	{
		return await _bookAppService.GetListAsync(input);
	}
}
