using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RealWorld.Infrastructure;

namespace RealWorld.Features.Profiles
{
    public class Details
    {
        public class Query : IRequest<Domain.Profile>
        {
            public string Username { get; set; }
        }

        public class QueryHandler : IAsyncRequestHandler<Query, Domain.Profile>
        {
            private readonly RealWorldContext _context;
            private readonly ICurrentUserAccessor _currentUserAccessor;

            public QueryHandler(RealWorldContext context, ICurrentUserAccessor currentUserAccessor)
            {
                _context = context;
                _currentUserAccessor = currentUserAccessor;
            }

            public async Task<Domain.Profile> Handle(Query message)
            {
                var currentUserName = _currentUserAccessor.GetCurrentUsername();

                var person = await _context.Persons.FirstOrDefaultAsync(x => x.Username == message.Username);
                var profile = Mapper.Map<Domain.Person, Domain.Profile>(person);

                if (currentUserName != null)
                {
                    var currentPerson = await _context.Persons.FirstOrDefaultAsync(x => x.Username == currentUserName);
                    if (currentPerson.Following.Any(x => x.Username == person.Username))
                    {
                        profile.Following = true;
                    }
                }
                return profile;
            }
        }
    }
}