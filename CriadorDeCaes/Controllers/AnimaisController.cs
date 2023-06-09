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
        /// <summary>
        /// objeto cm dados do servidor web
        /// </summary>
        private readonly IWebHostEnvironment _environment;

        // tem de ser obrigatoriamente instanciado no construtor
        // >> começa com "_" porque é uma ferramenta/recurso que utilizamos internamente
        // >> começa com minúscula por ser privado
        // variaveis publicas - CamelCase
        public AnimaisController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        /// <summary>
        /// Devolve um objecto do tipo "ActionResult" - invoca uma view, prepara um trabalho e entrega à view
        /// </summary>
        // GET: Animais
        public async Task<IActionResult> Index()    // invoca conteúdo para uma View chamada "Index"
        {   
            // pesquisar os dados dos animal, para os mostrar no ecrã
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

            // pesquisar os dados dos animal, para os mostrar no ecrã
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
        public async Task<IActionResult> Create([Bind("Nome,Sexo,DataNasc,DataCompra,PrecoCompraAux,RegistoLOP,RacaFK,CriadorFK")] Animais animal, IFormFile fotografia)
        {
            //vars auxiliares
            bool existeFoto = false;
            string nomeFoto = "";

            if (animal.RacaFK == 0 || animal.CriadorFK == 0)
            {
                //não escolhi a RacaFK
                //gerar uma mensagem de erro
                ModelState.AddModelError("", "Deve escolher uma Raça, por favor");
            }
            else
            {
                if (animal.CriadorFK == 0)
                {
                    //não escolhi o CriadorFK
                    ModelState.AddModelError("", "Deve escolher um Criador, por favor");
                }
                else
                {
                    //se cheguei aqui é porque escolhi uma raça e um criador
                    //vamos avaliar o ficheiro, se é que ele existe
                    if (fotografia == null)
                    {
                        //não há ficheiro (imagem) - colocar uma imagem por defeito
                        animal.ListaFotografias.Add(new Fotografias
                        {
                            Data = DateTime.Now,
                            Local = "no image",
                            Ficheiro = "noAnimal.jpg"
                        });


                    } else
                    {
                        //existe um ficheiro (será uma imagem?)
                        if (!(fotografia.ContentType == "image/jpeg" || fotografia.ContentType == "image/jpg" || fotografia.ContentType == "image/png"))
                            //existe ficheiro, mas não é uma imagem
                        {
                            ModelState.AddModelError("", "Por favor, introduza uma imagem válida (PNG ou JPG)");
                        }else
                        {
                            //há imagem - definir nome do ficheiro e onde guardar
                            existeFoto = true;
                            //nome do fihceiro - garantir que os nomes são únicos
                            Guid g = Guid.NewGuid();
                            nomeFoto = animal.CriadorFK + "_" + g.ToString();
                            string extensao = Path.GetExtension(fotografia.FileName).ToLower();

                            nomeFoto += extensao;

                            //onde guardar o ficheiro (wwwroot, algures na nossa aplicação ou repositorio de ficheiros?)
                            // vamos guardar no wwwroot mas apenas após guardarmos os daddos do animal na BD

                            //guardar os dados do ficheiro na BD
                            animal.ListaFotografias.Add(new Fotografias
                            {
                                Ficheiro = nomeFoto,
                                Local = "",
                                Data = DateTime.Now
                            });
                        }
                    }
                }
            }

            //atribuir os dados do PrecoCompraAux ao PrecoCompra
            if (!string.IsNullOrEmpty(animal.PrecoCompraAux))
            {
                animal.PrecoCompra = Convert.ToDecimal(animal.PrecoCompraAux.Replace('.', ','));
            }

            try
            {
                if (ModelState.IsValid)
                {   //adicionar os dados do 'animal' à BD mas apenas na memória do servidor web 
                    _context.Add(animal);
                    //transferir os dados para a BD
                    await _context.SaveChangesAsync();


                    //se cheguei aqui, vamos guarar o ficheiro no disco rígido 
                    if (existeFoto)
                    {
                        //determinar o local onde a foto será guardada (perguntar onde está o wwwroot e acrescentar '/imagens
                        //vamos precisar de um recurso do controller    - linha 22  IWebHostEnvironment
                        string nomePastaFoto = _environment.WebRootPath;
                        //juntar o nome da pasta onde serão guardadas as imagens
                        nomePastaFoto = Path.Combine(nomePastaFoto, "imagens");
                        // mas a pasta existe?
                        if (!Directory.Exists(nomePastaFoto))
                        {
                            Directory.CreateDirectory(nomePastaFoto);
                        }
                        // vamos iniciar a escrita do ficheiro no disco rígido
                        nomeFoto = Path.Combine(nomePastaFoto, nomeFoto);
                        using var stream = new FileStream(nomeFoto, FileMode.Create);
                        await fotografia.CopyToAsync(stream);
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Ocorreu um erro no acesso à Base de dados..."); //este erro vai ser lançado caso não sejam preenchidas as FK
                //throw;
            }

            //preparar os dados para serem enviados de novo para a View - temos de volta a carregar os dados para as DropDowns
            ViewData["CriadorFK"] = new SelectList(_context.Criadores, "Id", "Nome", animal.CriadorFK);
            ViewData["RacaFK"] = new SelectList(_context.Racas.OrderBy(r => r.Nome), "Id", "Nome", animal.RacaFK); //r de Registo
            return View(animal);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Sexo,DataNasc,DataCompra,PrecoCompraAux,RegistoLOP,RacaFK,CriadorFK")] Animais animais)
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
