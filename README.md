# API de Carteira Digital

Uma API RESTful para gerenciamento de carteiras, depósitos, transferências e transações, construída com ASP.NET Core, Entity Framework Core e PostgreSQL. A API suporta autenticação de usuários, operações com carteiras e histórico de transações com timestamps em UTC. Testes unitários estão incluídos para garantir confiabilidade.

## Índice
- Funcionalidades
- Tecnologias
- Estrutura do Projeto
- Instruções de Configuração
  - Pré-requisitos
  - Rodando o PostgreSQL com Docker
  - Configurando a Aplicação
  - Executando a API
- Endpoints da API
  - Endpoints de Autenticação
  - Endpoints de Usuário
  - Endpoints de Carteira
- Testes Unitários

## Funcionalidades
- **Gerenciamento de Usuários**: Criação de usuários com carteiras associadas.
- **Operações com Carteiras**: Consulta de saldo, depósitos e transferências entre carteiras.
- **Histórico de Transações**: Recuperação do histórico de transações com filtro por intervalo de datas.
- **Autenticação**: Autenticação baseada em JWT para acesso seguro.
- **Timestamps em UTC**: Todos os timestamps de transações são armazenados e retornados em UTC (formato ISO 8601 com sufixo `Z`).
- **Testes Unitários**: Testes abrangentes para controladores usando xUnit e Moq.
- **Banco de Dados**: PostgreSQL com Entity Framework Core para persistência de dados.
- **Docker**: PostgreSQL executado em um contêiner Docker para configuração fácil.

## Tecnologias
- **ASP.NET Core 8.0**: Framework para API web.
- **Entity Framework Core**: ORM para operações com banco de dados.
- **PostgreSQL**: Banco de dados relacional.
- **Npgsql**: Provedor PostgreSQL para EF Core.
- **xUnit & Moq**: Testes unitários e mocking.
- **Docker**: Contêinerização para PostgreSQL.
- **ShortId**: Geração de códigos únicos para carteiras.
- **JWT**: Autenticação.


## Instruções de Configuração

### Pré-requisitos
- **.NET SDK 8.0**: [Instalar](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker**: [Instalar](https://www.docker.com/get-started)
- **Cliente PostgreSQL** (opcional): Para acesso manual ao banco (ex.: pgAdmin, DBeaver).
- **Git**: Para clonar o repositório.

### Rodando o PostgreSQL com Docker
1. **Configure o arquivo `docker-compose.yml`** na raiz do projeto:
    ```yaml
      version: '3.8'
      
      services:
        postgres:
          image: postgres:latest
          environment:
            POSTGRES_USER: seusuario
            POSTGRES_PASSWORD: suasenha
            POSTGRES_DB: walletdb
          ports:
            - "5432:5432"
          volumes:
            - postgres_data:/var/lib/postgresql/data
          networks:
            - app-network
      
      volumes:
        postgres_data:
      
      networks:
        app-network:
          driver: bridge
    ```
2. **Inicie o PostgreSQL**:
    ```bash
    docker-compose up -d
    ```
   - Isso inicia um contêiner PostgreSQL com:
     - Usuário: `seusuario`
     - Senha: `suasenha`
     - Banco de dados: `walletdb`
     - Porta: `5432`
   - A flag `-d` executa o contêiner em modo detached.

### Configurando a Aplicação
1. **Clone o repositório**:
    ```bash
    git clone https://github.com/ViniciusMarins/wallet-api.git
    cd wallet-api
    ```
2. **Atualize o `appsettings.json`**:
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Port=5432;Database=seubanco;Username=seuusuario;Password=suasenha"
      },
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AllowedHosts": "*",
      "Jwt": {
        "SecretKey":"",
        "Issuer": "",
        "Audience": "",
        "ExpirationInMinutes": ""
      }
    ```
   - Atualize o `DefaultConnection` para corresponder às configurações do PostgreSQL.
   - Defina uma `Jwt:Key` segura para autenticação.

### Executando a API
1. **Aplique as migrações do banco de dados**:
    ```bash
    Update-Database
    ```
2. **Execute a aplicação**:
    ```bash
    dotnet run --project api
    ```
   - A API estará disponível em `"http://localhost:5214"`

## Endpoints da API

### Endpoints de Autenticação
- **POST /api/v1/auth**
  - **Descrição**: Realiza o login de um usuário previamente cadastrado.
  - **Corpo da Requisição**:
    ```json
      {
          "cpf": "1111111111",
          "password": "test123",
      }
    ```
  - **Resposta** (200 OK):
    ```json
    {
      "token": "eyAMSDAO_@MAS...",
    }
    ```
  - **Erro** (401 Unauthorized):
    ```json
    {
      "message": "Cpf or password invalid."
    }
    ```
    
### Endpoints de Usuário
- **POST /api/v1/users**
  - **Descrição**: Cria um novo usuário com uma carteira associada.
  - **Corpo da Requisição**:
    ```json
    {
      "name": "Vinicius",
      "cpf": "1111111111",
      "email": "v@gmail.com",
      "password": "test123"
    }
    ```
  - **Resposta** (201 Created):
    ```json
    {
      "message": "User created successfully.",
      "name": "Vinicius",
      "cpf": "1111111111",
      "walletCode": "ASDW2132_XA"
    }
    ```
    
### Endpoints de Carteira
- **GET /api/v1/wallets/{code}/balance**
  - **Descrição**: Consulta o saldo de uma carteira.
  - **Autorização**: Token Bearer necessário.
  - **Resposta** (200 OK):
    ```json
    {
      "balance": 100.00
    }
    ```
  - **Erro** (404 Not Found):
    ```json
    {
      "message": "Wallet not found or does not belong to the user."
    }
    ```

- **POST /api/v1/wallets/{code}/deposit**
  - **Descrição**: Deposita fundos em uma carteira.
  - **Autorização**: Token Bearer necessário.
  - **Corpo da Requisição**:
    ```json
    {
      "amount": 50.00
    }
    ```
  - **Resposta** (200 OK):
    ```json
    {
      "message": "Deposit completed successfully.",
      "balance": 150.00
    }
    ```
  - **Erro** (404 Not Found):
    ```json
    {
      "message": "Wallet not found or does not belong to the user."
    }
    ```

- **POST /api/v1/wallets/transfer**
  - **Descrição**: Transfere fundos entre carteiras.
  - **Autorização**: Token Bearer necessário.
  - **Corpo da Requisição**:
    ```json
    {
      "amount": 50.00,
      "fromWalletCode": "FROM_WALLET",
      "toWalletCode": "TO_WALLET"
    }
    ```
  - **Resposta** (200 OK):
    ```json
    {
      "message": "Transfer completed successfully."
    }
    ```
  - **Erros**:
    - 404 Not Found:
      ```json
      {
        "message": "Source wallet not found or does not belong to the user."
      }
      ```
      ```json
      {
        "message": "Destination wallet not found."
      }
      ```

- **GET /api/v1/wallets/{code}/transactions**
  - **Descrição**: Recupera o histórico de transações de uma carteira.
  - **Autorização**: Token Bearer necessário.
  - **Parâmetros de Consulta**:
    - `startDate`: Opcional, ISO 8601 (ex.: `2025-05-11T00:00:00Z`).
    - `endDate`: Opcional, ISO 8601 (ex.: `2025-05-12T23:59:59Z`).
  - **Resposta** (200 OK):
    ```json
    [
      {
        "createdAt": "2025-05-11T04:00:00Z",
        "amount": 100.00,
        "transactionType": "DEPOSIT",
        "status": "COMPLETED",
        "fromWalletCode": "WALLET_CODE",
        "toWalletCode": null
      }
    ]
    ```
  - **Erro** (404 Not Found):
    ```json
    {
      "message": "Wallet not found or does not belong to the user."
    }
    ```
### Testes Unitários

Testes unitários foram implementados para os controllers e services usando xUnit e Moq. Os testes cobrem:
  - Operações bem-sucedidas (ex.: consulta de saldo, depósitos, transferências, histórico de transações).
  - Casos de erro (ex.: carteira não encontrada, acesso não autorizado).
  - Códigos de status (200 OK, 201 Created, 404 Not Found).
  - Tratamento de tipos anônimos para validação de respostas.
  - Confiabilidade dos dados e retorno.

## Executando os Testes
1. **Navegue até o projeto de testes:**:
    ```bash
    cd api.Tests
    ```
2. **Execute os testes:**:
    ```bash
    dotnet test
    ```
