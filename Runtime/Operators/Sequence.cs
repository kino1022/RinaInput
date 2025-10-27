using System;
using R3;
using RinaInput.Lever.Direction.Definition;
using RinaInput.Signal;
using UnityEngine;

namespace RinaInput.Operators {
    public static partial class InputStreamOperator {
        
        public static Observable<Unit> Sequence(this Observable<InputSignal<Vector2>> stream, TimeSpan interval,
            float threshold, params Direction_Four[] directions) 
        {

            if (directions.Length is 0 ) {
                return Observable.Empty<Unit>();
            }

            var directionsStream = stream
                .OnMove(threshold)
                .Select(x => x.Value.DirectionizeFour(threshold))
                .DistinctUntilChanged()
                .Where(d => d != Direction_Four.Neutral);
            
            return directionsStream
                .Scan(
                    (nextIndex: 0, timestamp: 0.0),
                    (state, currentDirection) => {
                        
                        var currentTime = Time.realtimeSinceStartupAsDouble;
                        
                        if (state.nextIndex > 0 && (currentTime - state.timestamp > interval.TotalSeconds)) {
                            if (currentDirection == directions[0]) {
                                return (nextIndex: 1, timestamp: currentTime);
                            }
                            return (nextIndex: 0, timestamp: currentTime);
                        }

                        var expectedDirection = directions[state.nextIndex];

                        if (currentDirection == expectedDirection) {
                            return (nextIndex: state.nextIndex++, timestamp: currentTime);
                        }

                        if (currentDirection == directions[0]) {
                            return (nextIndex: 1, timestamp: currentTime);
                        }
                        
                        return (nextIndex: 0, timestamp: 0.0f);
                    })
                .Where(state => state.nextIndex == directions.Length)
                .Select(_ => Unit.Default);
        }

        public static Observable<Unit> Sequence(this Observable<InputSignal<Vector2>> stream, TimeSpan interval,
            float threshold, params Direction_Eight[] directions) 
        {
            if (directions.Length is 0 ) {
                return Observable.Empty<Unit>();
            }

            var directionsStream = stream
                .OnMove(threshold)
                .Select(x => x.Value.DirectionizeEight(threshold))
                .DistinctUntilChanged()
                .Where(d => d != Direction_Eight.Neutral);
            
            return directionsStream
                .Scan(
                    (nextIndex: 0, timestamp: 0.0),
                    (state, currentDirection) => {
                        
                        var currentTime = Time.realtimeSinceStartupAsDouble;
                        
                        if (state.nextIndex > 0 && (currentTime - state.timestamp > interval.TotalSeconds)) {
                            if (currentDirection == directions[0]) {
                                return (nextIndex: 1, timestamp: currentTime);
                            }
                            return (nextIndex: 0, timestamp: currentTime);
                        }

                        var expectedDirection = directions[state.nextIndex];

                        if (currentDirection == expectedDirection) {
                            return (nextIndex: state.nextIndex++, timestamp: currentTime);
                        }

                        if (currentDirection == directions[0]) {
                            return (nextIndex: 1, timestamp: currentTime);
                        }
                        
                        return (nextIndex: 0, timestamp: 0.0f);
                    })
                .Where(state => state.nextIndex == directions.Length)
                .Select(_ => Unit.Default);
        }
    }
}