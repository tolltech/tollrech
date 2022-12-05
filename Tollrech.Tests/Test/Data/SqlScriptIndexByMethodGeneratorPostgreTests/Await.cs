using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using SKBKontur.Billy.Core.Common.Quering;
using SKBKontur.Billy.Core.Common.Quering.Attributes;

namespace Tollrech.Tests.Test.Data.SqlScriptGeneratorTests
{
    public class EntityHandlerBase<T>
    {
        protected IQueryable<T> GetTable()
        {
            throw new Exception();
        }
    }

    [Table("OnlinePaymentSessionDbos")]
    public class OnlinePaymentSessionDbo
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string SessionId { get; set; }
        public DateTime Date { get; set; }
        public PaymentState State { get; set; }
        public OnlinePaymentAgent PaymentAgent { get; set; }
        public string Url { get; set; }
        public Guid VendorId { get; set; }
    }

    public enum PaymentState
    {
        Some
    }

    public class MyHadnler : EntityHandlerBase<OnlinePaymentSessionDbo>
    {
        public Task<OnlinePaymentSessionDbo[]> SelectAsync(DateTime exclusiveFromDate, PaymentState[] states, DateTime? exclusiveToDate = null)
        {
            var query = GetTable()
                .Where(x => states.Contains(x.State))
                .Where(x => x.Date > exclusiveFromDate);

            if (exclusiveToDate.HasValue)
            {
                query = query.Where(x => x.Date < exclusiveToDate.Value);
            }

            return query.OrderBy(x => x.Date).ToArrayAsync();
        }

        public Task<OnlinePaymentSessionDbo[]> SelectAsync(DateTime exclusiveFromDate, PaymentState state, OnlinePaymentAgent[] agents)
        {
            return GetTable()
                .Where(x => x.State == state)
                .Where(x => x.Date > exclusiveFromDate)
                .Where(x => agents.Contains(x.PaymentAgent));
        }

        public async Task<(PaymentState PaymentState, int Count)[]> CountByPayment{caret}StateAsync(OnlinePaymentAgent paymentAgent, DateTime exclusiveFromDate)
        {
            return (await GetTable()
                    .Where(x => x.Date > exclusiveFromDate)
                    .Where(x => x.PaymentAgent == paymentAgent)
                    .GroupBy(x => x.State)
                    .Select(x => new { PaymentState = x.Key, Count = x.Count() })
                    .ToArrayAsync().ConfigureAwait(false))
                .Select(x => (x.PaymentState, x.Count))
                .ToArray();
        }
    }
}
