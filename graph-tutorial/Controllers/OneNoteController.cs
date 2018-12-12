using graph_tutorial.Helpers;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Linq;
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
            var sections = await GraphHelper.GetSectionAsync();
            var pages = await GraphHelper.GetPagesAsync();

            ViewBag.Sections = sections;
            ViewBag.Pages = pages;

            GerarListaSuspensaNotebook(notebooks);
            GerarListaSuspensaSection(sections);

            return View("Index", notebooks);
        }

        private void GerarListaSuspensaNotebook(IEnumerable<Notebook> notebooks)
        {
            ViewBag.NotebooksListaSuspensa = new List<SelectListItem>();
            var listaSuspencaNotebooks = new List<SelectListItem>();
            foreach (var notebook in notebooks)
            {
                listaSuspencaNotebooks.Add(new SelectListItem
                {
                    Selected = false
                    ,
                    Text = notebook.DisplayName
                    ,
                    Value = notebook.Id
                });
            }
            ViewBag.NotebooksListaSuspensa = listaSuspencaNotebooks;
        }

        private void GerarListaSuspensaSection(IEnumerable<OnenoteSection> sections)
        {
            ViewBag.SectionsListaSuspensa = new List<SelectListItem>();
            var listaSuspencaSections = new List<SelectListItem>();
            foreach (var section in sections)
            {
                listaSuspencaSections.Add(new SelectListItem
                {
                    Selected = false
                    ,
                    Text = section.DisplayName
                    ,
                    Value = section.Id
                });
            }
            ViewBag.SectionsListaSuspensa = listaSuspencaSections;
        }

        public async Task<ActionResult> CadastrarNotebookAsync(string nomeNotebook)
        {
            await GraphHelper.CreateNotebookAsync(nomeNotebook);
            return View("Index");
        }

        public async Task<ActionResult> CadastrarSecaoAsync(string nomeSecao, string selectNotebook)
        {
            var notebooks = await GraphHelper.GetNotebookAsync();
            Notebook notebook = notebooks.First(item => item.Id.Equals(selectNotebook));

            await GraphHelper.CreateSectionAsync(new OnenoteSection
            {
                DisplayName = nomeSecao
                ,
                ParentNotebook = notebook
            });
            return View("Index");
        }

        public async Task<ActionResult> CadastrarPaginaAsync(string tituloPagina, string selectSection)
        {
            var sections = await GraphHelper.GetSectionAsync();
            OnenoteSection section = sections.First(item => item.Id.Equals(selectSection));

            await GraphHelper.CreatePageAsync(new OnenotePage
            {
                Title = tituloPagina
                ,
                ParentSection = section
            });
            return View("Index");
        }
    }
}