using System.Text.Json;
using Labora.Application.DTOs.Payments.Payme;

namespace Labora.Application.Tests.Payme;

public class PaymeRpcSerializationTests
{
    [Fact]
    public void RpcRequest_Serializes_WithConfirmedEnvelopeFieldNames()
    {
        PaymeRpcRequest<CheckTransactionRequestDto> request = new()
        {
            Method = "CheckTransaction",
            Params = new CheckTransactionRequestDto { Id = "5305e3bab097f420a44e3ecb" },
            Id = 12345
        };

        JsonDocument doc = JsonDocument.Parse(JsonSerializer.Serialize(request));
        JsonElement root = doc.RootElement;

        Assert.Equal("CheckTransaction", root.GetProperty("method").GetString());
        Assert.Equal("5305e3bab097f420a44e3ecb", root.GetProperty("params").GetProperty("id").GetString());
        Assert.Equal(12345, root.GetProperty("id").GetInt64());
    }

    [Fact]
    public void RpcResponse_Serializes_WithConfirmedResultAndErrorFieldNames()
    {
        PaymeRpcResponse<CheckPerformTransactionResponseDto> response = new()
        {
            Result = new CheckPerformTransactionResponseDto { Allow = true },
            Id = 1
        };

        JsonDocument doc = JsonDocument.Parse(JsonSerializer.Serialize(response));
        JsonElement root = doc.RootElement;

        Assert.True(root.GetProperty("result").GetProperty("allow").GetBoolean());
        Assert.False(root.TryGetProperty("error", out JsonElement errorElement) && errorElement.ValueKind != JsonValueKind.Null);
        Assert.Equal(1, root.GetProperty("id").GetInt64());
    }

    [Fact]
    public void PaymeError_Serializes_WithConfirmedFieldNames()
    {
        PaymeError error = new() { Code = PaymeErrorCodes.InvalidAmount, Data = null };

        JsonDocument doc = JsonDocument.Parse(JsonSerializer.Serialize(error));
        JsonElement root = doc.RootElement;

        Assert.Equal(-31001, root.GetProperty("code").GetInt32());
    }

    [Fact]
    public void PaymeAccountDto_Serializes_AsOrderIdSnakeCase()
    {
        PaymeAccountDto account = new() { OrderId = "abc-123" };

        string json = JsonSerializer.Serialize(account);

        Assert.Contains("\"order_id\":\"abc-123\"", json);
        Assert.DoesNotContain("OrderId", json);
    }

    [Fact]
    public void CreateTransactionResponseDto_Serializes_WithConfirmedSnakeCaseFieldNames()
    {
        CreateTransactionResponseDto dto = new()
        {
            CreateTime = 1_700_000_000_000,
            Transaction = "order-guid-value",
            State = 1
        };

        string json = JsonSerializer.Serialize(dto);

        Assert.Contains("\"create_time\":1700000000000", json);
        Assert.Contains("\"transaction\":\"order-guid-value\"", json);
        Assert.Contains("\"state\":1", json);
    }

    [Fact]
    public void CheckTransactionResponseDto_Serializes_WithConfirmedSnakeCaseFieldNames()
    {
        CheckTransactionResponseDto dto = new()
        {
            CreateTime = 1,
            PerformTime = 2,
            CancelTime = 0,
            Transaction = "order-guid-value",
            State = 2,
            Reason = null
        };

        string json = JsonSerializer.Serialize(dto);

        Assert.Contains("\"create_time\":1", json);
        Assert.Contains("\"perform_time\":2", json);
        Assert.Contains("\"cancel_time\":0", json);
        Assert.Contains("\"reason\":null", json);
    }

    [Fact]
    public void CreateTransactionRequestDto_RoundTrips_PreservingConfirmedFieldNames()
    {
        const string json = """
        {
          "id": "5305e3bab097f420a44e3ecb",
          "time": 1399114284039,
          "amount": 500000,
          "account": { "order_id": "197" }
        }
        """;

        CreateTransactionRequestDto? dto = JsonSerializer.Deserialize<CreateTransactionRequestDto>(json);

        Assert.NotNull(dto);
        Assert.Equal("5305e3bab097f420a44e3ecb", dto!.Id);
        Assert.Equal(1399114284039, dto.Time);
        Assert.Equal(500000, dto.Amount);
        Assert.Equal("197", dto.Account.OrderId);
    }

    [Fact]
    public void PaymeErrorCodes_AccountErrorRange_ClassifiesCorrectly()
    {
        Assert.True(PaymeErrorCodes.IsAccountError(-31050));
        Assert.True(PaymeErrorCodes.IsAccountError(-31075));
        Assert.True(PaymeErrorCodes.IsAccountError(-31099));
        Assert.False(PaymeErrorCodes.IsAccountError(-31049));
        Assert.False(PaymeErrorCodes.IsAccountError(-31100));
        Assert.False(PaymeErrorCodes.IsAccountError(PaymeErrorCodes.InvalidAmount));
    }

    [Fact]
    public void RpcRequest_OfficialDocumentationSampleId_RoundTrips()
    {
        // developer.help.paycom.uz/protokol-merchant-api/format-zaprosa example request uses "id": 2032
        // as the top-level JSON-RPC request id (distinct from the transaction id inside "params").
        PaymeRpcRequest<CheckTransactionRequestDto> request = new()
        {
            Method = "CheckTransaction",
            Params = new CheckTransactionRequestDto { Id = "5305e3bab097f420a44e3ecb" },
            Id = 2032
        };

        string json = JsonSerializer.Serialize(request);
        JsonElement root = JsonDocument.Parse(json).RootElement;
        Assert.Equal(2032, root.GetProperty("id").GetInt64());

        PaymeRpcRequest<CheckTransactionRequestDto>? roundTripped =
            JsonSerializer.Deserialize<PaymeRpcRequest<CheckTransactionRequestDto>>(json);
        Assert.Equal(2032, roundTripped!.Id);
    }

    [Fact]
    public void GetStatementResponseDto_OfficialEmptyExample_Deserializes()
    {
        // Verbatim (reformatted for readability only) from the GetStatement documentation page's
        // "Пример ответа" example response for an empty period.
        const string json = """
        {
          "result": {
            "transactions": []
          }
        }
        """;

        PaymeRpcResponse<GetStatementResponseDto>? response =
            JsonSerializer.Deserialize<PaymeRpcResponse<GetStatementResponseDto>>(json);

        Assert.NotNull(response);
        Assert.NotNull(response!.Result);
        Assert.Empty(response.Result!.Transactions);
    }

    [Fact]
    public void GetStatementResponseDto_OfficialNonEmptyExample_Deserializes()
    {
        // Verbatim (reformatted for readability only) from the GetStatement documentation page's
        // "Пример ответа" example response, single-item form (the page's own example uses "……" to
        // indicate further items follow, which is not valid JSON on its own).
        const string json = """
        {
          "result": {
            "transactions": [
              {
                "id": "5305e3bab097f420a62ced0b",
                "time": 1399114284039,
                "amount": 500000,
                "account": { "phone": "903595731" },
                "create_time": 1399114284039,
                "perform_time": 1399114285002,
                "cancel_time": 0,
                "transaction": "5123",
                "state": 2,
                "reason": null,
                "receivers": [
                  { "id": "5305e3bab097f420a62ced0b", "amount": 200000 },
                  { "id": "4215e6bab097f420a62ced01", "amount": 300000 }
                ]
              }
            ]
          }
        }
        """;

        PaymeRpcResponse<GetStatementResponseDto>? response =
            JsonSerializer.Deserialize<PaymeRpcResponse<GetStatementResponseDto>>(json);

        Assert.NotNull(response?.Result);
        PaymeStatementTransactionDto line = Assert.Single(response!.Result!.Transactions);

        Assert.Equal("5305e3bab097f420a62ced0b", line.Id);
        Assert.Equal(1399114284039, line.Time);
        Assert.Equal(500000, line.Amount);
        Assert.Equal(1399114284039, line.CreateTime);
        Assert.Equal(1399114285002, line.PerformTime);
        Assert.Equal(0, line.CancelTime);
        Assert.Equal("5123", line.Transaction);
        Assert.Equal(2, line.State);
        Assert.Null(line.Reason);

        Assert.NotNull(line.Receivers);
        Assert.Equal(2, line.Receivers!.Count);
        Assert.Equal("5305e3bab097f420a62ced0b", line.Receivers[0].Id);
        Assert.Equal(200000, line.Receivers[0].Amount);
        Assert.Equal("4215e6bab097f420a62ced01", line.Receivers[1].Id);
        Assert.Equal(300000, line.Receivers[1].Amount);

        // This example's own "account" object uses "phone" (that merchant's own field choice) rather
        // than ALP Jobs' "order_id" - proves unknown/differently-shaped account fields deserialize
        // safely (default OrderId) instead of throwing, since the account object's field name is
        // merchant-defined, not Payme-mandated.
        Assert.Equal(string.Empty, line.Account.OrderId);
    }

    [Fact]
    public void GetStatementResponseDto_Serializes_AsTransactionsPluralArrayKey()
    {
        // GetStatementResponseDto itself has no "transaction" (singular) property - that field only
        // exists per-item, inside PaymeStatementTransactionDto (the merchant's own transaction id).
        // Serializing with an empty list isolates the wrapper's own key from any per-item field.
        GetStatementResponseDto dto = new() { Transactions = [] };

        string json = JsonSerializer.Serialize(dto);

        Assert.Equal("""{"transactions":[]}""", json);
    }
}
