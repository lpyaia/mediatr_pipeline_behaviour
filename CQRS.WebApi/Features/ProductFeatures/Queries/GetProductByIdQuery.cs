using CQRS.WebApi.Domain.Models;
using CQRS.WebApi.Infrastructure.Context;
using CQRS.WebApi.PipelineBehaviors;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CQRS.WebApi.Infrastructure.Features.ProductFeatures.Queries
{
    public class GetProductByIdQuery : IMemoryCacheRequest<Product>
    {
        public int Id { get; set; }

        public string Name => $"GetProductByIdQuery: {Id}";

        public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Product>
        {
            private readonly IApplicationContext _context;

            public GetProductByIdQueryHandler(IApplicationContext context)
            {
                _context = context;
            }

            public async Task<Product> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
            {
                var product = _context.Products.Where(a => a.Id == query.Id).FirstOrDefault();
                if (product == null) return null;
                return product;
            }
        }
    }
}