# FreelaMatch API

[![.NET 8 CI](https://github.com/AnnaBLoz/freelamatch-api/actions/workflows/coverage.yml/badge.svg)](https://github.com/AnnaBLoz/freelamatch-api/actions/workflows/coverage.yml)
[![Coverage Report](https://img.shields.io/badge/coverage-view%20report-brightgreen)](https://annabloz.github.io/freelamatch-api/coverage/)

> API para gerenciamento de freelancers e projetos

## 📊 Code Coverage

- **Current Coverage:** ~55%
- [📈 View Detailed Coverage Report](https://annabloz.github.io/freelamatch-api/coverage/)

## 🚀 Tecnologias

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- xUnit + Moq + FluentAssertions

## 🧪 Testes
```bash
# Executar todos os testes
dotnet test

# Executar com cobertura
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

# Gerar relatório HTML local
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.opencover.xml" -targetdir:"coveragereport" -reporttypes:Html
```

## 🛠️ Como executar
```bash
# Restaurar dependências
dotnet restore freela-match-api/freela-match-api.sln

# Build
dotnet build freela-match-api/freela-match-api.sln --configuration Release

# Executar API
cd freela-match-api
dotnet run
```

## 📁 Estrutura do Projeto
```
FreelaMatch/
├── freela-match-api/           # Projeto principal (API)
│   ├── Controllers/           # Endpoints da API
│   ├── Services/              # Lógica de negócio
│   ├── Models/                # Modelos de dados
│   ├── Data/                  # Context do EF Core
│   └── DTOs/                  # Data Transfer Objects
├── freela-match-api-test/      # Projeto de testes
│   ├── Controllers/           # Testes dos controllers
│   └── Services/              # Testes dos services
└── .github/
    └── workflows/             # CI/CD com GitHub Actions
```

## 🔄 CI/CD

O projeto utiliza GitHub Actions para:
- ✅ Build automático em cada push/PR
- ✅ Execução de testes unitários
- ✅ Geração de relatório de cobertura
- ✅ Comentários automáticos em PRs com métricas
- ✅ Deploy do relatório no GitHub Pages

## 👥 Contribuindo

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Add: nova feature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

## 📝 Licença

Este projeto está sob a licença MIT.

---

**Desenvolvido por [Anna Loz](https://github.com/AnnaBLoz)**
