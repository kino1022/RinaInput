using System;
using R3;
using RinaInput.Lever.Direction.Definition;
using RinaInput.Lever.Signal;
using RinaInput.Signal;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RinaInput.Lever.Operator
{

    public static class LeverStreamOperator
    {

        /// <summary>
        /// レバーが倒された際に発行されるストリーム
        /// </summary>
        /// <param name="source"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static Observable<InputSignal<Vector2>> OnMove(this Observable<InputSignal<Vector2>> source, float threshold = 0.2f)
        {
            return source
                //入力されている状態をフィルタ
                .Where(x => x.Phase == InputActionPhase.Started || x.Phase == InputActionPhase.Performed)
                //入力がもしNeutralならフィルタ
                .Where(x => x.Value.DirectionizeFour(threshold) != Direction_Four.Neutral)
                //Observableの値をLeverInputSignalに変更
                .Select(x => new InputSignal<Vector2>(
                    x.Phase,
                    x.Value,
                    x.Time
                ));
        }

        /// <summary>
        /// いづれかの方向へ入力されているかどうかをストリームに流す
        /// </summary>
        /// <param name="source"></param>
        /// <param name="direction"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static Observable<bool> MoveAnyDirection(this Observable<LeverInputSignal> source, Direction_Four direction, float threshold = 0.2f)
        {
            return source
                .Select(x => x.Value.DirectionizeFour(threshold) == direction);
        }

        public static Observable<Unit> Sequence(this Observable<LeverInputSignal> source, TimeSpan interval, float threshold, params Direction_Four[] directions)
        {
            //方向指定がない場合は空のストリームを返す
            if (directions is null || directions.Length == 0) return Observable.Empty<Unit>();

            //LeverInputSignalのObsevableをDrection_FourのObservableに変換
            var directionsStream = source
                    //渡されたInputSignalを四方向に変換
                    .Select(x => x.Value.DirectionizeFour(threshold))
                    //変化が起こるまで待機
                    .DistinctUntilChanged()
                    //Neutralは変化から除外
                    .Where(d => d != Direction_Four.Neutral);

            return directionsStream
                    .Scan(
                        (nextIndex: 0, timestamp: 0.0),
                        (state, currentDirection) =>
                        {
                            var currentTime = Time.realtimeSinceStartupAsDouble;
                            //進行中に時間切れになった場合の分岐処理
                            if (state.nextIndex > 0 && (currentTime - state.timestamp > interval.TotalSeconds))
                            {
                                //最初の方向が入力された場合はインデックスを強制的に１
                                if (currentDirection == directions[0])
                                {
                                    return (nextIndex: 1, timestamp: currentTime);
                                }
                                //方向が違った場合は初期化
                                return (nextIndex: 0, timestamp: 0.0);
                            }

                            //現在判定を行うDirectionのインデックス
                            var expectedDirection = directions[state.nextIndex];
                            //方向が一致した場合はインデックスをインクリメントして、制限時間を更新
                            if (currentDirection == expectedDirection)
                            {
                                return (nextIndex: state.nextIndex++, timestamp: currentTime);
                            }
                            else
                            {
                                //方向が一致せずに最初の方向が入力された場合は初期化
                                if (currentDirection == directions[0])
                                {
                                    return (nextIndex: 1, timestamp: currentTime);
                                }
                                //全てを初期化
                                return (nextIndex: 0, timestamp: 0.0);
                            }
                        })
                    //全ての入力が完了したかどうかの検知
                    .Where(state => state.nextIndex == directions.Length)
                    //入力が完了していた場合はUnitを放流
                    .Select(_ => Unit.Default);
        }

        public static Observable<Unit> Sequence_Eight(this Observable<InputSignal<Vector2>> source, TimeSpan interval, float threshold, params Direction_Eight[] directions)
        {
            if (directions is null || directions.Length is 0) return Observable.Empty<Unit>();

            var directionStream = source
                .Select(x => x.Value.DirectionizeEight(threshold))
                .DistinctUntilChanged()
                .Where(d => d is not Direction_Eight.Neutral);

            return directionStream
                .Scan(
                    (nextIndex: 0, timestamp: 0.0),
                    (state, currentDirection) =>
                    {
                        var currentTime = Time.realtimeSinceStartupAsDouble;

                        if (state.nextIndex > 0 && currentTime - state.timestamp > interval.TotalSeconds)
                        {
                            if (currentDirection == directions[0])
                            {
                                return (nextIndex: 1, timestamp: currentTime);
                            }
                            return (nextIndex: 0, timestamp: 0.0);
                        }

                        var expectedDirection = directions[state.nextIndex];

                        if (currentDirection == expectedDirection)
                        {
                            return (nextIndex: state.nextIndex++, timestamp: currentTime);
                        }
                        else
                        {
                            if (currentDirection == directions[0])
                            {
                                return (nextIndex: 1, timestamp: currentTime);
                            }
                            return (nextIndex: 0, timestamp: 0.0);
                        }
                    })
                    .Where(state => state.nextIndex == directions.Length)
                    .Select(_ => Unit.Default);
        }
    }
}