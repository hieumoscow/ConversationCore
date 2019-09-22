using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BeautifulRestApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BeautifulRestApi.Services
{
    public sealed class DefaultConversationService : IConversationService
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public DefaultConversationService(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ConversationResource> GetConversationAsync(Guid id, CancellationToken ct)
        {
            var entity = await _context
                .Conversations
                .SingleOrDefaultAsync(x => x.Id == id, ct);
            
            return _mapper.Map<ConversationResource>(entity);
        }

        public async Task<Page<ConversationResource>> GetConversationsAsync(
            PagingOptions pagingOptions,
            CancellationToken ct)
        {
            IQueryable<ConversationEntity> query = _context.Conversations;
            // todo apply search
            // todo apply sort

            var size = await query.CountAsync(ct);

            var items = await query
                .Skip(pagingOptions.Offset.Value)
                .Take(pagingOptions.Limit.Value)
                .ProjectTo<ConversationResource>(_mapper.ConfigurationProvider)
                .ToArrayAsync(ct);

            return new Page<ConversationResource>
            {
                Items = items,
                TotalSize = size
            };
        }
    }
}
