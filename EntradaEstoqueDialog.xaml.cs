using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace GerenciadorEstoques
{
    public partial class EntradaEstoqueDialog : Window
    {
        public int Quantidade { get; private set; }
        private Produto produto;

        public EntradaEstoqueDialog(Produto produto)
        {
            InitializeComponent();
            this.produto = produto;
            txtProdutoNome.Text = $"{produto.Codigo} - {produto.Nome}";
            txtEstoqueAtual.Text = $"{produto.Quantidade} unidades";
            txtQuantidade.Focus();
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtQuantidade.Text) || !int.TryParse(txtQuantidade.Text, out int qtd) || qtd <= 0)
            {
                MessageBox.Show("Digite uma quantidade válida maior que zero!", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuantidade.Focus();
                return;
            }

            Quantidade = qtd;
            DialogResult = true;
            Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TxtNumero_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }
    }
}