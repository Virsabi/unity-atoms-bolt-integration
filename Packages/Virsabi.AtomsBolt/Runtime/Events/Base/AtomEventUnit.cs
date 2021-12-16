using System;
using Bolt;
using Ludiq;
using UnityAtoms;
using UnityEngine;

[Bolt.UnitCategory("Atom\\Event")]
public abstract class AtomEventUnit<T, U> : EventUnit<T> where U : AtomEvent<T>
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

    [DoNotSerialize] [PortLabelHidden] public ValueOutput value { get; private set; }
    [DoNotSerialize] public ControlInput RegisterInput { get; private set; }

    [DoNotSerialize] public ControlInput UnregisterInput { get; private set; }

    protected override void Definition()
    {
        base.Definition();
        target = ValueInput("target", (U) null).NullMeansSelf();
        value = ValueOutput<T>("value");
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
    }

    protected override void AssignArguments(Flow flow, T args)
    {
        flow.SetValue(value, args);
    }

    //public override EventHook GetHook(GraphReference reference) => !reference.hasData ? (EventHook) this.hookName : new EventHook(this.hookName, (object) reference.GetElementData<AtomEventUnit<T, U>.Data>((IGraphElementWithData) this).Target);
    //protected virtual string hookName => throw new InvalidImplementationException(string.Format("Missing event hook for '{0}'.", (object) this));

    private void UpdateTarget(GraphStack stack)
    {
        Data elementData = stack.GetElementData<Data>(this);
        bool isListening = elementData.isListening;
        U atomEvent = Flow.FetchValue<U>(target, stack.ToReference());

        if (atomEvent == null)
        {
            Debug.LogError("No event found from the value input");
        }

        if (!(atomEvent != elementData.Target))
            return;
        //if (isListening)
        //    StopListening(stack);
        elementData.Target = atomEvent;
        //if (!isListening)
        //    return;
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
            elementData.Target.Unregister((Action<T>) elementData.handler);
            elementData.handler = null;
        }

        elementData.isListening = false;
    }

    protected virtual void StartListening(GraphStack stack, bool updateTarget)
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
            void Handler(T args) => Trigger(reference, args);
            elementData.handler = (Action<T>) Handler;
            elementData.Target.Register(Handler);
        }

        elementData.isListening = true;
    }


    public override void StartListening(GraphStack stack) => StartListening(stack, false);

    //[SpecialName]
    //FlowGraph IUnit.get_graph() => this.graph;

    private new class Data : EventUnit<T>.Data
    {
        public U Target;
    }
}