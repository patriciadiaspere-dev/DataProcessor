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
   CREATE DATABASE dataprocessor CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
   CREATE USER 'dataprocessor_user'@'%' IDENTIFIED BY 'senha_forte';
   GRANT ALL PRIVILEGES ON dataprocessor.* TO 'dataprocessor_user'@'%';
   FLUSH PRIVILEGES;
   ```
4. Atualize `appsettings.json` com a connection string: `server=127.0.0.1;port=3306;database=dataprocessor;user=dataprocessor_user;password=senha_forte;`.

### Resolvendo "Connection refused" no DBeaver

- Confirme que o serviço MySQL está realmente iniciado. No Windows, abra *Services.msc* e dê **Start** no serviço.
- Caso esteja usando Docker, verifique se o container está rodando (`docker ps`) e expondo a porta `3306`.
- Teste com `127.0.0.1` em vez de `localhost` e habilite *Allow public key retrieval* na aba *Driver properties* caso use MySQL 8.
- Certifique-se de que nenhum firewall local esteja bloqueando a porta 3306.
- Depois de ajustar, clique em **Test Connection** novamente; se continuar falhando, consulte os logs do servidor MySQL para mensagens mais detalhadas.

### Passo a passo para preparar o ambiente e rodar as migrações

Siga nesta ordem (Windows/Developer PowerShell):

1. **Confirmar o SDK .NET**
   ```powershell
   dotnet --version
   ```
   Se o comando não existir, instale o SDK 6.x em <https://dotnet.microsoft.com/download>.

2. **Restaurar/instalar o `dotnet-ef`** (usa o manifesto do repositório)
   ```powershell
   dotnet tool restore
   # se preferir, instalação global:
   # dotnet tool install --global dotnet-ef
   ```
   Depois confirme:
   ```powershell
   dotnet ef --version
   ```

3. **Conferir a connection string**
   - Edite `src/DataProcessor.Api/appsettings.json` (chave `DefaultConnection`) para apontar para seu MySQL. Ex.: `server=127.0.0.1;port=3306;database=dataprocessor;user=dataprocessor_user;password=senha_forte;`.

4. **Aplicar as migrações** a partir da raiz do repo:
   ```powershell
   dotnet ef database update -p src/DataProcessor.Data -s src/DataProcessor.Api
   ```
   (opcional) use o script `scripts/aplicar-migracoes.ps1` que automatiza os passos acima.

5. **Erros comuns e correções rápidas**
   - *"não é reconhecido como nome de cmdlet"*: o terminal não achou o `dotnet` ou `dotnet-ef`. Rode `dotnet tool restore` (ou instale global) e abra um novo terminal.
   - *Falha de conexão MySQL*: revise host, porta e credenciais na connection string; confirme que o serviço está ativo e escutando na porta 3306.
   - *Permissão negada*: use um usuário MySQL com privilégio de `CREATE/ALTER` no banco configurado.
## Exportação Excel

O endpoint `/api/report/amazon-sales/export` retorna o arquivo `.xlsx` gerado com ClosedXML contendo o resumo e os itens.

## Como aplicar migrações (MySQL)
1. Instale o SDK .NET 6 (ou superior) e o `dotnet-ef` (já incluído no manifesto `.config/dotnet-tools.json`).
2. Configure a connection string em `src/DataProcessor.Api/appsettings.json` na chave `DefaultConnection`.
3. Execute, a partir da raiz do repositório:
   ```powershell
   dotnet tool restore
   dotnet ef database update -p src/DataProcessor.Data -s src/DataProcessor.Api
   ```
   Se preferir, use o script PowerShell `scripts/aplicar-migracoes.ps1`.
4. O erro exibido na captura (``<algo> não é reconhecido como nome de cmdlet``) normalmente ocorre quando o comando é colado com texto extra ou o `dotnet-ef` não está instalado. Rode `dotnet tool restore` antes e copie apenas o comando acima.
