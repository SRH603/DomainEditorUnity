using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blackout.UI
{
    /// <summary>
    /// A generic undo/redo handler
    /// </summary>
    public class UndoHandler : MonoBehaviour
    {
        [SerializeField, Tooltip("The maximum number of undo steps to record")]
        public int maxSteps = 30;
        
        
        private readonly List<IUndoStep> _actions = new List<IUndoStep>();
        private int _index;
        private bool _lastUndo = true;

        public event Action OnUndoAction;
        public event Action OnRedoAction;

        private void OnDisable()
        {
            _actions.Clear();
            _index = 0;
            _lastUndo = true;
        }
        
        /// <summary>
        /// Record an undo step
        /// </summary>
        /// <param name="undoStep"></param>
        public void AddStep(IUndoStep undoStep)
        {
            if (undoStep == null)
                return;

            if (_index < _actions.Count)
            {
                for (int i = _index + 1; i < _actions.Count - _index - 1; i++)
                    _actions[i].Free();
                
                _actions.RemoveRange(_index + 1, _actions.Count - _index - 1);
            }

            if (_actions.Count >= maxSteps)
            {
                _actions[0].Free();
                _actions.RemoveAt(0);
            }

            _actions.Add(undoStep);
            _index = _actions.Count;
        }

        /// <summary>
        /// Perform a undo step on the specified curve
        /// </summary>
        public void PerformUndo()
        {
            if (_actions.Count == 0 || _index == -1)
                return;
            
            if (!_lastUndo)
            {
                _index -= 1;
                _lastUndo = true;
            }

            if (_index == _actions.Count)
                _index = _actions.Count - 1;

            _actions[_index].PerformUndo();
            
            _index = Mathf.Clamp(_index - 1, -1, _actions.Count);
            
            OnUndoAction?.Invoke();
        }

        /// <summary>
        /// Perform a redo step on the specified curve
        /// </summary>
        public void PerformRedo()
        {
            if (_index >= _actions.Count)
                return;

            if (_lastUndo)
            {
                _index = Mathf.Clamp(_index + 1, -1, _actions.Count);
                _lastUndo = false;
            }

            if (_index == -1)
                _index = 0;

            _actions[_index].PerformRedo();
            
            _index = Mathf.Clamp(_index + 1, -1, _actions.Count);
            
            OnRedoAction?.Invoke();
        }
    }
}