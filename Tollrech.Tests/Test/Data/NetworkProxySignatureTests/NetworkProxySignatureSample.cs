#nullable enable
using System;
using System.Threading.Tasks;
using BusinessTaskApiClient.Models.DistributedSale;
using SKBKontur.Billy.Billing.BusinessObjects.Tariffication;
using SKBKontur.Billy.Billing.PriceService.Client.BusinessObjects.Identification;
using SKBKontur.Billy.Billing.PriceService.Client.Orders;
using SKBKontur.Billy.Billing.PurchaseService.Client.BusinessObjects;
using SKBKontur.Billy.Billing.PurchaseService.Client.BusinessObjects.Baskets;
using SKBKontur.Billy.Billing.PurchaseService.Client.BusinessObjects.GetAvailableProducts;
using SKBKontur.Billy.Billing.PurchaseService.Client.BusinessObjects.Orders;
using SKBKontur.Billy.Billing.PurchaseService.Client.BusinessObjects.Orders.Requests;
using SKBKontur.Billy.Core.Common.ProcessResults;
using SKBKontur.Billy.Core.Common.ProcessResults.NewResults;
using SKBKontur.Billy.Core.Networking.ServiceRPC.Attributes;

namespace SKBKontur.Billy.Billing.PurchaseService.Client;

[NetworkService("Billy.Billing.PurchaseService")]
public interface IBasketService
{
    #region CustomTariffs

    Task<Result<Guid[], OrderServiceFail>> AddCustomTariffsAsync{caret}(Guid basketId, CustomTariffDto[] customTariffs, SaleChannelDto saleChannel);

    #endregion
}

public record SaleChannelDto(
    Platform Platform,
    UserDto? User,
    string Id
);

public record UserDto(Guid PortalUserId);

public class CustomTariffDto
{
    public required Guid PackageId { get; init; }
    public required string TariffId { get; init; }
    public required GoodsItemDto[] Goods { get; init; }
    public required CustomTariffDto[] Modifiers { get; init; }
    public required OrderType OrderType { get; init; }
    public required Guid? ChangedPackageId { get; init; }

    public IEnumerable<CustomTariffDto> Flatten()
    {
        yield return this;
        foreach (var modifier in Modifiers.SelectMany(m => m.Flatten()))
        {
            yield return modifier;
        }
    }
}

public class GoodsItemDto
{
    public Guid Id { get; set; }
}

public enum Platform
{
    [PlatformName(Value = "Неизвестная")]
    Unknown = 0,

    [PlatformName(Value = "АРМ Партнера")]
    PartnersKontur = 1,
}
