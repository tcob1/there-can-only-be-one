using UnityEngine;
using System.Collections.Generic;

public interface IStateful
{
    string GetUniqueID();
    Dictionary<string, object> GetState();
    void SetState(Dictionary<string, object> state);
}
