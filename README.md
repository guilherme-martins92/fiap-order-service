# 🛒 Microserviço de Pedidos de Veículos

Este projeto é um microserviço de **Pedidos de Veículos**, desenvolvido como uma **AWS Lambda** utilizando **.NET 8**. Ele permite **criar pedidos de compra**, **listar todas as compras realizadas** e **atualizar o status de pagamento**.

## 🛠️ Tecnologias Utilizadas

- [.NET 8](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [AWS Lambda](https://aws.amazon.com/lambda/)
- [Amazon SQS](https://aws.amazon.com/sqs/)
- [Amazon DynamoDB](https://aws.amazon.com/dynamodb/)
- [Amazon API Gateway](https://aws.amazon.com/api-gateway/) *(opcional, para expor como API REST)*

## 📦 Funcionalidades

- 🆕 Criar novo pedido de compra
- 📋 Listar todos os pedidos
- 🔄 Atualizar status de pagamento

## 🔄 Fluxo da Operação

1. Cliente envia um pedido com seus dados, ID do veículo e quantidade.
2. O microserviço busca os dados do veículo no **microserviço de Catálogo**.
3. Com as informações completas, o pedido é salvo e enviado para uma **fila SQS**.
4. Um **consumer Lambda** lê da fila e envia os dados para uma **API de pagamento (simulada)**.
5. A resposta da API de pagamento é usada para **atualizar o status do pedido** no banco DynamoDB.

## 🚀 Deploy

O deploy pode ser feito via AWS CLI, AWS Console ou com ferramentas como [AWS SAM](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/what-is-sam.html) ou [AWS CDK](https://docs.aws.amazon.com/cdk/latest/guide/home.html).

## 📄 Exemplo de Payload

```json
{
  "customerDocument": "48056564504",
  "customerName": "Usuário de teste",
  "customerEmail": "teste@teste.com.br",
  "itens": [
    {
      "vehicleExternalId": "62ef0bb0-a4ce-4039-ae0a-6f483fcc3366",
      "amount": 1
    }
  ]
}
```

## 📌 Observações

- O **DynamoDB** armazena os pedidos e seu status.
- A **SQS** é usada para orquestrar o envio do pedido para a API de pagamento.
- O **consumer da fila** é uma segunda Lambda, também desenvolvida em **.NET 8**.
- A API de pagamento é simulada e responde com sucesso ou falha, permitindo testes realistas.
