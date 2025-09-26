using System;
using System.Collections.Generic;
using R3;
using RinaInput.Lever.Direction.Definition;
using RinaInput.Lever.Signal;
using RinaInput.Runtime.Operators;
using RinaInput.Runtime.Signal;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RinaInput.Lever.Operator {
    
    public static class LeverStreamOperator {

        /// <summary>
        /// レバーが倒された際に発行されるストリーム
        /// </summary>
        /// <param name="source"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static Observable<LeverInputSignal> OnMove(this Observable<InputSignal<Vector2>> source, float threshold = 0.2f) {
            return source
                .Where(x => x.Phase == InputActionPhase.Started || x.Phase == InputActionPhase.Performed)
                .Where(x => x.Value.DirectionizeFour(threshold) != Direction_Four.Neutral)
                .Select(x => new LeverInputSignal(
                    x.Phase,
                    x.Value,
                    x.Time
                ));
        }

        public static Observable<bool> MoveAnyDirection(this Observable<LeverInputSignal> source, Direction_Four direction, float threshold = 0.2f) {
            return source
                .Select(x => x.Value.DirectionizeFour(threshold) == direction);
        }
        
        
    }
}