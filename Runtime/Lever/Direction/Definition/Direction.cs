using UnityEngine;

namespace RinaInput.Lever.Direction.Definition{
    /// <summary>
    /// 八方向での方向を示すEnum
    /// </summary>
    public enum Direction_Eight {
        Neutral,
        Front,
        FrontRight,
        Right,
        BackRight,
        Back,
        BackLeft,
        Left,
        FrontLeft,
    }

    public enum Direction_Four {
        Neutral,
        Front,
        Right,
        Back,
        Left
    }

    public static class DirectionOperator {
        
        public static Direction_Eight DirectionizeEight(this Vector2 vec, float threshold) {
            var result = vec.GetDirectionIndex(8, threshold);
            switch (result) {
                case -1 : return Direction_Eight.Neutral;
                case 0: return Direction_Eight.Front;
                case 1: return Direction_Eight.FrontRight;
                case 2: return Direction_Eight.Right;
                case 3: return Direction_Eight.BackRight;
                case 4: return Direction_Eight.Back;
                case 5: return Direction_Eight.BackLeft;
                case 6: return Direction_Eight.Left;
                case 7: return Direction_Eight.FrontLeft;
                default: return Direction_Eight.Neutral;
            }
        }

        public static Direction_Four DirectionizeFour(this Vector2 vec, float threshold) {
            var result = vec.GetDirectionIndex(4, threshold);
            switch (result) {
                case -1: return Direction_Four.Neutral;
                case 0: return Direction_Four.Front;
                case 1: return Direction_Four.Right;
                case 2: return Direction_Four.Back;
                case 3: return Direction_Four.Left;
                default: return Direction_Four.Neutral;
            }
        }

        public static int GetDirectionIndex(this Vector2 vec, int directionsCount, float threshold) {

            if (vec.sqrMagnitude < threshold * threshold) return -1;
            
            var angleParDirection = 360.0f / directionsCount;
            
            var angle = Vector2.SignedAngle(Vector2.up, vec.normalized);

            if (angle < 0.0f) angle += 360.0f;
            
            return Mathf.FloorToInt(angle / angleParDirection);
        }
    }
    
}