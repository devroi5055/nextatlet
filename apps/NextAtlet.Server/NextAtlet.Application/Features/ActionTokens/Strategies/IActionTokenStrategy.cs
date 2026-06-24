using NextAtlet.Application.Common.Results;
using NextAtlet.Application.Features.ActionTokens.Models;
using NextAtlet.Domain.Entities.Identity;
using NextAtlet.Domain.Enumerations.Identity;

namespace NextAtlet.Application.Features.ActionTokens.Strategies
{
    public interface IActionTokenStrategy
    {
        public ActionTokenType ActionTokenType { get; }
        public bool authRequired { get; }
        public Task<Result> ExecuteAsync(ActionToken ActionToken, User? ActorUser, CancellationToken ct);
    }
}
