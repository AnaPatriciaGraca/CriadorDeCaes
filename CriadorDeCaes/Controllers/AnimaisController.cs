using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CriadorDeCaes.Data;
using CriadorDeCaes.Models;

namespace CriadorDeCaes.Controllers
{
    public class AnimaisController : Controller
    {   
        /// <summary>
        /// atributo para representar o acesso à base de dados
        /// </summary>
        private readonly ApplicationDbContext _context;
        // tem de ser obrigatoriamente instanciado no construtor
        // >> começa com "_" porque é uma ferramenta/recurso que utilizamos internamente
        // >> começa com minúscula por ser privado
        public AnimaisController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Devolve um objecto do tipo "ActionResult" - invoca uma view, prepara um trabalho e entrega à view
        /// </summary>
        // GET: Animais
        public async Task<IActionResult> Index()    // invoca conteúdo para uma View chamada "Index"
        {   
            // pesquisar os dados dos animais, para os mostrar no ecrã
            // SELECT * FROM Animais a  INNER JOIN Criadores c ON a.CriadorFK = c.ID
            //                          INNER JOIN Racas r ON a.RacaFK = r.ID
            // *** esta expressão está escrita em LINQ
            var animais = _context.Animais
                                        .Include(a => a.Criador)
                                        .Include(a => a.Raca);
            // invoco a view, fornecendo-lhe os dados que ela necessita
            return View(await animais.ToListAsync());   // o método é assíncrono porque tem um "await" (sempre!)
        }

        // GET: Animais/Details/5
        public async Task<IActionResult> Details(int? id) //"?" significa que é um parâmetro que pode ser nulo
        {   // proteção à pesquisa, caso a tabela dos Animais esteja vazia ou o Id seja nulo
            if (id == null || _context.Animais == null)
            {   // redirecionar para outro sítio! Por norma redireciona-se para o Index (para alterações fraudulentas ao URL)
                return NotFound();
            }

            // pesquisar os dados dos animais, para os mostrar no ecrã
            // SELECT * FROM Animais a  INNER JOIN Criadores c ON a.CriadorFK = c.ID
            //                          INNER JOIN Racas r ON a.RacaFK = r.ID
            // WHERE a.ID = id
            // *** esta expressão está escrita em LINQ
            var animal = await _context.Animais     // alterar nome da variavel para singular porque apenas estamos à procura de um animal
                .Include(a => a.Criador)
                .Include(a => a.Raca)
                .FirstOrDefaultAsync(m => m.Id == id); // igual ao LIMIT 1 caso retorne mais do que 1 resultado

            // proteção à pesquisa, caso o Id não exista
            if (animal == null)
            {
                return NotFound();
            }

            return View(animal);
        }

        // GET: Animais/Create
        /// <summary>
        /// invoca a view para criar um novo animal
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {   //chaves forasteiras para as tabelas "Animais" e " Racas"
            //preparar os dados que vão ficar associados às chaves forasteiras - trasportar dados do Controller para a View
            ViewData["CriadorFK"] = new SelectList(_context.Criadores, "Id", "Nome");   //valores selecionados no ecrã - alterar
            ViewData["RacaFK"] = new SelectList(_context.Racas.OrderBy(r=>r.Nome), "Id", "Nome");  // limitar e ordenar número de valores que aparecem na dropDown
            
            return View();  //não estou a fornecer dados específicos para a View processar
        }

        // POST: Animais/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Sexo,DataNasc,DataCompra,RegistoLOP,RacaFK,CriadorFK")] Animais animais)
        {
            if (ModelState.IsValid)
            {
                _context.Add(animais);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CriadorFK"] = new SelectList(_context.Criadores, "Id", "CodPostal", animais.CriadorFK);
            ViewData["RacaFK"] = new SelectList(_context.Racas, "Id", "Id", animais.RacaFK);
            return View(animais);
        }

        // GET: Animais/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Animais == null)
            {
                return NotFound();
            }

            var animais = await _context.Animais.FindAsync(id);
            if (animais == null)
            {
                return NotFound();
            }
            ViewData["CriadorFK"] = new SelectList(_context.Criadores, "Id", "CodPostal", animais.CriadorFK);
            ViewData["RacaFK"] = new SelectList(_context.Racas, "Id", "Id", animais.RacaFK);
            return View(animais);
        }

        // POST: Animais/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Sexo,DataNasc,DataCompra,RegistoLOP,RacaFK,CriadorFK")] Animais animais)
        {
            if (id != animais.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(animais);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimaisExists(animais.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CriadorFK"] = new SelectList(_context.Criadores, "Id", "CodPostal", animais.CriadorFK);
            ViewData["RacaFK"] = new SelectList(_context.Racas, "Id", "Id", animais.RacaFK);
            return View(animais);
        }

        // GET: Animais/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Animais == null)
            {
                return NotFound();
            }

            var animais = await _context.Animais
                .Include(a => a.Criador)
                .Include(a => a.Raca)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (animais == null)
            {
                return NotFound();
            }

            return View(animais);
        }

        // POST: Animais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Animais == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Animais'  is null.");
            }
            var animais = await _context.Animais.FindAsync(id);
            if (animais != null)
            {
                _context.Animais.Remove(animais);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnimaisExists(int id)
        {
          return _context.Animais.Any(e => e.Id == id);
        }
    }
}
