using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.X509;
using qualyteam.Models;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace qualyteam.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        [BindProperty]
        [Required(ErrorMessage = "O Código é obrigatório !")]
        [Range(1, double.MaxValue, ErrorMessage = "O Código deve ser maior que 0 !")]
        public int Codigo { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "O título é obrigatório !")]
        public string Titulo { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "A categoria é obrigatória !")]
        public string Categoria { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Selecione um processo !")]
        public string? ProcessoSelecionado { get; set; }


        [BindProperty]
        public List<SelectListItem> Processos { get; set; } = new List<SelectListItem>();

        [BindProperty]
        [Required(ErrorMessage = "Selecione um arquivo !")]
        public IFormFile Arquivo { get; set; }

        public List<DocumentoModel> Documentos { get; set; }
        public string MensagemErro { get; set; }
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            ProcessoSelecionado = null;
        }

        public void OnGet()
        {
            MySqlConnectionStringBuilder stringConexao = new MySqlConnectionStringBuilder();
            stringConexao.Server = "qualyteste.mysql.database.azure.com";
            stringConexao.UserID = "adminqualyteam";
            stringConexao.Password = "Gabi2022";
            stringConexao.SslMode = MySqlSslMode.Required;
            stringConexao.Database = "qualyteste";
            // Conectar no banco de dados
            using (MySqlConnection conexao = new MySqlConnection(stringConexao.ConnectionString))
            {
                conexao.Open();
                // Recuperar os documentos
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conexao;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"select a.id,titulo,Processoid, categoria, p.processo from documentos a
inner join processos p on a.Processoid = p.Id
order by titulo";
                MySqlDataReader rdr;
                rdr = cmd.ExecuteReader();
                Documentos = new List<DocumentoModel>();
                while (rdr.Read())
                {
                    var documento = new DocumentoModel();
                    documento.Id = rdr.GetInt32(rdr.GetOrdinal("Id"));
                    documento.Titulo = rdr.GetString(rdr.GetOrdinal("Titulo"));
                    documento.ProcessoId = rdr.GetInt32(rdr.GetOrdinal("ProcessoId"));
                    documento.Categoria = rdr.GetString(rdr.GetOrdinal("Categoria"));
                    documento.Processo = rdr.GetString(rdr.GetOrdinal("processo"));
                    Documentos.Add(documento);
                }
                rdr.Close();
                // Recuperar os processos
                cmd.CommandText = "select Id, processo from processos order by id";
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    SelectListItem item = new SelectListItem();
                    item.Value = rdr.GetString(rdr.GetOrdinal("id"));
                    item.Text = rdr.GetString(rdr.GetOrdinal("processo"));
                    Processos.Add(item);
                }
            }
        }
        public void OnPost()
        {
            var nomearquivo = Arquivo.FileName;
            var extensaoarquivo = Path.GetExtension(nomearquivo);
            extensaoarquivo = extensaoarquivo.ToLower();
            if (extensaoarquivo != ".pdf" && extensaoarquivo != ".doc" && extensaoarquivo != ".xls" && extensaoarquivo != ".docx" && extensaoarquivo != ".xlsx")
            {
                var msg = new StringBuilder();
                msg.AppendLine("Tipo de Arquivo Inválido. Tipos de arquivos permitidos: PDF, DOC, XLS, DOCX e XLSX.");
                msg.AppendLine("Nenhum dado foi gravado.");
                MensagemErro = msg.ToString();

            }
            if (string.IsNullOrEmpty(ProcessoSelecionado))
            {
                MensagemErro = "Selecione um Processo";
            }
            else
            {
                if (ModelState.IsValid == true)
                {
                    MySqlConnectionStringBuilder stringConexao = new MySqlConnectionStringBuilder();
                    stringConexao.Server = "qualyteste.mysql.database.azure.com";
                    stringConexao.UserID = "adminqualyteam";
                    stringConexao.Password = "Gabi2022";
                    stringConexao.SslMode = MySqlSslMode.Required;
                    stringConexao.Database = "qualyteste";
                    // Conectar no banco de dados
                    using (MySqlConnection conexao = new MySqlConnection(stringConexao.ConnectionString))
                    {
                        conexao.Open();
                        // Inserindo novo documento
                        MySqlCommand cmd = new MySqlCommand();
                        cmd.Connection = conexao;
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "insert into documentos(titulo,Processoid,categoria) values(@titulo,@id_processo,@categoria)";
                        cmd.Parameters.AddWithValue("@titulo", Titulo);
                        cmd.Parameters.AddWithValue("@categoria", Categoria);
                        cmd.Parameters.AddWithValue("@id_processo", ProcessoSelecionado);
                        cmd.ExecuteNonQuery();
                        // Recuperar os documentos
                        cmd.CommandText = @"select a.id,titulo,Processoid, categoria, p.processo from documentos a
inner join processos p on a.Processoid = p.Id
order by titulo";
                        MySqlDataReader rdr;
                        rdr = cmd.ExecuteReader();
                        Documentos = new List<DocumentoModel>();
                        while (rdr.Read())
                        {
                            var documento = new DocumentoModel();
                            documento.Id = rdr.GetInt32(rdr.GetOrdinal("Id"));
                            documento.Titulo = rdr.GetString(rdr.GetOrdinal("Titulo"));
                            documento.ProcessoId = rdr.GetInt32(rdr.GetOrdinal("ProcessoId"));
                            documento.Categoria = rdr.GetString(rdr.GetOrdinal("Categoria"));
                            documento.Processo = rdr.GetString(rdr.GetOrdinal("processo"));
                            Documentos.Add(documento);
                        }
                        rdr.Close();
                    }
                }
            }
        }
    }
}