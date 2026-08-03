using Microsoft.EntityFrameworkCore;
using MoneyMonkey.Communication.Request;
using MoneyMonkey.Communication.Response;
using MoneyMonkey.Data.Entities;

namespace MoneyMonkey.Data.Repository;
public class CreditCardPurchaseRepository
{
    private readonly MoneyMonkeyDbContext _context;

    public CreditCardPurchaseRepository(MoneyMonkeyDbContext context)
    {
        _context = context;
    }

    public async Task<CreditCardInstallmentResponse?> CreatePurchase(long userId, int creditCardId, CreditCardPurchaseRequest request)
    {
        var creditCard = await _context.CreditCards
            .FirstOrDefaultAsync(c => c.CreditCardId == creditCardId && c.UserId == userId);

        if (creditCard is null)
        {
            return null;
        }

        if (request.CategoryId is not null)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryId == request.CategoryId && c.UserId == userId);

            if (!categoryExists)
            {
                return null;
            }
        }

        var purchaseDate = request.PurchaseDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var installmentsCount = request.InstallmentsCount ?? 1;

        var purchase = new CreditCardPurchase
        {
            UserId = userId,
            CreditCardId = creditCardId,
            Description = request.Description,
            TotalValue = request.TotalValue,
            PurchaseDate = purchaseDate,
            InstallmentsCount = installmentsCount,
            CategoryId = request.CategoryId,
            IsSubscription = request.IsSubscription,
            CreatedAt = DateTime.UtcNow
        };

        _context.CreditCardPurchases.Add(purchase);
        await _context.SaveChangesAsync();

        var installments = BuildInstallments(purchase, creditCard.ClosingDay);
        _context.CreditCardInstallments.AddRange(installments);
        await _context.SaveChangesAsync();

        var firstInstallment = installments[0];
        return new CreditCardInstallmentResponse
        {
            CreditCardInstallmentId = firstInstallment.CreditCardInstallmentId,
            Description = purchase.Description,
            CategoryId = purchase.CategoryId,
            IsSubscription = purchase.IsSubscription,
            InstallmentNumber = firstInstallment.InstallmentNumber,
            InstallmentsCount = purchase.InstallmentsCount,
            Value = firstInstallment.Value,
            PurchaseDate = purchase.PurchaseDate
        };
    }

    public async Task<CreditCardInvoiceResponse?> GetInvoice(long userId, int creditCardId, int? month, int? year)
    {
        var creditCard = await _context.CreditCards
            .FirstOrDefaultAsync(c => c.CreditCardId == creditCardId && c.UserId == userId);

        if (creditCard is null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var resolvedMonth = month ?? today.Month;
        var resolvedYear = year ?? today.Year;

        var installments = await (
            from installment in _context.CreditCardInstallments
            join purchase in _context.CreditCardPurchases on installment.CreditCardPurchaseId equals purchase.CreditCardPurchaseId
            where purchase.CreditCardId == creditCardId
                && installment.InvoiceMonth == resolvedMonth
                && installment.InvoiceYear == resolvedYear
            select new CreditCardInstallmentResponse
            {
                CreditCardInstallmentId = installment.CreditCardInstallmentId,
                Description = purchase.Description,
                CategoryId = purchase.CategoryId,
                IsSubscription = purchase.IsSubscription,
                InstallmentNumber = installment.InstallmentNumber,
                InstallmentsCount = purchase.InstallmentsCount,
                Value = installment.Value,
                PurchaseDate = purchase.PurchaseDate
            }).ToListAsync();

        return new CreditCardInvoiceResponse
        {
            CreditCardId = creditCardId,
            InvoiceMonth = resolvedMonth,
            InvoiceYear = resolvedYear,
            DueDate = new DateOnly(resolvedYear, resolvedMonth, creditCard.DueDay),
            TotalValue = installments.Sum(i => i.Value),
            Installments = installments
        };
    }

    private static List<CreditCardInstallment> BuildInstallments(CreditCardPurchase purchase, int closingDay)
    {
        var (baseMonth, baseYear) = ComputeBaseInvoice(purchase.PurchaseDate, closingDay);
        var installments = new List<CreditCardInstallment>();
        var baseInstallmentValue = Math.Round(purchase.TotalValue / purchase.InstallmentsCount, 2, MidpointRounding.ToEven);
        var allocatedValue = baseInstallmentValue * (purchase.InstallmentsCount - 1);

        for (var i = 0; i < purchase.InstallmentsCount; i++)
        {
            var (month, year) = AddMonths(baseMonth, baseYear, i);
            var isLastInstallment = i == purchase.InstallmentsCount - 1;
            var value = isLastInstallment ? purchase.TotalValue - allocatedValue : baseInstallmentValue;

            installments.Add(new CreditCardInstallment
            {
                CreditCardPurchaseId = purchase.CreditCardPurchaseId,
                InstallmentNumber = i + 1,
                Value = value,
                InvoiceMonth = month,
                InvoiceYear = year
            });
        }

        return installments;
    }

    private static (int Month, int Year) ComputeBaseInvoice(DateOnly purchaseDate, int closingDay)
    {
        return purchaseDate.Day <= closingDay
            ? (purchaseDate.Month, purchaseDate.Year)
            : AddMonths(purchaseDate.Month, purchaseDate.Year, 1);
    }

    private static (int Month, int Year) AddMonths(int month, int year, int monthsToAdd)
    {
        var total = month - 1 + monthsToAdd;
        var newYear = year + total / 12;
        var newMonth = total % 12 + 1;
        return (newMonth, newYear);
    }
}
