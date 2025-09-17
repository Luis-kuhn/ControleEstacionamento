# Projeto de Entrevista  

Este repositório contém um projeto desenvolvido em **.NET 8** para fins de avaliação técnica. O projeto expõe uma API com documentação disponível via **Swagger**.  

## 🚀 Requisitos  

- [.NET SDK 8.0.414](https://dotnet.microsoft.com/pt-br/download/dotnet/thank-you/sdk-8.0.414-macos-arm64-installer)  

## ⚙️ Instalação e execução  

Restaurar dependências
```bash
dotnet restore
```
Rodar o projeto
```bash
dotnet run
```
A API estará disponível em:
👉 http://localhost:5089/swagger/index.html

🗄️ Banco de Dados (SQLite)

O projeto utiliza SQLite, portanto não é necessário instalar um servidor de banco de dados. O arquivo de banco será criado automaticamente na primeira execução das migrations.

Gerenciar migrations

Instale/atualize a ferramenta Entity Framework CLI (caso ainda não tenha):
```bash
dotnet tool update --global dotnet-ef
```

Dentro da pasta do projeto, crie a primeira migration (se necessário):
```bash
dotnet ef migrations add InitialCreate
```

Atualize o banco de dados:
```bash
dotnet ef database update
```

O arquivo .db (SQLite) será gerado no diretório do projeto (conforme configuração no appsettings.json).
