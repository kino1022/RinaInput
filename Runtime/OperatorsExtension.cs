using System;
using R3;

namespace RinaInput
{
    public static class OperatorsExtension
    {
        /// <summary>
        /// 特定のストリームが流れているかどうかのストリームを形成する
        /// </summary>
        /// <param name="stream">判定を行うストリーム</param>
        /// <param name="tickInterval">何フレーム毎に判定を行うか</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>ストリームが流れているかのストリーム</returns>
        public static Observable<bool> IsStreamActive<T>(this Observable<T> stream, int tickInterval = 1)
        {
            if (tickInterval < 0) {
                return Observable.Empty<bool>();
            }

            Observable<bool> trueGate = stream.Select(_ => true);

            Observable<bool> falseGate = stream
                .Debounce(TimeSpan.FromTicks(tickInterval))
                .Select(_ => false);

            return Observable.Merge(trueGate, falseGate);
        }
    }
}