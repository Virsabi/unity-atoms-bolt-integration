using System;
using Ludiq;
using UnityAtoms;
using UnityEngine;

namespace Bolt
{
  public abstract class AtomVariableUnit<T, P, E1, E2, F, U> : EventUnit<U> 
    where U : AtomVariable<T, P, E1, E2, F> 
    where P : struct, IPair<T>
    where E1 : AtomEvent<T>
    where E2 : AtomEvent<P>
    where F : AtomFunction<T, T> 
  {
    protected sealed override bool register => true;
    
    public override IGraphElementData CreateData() => new Data();

    /// <summary>The game object that listens for the event.</summary>
    /// <footer><a href="https://www.google.com/search?q=Bolt.AtomEventEventUnit%601.target">`AtomEventEventUnit.target` on google.com</a></footer>
    [DoNotSerialize]
    [NullMeansSelf]
    [PortLabel("Target")]
    [PortLabelHidden]
    private ValueInput target { get; set; }
    
    [DoNotSerialize]
    public ControlInput RegisterInput { get; private set; }
    [DoNotSerialize]
    public ControlInput UnregisterInput { get; private set; }
    [DoNotSerialize]
    public ControlInput InputFetch { get; private set; }

    protected override void Definition()
    {
      base.Definition();
      target = ValueInput("target", (U) null).NullMeansSelf();
      Value = ValueOutput<T>("value");
      OldValue = ValueOutput<T>("old value");
      InitialValue = ValueOutput<T>("initial value");
      RegisterInput = ControlInput("Register", delegate(Flow flow)
      {
        StartListening(flow.stack, true);
        return null;
      });
      
      UnregisterInput = ControlInput("Unregister", delegate(Flow flow)
      {
        StopListening(flow.stack);
        return null;
      });

      InputFetch = ControlInput("Fetch", Fetch);
    }

    private ControlOutput Fetch(Flow arg)
    {
      var reference = arg.stack.ToReference();
      Data elementData = reference.GetElementData<Data>(this);
      //bool isListening = elementData.isListening;
      U atomVariable = Flow.FetchValue<U>(target, arg.stack.ToReference());
      elementData.Target = atomVariable;
      Trigger(reference, elementData.Target);
      return null;
    }

    [DoNotSerialize]
    public ValueOutput Value { get; private set; }
    [DoNotSerialize]
    public ValueOutput OldValue { get; private set; }
    [DoNotSerialize]
    public ValueOutput InitialValue { get; private set; }

    protected override void AssignArguments(Flow flow, U args)
    {
      flow.SetValue(Value, args.Value);
      flow.SetValue(OldValue, args.OldValue);
      flow.SetValue(InitialValue, args.InitialValue);

    }


    //public override EventHook GetHook(GraphReference reference) => !reference.hasData ? (EventHook) this.hookName : new EventHook(this.hookName, (object) reference.GetElementData<AtomEventUnit<T, U>.Data>((IGraphElementWithData) this).Target);
    //protected virtual string hookName => throw new InvalidImplementationException(string.Format("Missing event hook for '{0}'.", (object) this));

    private void UpdateTarget(GraphStack stack)
    {
      Data elementData = stack.GetElementData<Data>(this);
      bool isListening = elementData.isListening;
      U atomVariable = Flow.FetchValue<U>(target, stack.ToReference());

      if (atomVariable == null)
      {
        Debug.LogError("No event found from the value input");
      }
      
      if (!(atomVariable != elementData.Target))
        return;
      //if (isListening)
      //  StopListening(stack);
      elementData.Target = atomVariable;
      //if (!isListening)
      //  return;
      //StartListening(stack, false);
    }

    public override void StopListening(GraphStack stack)
    {
      Data elementData = stack.GetElementData<Data>(this);
      if (!elementData.isListening)
        return;
      foreach (Flow activeCoroutine in elementData.activeCoroutines)
        activeCoroutine.StopCoroutine(false);
      if (register)
      {
        elementData.Target.Changed.Unregister((Action<T>) elementData.handler);
        elementData.handler = null;
      }
      elementData.isListening = false;
    }

    
    
    private void StartListening(GraphStack stack, bool updateTarget)
    {
      if (updateTarget)
        UpdateTarget(stack);
      Data elementData = stack.GetElementData<Data>(this);
      if (elementData.Target == null)
        return;
      
      if (elementData.isListening)
        return;
      
      if (register)
      {
        GraphReference reference = stack.ToReference();

        void Handler(T args) => Trigger(reference, elementData.Target);
        elementData.handler = (Action<T>) Handler;

        #if UNITY_EDITOR
        if (elementData.Target.Changed == null)
        {
          Debug.LogError("The Atom variable you are trying to register is missing an Atom Event Variable.");
        }
        #endif
        
        elementData.Target.Changed.Register((Action<T>) elementData.handler);
      }
      elementData.isListening = true;
    }
    

    public override void StartListening(GraphStack stack) => StartListening(stack, false);
    
    //[SpecialName]
    //FlowGraph IUnit.get_graph() => this.graph;

    private new class Data : EventUnit<U>.Data
    {
      public U Target;
    }
  }
}