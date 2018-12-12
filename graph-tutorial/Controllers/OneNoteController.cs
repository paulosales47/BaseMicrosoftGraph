using graph_tutorial.Helpers;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace graph_tutorial.Controllers
{
    [Authorize]
    public class OneNoteController : BaseController
    {
        // GET: OneNote
        public async Task<ActionResult> IndexAsync()
        {
            var notebooks = await GraphHelper.GetNotebookAsync();
            return View("Index",notebooks);
        }

        public async Task<ActionResult> CadastrarNovoNotebookAsync(string nomeNotebook)
        {
            await GraphHelper.CreateNotebookAsync(nomeNotebook);
            return View("Index");
        }
    }
}