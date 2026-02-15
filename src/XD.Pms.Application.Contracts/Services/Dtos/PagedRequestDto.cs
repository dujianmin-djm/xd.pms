using System;
using System.ComponentModel.DataAnnotations;

namespace XD.Pms.Services.Dtos;

[Serializable]
public class PagedRequestDto
{
	[Range(1, 1000)]
	public int Current { get; set; }

	[Range(1, 1000)]
	public int Size { get; set; }

	public string? Sorts { get; set; }
}