using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using RinaInput.Signal;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RinaInput.Operators {
    public static partial class InputStreamOperator {

        /// <summary>
        /// 指定したボタンが何かしらの入力された際に流れるストリーム
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<InputSignal<T>> OnPressed<T>(this Observable<InputSignal<T>> source) where T : struct {
            return source
                .Where(x => x.Phase == InputActionPhase.Started);
        }

        /// <summary>
        /// 指定したボタンが入力されているかどうかの真偽値ストリームを提供する
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<bool> OnPressedWithBoolean<T>(this Observable<InputSignal<T>> source) where T : struct {
            return source
                .Select(x => x.Phase == InputActionPhase.Started || x.Phase == InputActionPhase.Performed);
        }

        /// <summary>
        /// 一定時間押下状態が継続した際に流れるストリーム
        /// （変更点：指定時間以上押されていた場合に、Release（Canceled）が来たときに流れるようにする）
        /// </summary>
        /// <param name="source"></param>
        /// <param name="duration"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Observable<InputSignal<T>> Hold<T>(this Observable<InputSignal<T>> source, TimeSpan duration)
            where T : struct 
        {
            if (duration.TotalMilliseconds < 0) throw new ArgumentOutOfRangeException(nameof(duration));

            // Started をトリガーとして、次に来る Canceled を待ち、
            // Canceled 時点で押下時間が duration 以上なら流す
            return source
                .Where(x => x.Phase == InputActionPhase.Started)
                .Select(startSignal =>
                    source
                        .Where(x => x.Phase == InputActionPhase.Canceled && x.Time >= startSignal.Time)
                        .FirstAsync()
                        .ToObservable()
                        // Cancel が来た時点で startSignal.Time と比較して duration 以上であれば流す
                        .Where(endSignal => TimeSpan.FromSeconds(endSignal.Time - startSignal.Time) >= duration)
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
        public static Observable<InputSignal<T>[]> Tap<T>(this Observable<InputSignal<T>> source, int count, TimeSpan interval)
            where T : struct 
        {
            if (count < 0 || interval.TotalMilliseconds < 0) throw new ArgumentNullException();

            return source
                .OnPressed()
                //流れたストリームに対してスタンプを添加
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
                .Where(list => (list[^1].timestamp - list[0].timestamp) <= interval.TotalMilliseconds)
                //リスト内のシグナルをリストにしてストリームに放流
                .Select(list => list.Select(item => item.value).ToArray());

        }

        /// <summary>
        /// 猶予時間内での連続入力を判定するストリーム
        /// </summary>
        /// <param name="source"></param>
        /// <param name="count"></param>
        /// <param name="interval"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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
                .Where(list => (list[^1].timestamp - list[0].timestamp) <= interval.TotalMilliseconds)
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
                .Select(s => 
                    s
                    .FirstAsync()
                    .ToObservable()
                    .Select(x => x.Phase == InputActionPhase.Started || x.Phase == InputActionPhase.Canceled)
                )
                .Distinct();

            return Observable
                .CombineLatest(allStream)
                .Where(s => s.All(isPressed => isPressed))
                .Select(_ => Unit.Default)
                .ThrottleFirst(TimeSpan.FromMilliseconds(1));
        }

        private enum State {
            Idle, 
            Started
        }

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
            var pressStream = source.IsInputAny(others).ToArray();
            var total = pressStream.Length;

            return Observable
                //IEnumerable方式のストリームを統合
                .CombineLatest(pressStream)
                .Scan(
                    // State, StartTime, HitSet, IsCompleted
                    (State: State.Idle, StartTime: 0.0, Hits: new HashSet<int>(), IsCompleted: false),
                    (acc, currentStates) => {
                        var isAnyPressed = currentStates.Any(isPressed => isPressed);

                        switch (acc.State) {
                            case State.Idle:
                                if (isAnyPressed) {
                                    // 起点となる瞬間。現在 true のものをヒットとして登録
                                    var newHits = new HashSet<int>();
                                    for (var i = 0; i < currentStates.Length; i++) {
                                        if (currentStates[i]) newHits.Add(i);
                                    }
                                    return (State.Started, Time.realtimeSinceStartupAsDouble, newHits, newHits.Count == total);
                                }
                                return acc;

                            case State.Started:
                                // 時間経過チェック
                                var elapsed = Time.realtimeSinceStartupAsDouble - acc.StartTime;
                                if (elapsed > interval.TotalSeconds) {
                                    // 期間外 -> リセット
                                    return (State.Idle, 0.0, new HashSet<int>(), false);
                                }

                                // 既存のヒットセットに現在 true のものを追加
                                var merged = new HashSet<int>(acc.Hits);
                                for (var i = 0; i < currentStates.Length; i++) {
                                    if (currentStates[i]) merged.Add(i);
                                }

                                return (State.Started, acc.StartTime, merged, merged.Count == total);

                            default:
                                return acc;
                        }
                    })
                // IsCompletedフラグの変化のみを通す（前回 null を許容）
                .Scan(
                    (Previous: default((State, double, HashSet<int>, bool)?), Current: default((State, double, HashSet<int>, bool))),
                    (acc, current) => (Previous: acc.Current, Current: current)
                )
                .Where(x => x.Previous == null || x.Previous.Value.Item4 != x.Current.Item4)
                .Select(x => x.Current)
                .Where(s => s.Item4)
                .Select(_ => Unit.Default);
        }

        /// <summary>
        /// `Observable` のコレクションを受け取るオーバーロード。
        /// List/Array を直接渡したい場合はこちらを使うと呼び出しが楽になります。
        /// </summary>
        public static Observable<Unit> ChordInInterval<T>(this IEnumerable<Observable<InputSignal<T>>> streams, TimeSpan interval) where T : struct
        {
            var pressStream = streams
                .Select(s => s.Select(x => x.Phase is InputActionPhase.Started or InputActionPhase.Performed)
                    .DistinctUntilChanged())
                .ToArray();

            var total = pressStream.Length;

            return Observable
                .CombineLatest(pressStream)
                .Scan(
                    // State, StartTime, HitSet, IsCompleted
                    (State: State.Idle, StartTime: 0.0, Hits: new HashSet<int>(), IsCompleted: false),
                    (acc, currentStates) => {
                        var isAnyPressed = currentStates.Any(isPressed => isPressed);

                        switch (acc.State) {
                            case State.Idle:
                                if (isAnyPressed) {
                                    // 起点となる瞬間。現在 true のものをヒットとして登録
                                    var newHits = new HashSet<int>();
                                    for (var i = 0; i < currentStates.Length; i++) {
                                        if (currentStates[i]) newHits.Add(i);
                                    }
                                    return (State.Started, Time.realtimeSinceStartupAsDouble, newHits, newHits.Count == total);
                                }
                                return acc;

                            case State.Started:
                                // 時間経過チェック
                                var elapsed = Time.realtimeSinceStartupAsDouble - acc.StartTime;
                                if (elapsed > interval.TotalSeconds) {
                                    // 期間外 -> リセット
                                    return (State.Idle, 0.0, new HashSet<int>(), false);
                                }

                                // 既存のヒットセットに現在 true のものを追加
                                var merged = new HashSet<int>(acc.Hits);
                                for (var i = 0; i < currentStates.Length; i++) {
                                    if (currentStates[i]) merged.Add(i);
                                }

                                return (State.Started, acc.StartTime, merged, merged.Count == total);

                            default:
                                return acc;
                        }
                    })
                // IsCompletedフラグの変化のみを通す（前回 null を許容）
                .Scan(
                    (Previous: default((State, double, HashSet<int>, bool)?), Current: default((State, double, HashSet<int>, bool))),
                    (acc, current) => (Previous: acc.Current, Current: current)
                )
                .Where(x => x.Previous == null || x.Previous.Value.Item4 != x.Current.Item4)
                .Select(x => x.Current)
                .Where(s => s.Item4)
                .Select(_ => Unit.Default);
        }

        /// <summary>
        /// エッジ（Started）ベースの同時押し判定。
        /// どれかのボタンの Started を起点に、その起点から interval 以内に全てのボタンの Started が発生すれば発火します。
        /// （短押し → リリースがはやい場合でもカウントされるので、ユーザの要望にマッチしやすい方式です）
        /// </summary>
        public static Observable<Unit> ChordInIntervalByEdge<T>(this IEnumerable<Observable<InputSignal<T>>> streams, TimeSpan interval) where T : struct
        {
            var streamArray = streams.ToArray();
            var total = streamArray.Length;

            // 各ストリームの Started を (index, time) として発行するストリーム群
            var indexed = streamArray
                .Select((s, idx) => s
                    .Where(x => x.Phase == InputActionPhase.Started)
                    .Select(x => (Idx: idx, Time: x.Time)))
                .ToArray();

            var merged = Observable.Merge(indexed);

            return merged.SelectMany(startEvent =>
            {
                var startTime = startEvent.Time;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"[Chord] start idx={startEvent.Idx} time={startTime} interval={interval.TotalMilliseconds}ms");
#endif

                // 各ストリームについて、起点から interval 以内に Started が来た場合は true、来なければ false
                var checks = streamArray.Select((s, idx) =>
                {
                    if (idx == startEvent.Idx)
                    {
                        // 起点ストリームは既に発火しているので true を即座に返す
                        return Observable.Return(true);
                    }

                    // hitOrTimeout: ウィンドウ内に Started が来たら true を返す。ウィンドウ終了時にまだ来ていなければ false を返す
                    var hitOrTimeout = s
                        .Where(x => x.Phase == InputActionPhase.Started && x.Time > startTime && TimeSpan.FromSeconds(x.Time - startTime) <= interval)
                        .Select(_ => true)
                        .TakeUntil(Observable.Timer(interval))
                        .DefaultIfEmpty(false)
                        .FirstAsync()
                        .ToObservable();

                    return hitOrTimeout;
                }).ToArray();

                // 全ストリームの結果を合成して、全て true なら発火
                return Observable.CombineLatest(checks)
                    .Where(results => results.All(b => b))
                    .Select(_ => Unit.Default)
                    .Take(1);
            }).ThrottleFirst(TimeSpan.FromMilliseconds(1));
        }

        /// <summary>
        /// params 版のラッパー。既存の usage に合わせて呼べるようにします。
        /// </summary>
        public static Observable<Unit> ChordInIntervalByEdge<T>(this Observable<InputSignal<T>> source, TimeSpan interval, params Observable<InputSignal<T>>[] others) where T : struct
        {
            var all = new List<Observable<InputSignal<T>>> { source };
            all.AddRange(others);
            return all.ChordInIntervalByEdge(interval);
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
