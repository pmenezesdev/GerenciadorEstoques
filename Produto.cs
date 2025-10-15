using System.ComponentModel;

namespace GerenciadorEstoques
{
    public class Produto : INotifyPropertyChanged
    {
        private string _codigo;
        private string _nome;
        private string _categoria;
        private int _quantidade;
        private decimal _precoCusto;
        private decimal _preco;

        public string Codigo
        {
            get => _codigo;
            set
            {
                _codigo = value;
                OnPropertyChanged(nameof(Codigo));
            }
        }

        public string Nome
        {
            get => _nome;
            set
            {
                _nome = value;
                OnPropertyChanged(nameof(Nome));
            }
        }

        public string Categoria
        {
            get => _categoria;
            set
            {
                _categoria = value;
                OnPropertyChanged(nameof(Categoria));
            }
        }

        public int Quantidade
        {
            get => _quantidade;
            set
            {
                _quantidade = value;
                OnPropertyChanged(nameof(Quantidade));
                OnPropertyChanged(nameof(ValorTotalCusto));
                OnPropertyChanged(nameof(ValorTotalVenda));
                OnPropertyChanged(nameof(LucroPotencial));
            }
        }

        public decimal PrecoCusto
        {
            get => _precoCusto;
            set
            {
                _precoCusto = value;
                OnPropertyChanged(nameof(PrecoCusto));
                OnPropertyChanged(nameof(ValorTotalCusto));
                OnPropertyChanged(nameof(MargemLucro));
                OnPropertyChanged(nameof(LucroPotencial));
            }
        }

        public decimal Preco
        {
            get => _preco;
            set
            {
                _preco = value;
                OnPropertyChanged(nameof(Preco));
                OnPropertyChanged(nameof(ValorTotalVenda));
                OnPropertyChanged(nameof(MargemLucro));
                OnPropertyChanged(nameof(LucroPotencial));
            }
        }

        // Propriedades calculadas
        public decimal ValorTotalCusto => Quantidade * PrecoCusto;
        public decimal ValorTotalVenda => Quantidade * Preco;
        public decimal LucroPotencial => ValorTotalVenda - ValorTotalCusto;
        public decimal MargemLucro => PrecoCusto > 0 ? (Preco - PrecoCusto) / PrecoCusto : 0;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}