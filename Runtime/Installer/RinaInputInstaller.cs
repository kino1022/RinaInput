using System;
using RinaInput.Runtime.Provider;
using RinaInput.Runtime.Wrapper;
using RinaInput.Runtime.Wrapper.Interface;
using Sirenix.Serialization;
using VContainer;
using VContainer.Unity;

namespace RinaInput.Installer {
    /// <summary>
    /// RinaInputの稼働に必要なクラスをインストールするためのIInstaller
    /// </summary>
    [Serializable]
    public class RinaInputInstaller : IInstaller {

        [OdinSerialize] 
        private PlayerInputActions m_actionsMap;

        public void Install(IContainerBuilder builder) {
            
            builder
                .RegisterInstance(m_actionsMap)
                .As<PlayerInputActions>();

            builder
                .Register<IInputStreamProvider, InputStreamProvider>(Lifetime.Singleton)
                .As<IInputStreamProvider>();
            
            builder 
                .Register<IInputActionProvider, InputActionProvider>(Lifetime.Singleton)
                .As<IInputActionProvider>();
        }
    }
}