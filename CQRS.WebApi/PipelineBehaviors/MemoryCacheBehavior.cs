using MediatR;
using Microsoft.Extensions.Caching.Memory;
using System.Threading;
using System.Threading.Tasks;

namespace CQRS.WebApi.PipelineBehaviors
{
    public interface IMemoryCacheRequest<out TResponse> : IRequest<TResponse>
    {
        string Name { get; }
    }

    public class MemoryCacheBehavior<TRequest, TResponse> :
        IPipelineBehavior<TRequest, TResponse> where TRequest : IMemoryCacheRequest<TResponse>
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheBehavior(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            TResponse response;

            if (_memoryCache.TryGetValue(request.Name, out response))
            {
                return response;
            }

            response = await next();

            _memoryCache.Set(request.Name, response);

            return response;
        }
    }
}