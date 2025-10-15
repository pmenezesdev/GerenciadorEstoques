using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GerenciadorEstoques
{
    public partial class EntradaSaidaRapidaDialog : Window
    {
        public int Quantidade { get; private set; }
        private Produto produto;
        private bool isEntrada;

        public EntradaSaidaRapidaDialog(Produto produto, bool isEntrada)
        {
            InitializeComponent();
            this.produto = produto;
            this.isEntrada = isEntrada;
            ConfigurarInterface();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtQuantidade.Focus();
            txtQuantidade.SelectAll();
        }

        private void ConfigurarInterface()
        {
            txtProdutoNome.Text = $"{produto.Codigo} - {produto.Nome}";
            txtEstoqueAtual.Text = $"{produto.Quantidade} un.";
            txtPrecoUnitario.Text = produto.Preco.ToString("C2");

            if (isEntrada)
            {
                // Configuração para ENTRADA
                Title = "Entrada de Estoque";
                txtTitulo.Text = "📥 Entrada de Estoque";
                borderTitulo.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
                txtEstoqueAtual.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
                panelPreco.Visibility = Visibility.Collapsed;
                borderValorTotal.Visibility = Visibility.Collapsed;
                btnConfirmar.Content = "✓ Adicionar";
            }
            else
            {
                // Configuração para SAÍDA
                Title = "Saída de Estoque (Venda)";
                txtTitulo.Text = "📤 Saída de Estoque";
                borderTitulo.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
                txtEstoqueAtual.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));
                panelPreco.Visibility = Visibility.Visible;
                borderValorTotal.Visibility = Visibility.Visible;
                btnConfirmar.Content = "✓ Vender";

                // Cor diferente para botão de venda
                btnConfirmar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
            }
        }

        private void BtnQtdRapida_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null && button.Content != null)
            {
                txtQuantidade.Text = button.Content.ToString();
                txtQuantidade.Focus();
                txtQuantidade.SelectAll();
            }
        }

        private void BtnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtQuantidade.Text) ||
                !int.TryParse(txtQuantidade.Text, out int qtd) || qtd <= 0)
            {
                MessageBox.Show("Digite uma quantidade válida maior que zero!", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuantidade.Focus();
                txtQuantidade.SelectAll();
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
            if (!isEntrada && int.TryParse(txtQuantidade.Text, out int qtd) && qtd > 0)
            {
                var valorTotal = qtd * produto.Preco;
                txtValorTotal.Text = valorTotal.ToString("C2");
            }
            else if (!isEntrada)
            {
                txtValorTotal.Text = "R$ 0,00";
            }
        }

        private void TxtNumero_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Enter)
            {
                BtnConfirmar_Click(null, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                BtnCancelar_Click(null, null);
                e.Handled = true;
            }
            // Atalhos numéricos para botões rápidos
            else if (e.Key == Key.D1 || e.Key == Key.NumPad1)
            {
                txtQuantidade.Text = "1";
                txtQuantidade.SelectAll();
                e.Handled = true;
            }
            else if (e.Key == Key.D5 || e.Key == Key.NumPad5)
            {
                txtQuantidade.Text = "5";
                txtQuantidade.SelectAll();
                e.Handled = true;
            }
        }
    }
}