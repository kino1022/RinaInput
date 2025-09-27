using System;
using System.Collections.Generic;
using R3;
using RinaInput.Controller.Module;
using RinaInput.Lever.Direction.Definition;
using RinaInput.Lever.Operator;
using RinaInput.Lever.Signal;
using UnityEngine;

namespace RinaInput.Controller.Command.Lever {
    [CreateAssetMenu(menuName = "RinaInput/コマンド/レバー連続入力")]
    public class MoveRow : AInputCommand
    {

        private IInputModule<Vector2> m_lever;


        private float m_deadZone = 0.1f;

        private List<Direction_Four> m_directions = new List<Direction_Four>();

        protected override Observable<Unit> CreateStream()
        {

            if (m_directions.Count is 0 || m_directions is null || m_deadZone < 0.0f || m_inputGrace < 0)
            {
                return Observable.Empty<Unit>();
            }

            return m_lever
                .Stream
                .Select(x => new LeverInputSignal(
                    x.Phase,
                    x.Value,
                    x.Time
                ))
                .Sequence(
                    TimeSpan.FromMilliseconds(m_inputGrace),
                    m_deadZone,
                    m_directions.ToArray()
                );
        }
    }
}