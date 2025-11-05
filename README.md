# DataProcessor

API para processamento de relatórios Amazon Sales com autenticação JWT e suporte a exportação em Excel.

## Como funciona

1. Usuário realiza cadastro com CNPJ, e-mail, empresa e senha.
2. Login retorna um token JWT válido e o status atual (trial, expirado ou assinado).
3. Enquanto o trial de 3 dias estiver ativo, o usuário pode enviar o arquivo `.txt` da Amazon.
4. O backend processa o arquivo, agrupa os dados por ASIN e preço, e devolve um resumo em JSON.
5. Opcionalmente, o usuário pode solicitar o mesmo processamento em formato Excel.

## Endpoints principais

- `POST /api/auth/register`
- `POST /api/auth/login`
- `GET /api/user/me`
- `GET /api/user/status`
- `POST /api/report/amazon-sales`
- `POST /api/report/amazon-sales/export`

Todos os endpoints (exceto registro/login) exigem o token JWT no header `Authorization: Bearer {token}`.

## Configuração

A conexão MySQL e os parâmetros do JWT ficam em `appsettings.json`. Ajuste a connection string e a chave secreta antes de executar.

### Criando o banco e usuário MySQL

1. Inicie o servidor MySQL local (no Windows, procure por **MySQL80** em *Serviços* e deixe como *Em execução*).
2. No DBeaver crie uma nova conexão MySQL com `localhost` ou `127.0.0.1` na porta `3306` usando um usuário com permissão de administração.
3. Abra uma aba SQL e execute:
   ```sql
   CREATE DATABASE dataprocessor
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE USER 'nome_user'@'localhost' IDENTIFIED BY 'senha_forte';

GRANT SELECT, INSERT, UPDATE, DELETE,
      ALTER, CREATE, DROP, INDEX,
      CREATE VIEW, SHOW VIEW,
      CREATE ROUTINE, ALTER ROUTINE, EXECUTE,
      TRIGGER, CREATE TEMPORARY TABLES, LOCK TABLES
ON dataprocessor.* TO 'nome_user'@'localhost';

FLUSH PRIVILEGES;
   ```
4. Atualize `appsettings.json` com a connection string: `server=127.0.0.1;port=3306;database=dataprocessor;user=dataprocessor_user;password=senha_forte;`.

### Resolvendo "Connection refused" no DBeaver

- Confirme que o serviço MySQL está realmente iniciado. No Windows, abra *Services.msc* e dê **Start** no serviço.
- Caso esteja usando Docker, verifique se o container está rodando (`docker ps`) e expondo a porta `3306`.
- Teste com `127.0.0.1` em vez de `localhost` e habilite *Allow public key retrieval* na aba *Driver properties* caso use MySQL 8.
- Certifique-se de que nenhum firewall local esteja bloqueando a porta 3306.
- Depois de ajustar, clique em **Test Connection** novamente; se continuar falhando, consulte os logs do servidor MySQL para mensagens mais detalhadas.

## Exportação Excel

O endpoint `/api/report/amazon-sales/export` retorna o arquivo `.xlsx` gerado com ClosedXML contendo o resumo e os itens.
