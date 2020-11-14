using System.Threading;
using System.Threading.Tasks;
using Conduit.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conduit.Features.Comments
{
    [Route("articles")]
    public class CommentsController : Controller
    {
        private readonly IMediator _mediator;

        public CommentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("{slug}/comments")]
        [Authorize(AuthenticationSchemes = JwtIssuerOptions.Schemes)]
        public async Task<CommentEnvelope> Create(string slug, [FromBody]Create.Command command, CancellationToken cancellationToken)
        {
            command.Slug = slug;
            return await _mediator.Send(command, cancellationToken);
        }

        [HttpGet("{slug}/comments")]
        public async Task<CommentsEnvelope> Get(string slug, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new List.Query(slug), cancellationToken);
        }

        [HttpDelete("{slug}/comments/{id}")]
        [Authorize(AuthenticationSchemes = JwtIssuerOptions.Schemes)]
        public async Task Delete(string slug, int id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new Delete.Command(slug, id), cancellationToken);
        }
    }
}