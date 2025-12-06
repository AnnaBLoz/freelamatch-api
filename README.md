# FreelaMatch API

[![Coverage Report](https://img.shields.io/badge/coverage-view%20report-brightgreen)](https://annabloz.github.io/freelamatch-api/coverage/)

> API RESTful para gerenciamento da plataforma FreelaMatch - Conexão entre Freelancers e Empresas

## 📄 Sobre o Projeto

A **FreelaMatch API** é o backend da plataforma FreelaMatch, responsável por gerenciar toda a lógica de negócio, autenticação, persistência de dados e comunicação com o front-end. 

A API oferece endpoints para:

- **Autenticação e Autorização** com JWT e Identity
- **Gerenciamento de Usuários** (freelancers, empresas e administradores)
- **Sistema de Propostas** com criação, envio e acompanhamento de status
- **Sistema de Match** inteligente baseado em habilidades
- **Busca Avançada** com filtros de perfis e vagas
- **Avaliações e Feedbacks** mútuos entre freelancers e empresas
- **Notificações** sobre propostas e atualizações

## 🔗 Links Importantes

- **Front-end Web**: [freela-match-web](https://github.com/AnnaBLoz/freela-match-web)
- **Relatório de Cobertura**: [Coverage Report](https://annabloz.github.io/freelamatch-api/coverage/)
- **Gestão de Projeto**: [Jira Board](https://freela-match.atlassian.net/jira/software/projects/FLMT/boards/1)
- **Documentação**: [Confluence](https://freela-match.atlassian.net/wiki/spaces/~712020f968dda579d442699a6bba622cb5124c/folder/229556)

## 📊 Code Coverage

- **Cobertura Atual de Testes:**
  - [📈 Visualizar Relatório Detalhado de Cobertura](https://annabloz.github.io/freelamatch-api/coverage/)

## 🚀 Tecnologias

### Backend
- **.NET 8** - Framework principal
- **ASP.NET Core Web API** - Desenvolvimento da API RESTful
- **Entity Framework Core** - ORM para acesso a dados
- **MySQL Workbench** - Banco de dados relacional
- **Identity** - Gerenciamento de autenticação e autorização
- **JWT** - Autenticação via tokens

### Testes
- **xUnit** - Framework de testes
- **Moq** - Biblioteca para mocking
- **FluentAssertions** - Asserções fluentes e legíveis
- **Coverlet** - Cobertura de código

### Ferramentas
- **GitHub Actions** - CI/CD
- **DeepSource** - Análise de qualidade de código
- **ReportGenerator** - Geração de relatórios de cobertura

## 📋 Pré-requisitos

- .NET SDK 8.0 ou superior
- MySQL Workbench
- Visual Studio 2022 ou VS Code

## 🛠️ Instalação e Configuração

### 1. Clone o repositório
```bash
git clone https://github.com/AnnaBLoz/freelamatch-api.git
cd freelamatch-api
```

### 2. Configure o banco de dados

Edite o arquivo `appsettings.json` com sua connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FreelaMatchDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Execute as migrations
```bash
dotnet ef database update
```

### 4. Restaure as dependências
```bash
dotnet restore freela-match-api/freela-match-api.sln
```

### 5. Execute a aplicação
```bash
cd freela-match-api
dotnet run
```

A API estará disponível em: `https://localhost:5001` ou `http://localhost:5000`

## 🧪 Testes

### Executar todos os testes
```bash
dotnet test
```

### Executar com cobertura de código
```bash
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

### Gerar relatório HTML local
```bash
# Instalar a ferramenta (apenas uma vez)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Gerar relatório
reportgenerator -reports:"**/coverage.opencover.xml" -targetdir:"coveragereport" -reporttypes:Html

# Abrir relatório no navegador
start coveragereport/index.html
```

## 📁 Estrutura do Projeto
```
FreelaMatch/
├── freela-match-api/              # Projeto principal (API)
│   ├── Controllers/               # Endpoints da API
│   │   ├── AuthController.cs     # Autenticação
│   │   ├── UserController.cs     # Gerenciamento de usuários
│   │   ├── ProposalController.cs # Propostas
│   │   └── MatchController.cs    # Sistema de match
│   ├── Services/                  # Lógica de negócio
│   │   ├── IUserService.cs
│   │   ├── UserService.cs
│   │   └── ...
│   ├── Models/                    # Modelos de domínio
│   │   ├── User.cs
│   │   ├── Proposal.cs
│   │   └── ...
│   ├── Data/                      # Context do EF Core
│   │   └── ApplicationDbContext.cs
│   ├── DTOs/                      # Data Transfer Objects
│   │   ├── Requests/
│   │   └── Responses/
│   ├── Middlewares/               # Middlewares customizados
│   ├── Validators/                # Validações
│   └── Program.cs                 # Entry point
├── freela-match-api-test/         # Projeto de testes
│   ├── Controllers/               # Testes dos controllers
│   ├── Services/                  # Testes dos services
│   └── Helpers/                   # Utilitários de teste
└── .github/
    └── workflows/                 # CI/CD com GitHub Actions
        └── sonarcloud.yml
```

## 🔄 CI/CD

O projeto utiliza **GitHub Actions** para automação:

- ✅ **Build automático** em cada push/PR
- ✅ **Execução de testes unitários** com relatórios
- ✅ **Análise de qualidade** via DeepSource
- ✅ **Geração de cobertura de código**
- ✅ **Comentários automáticos** em PRs com métricas
- ✅ **Deploy do relatório** de cobertura no GitHub Pages

## 🔒 Segurança

A API implementa as seguintes práticas de segurança:

- **Autenticação JWT** com tokens de acesso e refresh
- **Hashing de senhas** com algoritmos seguros
- **Validação de entrada** em todos os endpoints
- **Proteção contra SQL Injection** via Entity Framework
- **CORS configurado** para domínios específicos
- **Rate Limiting** para prevenção de abusos
- **HTTPS** obrigatório em produção

## 📡 Endpoints

> **Nota**: Documentação completa da API disponível via Swagger em `/swagger`

## 🔨 Build e Deploy

### Build para Produção
```bash
dotnet build freela-match-api/freela-match-api.sln --configuration Release
```

### Publicar
```bash
dotnet publish freela-match-api/freela-match-api.csproj -c Release -o ./publish
```

## 🏗️ Arquitetura

A API segue os princípios de **Clean Architecture** e utiliza:

- **Padrão Repository** para acesso a dados
- **Injeção de Dependência** nativa do .NET
- **Separação de responsabilidades** em camadas
- **DTOs** para contratos de API

## 📖 Documentação Adicional

Para mais informações:
- [Documentação do .NET 8](https://learn.microsoft.com/pt-br/dotnet/)
- [Entity Framework Core](https://learn.microsoft.com/pt-br/ef/core/)
- [ASP.NET Core](https://learn.microsoft.com/pt-br/aspnet/core/)

## 👥 Autor

**Anna Beatriz Loz Silva e Souza**
- GitHub: [@AnnaBLoz](https://github.com/AnnaBLoz)
- Orientador: Prof. Diogo Vinícius Winck

## 🎓 Contexto Acadêmico

Este projeto foi desenvolvido como Trabalho de Conclusão de Curso do programa de Engenharia de Software do Centro Universitário Católica de Santa Catarina, representando a camada backend da plataforma FreelaMatch.

## 📄 Licença

Projeto acadêmico - Todos os direitos reservados © 2025

---

**Desenvolvido com .NET 8**

*Centro Universitário Católica de Santa Catarina - Joinville, SC - 2025*
