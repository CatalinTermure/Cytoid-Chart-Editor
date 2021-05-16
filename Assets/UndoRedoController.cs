using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoRedoController : MonoBehaviour
{
    public void Undo() => CommandSystem.Undo();
    public void Redo() => CommandSystem.Redo();
}
