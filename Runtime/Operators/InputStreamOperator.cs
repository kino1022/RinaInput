using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using R3;
using RinaInput.Signal;
using Sirenix.OdinValidator.Editor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RinaInput.Operators {
    public static class InputStreamOperator {

        /// <summary>
        /// 指定したボタンが何かしらの入力された際に流れるストリーム
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<InputSignal<T>> OnPressed<T>(this Observable<InputSignal<T>> source) where T : struct {
            return source.Where(_ => _.Phase == InputActionPhase.Started);
        }

        /// <summary>
        /// 一定時間押下状態が継続した際に流れるストリーム
        /// </summary>
        /// <param name="source"></param>
        /// <param name="duration"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<InputSignal<T>> Hold<T>(this Observable<InputSignal<T>> source, TimeSpan duration)
            where T : struct 
        {
            return source
                .Where(x => x.Phase == InputActionPhase.Canceled)
                .Select(startSignal =>
                    Observable
                        .Timer(duration)
                        .TakeUntil(source.Where(x => x.Phase == InputActionPhase.Canceled))
                        .Select(x => startSignal)
                )
                .Switch();
        }

        /// <summary>
        /// ストリームが流れているところに対して指定したストリームが流れた際に流れるストリーム
        /// </summary>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <param name="interval"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Observable<InputSignal<T>[]> Tap<T>(this Observable<InputSignal<T>> source, int count, long interval)
            where T : struct 
        {
            if (count < 0 || interval < 0) throw new ArgumentNullException();

            return source
                .OnPressed()
                .Timestamp()
                .Scan(
                    new List<(long timestamp, InputSignal<T> value)>(),
                    (list, current) => {
                        list.Add(current);
                        if (list.Count > count) {
                            list.RemoveAt(0);
                        }

                        return list;
                    })
                //入力回数分データがスタックされるまで待機
                .Where(list => list.Count == count)
                //リストの末尾とリストの先頭が猶予未満か感知
                .Where(list => (list[^1].timestamp - list[0].timestamp) <= interval)
                //リスト内のシグナルをリストにしてストリームに放流
                .Select(list => list.Select(item => item.value).ToArray());

        }

        public static Observable<Unit> TapInSpan<T>(this Observable<InputSignal<T>> source, int count, TimeSpan interval) where T : struct
        {
            return source
                .OnPressed()
                .Timestamp()
                .Scan(
                    new List<(long timestamp, InputSignal<T> value)>(),
                    (list, current) =>
                    {
                        list.Add(current);
                        if (list.Count > count)
                        {
                            list.RemoveAt(0);
                        }
                        return list;
                    })
                .Where(list => list.Count == count)
                .Where(list => (list[^1].timestamp - list[0].timestamp) <= interval.TotalSeconds)
                .Select(_ => Unit.Default);
        }

        /// <summary>
        /// 長押しを判定する
        /// </summary>
        /// <param name="source"></param>
        /// <param name="charge"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<TimeSpan> Charge<T>(this Observable<InputSignal<T>> source, TimeSpan charge)
            where T : struct
        {
            return source
                .OnPressed()
                .Select(start => (
                    Start: start.Time,
                    cancelledStream: source.Where(x => x.Phase == InputActionPhase.Canceled)))
                .SelectMany(t => t.cancelledStream.Select(end => TimeSpan.FromSeconds(end.Time - t.Start)))
                .Where(duration => duration < charge);
        }

        /// <summary>
        /// 厳密ではない同時押しの判定(長押しされている状態から残りのボタンを入力しても反応する)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="others"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<Unit> Chord<T>(this Observable<InputSignal<T>> source,
            params Observable<InputSignal<T>>[] others)
            where T : struct
        {
            var allStream = others
                .Concat(new List<Observable<InputSignal<T>>>(others.ToList()))
                .Select(s => s
                    .FirstAsync()
                    .ToObservable()
                    .Select(x => x.Phase == InputActionPhase.Started || x.Phase == InputActionPhase.Canceled)
                )
                .Distinct();

            return Observable
                .CombineLatest(allStream)
                .Where(s => s.All(IsPressed => IsPressed))
                .Select(_ => Unit.Default)
                .ThrottleFirst(TimeSpan.FromMilliseconds(1));
        }
        
        private enum State {Idle, Started}

        /// <summary>
        /// 時間制限を設けた複数ボタンの同時押しを検知するストリーム
        /// </summary>
        /// <param name="source"></param>
        /// <param name="interval">入力誤差に対して設ける猶予</param>
        /// <param name="others">同時に押すボタン</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<Unit> ChordInInterval<T>(this Observable<InputSignal<T>> source, TimeSpan interval,
            params Observable<InputSignal<T>>[] others) where T : struct 
        {
            //全てのストリームが入力されたかどうかのストリームを取得
            var pressStream = source.IsInputAny(others);

            return Observable
                //IEnumerable方式のストリームを統合
                .CombineLatest(pressStream)
                .Scan(
                    (State: State.Idle, StartTime: 0.0, IsCompleted: false),
                    (acc, currentStates) => {
                        //押されているかどうかの入力をいずれかに挿入する
                        var isAnyPressed = currentStates.Any(isPressed => isPressed);
                        //全てが入力されているかどうかの値を挿入する
                        var allPressed = currentStates.All(isPressed => isPressed);

                        switch (acc.State) {
                            //もし入力がなされていなかった場合には時間のみを現在の時間に更新する
                            case State.Idle:
                                if (isAnyPressed) {
                                    return (State.Started, Time.realtimeSinceStartupAsDouble, false);
                                }
                                break;
                            //入力が開始した場合の分岐処理
                            case State.Started:
                                //全体が既に入力されていた場合には現在の状態を返す
                                if (allPressed)
                                {
                                    return (acc.State, acc.StartTime, true);
                                }
                                //いずれも入力されていなかった場合は経過時間を初期化
                                if (!isAnyPressed)
                                {
                                    return (State.Idle, 0.0, false);
                                }
                                //経過時間が待機時間を上回った場合は全てを初期化する
                                if (Time.realtimeSinceStartupAsDouble - acc.StartTime > interval.TotalSeconds)
                                {
                                    return (State.Idle, 0.0, false);
                                }

                                break;
                        }

                        return (acc.State, acc.StartTime, acc.IsCompleted);
                    })
                // --- ここからが変更点 ---
                // DistinctUntilChanged(keySelector) を Scan と Where で手動実装
                .Scan(
                    // Item1: 前回の値, Item2: 今回の値 をタプルで保持
                    (Previous: default((State, double, bool)?), Current: default((State, double, bool))),
                    (acc, current) => (Previous: acc.Current, Current: current)
                )
                // IsCompletedフラグ(Item3)が、前回と今回で変化した瞬間だけを通過させる
                .Where(x => x.Previous == null || x.Previous.Value.Item3 != x.Current.Item3)
                // 今回の値だけを下流に流す
                .Select(x => x.Current)
                // --- 変更点ここまで ---
                .Where(s => s.Item3)
                .Select(_ => Unit.Default);
        }

        /// <summary>
        /// 渡された全ての入力ストリームを入力されているかどうかの真偽値型ソースに変換する
        /// </summary>
        /// <param name="source"></param>
        /// <param name="others"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Observable<bool>> IsInputAny<T>(this Observable<InputSignal<T>> source, params Observable<InputSignal<T>>[] others) where T : struct {
            var allStream = new List<Observable<InputSignal<T>>> {source};
            allStream.AddRange(others);

            return allStream
                .Select(s => s.Select(x => x.Phase is InputActionPhase.Started or InputActionPhase.Performed)
                    .DistinctUntilChanged()
                );
        }
    }
}