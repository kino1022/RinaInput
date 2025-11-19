using System;
using RinaInput.Provider;
using RinaInput.Wrapper.Interface;
using RinaInput.Wrapper;
using Sirenix.Serialization;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace RinaInput.Installer {
    /// <summary>
    /// RinaInputの稼働に必要なクラスをインストールするためのIInstaller
    /// </summary>
    [Serializable]
    public class RinaInputInstaller : IInstaller {

        [OdinSerialize] 
        private InputActionAsset m_actionsMap;

        public void Install(IContainerBuilder builder) {

            builder
                .RegisterInstance(m_actionsMap)
                .As<InputActionAsset>();

            builder
                .Register<IInputStreamProvider, InputStreamProvider>(Lifetime.Singleton)
                .As<IInputStreamProvider>();
        }
    }
}