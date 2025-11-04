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

## Exportação Excel

O endpoint `/api/report/amazon-sales/export` retorna o arquivo `.xlsx` gerado com ClosedXML contendo o resumo e os itens.
