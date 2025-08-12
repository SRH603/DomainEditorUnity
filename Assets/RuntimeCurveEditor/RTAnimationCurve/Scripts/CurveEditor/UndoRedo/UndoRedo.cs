using System.Collections.Generic;
using UnityEngine;

namespace RuntimeCurveEditor
{
    public class UndoRedo
    {
        int undoIndex;

        List<Operation> operations = new List<Operation>();//TODO use two Stacks(one for undo, one for redo) instead of List

        const int MAX_OPERATIONS = 20;

        RectTransform undo;
        RectTransform redo;

        public UndoRedo(RectTransform undo, RectTransform redo) {
            this.undo = undo;
            this.redo = redo;
        }

        public void Undo() {
            if (operations.Count > undoIndex) {
                if (!redo.gameObject.activeSelf)
                {
                    redo.gameObject.SetActive(true);
                }
                undoIndex += 1;
                Operation undoneOperation = operations[operations.Count - undoIndex];
                undoneOperation.Undo();
                if (operations.Count == undoIndex)
                {
                    undo.gameObject.SetActive(false);
                }
            }
        }

        public void Redo() {
            if (undoIndex > 0) {
                if (!undo.gameObject.activeSelf)
                {
                    undo.gameObject.SetActive(true);
                }
                operations[operations.Count - undoIndex].Redo();
                undoIndex -= 1;
                if (undoIndex == 0)
                {
                    redo.gameObject.SetActive(false);
                }
            }
        }

        public void AddOperation(Operation operation) {
            if (redo.gameObject.activeSelf)
            {
                redo.gameObject.SetActive(false);
            }
            if (!undo.gameObject.activeSelf)
            {
                undo.gameObject.SetActive(true);
            }

            while (undoIndex > 0) {//once a new operation takes place the undone operations are lost forever
                undoIndex -= 1;
                operations.RemoveAt(operations.Count - 1);
            }
            operations.Add(operation);
            undoIndex = 0;
            if (operations.Count > MAX_OPERATIONS) {
                operations.RemoveAt(0);
            }
        }

        public void ClearOperationsList() {
            if (redo.gameObject.activeSelf)
            {
                redo.gameObject.SetActive(false);
            }
            if (undo.gameObject.activeSelf)
            {
                undo.gameObject.SetActive(false);
            }
            undoIndex = 0;
            operations.Clear();
        }

    }
}