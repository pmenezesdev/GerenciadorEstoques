using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GerenciadorEstoques.Models;
using Microsoft.Win32;
using GerenciadorEstoques.Models;

namespace GerenciadorEstoques
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<Produto> produtos;
        private ObservableCollection<Produto> produtosFiltrados;
        private Produto produtoSelecionado;
        private const string ARQUIVO_DADOS = "estoque.json";
        private const string ARQUIVO_HISTORICO = "historico.json";
        private List<MovimentacaoEstoque> historico;

        public MainWindow()
        {
            InitializeComponent();

            // Inicializa TODAS as coleções ANTES de qualquer operação
            produtos = new ObservableCollection<Produto>();
            produtosFiltrados = new ObservableCollection<Produto>();
            historico = new List<MovimentacaoEstoque>();

            // Define o ItemsSource
            dgProdutos.ItemsSource = produtosFiltrados;

            // Aguarda a janela carregar completamente
            this.Loaded += (s, e) =>
            {
                CarregarDados();
                CarregarHistorico();
                AtualizarEstatisticas();
                txtCodigo.Focus();
            };
        }

        // Atalhos de teclado
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                if (produtoSelecionado != null)
                {
                    txtCodigo.Focus();
                    txtCodigo.SelectAll();
                }
            }
            else if (e.Key == Key.F3)
            {
                txtBusca.Focus();
                txtBusca.SelectAll();
            }
            else if (e.Key == Key.Escape)
            {
                LimparCampos();
            }
            else if (e.Key == Key.Delete)
            {
                if (produtoSelecionado != null && !txtCodigo.IsFocused && !txtNome.IsFocused &&
                    !txtQuantidade.IsFocused && !txtPreco.IsFocused && !txtPrecoCusto.IsFocused && !txtBusca.IsFocused)
                {
                    BtnRemoverRapido_Click(null, null);
                }
            }
        }

        // Adiciona novo produto
        private void BtnAdicionar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos())
                return;

            if (produtos.Any(p => p.Codigo.Equals(txtCodigo.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Já existe um produto com este código!", "Código Duplicado",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCodigo.Focus();
                txtCodigo.SelectAll();
                return;
            }

            var produto = new Produto
            {
                Codigo = txtCodigo.Text.Trim(),
                Nome = txtNome.Text.Trim(),
                Categoria = (cmbCategoria.SelectedItem as ComboBoxItem)?.Content.ToString(),
                Quantidade = int.Parse(txtQuantidade.Text),
                PrecoCusto = string.IsNullOrWhiteSpace(txtPrecoCusto.Text) ? 0 :
                    decimal.Parse(txtPrecoCusto.Text.Replace(",", "."), CultureInfo.InvariantCulture),
                Preco = decimal.Parse(txtPreco.Text.Replace(",", "."), CultureInfo.InvariantCulture)
            };

            produtos.Add(produto);
            RegistrarMovimentacao(produto.Codigo, "Cadastro Inicial", produto.Quantidade, produto.Quantidade);
            AtualizarFiltro();
            LimparCampos();
            SalvarDados();
            AtualizarEstatisticas();

            MostrarNotificacao($"✓ Produto '{produto.Nome}' adicionado com sucesso!", "#4CAF50");
        }

        // Atualiza produto existente
        private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            if (produtoSelecionado == null)
                return;

            if (!ValidarCampos())
                return;

            var codigoAlterado = !produtoSelecionado.Codigo.Equals(txtCodigo.Text.Trim(), StringComparison.OrdinalIgnoreCase);
            if (codigoAlterado && produtos.Any(p => p.Codigo.Equals(txtCodigo.Text.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Já existe um produto com este código!", "Código Duplicado",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCodigo.Focus();
                txtCodigo.SelectAll();
                return;
            }

            int quantidadeAnterior = produtoSelecionado.Quantidade;
            int novaQuantidade = int.Parse(txtQuantidade.Text);

            produtoSelecionado.Codigo = txtCodigo.Text.Trim();
            produtoSelecionado.Nome = txtNome.Text.Trim();
            produtoSelecionado.Categoria = (cmbCategoria.SelectedItem as ComboBoxItem)?.Content.ToString();
            produtoSelecionado.Quantidade = novaQuantidade;
            produtoSelecionado.PrecoCusto = string.IsNullOrWhiteSpace(txtPrecoCusto.Text) ? 0 :
                decimal.Parse(txtPrecoCusto.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            produtoSelecionado.Preco = decimal.Parse(txtPreco.Text.Replace(",", "."), CultureInfo.InvariantCulture);

            if (quantidadeAnterior != novaQuantidade)
            {
                RegistrarMovimentacao(produtoSelecionado.Codigo, "Ajuste Manual",
                    novaQuantidade - quantidadeAnterior, novaQuantidade);
            }

            dgProdutos.Items.Refresh();
            LimparCampos();
            SalvarDados();
            AtualizarEstatisticas();

            MostrarNotificacao("✓ Produto atualizado com sucesso!", "#2196F3");
        }

        // Cancela operação
        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            LimparCampos();
        }

        // Edição rápida via botão
        private void BtnEditarRapido_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var produto = button?.Tag as Produto;
            if (produto != null)
            {
                dgProdutos.SelectedItem = produto;
                txtCodigo.Focus();
                txtCodigo.SelectAll();
            }
        }

        // Remoção rápida via botão
        private void BtnRemoverRapido_Click(object sender, RoutedEventArgs e)
        {
            Produto produto = null;

            if (sender != null)
            {
                var button = sender as Button;
                produto = button?.Tag as Produto;
            }
            else
            {
                produto = produtoSelecionado;
            }

            if (produto == null)
                return;

            var result = MessageBox.Show(
                $"Remover produto?\n\n" +
                $"• {produto.Nome}\n" +
                $"• Código: {produto.Codigo}\n" +
                $"• Qtd: {produto.Quantidade} | Valor: {produto.ValorTotalVenda:C2}",
                "Confirmar Exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                RegistrarMovimentacao(produto.Codigo, "Produto Removido", -produto.Quantidade, 0);
                produtos.Remove(produto);
                AtualizarFiltro();
                LimparCampos();
                SalvarDados();
                AtualizarEstatisticas();

                MostrarNotificacao("✓ Produto removido!", "#F44336");
            }
        }

        // Entrada rápida direto da grid
        private void BtnEntradaRapida_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var produto = button?.Tag as Produto;
            if (produto == null)
                return;

            var dialog = new EntradaSaidaRapidaDialog(produto, true);
            if (dialog.ShowDialog() == true)
            {
                int quantidade = dialog.Quantidade;
                produto.Quantidade += quantidade;
                RegistrarMovimentacao(produto.Codigo, "Entrada de Estoque",
                    quantidade, produto.Quantidade);
                dgProdutos.Items.Refresh();
                SalvarDados();
                AtualizarEstatisticas();

                MostrarNotificacao($"✓ {produto.Nome}: +{quantidade} unidades", "#4CAF50");
            }
        }

        // Saída rápida direto da grid
        private void BtnSaidaRapida_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var produto = button?.Tag as Produto;
            if (produto == null)
                return;

            var dialog = new EntradaSaidaRapidaDialog(produto, false);
            if (dialog.ShowDialog() == true)
            {
                int quantidade = dialog.Quantidade;

                if (quantidade > produto.Quantidade)
                {
                    var result = MessageBox.Show(
                        $"Quantidade solicitada: {quantidade}\nEstoque disponível: {produto.Quantidade}\n\n" +
                        "Deseja registrar mesmo assim (estoque negativo)?",
                        "Estoque Insuficiente",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                        return;
                }

                decimal valorVenda = quantidade * produto.Preco;
                produto.Quantidade -= quantidade;
                RegistrarMovimentacao(produto.Codigo, "Saída de Estoque (Venda)",
                    -quantidade, produto.Quantidade);
                dgProdutos.Items.Refresh();
                SalvarDados();
                AtualizarEstatisticas();

                MostrarNotificacao($"✓ {produto.Nome}: -{quantidade} un. | {valorVenda:C2}", "#FF9800");
            }
        }

        // Entrada de estoque em lote
        private void BtnEntrada_Click(object sender, RoutedEventArgs e)
        {
            if (produtoSelecionado == null)
            {
                MessageBox.Show("Selecione um produto primeiro!", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new EntradaSaidaRapidaDialog(produtoSelecionado, true);
            if (dialog.ShowDialog() == true)
            {
                int quantidade = dialog.Quantidade;
                produtoSelecionado.Quantidade += quantidade;
                RegistrarMovimentacao(produtoSelecionado.Codigo, "Entrada de Estoque",
                    quantidade, produtoSelecionado.Quantidade);
                dgProdutos.Items.Refresh();
                SalvarDados();
                AtualizarEstatisticas();

                MostrarNotificacao($"✓ +{quantidade} unidades adicionadas", "#4CAF50");
            }
        }

        // Saída de estoque (venda)
        private void BtnSaida_Click(object sender, RoutedEventArgs e)
        {
            if (produtoSelecionado == null)
            {
                MessageBox.Show("Selecione um produto primeiro!", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new EntradaSaidaRapidaDialog(produtoSelecionado, false);
            if (dialog.ShowDialog() == true)
            {
                int quantidade = dialog.Quantidade;

                if (quantidade > produtoSelecionado.Quantidade)
                {
                    var result = MessageBox.Show(
                        $"Quantidade solicitada: {quantidade}\nEstoque disponível: {produtoSelecionado.Quantidade}\n\n" +
                        "Deseja registrar mesmo assim (estoque negativo)?",
                        "Estoque Insuficiente",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                        return;
                }

                decimal valorVenda = quantidade * produtoSelecionado.Preco;
                produtoSelecionado.Quantidade -= quantidade;
                RegistrarMovimentacao(produtoSelecionado.Codigo, "Saída de Estoque (Venda)",
                    -quantidade, produtoSelecionado.Quantidade);
                dgProdutos.Items.Refresh();
                SalvarDados();
                AtualizarEstatisticas();

                MostrarNotificacao($"✓ Venda: -{quantidade} un. | {valorVenda:C2}", "#FF9800");
            }
        }

        // Gerar relatório
        private void BtnRelatorio_Click(object sender, RoutedEventArgs e)
        {
            var relatorio = new StringBuilder();
            relatorio.AppendLine("═══════════════════════════════════════════════════════");
            relatorio.AppendLine("        RELATÓRIO DE ESTOQUE - MERCADINHO");
            relatorio.AppendLine($"        Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}");
            relatorio.AppendLine("═══════════════════════════════════════════════════════");
            relatorio.AppendLine();

            relatorio.AppendLine("📊 RESUMO GERAL");
            relatorio.AppendLine("───────────────────────────────────────────────────────");
            relatorio.AppendLine($"Total de Produtos: {produtos.Count}");
            relatorio.AppendLine($"Total de Itens em Estoque: {produtos.Sum(p => p.Quantidade)}");
            relatorio.AppendLine($"Valor Investido (Custo): {produtos.Sum(p => p.ValorTotalCusto):C2}");
            relatorio.AppendLine($"Valor de Venda Total: {produtos.Sum(p => p.ValorTotalVenda):C2}");
            relatorio.AppendLine($"Lucro Potencial: {produtos.Sum(p => p.LucroPotencial):C2}");
            relatorio.AppendLine();

            var estoquesBaixos = produtos.Where(p => p.Quantidade <= 10 && p.Quantidade > 0).ToList();
            if (estoquesBaixos.Any())
            {
                relatorio.AppendLine("⚠️ PRODUTOS COM ESTOQUE BAIXO (≤ 10 unidades)");
                relatorio.AppendLine("───────────────────────────────────────────────────────");
                foreach (var produto in estoquesBaixos.OrderBy(p => p.Quantidade))
                {
                    relatorio.AppendLine($"• {produto.Nome} ({produto.Codigo}) - {produto.Quantidade} unidades");
                }
                relatorio.AppendLine();
            }

            var estoquesZerados = produtos.Where(p => p.Quantidade == 0).ToList();
            if (estoquesZerados.Any())
            {
                relatorio.AppendLine("❌ PRODUTOS COM ESTOQUE ZERADO");
                relatorio.AppendLine("───────────────────────────────────────────────────────");
                foreach (var produto in estoquesZerados)
                {
                    relatorio.AppendLine($"• {produto.Nome} ({produto.Codigo})");
                }
                relatorio.AppendLine();
            }

            relatorio.AppendLine("💰 PRODUTOS MAIS VALIOSOS (Top 10)");
            relatorio.AppendLine("───────────────────────────────────────────────────────");
            var top10Valiosos = produtos.OrderByDescending(p => p.ValorTotalVenda).Take(10);
            foreach (var produto in top10Valiosos)
            {
                relatorio.AppendLine($"• {produto.Nome} - {produto.Quantidade} un. - {produto.ValorTotalVenda:C2}");
            }
            relatorio.AppendLine();

            relatorio.AppendLine("📦 ESTOQUE POR CATEGORIA");
            relatorio.AppendLine("───────────────────────────────────────────────────────");
            var porCategoria = produtos.GroupBy(p => p.Categoria ?? "Sem Categoria");
            foreach (var grupo in porCategoria.OrderBy(g => g.Key))
            {
                var qtdProdutos = grupo.Count();
                var qtdItens = grupo.Sum(p => p.Quantidade);
                var valorTotal = grupo.Sum(p => p.ValorTotalVenda);
                relatorio.AppendLine($"• {grupo.Key}: {qtdProdutos} produtos, {qtdItens} itens, {valorTotal:C2}");
            }
            relatorio.AppendLine();

            if (historico.Any())
            {
                relatorio.AppendLine("📋 ÚLTIMAS MOVIMENTAÇÕES (10 mais recentes)");
                relatorio.AppendLine("───────────────────────────────────────────────────────");
                var ultimasMovimentacoes = historico.OrderByDescending(h => h.Data).Take(10);
                foreach (var mov in ultimasMovimentacoes)
                {
                    var sinal = mov.Quantidade > 0 ? "+" : "";
                    relatorio.AppendLine($"• {mov.Data:dd/MM/yyyy HH:mm} - {mov.CodigoProduto} - {mov.Tipo}");
                    relatorio.AppendLine($"  Qtd: {sinal}{mov.Quantidade} | Saldo: {mov.QuantidadeApos}");
                }
            }

            relatorio.AppendLine();
            relatorio.AppendLine("═══════════════════════════════════════════════════════");
            relatorio.AppendLine("               FIM DO RELATÓRIO");
            relatorio.AppendLine("═══════════════════════════════════════════════════════");

            var resultado = MessageBox.Show(relatorio.ToString(), "Relatório de Estoque",
                MessageBoxButton.OKCancel, MessageBoxImage.Information);

            if (resultado == MessageBoxResult.OK)
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Arquivo de Texto (*.txt)|*.txt",
                    FileName = $"Relatorio_Estoque_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveDialog.FileName, relatorio.ToString());
                    MostrarNotificacao("✓ Relatório salvo com sucesso!", "#9C27B0");
                }
            }
        }// Exportar dados para CSV
        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            if (produtos.Count == 0)
            {
                MessageBox.Show("Não há produtos para exportar!", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Arquivo CSV (*.csv)|*.csv",
                FileName = $"Estoque_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var csv = new StringBuilder();
                    csv.AppendLine("Codigo;Nome;Categoria;Quantidade;Preco_Custo;Preco_Venda;Margem_Lucro;Valor_Total_Custo;Valor_Total_Venda;Lucro_Potencial");

                    foreach (var produto in produtos)
                    {
                        csv.AppendLine($"{produto.Codigo};{produto.Nome};{produto.Categoria};{produto.Quantidade};" +
                            $"{produto.PrecoCusto:F2};{produto.Preco:F2};{produto.MargemLucro:F4};" +
                            $"{produto.ValorTotalCusto:F2};{produto.ValorTotalVenda:F2};{produto.LucroPotencial:F2}");
                    }

                    File.WriteAllText(saveDialog.FileName, csv.ToString(), Encoding.UTF8);
                    MostrarNotificacao($"✓ Dados exportados com sucesso!", "#2196F3");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao exportar dados: {ex.Message}", "Erro",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Seleção de produto no grid
        private void DgProdutos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            produtoSelecionado = dgProdutos.SelectedItem as Produto;

            if (produtoSelecionado != null)
            {
                txtCodigo.Text = produtoSelecionado.Codigo;
                txtNome.Text = produtoSelecionado.Nome;

                for (int i = 0; i < cmbCategoria.Items.Count; i++)
                {
                    var item = cmbCategoria.Items[i] as ComboBoxItem;
                    if (item?.Content.ToString() == produtoSelecionado.Categoria)
                    {
                        cmbCategoria.SelectedIndex = i;
                        break;
                    }
                }

                txtQuantidade.Text = produtoSelecionado.Quantidade.ToString();
                txtPrecoCusto.Text = produtoSelecionado.PrecoCusto.ToString("F2", CultureInfo.InvariantCulture).Replace(".", ",");
                txtPreco.Text = produtoSelecionado.Preco.ToString("F2", CultureInfo.InvariantCulture).Replace(".", ",");

                btnAdicionar.IsEnabled = false;
                btnAtualizar.IsEnabled = true;
            }
        }

        // Editar ao dar duplo clique
        private void DgProdutos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (produtoSelecionado != null)
            {
                txtCodigo.Focus();
                txtCodigo.SelectAll();
            }
        }

        // Filtro de busca
        private void TxtBusca_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtPlaceholderBusca != null)
                txtPlaceholderBusca.Visibility = string.IsNullOrEmpty(txtBusca.Text) ? Visibility.Visible : Visibility.Collapsed;

            AtualizarFiltro();
        }

        // Filtro por tipo de estoque
        private void CmbFiltroEstoque_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AtualizarFiltro();
        }

        // Atualiza lista filtrada - COM PROTEÇÃO TOTAL
        private void AtualizarFiltro()
        {
            // Proteção contra chamadas antes da inicialização
            if (produtosFiltrados == null || produtos == null)
                return;

            produtosFiltrados.Clear();

            var termo = txtBusca?.Text?.Trim().ToLower() ?? "";
            var produtosFilt = produtos.AsEnumerable();

            if (!string.IsNullOrEmpty(termo))
            {
                produtosFilt = produtosFilt.Where(p =>
                    (p.Codigo?.ToLower().Contains(termo) ?? false) ||
                    (p.Nome?.ToLower().Contains(termo) ?? false) ||
                    (p.Categoria?.ToLower().Contains(termo) ?? false));
            }

            // Verifica se cmbFiltroEstoque já foi inicializado
            if (cmbFiltroEstoque != null && cmbFiltroEstoque.SelectedIndex > 0)
            {
                switch (cmbFiltroEstoque.SelectedIndex)
                {
                    case 1: // Estoque Baixo
                        produtosFilt = produtosFilt.Where(p => p.Quantidade <= 10 && p.Quantidade > 0);
                        break;
                    case 2: // Estoque Zerado
                        produtosFilt = produtosFilt.Where(p => p.Quantidade == 0);
                        break;
                    case 3: // Estoque Normal
                        produtosFilt = produtosFilt.Where(p => p.Quantidade > 10);
                        break;
                }
            }

            foreach (var produto in produtosFilt.OrderBy(p => p.Nome))
            {
                produtosFiltrados.Add(produto);
            }
        }

        // Validação de entrada numérica
        private void TxtNumero_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumeric(e.Text);
        }

        // Validação de entrada decimal
        private void TxtDecimal_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            e.Handled = !IsDecimal(fullText);
        }

        private bool IsNumeric(string text)
        {
            return Regex.IsMatch(text, @"^[0-9]+$");
        }

        private bool IsDecimal(string text)
        {
            return Regex.IsMatch(text, @"^[0-9]*[,.]?[0-9]*$");
        }

        // Valida campos do formulário
        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtCodigo.Text))
            {
                MessageBox.Show("O código do produto é obrigatório!", "Campo Obrigatório",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCodigo.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("O nome do produto é obrigatório!", "Campo Obrigatório",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNome.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtQuantidade.Text) || !int.TryParse(txtQuantidade.Text, out int quantidade) || quantidade < 0)
            {
                MessageBox.Show("A quantidade deve ser um número inteiro válido e não negativo!",
                    "Valor Inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuantidade.Focus();
                txtQuantidade.SelectAll();
                return false;
            }

            var precoTexto = txtPreco.Text.Replace(",", ".");
            if (string.IsNullOrWhiteSpace(precoTexto) || !decimal.TryParse(precoTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal preco) || preco < 0)
            {
                MessageBox.Show("O preço de venda deve ser um número válido e não negativo!", "Valor Inválido",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPreco.Focus();
                txtPreco.SelectAll();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtPrecoCusto.Text))
            {
                var custoTexto = txtPrecoCusto.Text.Replace(",", ".");
                if (!decimal.TryParse(custoTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal custo) || custo < 0)
                {
                    MessageBox.Show("O preço de custo deve ser um número válido e não negativo!", "Valor Inválido",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPrecoCusto.Focus();
                    txtPrecoCusto.SelectAll();
                    return false;
                }
            }

            return true;
        }

        // Limpa campos do formulário
        private void LimparCampos()
        {
            txtCodigo.Clear();
            txtNome.Clear();
            cmbCategoria.SelectedIndex = 0;
            txtQuantidade.Clear();
            txtPrecoCusto.Clear();
            txtPreco.Clear();
            dgProdutos.SelectedItem = null;
            produtoSelecionado = null;
            btnAdicionar.IsEnabled = true;
            btnAtualizar.IsEnabled = false;
            txtCodigo.Focus();
        }

        // Atualiza estatísticas
        private void AtualizarEstatisticas()
        {
            if (txtTotal == null || produtos == null)
                return;

            txtTotal.Text = produtos.Count.ToString();
            txtTotalItens.Text = produtos.Sum(p => p.Quantidade).ToString();
            txtValorCusto.Text = produtos.Sum(p => p.ValorTotalCusto).ToString("C2");
            txtValorTotal.Text = produtos.Sum(p => p.ValorTotalVenda).ToString("C2");
            txtLucroPotencial.Text = produtos.Sum(p => p.LucroPotencial).ToString("C2");
        }

        // Registra movimentação no histórico
        private void RegistrarMovimentacao(string codigoProduto, string tipo, int quantidade, int quantidadeApos)
        {
            historico.Add(new MovimentacaoEstoque
            {
                Data = DateTime.Now,
                CodigoProduto = codigoProduto,
                Tipo = tipo,
                Quantidade = quantidade,
                QuantidadeApos = quantidadeApos
            });

            SalvarHistorico();
        }

        // Sistema de notificação toast
        private void MostrarNotificacao(string mensagem, string cor)
        {
            var toast = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cor)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(15, 10, 15, 10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 10, 0, 0),
                Opacity = 0
            };

            var texto = new TextBlock
            {
                Text = mensagem,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 13
            };

            toast.Child = texto;

            // Adiciona ao grid principal
            var mainGrid = this.Content as Grid;
            if (mainGrid != null)
            {
                Grid.SetRowSpan(toast, 4);
                mainGrid.Children.Add(toast);

                // Animação de fade in/out
                var fadeIn = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(300)
                };

                var fadeOut = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(300),
                    BeginTime = TimeSpan.FromSeconds(2)
                };

                fadeOut.Completed += (s, e) => mainGrid.Children.Remove(toast);

                toast.BeginAnimation(OpacityProperty, fadeIn);
                toast.BeginAnimation(OpacityProperty, fadeOut);
            }
        }

        // Salva dados
        private void SalvarDados()
        {
            try
            {
                var json = JsonSerializer.Serialize(produtos.ToList(), new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(ARQUIVO_DADOS, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar dados: {ex.Message}", "Erro de Gravação",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Carrega dados
        private void CarregarDados()
        {
            try
            {
                if (File.Exists(ARQUIVO_DADOS))
                {
                    var json = File.ReadAllText(ARQUIVO_DADOS);
                    var lista = JsonSerializer.Deserialize<List<Produto>>(json);

                    if (lista != null && lista.Count > 0)
                    {
                        produtos.Clear();
                        foreach (var produto in lista)
                        {
                            produtos.Add(produto);
                        }
                        AtualizarFiltro();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar dados: {ex.Message}\n\nO sistema iniciará sem dados.",
                    "Erro ao Carregar", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Salva histórico
        private void SalvarHistorico()
        {
            try
            {
                var json = JsonSerializer.Serialize(historico, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(ARQUIVO_HISTORICO, json);
            }
            catch { }
        }

        // Carrega histórico
        private void CarregarHistorico()
        {
            try
            {
                if (File.Exists(ARQUIVO_HISTORICO))
                {
                    var json = File.ReadAllText(ARQUIVO_HISTORICO);
                    historico = JsonSerializer.Deserialize<List<MovimentacaoEstoque>>(json) ?? new List<MovimentacaoEstoque>();
                }
            }
            catch { }
        }
    }

}