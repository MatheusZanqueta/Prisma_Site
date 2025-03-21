using AppLoginAspCoreHL.CarrinhoCompra;
using AppLoginAspCoreHL.Libraries.Filtro;
using AppLoginAspCoreHL.Libraries.Login;
using AppLoginAspCoreHL.Models;
using AppLoginAspCoreHL.Models.Constant;
using AppLoginAspCoreHL.Repository;
using AppLoginAspCoreHL.Repository.Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using System;
using System.Diagnostics;

namespace AppLoginAspCoreHL.Controllers
{
    public class HomeController : Controller
    {
        private IClienteRepository _clienteRepository;
        private IEnderecoRepository _enderecoRepository;
        private ILivroRepository _livroRepository;
        private LoginCliente _loginCliente;
        private CookieCarrinhoCompra _cookieCarrinhoCompra;
        private IPedidoRepository _pedidoRepository;
        private IItemRepository _itemRepository;
        private IPesquisaRepository _pesquisaRepository;
        private ICategoriaRepository _categoriaRepository;

        

        public HomeController(IClienteRepository clienteRepository, LoginCliente loginCliente, IEnderecoRepository enderecoRepository, CookieCarrinhoCompra cookieCarrinhoCompra, ILivroRepository livroRepository, IPedidoRepository pedidoRepository, IItemRepository itemRepository, IPesquisaRepository pesquisaRepository, ICategoriaRepository categoriaRepository)
        {
            _clienteRepository = clienteRepository;
            _loginCliente = loginCliente;
            _enderecoRepository = enderecoRepository;
            _cookieCarrinhoCompra = cookieCarrinhoCompra;
            _livroRepository = livroRepository;
            _pedidoRepository = pedidoRepository;
            _itemRepository = itemRepository;
            _pesquisaRepository = pesquisaRepository;
            _categoriaRepository = categoriaRepository;
        }
        public IActionResult Index()
        {
            return View(_livroRepository.ObterTodosLivros());
        }

        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login([FromForm] Cliente cliente)
        {
            Cliente clienteDB = _clienteRepository.Login(cliente.Login, cliente.Senha);

            if(clienteDB.Login != null && clienteDB.Senha != null)
            {
                _loginCliente.Login(clienteDB);
                return new RedirectResult(Url.Action(nameof(PainelUsuario)));
            }
            else
            {
                ViewData["MSG_E"] = "Usu�rio n�o localizado, por favor verifique e-mail e senha digitado";
                return View();            
            }
        }

        public IActionResult PainelUsuario()
        {
            Cliente cli = _clienteRepository.ObterCliente(_loginCliente.GetCliente().Id);
            return View(cli);
        }
        [HttpGet]
        [ValidateHttpReferer]
        public IActionResult EditUsuario(int id)
        {
            Cliente cli = _clienteRepository.ObterCliente(id);
            return View(cli);
        }
        [HttpPost]
        public IActionResult EditUsuario([FromForm] Cliente cli)
        {
            if (ModelState.IsValid)
            {
                _clienteRepository.Atualizar(cli);
                TempData["MSG_S"] = "Registro salvo com sucesso!";
                return RedirectToAction(nameof(PainelUsuario));
            }
            return View();
        }
        [ClienteAutorizacao]
        public IActionResult LogoutCliente()
        {
            _loginCliente.Logout();
            return new RedirectResult(Url.Action(nameof(Index)));
        }
        [HttpGet]
        public IActionResult Cadastrar()
        {
            IEnumerable<Estado> listaEstados = _enderecoRepository.ObterTodosEstados();
            ViewBag.ListaEstados = new SelectList(listaEstados, "uf", "nome");
            return View();
        }
        [HttpPost]
        public IActionResult Cadastrar([FromForm] Cliente cliente) 
        {
            var listaEstados = _enderecoRepository.ObterTodosEstados();
            ViewBag.ListaEstados = new SelectList(listaEstados, "uf", "nome");
            if (ModelState.IsValid)
            {
                _clienteRepository.Cadastrar(cliente);
                return RedirectToAction(nameof(Login));
            }
            return View();
                
        }

        public IActionResult PainelEndereco()
        {
            Endereco en = _enderecoRepository.ObterEndereco(_loginCliente.GetCliente().Id);
            return View(en);
        }

        [HttpGet]
        public IActionResult EditEndereco(int id)
        {
            Endereco en = _enderecoRepository.ObterEndereco(id);
            IEnumerable<Estado> listaEstados = _enderecoRepository.ObterTodosEstados();
            ViewBag.ListaEstados = new SelectList(listaEstados, "uf", "nome");
            return View(en);
        }

        [HttpPost]
        public IActionResult EditEndereco([FromForm] Endereco endereco)
        {
            var listaEstados = _enderecoRepository.ObterTodosEstados();
            ViewBag.ListaEstados = new SelectList(listaEstados, "uf", "nome");
            if (ModelState.IsValid)
            {
                _enderecoRepository.Atualizar(endereco);
                return RedirectToAction(nameof(PainelEndereco));
            }
            return View();
        }
        public IActionResult AdicionarItem(int id)
        {
            Livro produto = _livroRepository.ObterLivro(id);

            if (produto == null)
            {
                return View("N�o existe item");
            }
            else
            {
                var item = new Livro()
                {
                    Id = id,
                    QuantidadeEstq = 1,
                    Imagem = produto.Imagem,
                    Titulo = produto.Titulo,
                    Preco = produto.Preco
                };
                _cookieCarrinhoCompra.Cadastrar(item);
                return RedirectToAction(nameof(Carrinho));
            }
        }
        public IActionResult DiminuirItem(int id)
        {
            Livro produto = _livroRepository.ObterLivro(id);

            if (produto == null)
            {
                return View("N�o existe item");
            }
            else
            {
                var item = new Livro()
                {
                    Id = id,
                    QuantidadeEstq = 1,
                    Imagem = produto.Imagem,
                    Titulo = produto.Titulo,
                    Preco = produto.Preco
                };
                _cookieCarrinhoCompra.Diminuir(item);
                return RedirectToAction(nameof(Carrinho));
            }
        }
        public IActionResult Carrinho()
        {
            if(_cookieCarrinhoCompra.Consultar().Count == 0)
            {
                ViewData["MSG_E"] = "Seu carrinho est� vazio! ";
                return View("Erro");
            }
            return View(_cookieCarrinhoCompra.Consultar());
        }
        public IActionResult RemoverItem(int id)
        {
            _cookieCarrinhoCompra.Remover(new Livro() { Id = id });
            return RedirectToAction(nameof(Carrinho));
        }

        [ClienteAutorizacao]
        public IActionResult ConfirmaEndereco()
        {
            return View(_enderecoRepository.ObterEndereco(_loginCliente.GetCliente().Id));
        }


        [ClienteAutorizacao]
        public IActionResult SalvarCarrinho(Pedido pedido)
        {
            List<Livro> carrinho = _cookieCarrinhoCompra.Consultar();

            if (carrinho.Count > 0)
            {
                Pedido mdE = new Pedido();
                ItensPedido mdI = new ItensPedido();

                mdE.Id_usu = _loginCliente.GetCliente().Id;
                mdE.Situacao = PedidoTipoConstant.Andamento;


                _pedidoRepository.Cadastrar(mdE);
                _pedidoRepository.BuscarPedidoPorId(pedido);

                double valorTotalItens = 0;
                for (int i = 0; i < carrinho.Count; i++)
                {
                    mdI.Id_pedido = Convert.ToInt32(pedido.Id_pedido);
                    mdI.Id_liv = Convert.ToInt32(carrinho[i].Id);
                    mdI.QtItens = Convert.ToInt32(carrinho[i].QuantidadeEstq);
                    mdI.VlTotal = Convert.ToDouble(carrinho[i].Preco * carrinho[i].QuantidadeEstq);
                    valorTotalItens += mdI.VlTotal;
                    _itemRepository.Cadastrar(mdI);
                }

                _pedidoRepository.InputValor(Math.Round(valorTotalItens, 2), pedido.Id_pedido);

                _cookieCarrinhoCompra.RemoverTodos();
                ViewData["NumPedido"] = pedido.Id_pedido;
                return View(_itemRepository.ObterTodosItensPedido(pedido.Id_pedido, mdE.Id_usu));
            }
            return RedirectToAction(nameof(Carrinho));

        }
        [ClienteAutorizacao]
        public IActionResult DetalhesPedido()
        {
            return View();
        }
        
        [HttpGet]
        public IActionResult Search(string searchString)
        {
            List<PesquisaLivro> livros = _pesquisaRepository.PesquisarLivros(searchString);
            if(livros.Count == 0)
            {
                ViewData["MSG_E"] = "Nenhum livro foi encontrado! ";
                return View("Erro");
            }
            return View(livros);
        }
        public IActionResult SearchByCategoria(int id)
        {
            List<PesquisaLivro> livros = _pesquisaRepository.PesquisarLivrosPorCategoria(id);
            if (livros.Count == 0)
            {
                ViewData["MSG_E"] = "Nenhum livro foi encontrado! ";
                return View("Erro");
            }
            return View(livros);
        }
        public IActionResult Detalhes(int Id)
        {
            Livro liv = _livroRepository.ObterLivro(Id);
            return View(liv);
        }

        public IActionResult SaibaMais()
        {
            return View();
        }
        public IActionResult TodosLivros() 
        {
            return View(_livroRepository.ObterTodosLivros());
        }   
    }
}
