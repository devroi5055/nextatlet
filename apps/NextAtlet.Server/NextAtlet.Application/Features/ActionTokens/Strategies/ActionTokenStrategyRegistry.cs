using NextAtlet.Application.Common.Results;
using NextAtlet.Domain.Enumerations.Identity;

namespace NextAtlet.Application.Features.ActionTokens.Strategies
{
    public class ActionTokenStrategyRegistry
    {
        private readonly Dictionary<ActionTokenType, IActionTokenStrategy> _map;

        public ActionTokenStrategyRegistry(IEnumerable<IActionTokenStrategy> strategies)
        {
            _map = strategies.ToDictionary(x => x.ActionTokenType);
        }

        public IActionTokenStrategy Get(ActionTokenType type)
        {
            return _map[type];
        }
    }
}
