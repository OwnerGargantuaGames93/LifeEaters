using System.Collections.Generic;
using Ink.Runtime;
using UnityEngine;

namespace Control.DialogueHandler
{
    public class InkVariables
    {
        private Dictionary<string, Ink.Runtime.Object> _variables;
        
        public InkVariables(Story story)
        {
            _variables = new Dictionary<string, Ink.Runtime.Object>();
            foreach (var name in story.variablesState)
            {
                var value = story.variablesState.GetVariableWithName(name);
                _variables.Add(name, value);
            }
        }
        
        public void SyncVariablesAndStartListening(Story story)
        {
            SyncVariablesToStory(story);
            story.variablesState.variableChangedEvent += UpdateVariableState;
        }
        
        public void StopListening(Story story)
        {
            story.variablesState.variableChangedEvent -= UpdateVariableState;
        }
        
        public void ReloadFromStory(Story story)
        {
            _variables.Clear();
            foreach (var name in story.variablesState)
            {
                var value = story.variablesState.GetVariableWithName(name);
                _variables.Add(name, value);
                
                Debug.Log($"[InkVariables] Reloaded variable: {name} with value: {value}");
            }
        }

        public void UpdateVariableState(string name, Ink.Runtime.Object value)
        {
            if (!_variables.ContainsKey(name))
            {
                // Debug.Log("[InkVariables] Warning: Variable not found in local state: " + name);
                return;
            }
            
            // Debug.Log("[InkVariables] Variable changed: " + name + " with new value: " + value);
            _variables[name] = value;
        }
        
        public void SyncVariablesToStory(Story story)
        {
            foreach (var variable in _variables)
            {
                // Debug.Log("[InkVariables] Syncing variable to story: " + variable.Key + " with value: " + variable.Value);
                story.variablesState.SetGlobal(variable.Key, variable.Value);
            }
        }
        
        public void DebugInkVariables(Story story)
        {
            Debug.Log("[InkVariables] Current variables:");
            foreach (var name in story.variablesState)
            {
                var value = story.variablesState.GetVariableWithName(name);
                
                Debug.Log($"[InkVariables] Variable: {name}");

                if (value == null)
                {
                    Debug.LogWarning($"[InkVariables] Value is NULL for variable: {name}");
                    continue;
                }

                switch (value)
                {
                    case StringValue stringValue:
                        Debug.Log($"[InkVariables] Value (string): {stringValue.value}");
                        break;
                    case IntValue intValue:
                        Debug.Log($"[InkVariables] Value (int): {intValue.value}");
                        break;
                    case FloatValue floatValue:
                        Debug.Log($"[InkVariables] Value (float): {floatValue.value}");
                        break;
                    case BoolValue boolValue:
                        Debug.Log($"[InkVariables] Value (bool): {boolValue.value}");
                        break;
                    default:
                        Debug.Log($"[InkVariables] Value (unknown type): {value}");
                        break;
                }
            }
        }
    }
    
    #region Events
    
    public struct EInkVariableChanged
    {
        public readonly string VariableName;
        public readonly Ink.Runtime.Object NewValue;

        public EInkVariableChanged(string variableName, Ink.Runtime.Object newValue)
        {
            VariableName = variableName;
            NewValue = newValue;
        }
    }
    
    #endregion
}