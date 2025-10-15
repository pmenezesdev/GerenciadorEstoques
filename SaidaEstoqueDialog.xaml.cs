using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GerenciadorEstoques
{
    public partial class SaidaEstoqueDialog : Window
    {
        public int Quantidade { get; private set; }
        private Produto produto;

        public SaidaEstoqueDialog(Produto produto)
        {
            InitializeComponent();
            this.produto = produto;
            txtProdutoNome.Text = $"{produto.Codigo} - {produto.Nome}";
            txtEstoqueAtual.Text = $"{produto.Quantidade} unidades";
            txtPrecoUnitario.Text = produto.Preco.ToString("C2");
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

        private void TxtQuantidade_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(txtQuantidade.Text, out int qtd) && qtd > 0)
            {
                var valorTotal = qtd * produto.Preco;
                txtValorTotal.Text = $"Valor Total: {valorTotal:C2}";
            }
            else
            {
                txtValorTotal.Text = "Valor Total: R$ 0,00";
            }
        }

        private void TxtNumero_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }
    }
}