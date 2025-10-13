using System;
using System.Collections.Generic;
using R3;
using RinaInput.Signal;
using RinaInput.Wrapper.Interface;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace RinaInput.Provider {
    public class InputStreamProvider : IInputStreamProvider , IStartable {

        private readonly IObjectResolver m_resolver;

        private readonly Dictionary<Guid, object> m_actionCache = new();
        
        private IInputActionProvider m_actionProvider;
        
        
        [Inject]
        public InputStreamProvider(IObjectResolver resolver) {
            m_resolver = resolver;
        }

        public void Start() {
            m_actionProvider = m_resolver.Resolve<IInputActionProvider>();
        }

        public Observable<InputSignal<T>> GetStream<T>(InputAction action) where T : struct {
            if (action is null) return Observable.Empty<InputSignal<T>>();

            var actionId = action.id;

            if (m_actionCache.TryGetValue(actionId, out var cachedStream)) {
                if (cachedStream is Observable<InputSignal<T>> stream) {
                    return stream;
                }
                // 型が一致しない場合、同じアクションに異なる型を要求しているため、設計ミスの可能性が高い
                throw new InvalidOperationException(
                    $"Action '{action.name}' was already cached with a different type. " +
                    $"Requested: {typeof(T)}, Cached: {cachedStream.GetType().GetGenericArguments()[0].GetGenericArguments()[0]}"
                );
            }

            var startStream =
                Observable
                    .FromEvent<InputAction.CallbackContext>(
                        x => action.started += x,
                        x => action.started -= x
                    )
                    .Select(ctx => new InputSignal<T>(
                        InputActionPhase.Started,
                        ctx.ReadValue<T>(),
                        Time.realtimeSinceStartupAsDouble
                    ));

            var performedStream =
                Observable
                    .FromEvent<InputAction.CallbackContext>(
                        x => action.performed += x,
                        x => action.performed -= x
                    )
                    .Select(ctx => new InputSignal<T>(
                        InputActionPhase.Performed,
                        ctx.ReadValue<T>(),
                        Time.realtimeSinceStartupAsDouble
                    ));

            var canceledStream =
                Observable
                    .FromEvent<InputAction.CallbackContext>(
                        x => action.canceled += x,
                        x => action.canceled -= x
                    )
                    .Select(ctx => new InputSignal<T>(
                        InputActionPhase.Canceled,
                        ctx.ReadValue<T>(),
                        Time.realtimeSinceStartupAsDouble
                    ));

            var newStream = Observable
                .Merge(startStream, canceledStream, performedStream)
                .Share();

            if (newStream is object obj) {
                m_actionCache[actionId] = obj;
            }
            return newStream;
        }

        public Observable<InputSignal<T>> GetStream<T>(InputActionReference actionReference) where T : struct {
            return actionReference?.action is not null
                ? GetStream<T>(actionReference.action)
                : Observable.Empty<InputSignal<T>>();
        }
    }
}