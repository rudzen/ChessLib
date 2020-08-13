using Rudz.Chess.MoveGeneration;

namespace Rudz.Chess.ObjectPoolPolicies
{
    using Microsoft.Extensions.ObjectPool;

    public class MoveListPolicy : IPooledObjectPolicy<IMoveList>
    {
        public IMoveList Create()
        {
            return new MoveList();
        }

        public bool Return(IMoveList obj)
        {
            obj.Clear();
            return true;
        }
    }
}