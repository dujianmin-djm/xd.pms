using System.Threading.Tasks;
using XD.Pms.Books;
using Microsoft.AspNetCore.Mvc;

namespace XD.Pms.Web.Pages.Books
{
    public class CreateModalModel : PmsPageModel
    {
        [BindProperty]
        public CreateUpdateBookDto Book { get; set; } = default!;

		private readonly IBookAppService _bookAppService;

        public CreateModalModel(IBookAppService bookAppService)
        {
            _bookAppService = bookAppService;
        }

        public void OnGet()
        {
            Book = new CreateUpdateBookDto();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _bookAppService.CreateAsync(Book);
            return NoContent();
        }
    }
}
