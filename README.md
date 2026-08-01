# MoneyMonkey

API para controle financeiro pessoal (receitas, despesas e categorias), construída em ASP.NET Core 8 com PostgreSQL, autenticação JWT e arquitetura em camadas.

## Estrutura da solução

`MoneyMonkey.sln`, na raiz deste diretório, referencia cinco projetos:

| Projeto | Responsabilidade |
|---|---|
| `MoneyMonkey.API` | Host Web API (Controllers, `Program.cs`, configuração, JWT/Swagger) |
| `MoneyMonkey.Application` | Serviços (regras de negócio), geração de token JWT |
| `MoneyMonkey.Communication` | DTOs (`Request/`, `Response/`) e `Enums/` compartilhados — sem dependências de outros projetos |
| `MoneyMonkey.Data` | `DbContext` do EF Core, entidades, repositórios e Migrations |
| `MoneyMonkey.Tests` | Testes (xUnit) de repositórios e serviços |

Direção de dependência: `API -> Application -> Data -> Communication` (API e Data também referenciam Communication diretamente).

Fluxo padrão por funcionalidade: **Controller → Service → Repository → `MoneyMonkeyDbContext`**, trafegando DTOs (não entidades) entre as camadas.

## Pré-requisitos

- .NET 8 SDK
- PostgreSQL rodando e acessível (o schema — tabelas `users`, `credentials`, `categories`, `transactions` — é criado automaticamente pelas EF Core Migrations na primeira vez que a API sobe; não precisa criar nada manualmente no banco)

## Configuração

A connection string e o segredo do JWT ficam em `MoneyMonkey.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=moneymonkeydb;Username=postgres;Password=1234"
  },
  "Jwt": {
    "Secret": "...",
    "Issuer": "MoneyMonkey.API",
    "Audience": "MoneyMonkey.Client",
    "ExpirationMinutes": 60
  }
}
```

> Os valores versionados são placeholders de desenvolvimento local, não segredos reais.

## Executando

A partir deste diretório (raiz da solução):

```bash
dotnet tool restore                  # instala o dotnet-ef local (só na primeira vez)
dotnet restore MoneyMonkey.sln
dotnet build MoneyMonkey.sln
dotnet run --project MoneyMonkey.API
```

Ao subir, a API aplica automaticamente qualquer migration pendente no banco configurado (`Database.Migrate()` no `Program.cs`) — não é preciso rodar nenhum comando de migration manualmente para o dia a dia.

O Swagger UI abre automaticamente conforme o perfil de execução (`MoneyMonkey.API/Properties/launchSettings.json`):

- `http`: http://localhost:5217/swagger
- `https`: https://localhost:7002/swagger (e http://localhost:5217)

## Migrations (EF Core)

O schema é versionado com EF Core Migrations, uma tabela por arquivo (`MoneyMonkey.Data/Migrations/`). Para criar uma nova migration após alterar `MoneyMonkeyDbContext` ou as entidades:

```bash
dotnet dotnet-ef migrations add NomeDaMudanca --project MoneyMonkey.Data --startup-project MoneyMonkey.API
```

Isso só gera o arquivo C# da migration (não altera o banco). Para aplicar manualmente sem subir a API:

```bash
dotnet dotnet-ef database update --project MoneyMonkey.Data --startup-project MoneyMonkey.API
```

As migrations já aplicadas ficam registradas na tabela `Evolution` (nome customizado do histórico de migrations do EF, em vez do padrão `__EFMigrationsHistory`).

## Autenticação

Login em `POST /api/auth/login` retorna um JWT (claims: `sub` = UserId, `ClaimTypes.Name` = nome completo, `ClaimTypes.Role` = tipo de usuário). Envie o token em `Authorization: Bearer <token>` nas demais rotas protegidas.

Senhas são hasheadas com `IPasswordHasher<User>` do ASP.NET Identity e armazenadas separadamente na tabela `credentials`.

## Endpoints

| Método | Rota | Autenticado | Descrição |
|---|---|---|---|
| POST | `/api/auth/login` | Não | Autentica e retorna um JWT |
| GET | `/api/user` | Sim | Lista usuários |
| POST | `/api/user` | Não | Cria usuário + credencial |
| GET | `/api/category` | Sim | Lista categorias do usuário autenticado |
| POST | `/api/category` | Sim | Cria categoria para o usuário autenticado |
| GET | `/api/transaction` | Sim | Lista transações do usuário autenticado |
| POST | `/api/transaction` | Sim | Cria transação (valida que a categoria pertence ao usuário) |

Todas as consultas de categorias/transações são isoladas por `userId` (multi-tenancy por convenção, escopo aplicado no repositório).

## Modelo de dados

- **User**: `UserId`, `FirstName`, `LastName`, `Type` (`UserType`: Pf, Pj, Staff, Admin)
- **Credential**: `CredentialId`, `UserId`, `Username`, `Password` (hash)
- **Category**: `CategoryId`, `UserId`, `Name`, `Type` (`TransactionType`: Entrada, Saida), `CreatedAt`
- **Transaction**: `TransactionId`, `UserId`, `TransactionName`, `Value`, `Type` (`TransactionType`), `PaymentMethod?` (Pix, Dinheiro, CartaoCredito, CartaoDebito, Boleto, Transferencia, Outro), `CategoryId?`, `TransactionDate`, `CreatedAt`, `UpdatedAt`

Os enums `UserType`, `TransactionType` e `PaymentMethod` são armazenados como `text` no banco (conversão feita pelo EF Core via `HasConversion<string>()`), não como tipos enum nativos do Postgres.
