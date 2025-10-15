# 🛒 Sistema de Gerenciamento de Estoques

Sistema completo de controle de estoque desenvolvido em C# com WPF para pequenos mercados e comércios.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![C#](https://img.shields.io/badge/C%23-12.0-239120)
![WPF](https://img.shields.io/badge/WPF-Desktop-0078D4)
![License](https://img.shields.io/badge/license-MIT-blue)

## 📋 Sobre o Projeto

Sistema desktop desenvolvido para facilitar o gerenciamento de estoque em mercadinhos de bairro, com interface intuitiva e funcionalidades práticas para o dia a dia. Projeto criado como solução real para otimizar o controle de inventário em pequenos negócios.

## ✨ Funcionalidades

- ✅ **Cadastro Completo de Produtos** - Código, nome, categoria, quantidade, preço de custo e venda
- 📥 **Entrada de Estoque** - Registro rápido de entrada de mercadorias com botões de quantidade
- 📤 **Saída de Estoque (Vendas)** - Controle de vendas com cálculo automático do valor
- 📊 **Estatísticas em Tempo Real** - Total de produtos, itens, valor investido, valor de venda e lucro potencial
- 🔍 **Busca e Filtros Avançados** - Por código, nome, categoria, estoque baixo/zerado
- 📋 **Relatórios Detalhados** - Análise completa do estoque com histórico de movimentações
- 💾 **Exportação CSV** - Dados prontos para Excel e análises externas
- 📝 **Histórico de Movimentações** - Rastreamento completo de todas operações
- 🌙 **Interface Dark Mode** - Design moderno e confortável para uso prolongado
- ⌨️ **Atalhos de Teclado** - Operações rápidas (F2, F3, Delete, Enter, ESC)
- 🎯 **Botões de Ação Rápida** - Entrada/saída/edição/exclusão diretamente na tabela
- 🔔 **Notificações Toast** - Feedback visual não-intrusivo

## 🚀 Tecnologias Utilizadas

- **Linguagem:** C# 12.0
- **Framework:** .NET 8.0
- **Interface:** WPF (Windows Presentation Foundation)
- **Arquitetura:** MVVM com Data Binding
- **Persistência:** JSON (System.Text.Json)
- **Padrões:** INotifyPropertyChanged, ObservableCollection

## 🎯 Diferenciais Técnicos

- Interface responsiva e otimizada para performance
- Validações em tempo real dos dados com feedback visual
- Sistema de notificações toast com animações suaves
- Cálculo automático de margem de lucro e valores totais
- Botões de ação rápida diretamente na grid para agilidade
- Suporte completo a operações via teclado
- Sistema de filtros inteligentes (estoque baixo, zerado, normal)
- Diálogo unificado para entrada/saída com interface adaptativa
- Histórico completo de movimentações para auditoria

## 📦 Como Usar

### Pré-requisitos

- Windows 10/11
- .NET 8.0 Runtime ([Download aqui](https://dotnet.microsoft.com/download/dotnet/8.0))
- Visual Studio 2022 (para desenvolvimento)

### Instalação

1. Clone o repositório:
```bash
git clone https://github.com/pmenezesdev/GerenciadorEstoques.git
```

2. Abra o arquivo `GerenciadorEstoques.sln` no Visual Studio 2022

3. Restaure os pacotes NuGet (automático ao abrir)

4. Compile e execute (F5)

### Tela Principal
- Interface dark mode completa
- Grid com todas as informações dos produtos
- Estatísticas em tempo real no rodapé
- Formulário de cadastro recolhível

### Entrada/Saída de Estoque
- Modal com botões rápidos (1, 5, 10, 20, 50)
- Cálculo automático do valor total
- Interface adaptativa (verde para entrada, laranja para saída)

### Relatórios
- Resumo geral do estoque
- Produtos com estoque baixo
- Produtos zerados
- Top 10 mais valiosos
- Análise por categoria
- Últimas movimentações

## 📊 Estrutura do Projeto

```
GerenciadorEstoques/
├── MainWindow.xaml                    # Interface principal
├── MainWindow.xaml.cs                 # Lógica da tela principal
├── Produto.cs                         # Modelo de dados com propriedades calculadas
├── EntradaSaidaRapidaDialog.xaml     # Interface de movimentação de estoque
├── EntradaSaidaRapidaDialog.xaml.cs  # Lógica de entrada/saída
├── App.xaml                           # Configuração da aplicação
├── App.xaml.cs                        # Inicialização
├── estoque.json                       # Dados dos produtos (gerado automaticamente)
└── historico.json                     # Histórico de movimentações (gerado automaticamente)
```

## 🎯 Roadmap - Melhorias Futuras

- [ ] Integração com leitor de código de barras
- [ ] Impressão de etiquetas de preço
- [ ] Backup automático em nuvem (Google Drive, OneDrive)
- [ ] Sistema de login e controle de permissões multi-usuário
- [ ] Dashboard com gráficos de vendas e análises
- [ ] Notificações de estoque baixo por e-mail
- [ ] Módulo PDV (Ponto de Venda) integrado
- [ ] Cadastro de fornecedores por produto
- [ ] Controle de validade de produtos
- [ ] Migração para banco de dados (SQLite/SQL Server)
- [ ] Relatórios em PDF
- [ ] App mobile complementar (Xamarin/MAUI)

## ⌨️ Atalhos de Teclado

| Tecla | Ação |
|-------|------|
| **F2** | Editar produto selecionado |
| **F3** | Focar no campo de busca |
| **Delete** | Remover produto selecionado |
| **ESC** | Cancelar operação/Limpar formulário |
| **Enter** | Confirmar operação em diálogos |
| **Tab** | Navegar entre campos |

## 📝 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 👤 Autor

**Pedro Menezes**

- GitHub: [@pmenezesdev](https://github.com/pmenezesdev)
- LinkedIn: [Pedro Menezes](https://www.linkedin.com/in/pmenezesdev/)

## 💡 Aprendizados

Este projeto foi desenvolvido para demonstrar:
- Habilidades em C# e .NET
- Conhecimento de WPF e XAML
- Padrão MVVM
- Boas práticas de desenvolvimento
- Interface de usuário intuitiva
- Pensamento orientado ao usuário final

---

⭐ **Se este projeto foi útil para você, considere dar uma estrela!**

---

**Desenvolvido com 💙 por Pedro Menezes**