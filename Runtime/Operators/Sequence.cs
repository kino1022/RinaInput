using System;
using R3;
using RinaInput.Lever.Direction.Definition;
using RinaInput.Signal;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RinaInput.Operators {
    public static partial class InputStreamOperator {
        
        /// <summary>
        /// 厳格モード: 指定順に方向が入力されることを要求し、余計な方向入力が来たら即リセットします。
        /// - Started を起点にする
        /// - Neutral は無視
        /// - 各入力は直前のマッチから grace 秒以内である必要がある
        /// - 間違った入力が来たらリセット。ただしその入力が最初の方向と一致するならそこで新たに開始
        /// </summary>
        public static Observable<Unit> SequenceFour (this Observable<InputSignal<Vector2>> source, TimeSpan grace, float threshold = 0.1f ,params Direction_Four[] directions) {
            if (directions == null || directions.Length == 0) return Observable.Empty<Unit>();
            if (grace.TotalMilliseconds < 0) throw new ArgumentOutOfRangeException(nameof(grace));

            return source
                // Started のエッジのみを起点にする
                .Where(x => x.Phase == InputActionPhase.Started)
                // Vector2 を方向に変換
                .Select(x => (Time: x.Time, Dir: x.Value.DirectionizeFour(threshold)))
                // 中立は無視（厳格モードでも中立は余計な入力ではない）
                .Where(t => t.Dir != Direction_Four.Neutral)
                // 状態を持つスキャンで厳格なシーケンス判定を行う
                .Scan(
                    // NextIndex: 次に期待する directions のインデックス
                    // LastTime: 前回マッチした入力の time
                    // Emit: この入力でシーケンス完了（発火）したか
                    (NextIndex: 0, LastTime: 0.0, Emit: false),
                    (state, cur) => {
                        var next = state.NextIndex;
                        var last = state.LastTime;
                        var emit = false;

                        // ヘルパー: 指定の入力でシーケンス開始を試みる
                        static (int next, double last, bool emit) TryStart((double Time, Direction_Four Dir) curInput, Direction_Four[] dirs) {
                            if (curInput.Dir == dirs[0]) {
                                if (dirs.Length == 1) return (0, 0.0, true);
                                return (1, curInput.Time, false);
                            }
                            return (0, 0.0, false);
                        }

                        if (next == 0) {
                            // まだ開始していない -> 今の入力で開始できるか
                            var res = TryStart(cur, directions);
                            next = res.next;
                            last = res.last;
                            emit = res.emit;
                        } else {
                            // 既にシーケンスが進行中 -> 時間チェック
                            var elapsed = cur.Time - last;
                            if (elapsed > grace.TotalSeconds) {
                                // タイムアウト -> リセットだが、現在の入力が先頭と一致すればそこで新しく開始
                                var res = TryStart(cur, directions);
                                next = res.next;
                                last = res.last;
                                emit = res.emit;
                            } else {
                                // 時間内 -> 期待方向と合っているか
                                if (cur.Dir == directions[next]) {
                                    // マッチ -> 進める
                                    next++;
                                    last = cur.Time;
                                    if (next >= directions.Length) {
                                        // 完了
                                        emit = true;
                                        next = 0;
                                        last = 0.0;
                                    }
                                } else {
                                    // 間違った入力 -> 即リセット。だがこの入力が先頭と一致するならそこで新規開始
                                    var res = TryStart(cur, directions);
                                    next = res.next;
                                    last = res.last;
                                    emit = res.emit;
                                }
                            }
                        }

                        return (NextIndex: next, LastTime: last, Emit: emit);
                    }
                )
                // 発火したものだけ流す
                .Where(s => s.Emit)
                .Select(_ => Unit.Default);
        }
    }
}