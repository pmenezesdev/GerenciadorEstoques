using System;

namespace GerenciadorEstoques.Models
{
    public class MovimentacaoEstoque
    {
        public DateTime Data { get; set; }
        public string CodigoProduto { get; set; }
        public string Tipo { get; set; }
        public int Quantidade { get; set; }
        public int QuantidadeApos { get; set; }
    }
}