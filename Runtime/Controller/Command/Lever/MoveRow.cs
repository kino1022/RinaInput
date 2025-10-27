using System;
using System.Collections.Generic;
using R3;
using RinaInput.Controller.Module;
using RinaInput.Lever.Direction.Definition;
using RinaInput.Operators;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace RinaInput.Controller.Command.Lever {
    [CreateAssetMenu(menuName = "RinaInput/コマンド/レバー連続入力")]
    public class MoveRow : AInputCommand
    {
        [OdinSerialize]
        [LabelText("入力ソース")]
        private IInputModule<Vector2> m_lever;

        [SerializeField]
        [LabelText("入力デットゾーン")]
        [ProgressBar(0.0f,1.0f)]
        private float m_deadZone = 0.1f;

        [OdinSerialize]
        [LabelText("入力方向")]
        private List<Direction_Four> m_directions = new List<Direction_Four>();

        protected override Observable<Unit> CreateStream()
        {

            if (m_directions.Count is 0 || m_directions is null || m_deadZone < 0.0f || m_inputGrace < 0)
            {
                return Observable.Empty<Unit>();
            }

            return m_lever
                .Stream
                .Sequence(
                    TimeSpan.FromSeconds(m_inputGrace),
                    m_deadZone,
                    m_directions.ToArray()
                );
        }
    }
}