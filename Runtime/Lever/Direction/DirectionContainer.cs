
using RinaInput.Lever.Direction.Definition;
using UnityEngine;

namespace RinaInput.Lever.Direction {
    public struct DirectionContainer {

        private Direction_Four m_four;

        private Direction_Eight m_eight;

        public DirectionContainer(Vector2 vec) {
            m_four = vec.DirectionizeFour(0.3f);
            m_eight = vec.DirectionizeEight(0.3f);
        }
        
    }
}